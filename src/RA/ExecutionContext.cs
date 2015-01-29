using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using RestSharp;
using RA.Extensions;

namespace RA
{
    public class ExecutionContext
    {
        private readonly SetupContext _setupContext;
        private readonly HttpActionContext _httpActionContext;
        private readonly RestClient _restClient;

        public ExecutionContext(SetupContext setupContext, HttpActionContext httpActionContext)
        {
            _setupContext = setupContext;
            _httpActionContext = httpActionContext;
            _restClient = new RestClient(_httpActionContext.Uri().SchemeAndHost());
        }

        public ResponseContext Then()
        {
            switch (_httpActionContext.HttpAction())
            {
                case HttpActionType.GET:
                    return Get();
                case HttpActionType.POST:
                    return Post();
                default:
                    throw new Exception("should not have gotten here");
            }
        }

        private ResponseContext Get()
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

            return BuildFromRestResponse(_restClient.Execute(request));
        }

        private ResponseContext Post()
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

            return BuildFromRestResponse(_restClient.Execute(request));
        }

        private ResponseContext BuildFromRestResponse(IRestResponse restResponse)
        {
            return new ResponseContext(
                restResponse.StatusCode,
                restResponse.ContentType,
                restResponse.ContentEncoding,
                restResponse.ContentLength,
                restResponse.Content,
                restResponse.Headers.Select(x => new KeyValuePair<string, string>(x.Name, x.Value.ToString()))
                    .ToDictionary(x => x.Key, x => x.Value)
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