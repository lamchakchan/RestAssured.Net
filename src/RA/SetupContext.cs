using System;
using System.Collections.Generic;
using System.Linq;
using RA.Extensions;

namespace RA
{
    public class SetupContext
    {
        private string _name;
        private string _host;
        private string _uri;
        private string _body;
        private Dictionary<string, string> _headers = new Dictionary<string, string>();
        private Dictionary<string, string> _parameters = new Dictionary<string, string>(); 

        public SetupContext Name(string name)
        {
            _name = name;
            return this;
        }

        public string Name()
        {
            return _name;
        }

        public SetupContext Host(string host)
        {
            _host = host;
            return this;
        }

        public string Host()
        {
            return _host;
        }

        public SetupContext Uri(string uri)
        {
            _uri = uri;
            return this;
        }

        public string Uri()
        {
            return _uri;
        }

        public SetupContext Body(string body)
        {
            _body = body;
            return this;
        }

        public string Body()
        {
            return _body;
        }

        public SetupContext Header(string key, string value)
        {
            if (!_headers.ContainsKey(key))
                _headers.Add(key, value);
            return this;
        }

        public Dictionary<string, string> Headers()
        {
            return _headers.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToDictionary(x => x.Key, x => x.Value);
        }

        public SetupContext Param(string key, string value)
        {
            if (!_parameters.ContainsKey(key))
                _parameters.Add(key, value);
            return this;
        }

        public Dictionary<string, string> Params()
        {
            return _parameters.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToDictionary(x => x.Key, x => x.Value);
        }

        public HttpActionContext When()
        {
            return new HttpActionContext(this);
        }

        public SetupContext Clone()
        {
            var setupContext = new SetupContext()
                .Name(_name)
                .Host(_host)
                .Uri(_uri);

            foreach (var header in _headers)
            {
                setupContext.Header(header.Key, header.Value);
            }

            foreach (var parameter in _parameters)
            {
                setupContext.Param(parameter.Key, parameter.Value);
            }

            return setupContext;
        }

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

            "parameters".WriteHeader();
            foreach (var parameter in _parameters)
            {
                "{0} : {1}".WriteLine(parameter.Key, parameter.Value);
            }

            return this;
        }
    }
}
