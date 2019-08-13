﻿using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using RA.Exceptions;
// using RA.Tests.Data;

namespace RA.Tests
{
    [TestFixture]
    public class MockResponseContextWithJson
    {
        private readonly ResponseContext _responseWithObject, _responseWithObject2;
        private readonly ResponseContext _responseWithArray;
        private readonly ResponseContext _responseWithNothing;
        private static readonly int _mockElapsedMs = 500;
        private readonly TimeSpan _mockElapsedTimespan = new TimeSpan(0, 0, 0, 0, _mockElapsedMs);

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
                _mockElapsedTimespan, loadResults);
            _responseWithObject2 = new ResponseContext(HttpStatusCode.OK, responseObjectContent, header,
                _mockElapsedTimespan, loadResults);
            _responseWithArray = new ResponseContext(HttpStatusCode.OK, responseArrayContent, header,
                _mockElapsedTimespan, loadResults);
            _responseWithNothing = new ResponseContext(HttpStatusCode.OK, "", emptyHeader, _mockElapsedTimespan,
                loadResults);
        }

        [Test]
        public void NoSchemaShouldPass()
        {
            _responseWithObject2
                .AssertAll();
        }

        [Test]
        public void AllowMultipleWithoutSchemaAssertion()
        {
            _responseWithObject2
                .TestStatus("first", code => code == 200)
                .TestBody("secon1", body => body.id != null)
                .AssertAll();
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
                .TestElaspedTime("faster elapsed time", x => x > _mockElapsedMs - 1)
                .Assert("faster elapsed time");
        }

        [Test]
        public void LesserExecutionTimeShouldFail()
        {
            Assert.Throws<AssertException>(() =>
            {
                _responseWithObject
                    .TestElaspedTime("slower elapsed time", x => x > _mockElapsedMs + 1)
                    .Assert("slower elapsed time");
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
                .TestHeader("content header has app/json upper", "CONTENT-TYPE", x => x == "application/json")
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
        public void WriteAssertions()
        {
            _responseWithObject.WriteAssertions();
        }

        [Test]
        public void AllowMultipleNamedAssertions()
        {
            _responseWithObject
                .TestStatus("first", code => code == 200)
                .TestBody("second", body => body.id != null)
                .Assert("first")
                .Assert("second");
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
    }
}