using System;
using RA.Extensions;

namespace RA
{
    public enum HttpActionType
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    public class HttpActionContext
    {
        private readonly SetupContext _setupContext;
        private string _url;
        private Uri _uri;
        private bool _isLoadTest = false;
        private int _threads = 1;
        private int _seconds = 60;
        private HttpActionType _httpAction;

        public HttpActionContext(SetupContext setupContext)
        {
            _setupContext = setupContext;
        }

        public HttpActionType HttpAction()
        {
            return _httpAction;
        }

        public string Url()
        {
            return _url;
        }

        public Uri Uri()
        {
            return _uri;
        }

        public bool IsLoadTest()
        {
            return _isLoadTest;
        }

        public int Threads()
        {
            return _threads;
        }

        public int Seconds()
        {
            return _seconds;
        }

        public HttpActionContext Load(int threads = 1, int seconds = 60)
        {
            _isLoadTest = true;

            if (threads < 0) threads = 1;
            _threads = threads;

            if (seconds < 0) seconds = 60;
            _seconds = seconds;

            return this;
        }

        public ExecutionContext Get(string url)
        {
            return SetHttpAction(url, HttpActionType.GET);
        }

        public ExecutionContext Get()
        {
            return SetHttpAction(null, HttpActionType.GET);
        }

        public ExecutionContext Post(string url)
        {
            return SetHttpAction(url, HttpActionType.POST);
        }

        public ExecutionContext Put(string url)
        {
            return SetHttpAction(url, HttpActionType.PUT);
        }

        public ExecutionContext Delete(string url)
        {
            return SetHttpAction(url, HttpActionType.DELETE);
        }

        public HttpActionContext Debug()
        {
            "url".WriteLine();
            "- {0}".WriteLine(_url);

            "action".WriteLine();
            "- {0}".WriteLine(_httpAction.ToString());

            return this;
        }

        private ExecutionContext SetHttpAction(string url, HttpActionType actionType)
        {
            SetUrl(url);
            _httpAction = actionType;
            return new ExecutionContext(_setupContext, this);
        }

        private void SetUrl(string url)
        {
            if (url.IsEmpty() && _setupContext.Host().IsEmpty())
                throw new ArgumentException("url must be provided");

            var uri = url.IsNotEmpty()
                ? new Uri(url.FixProtocol())
                : new Uri(new Uri(_setupContext.Host().FixProtocol()), _setupContext.Uri());

            _url = uri.OriginalString;
            _uri = uri;
        }
    }
}