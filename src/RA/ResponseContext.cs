using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using RA.Exceptions;
using RA.Extensions;

namespace RA
{
    public class ResponseContext
    {
        private readonly HttpStatusCode _statusCode;
        private readonly long _contentLength;
        private readonly string _content;
        private readonly string _contentType;
        private readonly string _contentEncoding;
        private dynamic _parsedContent;
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
        private readonly Dictionary<string, bool>  _assertions = new Dictionary<string, bool>();
        private bool _isSchemaValid = false;
        private List<string> _schemaErrors = new List<string>(); 
        
        public ResponseContext(HttpStatusCode statusCode, string contentType, string contentEncoding, long contentLength, string content, Dictionary<string, string> headers)
        {
            _statusCode = statusCode;
            _contentType = contentType;
            _contentEncoding = contentEncoding;
            _contentLength = contentLength;
            _content = content;
            _headers = headers;

            Parse();
        }

        public ResponseContext Test(string ruleName, Func<dynamic, bool> predicate)
        {
            if(_assertions.ContainsKey(ruleName))
                throw new ArgumentException(string.Format("({0}) already exist", ruleName));

            var result = false;

            try
            {
                result = predicate.Invoke(_parsedContent);
            }
            catch (Exception ex) 
            { }

            _assertions.Add(ruleName, result);

            return this;
        }

        public ResponseContext Schema(string schema)
        {
            JsonSchema jsonSchema = null;

            try
            {
                jsonSchema = JsonSchema.Parse(schema);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Schema is not valid", "schema", ex);
            }

            IList<string> messages;

            _isSchemaValid = JObject.Parse(_content).IsValid(jsonSchema, out messages);

            if (!_isSchemaValid)
            {
                foreach (var message in messages)
                {
                    _schemaErrors.Add(message);
                }
            }

            return this;
        }

        public void Assert(string ruleName)
        {
            if (_assertions.ContainsKey(ruleName))
            {
                if(!_assertions[ruleName])
                    throw new AssertException(string.Format("({0}) Test Failed", ruleName));
            }
        }

        public void AssertSchema()
        {
            if (!_isSchemaValid)
            {
                throw new AssertException(string.Format("Schema Check Failed"));
            }
        }

        public void AssertAll()
        {
            foreach (var assertion in _assertions)
            {
                if(!assertion.Value)
                    throw new AssertException(string.Format("({0}) Test Failed", assertion.Key));
            }
        }

        private void Parse()
        {
            if (_contentType.Contains("json"))
            {
                _parsedContent = JObject.Parse(_content);
                return;
            }

            throw new Exception(string.Format("({0}) not supported", _contentType));
        }

        public ResponseContext Debug()
        {
            "status code".WriteHeader();
            ((int)_statusCode).ToString().WriteLine();

            "content type".WriteHeader();
            _contentType.WriteLine();

            "content length".WriteHeader();
            _contentLength.ToString().WriteLine();

            "content encoding".WriteHeader();
            _contentEncoding.WriteLine();

            "response headers".WriteHeader();
            foreach (var header in _headers)
            {
                "{0} : {1}".WriteLine(header.Key, header.Value);
            }

            "content".WriteHeader();
            "{0}\n".Write(_content);

            "assertions".WriteHeader();
            foreach (var assertion in _assertions)
            {
                "{0} : {1}".WriteLine(assertion.Key, assertion.Value);
            }

            "schema errors".WriteHeader();
            foreach (var schemaError in _schemaErrors)
            {
                schemaError.WriteLine();
            }

            return this;
        }

        public ResponseContext WriteAssertions()
        {
            "assertions".WriteHeader();
            foreach (var assertion in _assertions)
            {
                assertion.WriteTest();
            }

            "schema validation".WriteHeader();
            if (_isSchemaValid) ConsoleExtensions.WritePassedTest();
            else ConsoleExtensions.WriteFailedTest();

            return this;
        }
    }
}