using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using RA.Enums;
using RA.Extensions;

namespace RA
{
    public class SetupContext
    {
        private string _name;
        private string _host;
        private int _port;
        private string _uri;
        private string _body;
        private HttpClient _httpClient;
        private bool _useHttps;
        private Dictionary<string, string> _headers = new Dictionary<string, string>();
        private Dictionary<string, string> _parameters = new Dictionary<string, string>();
        private Dictionary<string, string> _queryStrings = new Dictionary<string, string>();
		private Dictionary<string, string> _cookies = new Dictionary<string, string>();
        private List<FileContent> _files = new List<FileContent>(); 

        private Func<string, IDictionary<string, string>, List<string>> GetHeaderFor = (filter, headers) =>
        {
            var value =
                headers.Where(x => x.Key.Equals(filter, StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => x.Value)
                    .DefaultIfEmpty(string.Empty)
                    .First();

            return !string.IsNullOrEmpty(value) ? value.Split(new[] { ',' }).Select(x => x.Trim()).ToList() : new List<string>();
        };



        /// <summary>
        /// Setup the name of the test suite.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SetupContext Name(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Returns the name of the test suite.
        /// </summary>
        /// <returns></returns>
        public string Name()
        {
            return _name;
        }

        /// <summary>
        /// Setup the host value to use.
        /// eg: http://www.consoco.com
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public SetupContext Host(string host)
        {
            _host = host;
            return this;
        }

        /// <summary>
        /// Sets the port used for the interaction 
        /// e.g. localhost:1465 
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public SetupContext Port(int port)
        {
            _port = port;
            return this;
        }

        /// <summary>
        /// Returns the host value.
        /// </summary>
        /// <returns></returns>
        public string Host()
        {
            return PortSpecified() ? $"{_host}:{_port}":_host;
        }

        private bool PortSpecified()
        {
            return _port > 0 && _port != 88;
        }

        /// <summary>
        /// Setup the Uri, this is usually every after the host name.
        /// eg: /resource/identifer/path?query=value1
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public SetupContext Uri(string uri)
        {
            _uri = uri;
            return this;
        }

        /// <summary>
        /// Returns the Uri.
        /// </summary>
        /// <returns></returns>
        public string Uri()
        {
            return _uri;
        }

        /// <summary>
        /// Set a body of content to be used with a POST/PUT/DELETE action.
        /// This is usually a string blob of JSON or XML.  The body will not be used if Params exist.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public SetupContext Body(string body)
        {
            _body = body;
            return this;
        }

        /// <summary>
        /// Set a body of content to be used with a POST/PUT/DELETE action.
        /// An object will be serialized into JSON or XML.  The body will not be used if Params exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <returns></returns>
        public SetupContext Body<T>(T body) where T : class
        {
            //In the future, this needs to detect the content type and use
            //the correct serializer.
            _body = JsonConvert.SerializeObject(body);
            return this;
        }

        /// <summary>
        /// Return the body of content.
        /// </summary>
        /// <returns></returns>
        public string Body()
        {
            return _body;
        }

        /// <summary>
        /// Set a file to be used with a POST/PUT/DELETE action.  Adding a file will convert the body
        /// to a multipart/form.
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <param name="contentDispositionName"></param>
        /// <param name="contentType">eg: image/jpeg or application/octet-stream</param>
        /// <param name="content">Byte array of the data.  File.ReadAllBytes()</param>
        /// <returns></returns>
        public SetupContext File(string fileName, string contentDispositionName, string contentType, byte[] content)
        {
            _files.Add(new FileContent(fileName, contentDispositionName, contentType, content));
            return this;
        }

        /// <summary>
        /// Returns all files.
        /// </summary>
        /// <returns></returns>
        public List<FileContent> Files()
        {
            return _files.Select(x => new FileContent(x.FileName, x.ContentDispositionName, x.ContentType, x.Content)).ToList();
        }

		/// <summary>
		/// Set a cookie value pair.
		/// eg: name : X-XSRF-TOKEN and value : 123456789
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public SetupContext Cookie(string name, string value)
	    {
		    if(!_cookies.ContainsKey(name))
				_cookies.Add(name, value);
		    return this;
	    }

		/// <summary>
		/// Set cookie value pairs.
		/// eg: name : X-XSRF-TOKEN and value : 123456789
		/// </summary>
		/// <param name="cookies"></param>
		/// <returns></returns>
		public SetupContext Cookies(Dictionary<string, string> cookies)
	    {
		    foreach (var cookie in cookies)
		    {
				if (!_cookies.ContainsKey(cookie.Key))
					_cookies.Add(cookie.Key, cookie.Value);
			}
		    return this;
	    }

	    /// <summary>
	    /// Return all cookies.
	    /// </summary>
	    /// <returns></returns>
	    public Dictionary<string, string> Cookies()
	    {
		    return _cookies.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToDictionary(x => x.Key, x => x.Value);
	    }

		/// <summary>
		/// Set a Http request header value pair.
		/// eg: key : content-type and value : application/json
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public SetupContext Header(string key, string value)
        {
            if (!_headers.ContainsKey(key))
                _headers.Add(key, value);
            return this;
        }

	    /// <summary>
	    /// Sets multiple Http request header value pairs.
	    /// </summary>
	    /// <param name="headers"></param>
	    /// <returns></returns>
	    public SetupContext Headers(Dictionary<string, string> headers)
	    {
		    foreach (var header in headers)
		    {
			    if (!_headers.ContainsKey(header.Key))
				    _headers.Add(header.Key, header.Value);
		    }

		    return this;
	    }

		/// <summary>
		/// Return all headers.
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, string> Headers()
        {
            return _headers.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Return the value for a content-type header.
        /// </summary>
        /// <returns></returns>
        public List<string> HeaderContentType()
        {
            return GetHeaderFor(HeaderType.ContentType.Value, _headers);
        }

        /// <summary>
        /// Return the value for an accept header.
        /// </summary>
        /// <returns></returns>
        public List<string> HeaderAccept()
        {
            return GetHeaderFor(HeaderType.Accept.Value, _headers);
        }

        /// <summary>
        /// Return the value for an accept-encoding header.
        /// </summary>
        /// <returns></returns>
        public List<string> HeaderAcceptEncoding()
        {
            return GetHeaderFor(HeaderType.AcceptEncoding.Value, _headers);
        }

        /// <summary>
        /// Return the value for an accept-charset header.
        /// </summary>
        /// <returns></returns>
        public List<string> HeaderAcceptCharset()
        {
            return GetHeaderFor(HeaderType.AcceptCharset.Value, _headers);
        }

        /// <summary>
        /// Return all headers except for content-type, accept, accept-encoding and accept-charset
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> HeaderForEverythingElse()
        {
            return
                _headers.Where(
                    x =>
                        !HeaderType.GetAll()
                            .Any(y => y.Value.Equals(x.Key, StringComparison.InvariantCultureIgnoreCase)))
                    .ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Set a Form value pair.  This is any key/value pair that needs to be formatted into the body of the request
        /// for POST/PUT/DELETE actions.  Using this will negate the usage of Body()
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SetupContext Param(string key, string value)
        {
            if (!_parameters.ContainsKey(key))
                _parameters.Add(key, value);
            return this;
        }

	    /// <summary>
	    /// Set multiple Form value pairs.  This is any key/value pairs that needs to be formatted into the body of the request
	    /// for POST/PUT/DELETE actions.  Using this will negate the usage of Body()
	    /// </summary>
	    /// <param name="key"></param>
	    /// <param name="value"></param>
	    /// <returns></returns>
		public SetupContext Params(Dictionary<string, string> parameters)
	    {
		    foreach (var parameter in parameters)
		    {
				if (!_parameters.ContainsKey(parameter.Key))
				    _parameters.Add(parameter.Key, parameter.Value);
			}
		    return this;
		}

        /// <summary>
        /// Return form value pairs.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> Params()
        {
            return _parameters.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Set a Querystring value pair.  This is any key/value pair that needs to go into the Url.  This will be emitted with all Http verbs.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SetupContext Query(string key, string value)
        {
            if (!_queryStrings.ContainsKey(key))
                _queryStrings.Add(key, value);
            return this;
        }

	    /// <summary>
	    /// Set multiple Querystring value pairs.  This is any key/value pairs that needs to go into the Url.  This will be emitted with all Http verbs.
	    /// </summary>
	    /// <param name="key"></param>
	    /// <param name="value"></param>
	    /// <returns></returns>
		public SetupContext Queries(Dictionary<string, string> queries)
	    {
		    foreach (var query in queries)
		    {
				if (!_queryStrings.ContainsKey(query.Key))
					_queryStrings.Add(query.Key, query.Value);
			}
		    return this;
		}

        /// <summary>
        /// Return Querystring value pairs.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> Queries()
        {
            return _queryStrings.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToDictionary(x => x.Key, x => x.Value);
        }

        public SetupContext HttpClient(HttpClient client)
        {
            _httpClient = client;
            return this;
        }

        public HttpClient HttpClient()
        {
            if (_httpClient != null) return _httpClient;

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				//we will add cookies as a header latter in the request
				UseCookies = false,
            };
            _httpClient = new HttpClient(handler, true);
            return _httpClient;
        }

        /// <summary>
        /// Determines that the protocol will be HTTPS insted of HTTP
        /// e.g. https://...
        /// </summary>
        /// <returns></returns>
        public SetupContext UseHttps()
        {
            _useHttps = true;
            return this;
        }

        public bool UsesHttps()
        {
            return _useHttps;
        }

        public HttpActionContext When()
        {
            return new HttpActionContext(this);
        }

        /// <summary>
        /// Deep copy of this object
        /// </summary>
        /// <returns></returns>
        public SetupContext Clone()
        {
            var setupContext = new SetupContext()
                .Name(_name)
                .Host(_host)
                .Uri(_uri)
                .Body(_body)
                .HttpClient(_httpClient);

	        setupContext.Headers(_headers);

	        setupContext.Queries(_queryStrings);

	        setupContext.Params(_parameters);

            foreach (var file in _files)
            {
                setupContext.File(file.FileName, file.ContentDispositionName, file.ContentType, file.Content);
            }

            return setupContext;
        }

        /// <summary>
        /// Output all debug values from the setup context.
        /// </summary>
        /// <returns></returns>
        public SetupContext Debug()
        {
            "name".WriteHeader();
            _name.WriteLine();

            "host".WriteHeader();
            _host.WriteLine();

            "uri".WriteHeader();
            _uri.WriteLine();

            "body".WriteHeader();
            "{0}\n".Write(_body);

            "request headers".WriteHeader();
            foreach (var header in _headers)
            {
                "{0} : {1}".WriteLine(header.Key, header.Value);
            }

            "querystrings".WriteHeader();
            foreach (var queryString in _queryStrings)
            {
                "{0} : {1}".WriteLine(queryString.Key, queryString.Value);
            }

            "parameters".WriteHeader();
            foreach (var parameter in _parameters)
            {
                "{0} : {1}".WriteLine(parameter.Key, parameter.Value);
            }

            return this;
        }
    }
}
