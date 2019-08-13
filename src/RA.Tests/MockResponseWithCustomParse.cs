using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace RA.Tests
{
    [TestFixture]
    public class MockResponseWithCustomParse
    {
        private readonly ResponseContext _responseWithCsv;
        private static readonly int _mockElapsedMs = 500;
        private readonly TimeSpan _mockElapsedTimespan = new TimeSpan(0, 0, 0, 0, _mockElapsedMs);

        public static dynamic CsvToJson(string input)
        {
            string[] lines = input.Split("\n");
            string[] keys = lines[0].Split(",");
            JArray jarr = new JArray();
            for(int i=1;i<lines.Length;i++)
            {
                string[] values = lines[i].Split(",");
                JObject jobj = new JObject();
                for(int j=0;j<values.Length;j++)
                    jobj.Add(keys[j],values[j]);
                jarr.Add(jobj);
            }
            return jarr;
        }
        public MockResponseWithCustomParse()
        {
            var responseObjectContent =
                "key,value\n" +
                "AL,Alabama\n" +
                "AK,Alaska";

            var header = new Dictionary<string, IEnumerable<string>>
            {
                {
                    "Content-Type",
                    new List<string> {"text/csv"}
                }
            };

            var emptyHeader = new Dictionary<string, IEnumerable<string>>();

            var loadResults = new List<LoadResponse> {new LoadResponse(200, 78978078)};

            ResponseContext.AddParser("csv", CsvToJson);

            _responseWithCsv = new ResponseContext(HttpStatusCode.OK, responseObjectContent, header,
                _mockElapsedTimespan, loadResults);
        }

        [Test]
        public void CustomParserShouldPass()
        {
            _responseWithCsv
                .TestHeader("testHeader","Content-type", x => x.Contains("csv"))
                .TestBody("first item has AL", x => x[0].key == "AL")
                .TestBody("first item has Alabama", x => x[0].value == "Alabama")
                .TestBody("second item has AK", x => x[1].key == "AK")
                .AssertAll();
        }
    }
}