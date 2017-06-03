using System;
using RA.Extensions;

namespace RA
{
    public enum HttpActionType
    {
        GET,
        POST,
        PUT,
        PATCH,
        DELETE
    }

    public class HttpActionContext
    {
        private readonly SetupContext _setupContext;
        private string _url;
        private bool _isLoadTest = false;
        private int _threads = 1;
        private int _seconds = 60;
        private HttpActionType _httpAction;

        public HttpActionContext(SetupContext setupContext)
        {
            _setupContext = setupContext;
        }

        /// <summary>
        /// Return the Http Verb that will be used for the test.
        /// </summary>
        /// <returns></returns>
        public HttpActionType HttpAction()
        {
            return _httpAction;
        }

        /// <summary>
        /// Return the Url
        /// </summary>
        /// <returns></returns>
        public string Url()
        {
            return _url;
        }

        /// <summary>
        /// Return value indicating setup for load test.
        /// </summary>
        /// <returns></returns>
        public bool IsLoadTest()
        {
            return _isLoadTest;
        }

        /// <summary>
        /// Return thread count used in load test.
        /// </summary>
        /// <returns></returns>
        public int Threads()
        {
            return _threads;
        }

        /// <summary>
        /// Return seconds to use for load test.
        /// </summary>
        /// <returns></returns>
        public int Seconds()
        {
            return _seconds;
        }

        /// <summary>
        /// Configure load test with the number of threads and amount of time in seconds to run the test with.
        /// Default of 1 thread and 60 seconds are used if no values are specified.
        /// </summary>
        /// <param name="threads"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public HttpActionContext Load(int threads = 1, int seconds = 60)
        {
            _isLoadTest = true;

            if (threads < 0) threads = 1;
            _threads = threads;

            if (seconds < 0) seconds = 60;
            _seconds = seconds;

            return this;
        }

        /// <summary>
        /// Configure test with a GET verb.  The url parameter is optional if similar info was provided through the setup context.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ExecutionContext Get(string url = null)
        {
            return SetHttpAction(url, HttpActionType.GET);
        }

        /// <summary>
        /// Configure test with a POST verb.  The url parameter is optional if similar info was provided through the setup context.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ExecutionContext Post(string url = null)
        {
            return SetHttpAction(url, HttpActionType.POST);
        }

        /// <summary>
        /// Configure test with a PUT verb.  The url parameter is optional if similar info was provided through the setup context.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ExecutionContext Put(string url = null)
        {
            return SetHttpAction(url, HttpActionType.PUT);
        }

        /// <summary>
        /// Configure test with a PATCH verb.  The url parameter is optional if similar info was provided through the setup context.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ExecutionContext Patch(string url = null)
        {
            return SetHttpAction(url, HttpActionType.PATCH);
        }

        /// <summary>
        /// Configure test with a DELETE verb.  The url parameter is optional if similar info was provided through the setup context.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ExecutionContext Delete(string url = null)
        {
            return SetHttpAction(url, HttpActionType.DELETE);
        }

        /// <summary>
        /// Output all debug values from the setup context.
        /// </summary>
        /// <returns></returns>
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

        internal void SetUrl(string url)
        {
            if (url.IsEmpty() && _setupContext.Host().IsEmpty())
                throw new ArgumentException("url must be provided");

            var uri = url.IsNotEmpty()
                ? new Uri(url.FixProtocol(_setupContext.UsesHttps()))
                : new Uri(new Uri(_setupContext.Host().FixProtocol(_setupContext.UsesHttps())), _setupContext.Uri());

            _url = uri.OriginalString;
        }
    }
}