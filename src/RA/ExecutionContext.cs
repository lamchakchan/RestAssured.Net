using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using RestSharp;
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
        private readonly RestClient _restClient;
        private RestRequest _restRequest;
        private ConcurrentQueue<LoadResponse> _loadReponses = new ConcurrentQueue<LoadResponse>(); 

        public ExecutionContext(SetupContext setupContext, HttpActionContext httpActionContext)
        {
            _setupContext = setupContext;
            _httpActionContext = httpActionContext;
            _restClient = new RestClient(_httpActionContext.Uri().SchemeAndHost());
        }

        public ResponseContext Then()
        {
            var request = Build();

            if (_httpActionContext.IsLoadTest())
                StartCallsForLoad(request);

            return BuildFromRestResponse(_restClient.Execute(request));
        }

        #region HttpAction Strategy
        private RestRequest Build()
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

        private RestRequest BuildGet()
        {
            var request = new RestRequest(_httpActionContext.Uri().PathAndQuery, Method.GET);

            var headers = _setupContext.Headers();

            if (headers.Any())
                request.Parameters.RemoveAll(x => x.Type == ParameterType.HttpHeader);

            foreach (var header in headers)
            {
                request.AddHeader(header.Key, header.Value);
            }

            foreach (var param in _setupContext.Params())
            {
                request.AddParameter(param.Key, param.Value, ParameterType.QueryString);
            }

            return request;
        }

        private RestRequest BuildPost()
        {
            var request = new RestRequest(_httpActionContext.Uri().PathAndQuery, Method.POST);

            var headers = _setupContext.Headers();

            if (headers.Any())
                request.Parameters.RemoveAll(x => x.Type == ParameterType.HttpHeader);

            foreach (var header in headers)
            {
                request.AddHeader(header.Key, header.Value);
            }

            foreach (var param in _setupContext.Params())
            {
                request.AddParameter(param.Key, param.Value, ParameterType.GetOrPost);
            }

            request.RequestFormat = DataFormat.Json;
            request.AddParameter("text/json", _setupContext.Body(), ParameterType.RequestBody);

            return request;
        }

        private RestRequest BuildPut()
        {
            var request = new RestRequest(_httpActionContext.Uri().PathAndQuery, Method.PUT);

            var headers = _setupContext.Headers();

            if (headers.Any())
                request.Parameters.RemoveAll(x => x.Type == ParameterType.HttpHeader);

            foreach (var header in headers)
            {
                request.AddHeader(header.Key, header.Value);
            }

            foreach (var param in _setupContext.Params())
            {
                request.AddParameter(param.Key, param.Value, ParameterType.GetOrPost);
            }

            request.RequestFormat = DataFormat.Json;
            request.AddParameter("text/json", _setupContext.Body(), ParameterType.RequestBody);

            return request;
        }

        private RestRequest BuildDelete()
        {
            var request = new RestRequest(_httpActionContext.Uri().PathAndQuery, Method.DELETE);

            var headers = _setupContext.Headers();

            if (headers.Any())
                request.Parameters.RemoveAll(x => x.Type == ParameterType.HttpHeader);

            foreach (var header in headers)
            {
                request.AddHeader(header.Key, header.Value);
            }

            foreach (var param in _setupContext.Params())
            {
                request.AddParameter(param.Key, param.Value, ParameterType.GetOrPost);
            }

            request.RequestFormat = DataFormat.Json;
            request.AddParameter("text/json", _setupContext.Body(), ParameterType.RequestBody);

            return request;
        }
        #endregion

        #region Load Test Code

        public void StartCallsForLoad(RestRequest request)
        {
            ServicePointManager.DefaultConnectionLimit = _httpActionContext.Threads();

            var cancellationTokenSource = new CancellationTokenSource();

            var taskThreads = new List<Task>();
            for(var i = 0; i < _httpActionContext.Threads(); i++)
            {
                taskThreads.Add(Task.Run(async () => await SingleThread(request, cancellationTokenSource.Token), cancellationTokenSource.Token));
            }

            Timer timer = null;
            timer = new Timer((ignore) =>
                              {
                                  timer.Dispose();
                                  cancellationTokenSource.Cancel();
                              }, null, TimeSpan.FromSeconds(_httpActionContext.Seconds()), TimeSpan.FromMilliseconds(-1));

            AsyncContext.Run(async () => await Task.WhenAll(taskThreads));
        }

        public async Task SingleThread(RestRequest request, CancellationToken cancellationToken)
        {
            do
            {
                await MeasureSingleCall(request);
            } while (!cancellationToken.IsCancellationRequested); 
        }

        public async Task MeasureSingleCall(RestRequest request)
        {
            var loadResponse = new LoadResponse(-1, -1);
            _loadReponses.Enqueue(loadResponse);
            var watch = new Stopwatch();
            watch.Start();
            var response = await _restClient.ExecuteTaskAsync(request);
            watch.Stop();
            loadResponse.StatusCode = (int) response.StatusCode;
            loadResponse.Ticks = watch.ElapsedTicks;
        }

        #endregion

        private ResponseContext BuildFromRestResponse(IRestResponse restResponse)
        {
            return new ResponseContext(
                restResponse.StatusCode,
                restResponse.ContentType,
                restResponse.ContentEncoding,
                restResponse.ContentLength,
                restResponse.Content,
                restResponse.Headers.Select(x => new KeyValuePair<string, string>(x.Name, x.Value.ToString()))
                    .ToDictionary(x => x.Key, x => x.Value),
                _loadReponses.ToList()
                );

        }

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