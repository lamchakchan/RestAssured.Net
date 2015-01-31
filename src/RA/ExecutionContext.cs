using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Nito.AsyncEx;
using RA.Extensions;

namespace RA
{
    public class LoadResponse
    {
        public LoadResponse(int statusCode, long ticks)
        {
            Ticks = ticks;
            StatusCode = statusCode;
        }
        public long Ticks { get; set; }
        public int StatusCode { get; set; }
    }

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

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            _httpClient = new HttpClient(handler, true);
        }

        public ResponseContext Then()
        {
            if (_httpActionContext.IsLoadTest())
                StartCallsForLoad();

            var response = AsyncContext.Run(async () => await _httpClient.SendAsync(BuildRequest()));
            return BuildFromResponse(response);
        }

        #region HttpAction Strategy
        private HttpRequestMessage BuildRequest()
        {
            switch (_httpActionContext.HttpAction())
            {
                case HttpActionType.GET:
                    return BuildGet();
                case HttpActionType.POST:
                    return BuildPost();
                case HttpActionType.PUT:
                    return BuildPut();
                case HttpActionType.DELETE:
                    return BuildDelete();
                default:
                    throw new Exception("should not have gotten here");
            }
        }

        private HttpRequestMessage BuildGet()
        {
            var builder = new UriBuilder(_httpActionContext.Url());
            var query = HttpUtility.ParseQueryString(builder.Query);

            foreach (var param in _setupContext.Params())
            {
                query.Add(param.Key, param.Value);
            }

            builder.Query = query.ToString();

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(builder.ToString()),
                Method = HttpMethod.Get
            };

            BuildHeaders(request);
            
            return request;
        }

        private HttpRequestMessage BuildPost()
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = _httpActionContext.Uri(),
                Method = HttpMethod.Post
            };

            BuildHeaders(request);

            if (_setupContext.Params().Any())
                request.Content =
                    new FormUrlEncodedContent(
                        _setupContext.Params().Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToList());
            else
                request.Content = new StringContent(_setupContext.Body(), Encoding.UTF8, _setupContext.HeaderContentType().FirstOrDefault());

            return request;
        }

        private HttpRequestMessage BuildPut()
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = _httpActionContext.Uri(),
                Method = HttpMethod.Put
            };

            BuildHeaders(request);

            if (_setupContext.Params().Any())
                request.Content =
                    new FormUrlEncodedContent(
                        _setupContext.Params().Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToList());
            else
                request.Content = new StringContent(_setupContext.Body(), Encoding.UTF8, _setupContext.HeaderContentType().FirstOrDefault());

            return request;
        }

        private HttpRequestMessage BuildDelete()
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = _httpActionContext.Uri(),
                Method = HttpMethod.Delete
            };

            BuildHeaders(request);

            if (_setupContext.Params().Any())
                request.Content =
                    new FormUrlEncodedContent(
                        _setupContext.Params().Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToList());
            else
                request.Content = new StringContent(_setupContext.Body(), Encoding.UTF8, _setupContext.HeaderContentType().FirstOrDefault());

            return request;
        }

        private void BuildHeaders(HttpRequestMessage request)
        {
            _setupContext.HeaderAccept().ForEach(x => request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(x)));
            _setupContext.HeaderAcceptEncoding().ForEach(x => request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(x)));
            _setupContext.HeaderAcceptCharset().ForEach(x => request.Headers.AcceptCharset.Add(new StringWithQualityHeaderValue(x)));
            _setupContext.HeaderForEverythingElse().ForEach(x => request.Headers.Add(x.Key, x.Value));
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
                await MeasureSingleCall();
            } while (!cancellationToken.IsCancellationRequested); 
        }

        public async Task MeasureSingleCall()
        {
            var loadResponse = new LoadResponse(-1, -1);
            _loadReponses.Enqueue(loadResponse);
            var watch = new Stopwatch();
            watch.Start();
            var response = await _httpClient.SendAsync(BuildRequest());
            watch.Stop();
            loadResponse.StatusCode = (int) response.StatusCode;
            loadResponse.Ticks = watch.ElapsedTicks;
        }

        #endregion

        private ResponseContext BuildFromResponse(HttpResponseMessage response)
        {
            var content = AsyncContext.Run(async () => await response.Content.ReadAsStringAsync());

            return new ResponseContext(
                response.StatusCode,
                response.Content.Headers.ContentType.MediaType,
                response.Content.Headers.ContentEncoding.FirstOrDefault(),
                response.Content.Headers.ContentLength.Value,
                content,
                response.Content.Headers.ToDictionary(x => x.Key, x => x.Value),
                _loadReponses.ToList()
                );

        }

        //private ResponseContext BuildFromRestResponse(IRestResponse restResponse)
        //{
        //    return new ResponseContext(
        //        restResponse.StatusCode,
        //        restResponse.ContentType,
        //        restResponse.ContentEncoding,
        //        restResponse.ContentLength,
        //        restResponse.Content,
        //        restResponse.Headers.Select(x => new KeyValuePair<string, string>(x.Name, x.Value.ToString()))
        //            .ToDictionary(x => x.Key, x => x.Value),
        //        _loadReponses.ToList()
        //        );

        //}

        public ExecutionContext Debug()
        {
            "host".WriteHeader();
            _httpActionContext.Uri().Host.WriteLine();
            "absolute path".WriteHeader();
            _httpActionContext.Uri().AbsolutePath.WriteLine();
            "absolute uri".WriteHeader();
            _httpActionContext.Uri().AbsoluteUri.WriteLine();
            "authority".WriteHeader();
            _httpActionContext.Uri().Authority.WriteLine();
            "path and query".WriteHeader();
            _httpActionContext.Uri().PathAndQuery.WriteLine();
            "original string".WriteHeader();
            _httpActionContext.Uri().OriginalString.WriteLine();
            "scheme".WriteHeader();
            _httpActionContext.Uri().Scheme.WriteLine();

            return this;
        }
    }
}