using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using RA.Enums;
using RA.Exceptions;
using RA.Extensions;
using System.Xml.Linq;

namespace RA
{
    public class ResponseContext
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;
        private dynamic _parsedContent;
        private readonly Dictionary<string, IEnumerable<string>> _headers = new Dictionary<string, IEnumerable<string>>();
        private TimeSpan _elapsedExecutionTime;
        private readonly Dictionary<string, double> _loadValues = new Dictionary<string, double>();
        private readonly Dictionary<string, bool> _assertions = new Dictionary<string, bool>();
        private readonly List<LoadResponse> _loadResponses;
        private bool _isSchemaValid = true;
        private List<string> _schemaErrors = new List<string>();


        public ResponseContext(HttpStatusCode statusCode, string content, Dictionary<string, IEnumerable<string>> headers, TimeSpan elaspedExecutionTime, List<LoadResponse> loadResponses)
        {
            _statusCode = statusCode;
            _content = content;
            _headers = headers;
            _elapsedExecutionTime = elaspedExecutionTime;
            _loadResponses = loadResponses ?? new List<LoadResponse>();

            Initialize();
        }

        /// <summary>
        /// Retrieve an object from the response document.
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public object Retrieve(Func<dynamic, object> func)
        {
            try
            {
                return func.Invoke(_parsedContent).Value;
            }
            catch { }

            try
            {
                return func.Invoke(_parsedContent);
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Setup a test against the body of response document.
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public ResponseContext TestBody(string ruleName, Func<dynamic, bool> func)
        {
            return TestWrapper(ruleName, () => func.Invoke(_parsedContent));
        }

        /// <summary>
        /// Setup a test against the response headers.
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public ResponseContext TestHeader(string ruleName, string key, Func<string, bool> func)
        {
            return TestWrapper(ruleName, () => func.Invoke(HeaderValue(key.Trim())));
        }

        /// <summary>
        /// Setup a test against the response time (total milliseconds)
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public ResponseContext TestElaspedTime(string ruleName, Func<double, bool> func)
        {
            return TestWrapper(ruleName, () => func.Invoke(_elapsedExecutionTime.TotalMilliseconds));
        }

        /// <summary>
        /// Setup a test against the calculated load test values.  The entire set of
        /// load test values are only available if a load test was configured with the
        /// When().Load() call.
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public ResponseContext TestLoad(string ruleName, string key, Func<double, bool> func)
        {
            return TestWrapper(ruleName, () => func.Invoke(LoadValue(key.Trim())));
        }

        /// <summary>
        /// Setup a test against the response status
        /// eg: OK 200 or Error 400
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public ResponseContext TestStatus(string ruleName, Func<int, bool> func)
        {
            return TestWrapper(ruleName, () => func.Invoke((int)_statusCode));
        }

        private ResponseContext TestWrapper(string ruleName, Func<bool> func)
        {
            if (_assertions.ContainsKey(ruleName))
                throw new ArgumentException(string.Format("Rule for ({0}) already exist", ruleName));

            var result = false;

            try
            {
                result = func.Invoke();
            }
            catch
            { }

            _assertions.Add(ruleName, result);

            return this;
        }

        /// <summary>
        /// Setup a schema to validate the response body structure with.
        /// For JSON, v3 and v4 of the JSON schema draft specificiation are respected.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public ResponseContext Schema(string schema)
        {
            JSchema jSchema = null;

            try
            {
                jSchema = JSchema.Parse(schema);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Schema is not valid", "schema", ex);
            }

            IList<string> messages;

            var trimmedContent = _content.TrimStart();

            _isSchemaValid =
                trimmedContent.StartsWith("{")
                    ? JObject.Parse(_content).IsValid(jSchema, out messages)
                    : JArray.Parse(_content).IsValid(jSchema, out messages);

            if (!_isSchemaValid)
            {
                foreach (var message in messages)
                {
                    _schemaErrors.Add(message);
                }
            }

            return this;
        }

        public ResponseContext Assert(string ruleName)
        {
            if (!_assertions.ContainsKey(ruleName)) return this;

            if (!_assertions[ruleName])
                throw new AssertException($"({ruleName}) Test Failed");
            // in order to allow multiple asserts
            return this;
        }

        /// <summary>
        /// Assert schema for validity.  Failures will produce an AssertException.
        /// </summary>
        public void AssertSchema()
        {
            if (!_isSchemaValid)
            {
                throw new AssertException(string.Format("Schema Check Failed"));
            }
        }

        /// <summary>
        /// Assert all test and schema for validity. You can optionally skip schema validation.  Failures will produce an AssertException.
        /// </summary>
        /// /// <param name="assertSchema"></param>
        public void AssertAll(bool assertSchema = true)
        {
            Console.WriteLine(_assertions.Count);
            foreach (var assertion in _assertions)
            {
                Console.WriteLine(assertion.Value);
                if (!assertion.Value)
                    throw new AssertException(string.Format("({0}) Test Failed", assertion.Key));
            }
            if (assertSchema)
                AssertSchema();
        }

        private void Initialize()
        {
            Parse();
            ParseLoad();
        }

        private void Parse()
        {
            var contentType = ContentType();

            if (contentType.Contains("json"))
            {
                if (!string.IsNullOrEmpty(_content))
                {
                    try
                    {
                        _parsedContent = JObject.Parse(_content);
                        return;
                    }
                    catch
                    {
                    }

                    try
                    {
                        _parsedContent = JArray.Parse(_content);
                        return;
                    }
                    catch
                    {
                    }

                    try
                    {
                        _parsedContent = JContainer.Parse(_content);
                        return;
                    }
                    catch
                    {
                    }


                }
                else
                {
                    return;
                }
            }
            else if (contentType.Contains("xml"))
            {
                if (!string.IsNullOrEmpty(_content))
                {
                    try
                    {
                        _parsedContent = XDocument.Parse(_content);
                        return;
                    }
                    catch
                    {
                    }
                }
            }

            if (!string.IsNullOrEmpty(_content))
                throw new Exception(string.Format("({0}) not supported", contentType));
        }

        private void ParseLoad()
        {
            if (_loadResponses.Any())
            {
                _loadValues.Add(LoadValueTypes.TotalCall.Value, _loadResponses.Count);
                _loadValues.Add(LoadValueTypes.TotalSucceeded.Value, _loadResponses.Count(x => x.StatusCode == (int)HttpStatusCode.OK));
                _loadValues.Add(LoadValueTypes.TotalLost.Value, _loadResponses.Count(x => x.StatusCode == -1));
                _loadValues.Add(LoadValueTypes.AverageTTLMs.Value, new TimeSpan((long)_loadResponses.Where(x => x.StatusCode == (int)HttpStatusCode.OK).Average(x => x.Ticks)).TotalMilliseconds);
                _loadValues.Add(LoadValueTypes.MaximumTTLMs.Value, new TimeSpan(_loadResponses.Where(x => x.StatusCode == (int)HttpStatusCode.OK).Max(x => x.Ticks)).TotalMilliseconds);
                _loadValues.Add(LoadValueTypes.MinimumTTLMs.Value, new TimeSpan(_loadResponses.Where(x => x.StatusCode == (int)HttpStatusCode.OK).Min(x => x.Ticks)).TotalMilliseconds);
            }
        }

        private string ContentType()
        {
            return HeaderValue(HeaderType.ContentType.Value);
        }

        private string HeaderValue(string key)
        {
            return _headers.Where(x => x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => string.Join(", ", x.Value))
                    .DefaultIfEmpty(string.Empty)
                    .FirstOrDefault();
        }

        private double LoadValue(string key)
        {
            return _loadValues.Where(x => x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    .Select(x => x.Value)
                    .DefaultIfEmpty(0)
                    .FirstOrDefault();
        }

        /// <summary>
        /// Output all debug values from the setup context.
        /// </summary>
        /// <returns></returns>
        public ResponseContext Debug()
        {
            "status code".WriteHeader();
            ((int)_statusCode).ToString().WriteLine();

            "response headers".WriteHeader();
            foreach (var header in _headers)
            {
                "{0} : {1}".WriteLine(header.Key, string.Join(", ", header.Value));
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

            if (_loadResponses.Any())
            {
                "load test result".WriteHeader();
                LoadValueTypes.GetAll().ForEach(x => "{0} {1}".WriteLine(_loadValues[x.Value], x.DisplayName.ToLower()));
            }

            return this;
        }

        /// <summary>
        /// Write the response of all asserted test via Console.IO
        /// </summary>
        /// <returns></returns>
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