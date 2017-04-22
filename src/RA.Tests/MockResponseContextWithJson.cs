using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using RA.Exceptions;
using RestAssured.Tests.Data;

namespace RA.Tests
{
    [TestFixture]
    public class MockResponseContextWithJson
    {
        private readonly ResponseContext _responseWithObject;
        private readonly ResponseContext _responseWithArray;
        private readonly ResponseContext _responseWithNothing;
        private static readonly int _mockElaspedMs = 500;
        private readonly TimeSpan _mockElaspedTimespan = new TimeSpan(0, 0, 0, 0, _mockElaspedMs);

        public MockResponseContextWithJson()
        {
            var responseObjectContent =
                "{" +
                "\"id\":\"3a6b4e0b-8e5c-df11-849b-0014c258f21e\", " +
                "\"products\": [" +
                "{\"id\" : \"2f355deb-423e-46aa-8d53-071b01018465\"}, " +
                "{\"id\" : \"065983e6-092a-491b-99b0-be3de3fe74c9\", \"name\" : \"wizzy bang\"}" +
                "]" +
                "}";
            var responseArrayContent =
                "[" +
                "{\"key\":\"AL\", \"value\":\"Alabama\"}," +
                "{\"key\":\"AK\", \"value\":\"Alaska\"}" +
                "]";

            var header = new Dictionary<string, IEnumerable<string>>
            {
                {
                    "Content-Type",
                    new List<string> {"application/json"}
                }
            };

            var emptyHeader = new Dictionary<string, IEnumerable<string>>();


            var loadResults = new List<LoadResponse> {new LoadResponse(200, 78978078)};
            _responseWithObject = new ResponseContext(HttpStatusCode.OK, responseObjectContent, header,
                _mockElaspedTimespan, loadResults);
            _responseWithArray = new ResponseContext(HttpStatusCode.OK, responseArrayContent, header,
                _mockElaspedTimespan, loadResults);
            _responseWithNothing = new ResponseContext(HttpStatusCode.OK, "", emptyHeader, _mockElaspedTimespan,
                loadResults);
        }

        [Test]
        public void AccessingMissingNameShouldThrow()
        {
            Assert.Throws<AssertException>(() =>
            {
                _responseWithObject
                    .TestBody("should blow up", x => x.products[0].name == "")
                    .Assert("should blow up");
            });
        }

        [Test]
        public void ArrayResponseContentShouldPass()
        {
            _responseWithArray
                .TestBody("first item has AL", x => x[0].key == "AL")
                .Assert("first item has AL");
        }

        [Test]
        public void DuplicateRuleShouldThrow()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _responseWithObject
                    .TestBody("should blow up again", x => x.product[1].name != "")
                    .TestBody("should blow up again", x => x.product[1].name != "");
            });
        }

        [Test]
        public void EmptyResponseShouldPass()
        {
            _responseWithNothing
                .TestStatus("is-ok", x => x == 200)
                .Assert("is-ok");
        }

        [Test]
        public void GreaterExecutionTimeShouldPass()
        {
            _responseWithObject
                .TestElaspedTime("faster elasped time", x => x > _mockElaspedMs - 1)
                .Assert("faster elasped time");
        }

        [Test]
        public void LesserExecutionTimeShouldFail()
        {
            Assert.Throws<AssertException>(() =>
            {
                _responseWithObject
                    .TestElaspedTime("slower elasped time", x => x > _mockElaspedMs + 1)
                    .Assert("slower elasped time");
            });
        }

        [Test]
        public void ProductCountShouldBeTwo()
        {
            _responseWithObject
                .TestBody("there is two products", x => x.products.Count == 2)
                .Assert("there is two products");
        }

        [Test]
        public void RetrieveShouldPass()
        {
            var id = _responseWithObject.Retrieve(x => x.id);
            var name = _responseWithObject.Retrieve(x => x.products[1].name);
            Assert.AreEqual(id, "3a6b4e0b-8e5c-df11-849b-0014c258f21e");
            Assert.AreEqual(name, "wizzy bang");
        }

        [Test]
        public void RootIdShouldBeValid()
        {
            _responseWithObject
                .TestBody("root id exist", x => x.id.ToString() == "3a6b4e0b-8e5c-df11-849b-0014c258f21e")
                .Assert("root id exist");
        }

        [Test]
        public void SecondProductShouldHaveNameWizzyBang()
        {
            _responseWithObject
                .TestBody("second product has a name", x => x.products[1].name == "wizzy bang")
                .Assert("second product has a name");
        }

        [Test]
        public void TestHeaderWithContentTypeLowerCased()
        {
            _responseWithObject
                .TestHeader("content header has app/json lower", "content-type", x => x.Contains("application/json"))
                .Assert("content header has app/json lower");
        }

        [Test]
        public void TestHeaderWithContentTypeUpperCased()
        {
            _responseWithObject
                .TestHeader("content header has app/json uppper", "CONTENT=TYPE", x => x == "application/json")
                .Assert("content header has app/json upper");
        }

        [Test]
        public void TestLoadforMoreThanTotalCallShouldThrow()
        {
            Assert.Throws<AssertException>(() =>
            {
                _responseWithObject
                    .TestLoad("load for more than total call", "total-call", x => x > 3)
                    .Assert("load for more than total call");
            });
        }

        [Test]
        public void TestLoadForTotalCall()
        {
            _responseWithObject
                .TestLoad("load for total call", "total-call", x => x > 0)
                .Assert("load for total call");
        }

        [Test]
        public void TestV3InvalidSchema()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _responseWithObject
                    .Schema(Resource.V3InvalidSchema);
            });
        }

        [Test]
        public void TestV3RestrictiveSchema()
        {
            Assert.Throws<AssertException>(() =>
            {
                _responseWithObject
                    .Schema(Resource.V3RestrictiveSchema)
                    .AssertSchema();
            });
        }

        [Test]
        public void TestV3ValidSchema()
        {
            _responseWithObject
                .Schema(Resource.V3ValidSchema);
        }

        [Test]
        public void TestV4ValidSchema()
        {
            _responseWithObject
                .Schema(Resource.V4ValidSchema);
        }

        [Test]
        public void WriteAssertions()
        {
            _responseWithObject.WriteAssertions();
        }
    }
}