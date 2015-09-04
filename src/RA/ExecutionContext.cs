using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Nito.AsyncEx;
using RA.Extensions;
using RA.Net;

namespace RA
{
    public class ExecutionContext
    {
        private readonly SetupContext _setupContext;
        private readonly HttpActionContext _httpActionContext;
        private readonly HttpClient _httpClient;
        private ConcurrentQueue<LoadResponse> _loadReponses = new ConcurrentQueue<LoadResponse>();

        public ExecutionContext(SetupContext setupContext, HttpActionContext httpActionContext)
        {
            _setupContext = setupContext;
            _httpActionContext = httpActionContext;

            var httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };

            var handler = !_setupContext.ContentMD5() ? (HttpMessageHandler) httpClientHandler : new ContentMd5Handler()
            {
                InnerHandler = httpClientHandler
            };
            _httpClient = new HttpClient(handler, true);
        }

        public ResponseContext Then()
        {
            if (_httpActionContext.IsLoadTest())
                StartCallsForLoad();

            var response = AsyncContext.Run(async () => await ExecuteCall());
            return BuildFromResponse(response);
        }

        #region HttpAction Strategy
        private HttpRequestMessage BuildRequest()
        {
            HttpRequestMessage message = null;
            switch (_httpActionContext.HttpAction())
            {
                case HttpActionType.GET:
                    message = BuildGet();
                    break;
                case HttpActionType.POST:
                    message = BuildPost();
                    break;
                case HttpActionType.PUT:
                    message = BuildPut();
                    break;
                case HttpActionType.DELETE:
                    message = BuildDelete();
                    break;
                default:
                    throw new Exception("should not have gotten here");
            }

            return message;
        }

        private HttpRequestMessage BuildGet()
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = BuildUri(),
                Method = HttpMethod.Get
            };

            AppendHeaders(request);
            
            return request;
        }

        private HttpRequestMessage BuildPost()
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = BuildUri(),
                Method = HttpMethod.Post
            };

            AppendHeaders(request);

            request.Content = BuildContent();

            return request;
        }

        private HttpRequestMessage BuildPut()
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = BuildUri(),
                Method = HttpMethod.Put
            };

            AppendHeaders(request);

            request.Content = BuildContent();

            return request;
        }

        private HttpRequestMessage BuildDelete()
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = BuildUri(),
                Method = HttpMethod.Delete
            };

            AppendHeaders(request);

            request.Content = BuildContent();

            return request;
        }

        private Uri BuildUri()
        {
            var builder = new UriBuilder(_httpActionContext.Url());
            var query = HttpUtility.ParseQueryString(builder.Query);

            foreach (var queryString in _setupContext.Queries())
            {
                query.Add(queryString.Key, queryString.Value);
            }

            builder.Query = query.ToString();
            return new Uri(builder.ToString());
        }

        private void AppendHeaders(HttpRequestMessage request)
        {
            _setupContext.HeaderAccept().ForEach(x => request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(x)));
            _setupContext.HeaderAcceptEncoding().ForEach(x => request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(x)));
            _setupContext.HeaderAcceptCharset().ForEach(x => request.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue(x)));
            _setupContext.HeaderAuthorization().ForEach(x =>
            {
                var match = Regex.Match(x, "^\\s?(?<scheme>.*)\\s(?<parameter>.*)");
                request.Headers.Authorization = new AuthenticationHeaderValue(match.Groups["scheme"].Value, match.Groups["parameter"].Value);
            });
            _setupContext.HeaderForEverythingElse().ForEach(x => request.Headers.TryAddWithoutValidation(x.Key, x.Value));
            
        }

        #endregion

        #region Content Buildup Strategy

        private HttpContent BuildContent()
        {
            if (_setupContext.Files().Any())
                return BuildMultipartContent();
            if (_setupContext.Params().Any())
                return BuildFormContent();
            if (!string.IsNullOrEmpty(_setupContext.Body()))
                return BuildStringContent();

            return null;
        }

        private HttpContent BuildMultipartContent()
        {
            var content = new MultipartFormDataContent();

            _setupContext.Params().ForEach(x => content.Add(new StringContent(x.Value), x.Key.Quote()));

            _setupContext.Files().ForEach(x =>
            {
                var fileContent = new ByteArrayContent(x.Content);
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    Name = x.ContentDispositionName.Quote(),
                    FileName = x.FileName
                };
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(x.ContentType);

                content.Add(fileContent);
            });

            return content;
        }

        private HttpContent BuildFormContent()
        {
            return new FormUrlEncodedContent(
                _setupContext.Params().Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToList());
        }

        private HttpContent BuildStringContent()
        {
            return new StringContent(_setupContext.Body(), Encoding.UTF8,
                _setupContext.HeaderContentType().FirstOrDefault());
        }

        #endregion

        #region Load Test Code

        public void StartCallsForLoad()
        {
            ServicePointManager.DefaultConnectionLimit = _httpActionContext.Threads();

            var cancellationTokenSource = new CancellationTokenSource();

            var taskThreads = new List<Task>();
            for(var i = 0; i < _httpActionContext.Threads(); i++)
            {
                taskThreads.Add(Task.Run(async () =>
                {
                    await SingleThread(cancellationTokenSource.Token);
                }, cancellationTokenSource.Token));
            }

            Timer timer = null;
            timer = new Timer((ignore) =>
                              {
                                  timer.Dispose();
                                  cancellationTokenSource.Cancel();
                              }, null, TimeSpan.FromSeconds(_httpActionContext.Seconds()), TimeSpan.FromMilliseconds(-1));

            AsyncContext.Run(async () => await Task.WhenAll(taskThreads));
        }

        public async Task SingleThread(CancellationToken cancellationToken)
        {
            do
            {
                await MapCall();
            } while (!cancellationToken.IsCancellationRequested); 
        }

        public async Task MapCall()
        {
            var loadResponse = new LoadResponse(-1, -1);
            _loadReponses.Enqueue(loadResponse);

            var result = await ExecuteCall();
            loadResponse.StatusCode = (int) result.Response.StatusCode;
            loadResponse.Ticks = result.ElaspedExecution.Ticks;
        }

        #endregion

        private async Task<HttpResponseMessageWrapper> ExecuteCall()
        {
            var watch = new Stopwatch();
            watch.Start();
            var response = await _httpClient.SendAsync(BuildRequest());
            watch.Stop();
            return new HttpResponseMessageWrapper {ElaspedExecution = watch.Elapsed, Response = response};
        }

        private ResponseContext BuildFromResponse(HttpResponseMessageWrapper result)
        {
            var content = AsyncContext.Run(async () => await result.Response.Content.ReadAsStringAsync());

            return new ResponseContext(
                result.Response.StatusCode,
                content,
                result.Response.Content.Headers.ToDictionary(x => x.Key.Trim(), x => x.Value),
                result.ElaspedExecution,
                _loadReponses.ToList()
                );

        }

        /// <summary>
        /// Output all debug values from the setup context.
        /// </summary>
        /// <returns></returns>
        public ExecutionContext Debug()
        {
            var uri = BuildUri();

            "host".WriteHeader();
            uri.Host.WriteLine();
            "absolute path".WriteHeader();
            uri.AbsolutePath.WriteLine();
            "absolute uri".WriteHeader();
            uri.AbsoluteUri.WriteLine();
            "authority".WriteHeader();
            uri.Authority.WriteLine();
            "path and query".WriteHeader();
            uri.PathAndQuery.WriteLine();
            "original string".WriteHeader();
            uri.OriginalString.WriteLine();
            "scheme".WriteHeader();
            uri.Scheme.WriteLine();

            return this;
        }
    }
}