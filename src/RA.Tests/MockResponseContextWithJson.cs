using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using RA.Exceptions;
using RestAssured.Tests.Data;

namespace RA.Tests
{
    [TestFixture]
    public class MockSetupContextWithJson
    {
        public SetupContext _setup;
        private string _id;
        private string _name1;
        private string _name2;

        public MockSetupContextWithJson()
        {
            _id = Guid.NewGuid().ToString();
            _name1 = "Foo";
            _name2 = "f00Baz";
            var body = new
            {
                id = _id,
                company1 = new
                {
                    name = _name1
                },
                company2 = new
                {
                    name = _name2
                }
            };

            _setup = new SetupContext();
            _setup.Body(body);
        }

        [Test]
        public void BodySerializationMatch()
        {
            Assert.Equals(_setup.Body(),
                "{\"id\":\"4fc99dd1-9aad-4d63-a247-75c9b72de204\",\"company1\":{\"name\":\"Foo\"},\"company2\":{\"name\":\"f00Baz\"}}");
        }
    }

    [TestFixture]
    public class MockResponseContextWithJson
    {
        private ResponseContext _response;

        public MockResponseContextWithJson()
        {
            var responseContent =
                "{" +
                    "\"id\":\"3a6b4e0b-8e5c-df11-849b-0014c258f21e\", " +
                    "\"products\": [" +
                        "{\"id\" : \"2f355deb-423e-46aa-8d53-071b01018465\"}, " +
                        "{\"id\" : \"065983e6-092a-491b-99b0-be3de3fe74c9\", \"name\" : \"wizzy bang\"}" +
                    "]" +
                "}";
            var header = new Dictionary<string, IEnumerable<string>>()
                         {
                             {
                                 "Content-Type",
                                 new List<string> {"application/json"}
                             }
                         };
            var loadResults = new List<LoadResponse>() {new LoadResponse(200, 78978078)};
            _response = new ResponseContext(HttpStatusCode.OK, responseContent, header, loadResults);
        }

        [Test]
        public void RootIdShouldBeValid()
        {
            _response
                .TestBody("root id exist", x => x.id.ToString() == "3a6b4e0b-8e5c-df11-849b-0014c258f21e")
                .Assert("root id exist");
        }

        [Test]
        public void ProductCountShouldBeTwo()
        {
            _response
                .TestBody("there is two products", x => x.products.Count == 2)
                .Assert("there is two products");
        }

        [Test]
        public void SecondProductShouldHaveNameWizzyBang()
        {
            _response
                .TestBody("second product has a name", x => x.products[1].name == "wizzy bang")
                .Assert("second product has a name");
        }

        [Test]
        [ExpectedException(typeof(AssertException))]
        public void AccessingMissingNameShouldThrow()
        {
            _response
                .TestBody("should blow up", x => x.products[0].name == "")
                .Assert("should blow up");
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void DuplicateRuleShouldThrow()
        {
            _response
                .TestBody("should blow up again", x => x.product[1].name != "")
                .TestBody("should blow up again", x => x.product[1].name != "");
        }

        [Test]
        public void TestV3ValidSchema()
        {
            _response
                .Schema(Resource.V3ValidSchema);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestV3InvalidSchema()
        {
            _response
                .Schema(Resource.V3InvalidSchema);
        }

        [Test]
        [ExpectedException(typeof (AssertException))]
        public void TestV3RestrictiveSchema()
        {
            _response
                .Schema(Resource.V3RestrictiveSchema)
                .AssertSchema();
        }

        [Test]
        public void TestHeaderWithContentTypeUpperCased()
        {
            _response
                .TestHeader("content header has app/json uppper", "CONTENT=TYPE", x => x == "application/json")
                .Assert("content header has app/json upper");
        }

        [Test]
        public void TestHeaderWithContentTypeLowerCased()
        {
            _response
                .TestHeader("content header has app/json lower", "content-type", x => x.Contains("application/json"))
                .Assert("content header has app/json lower");
        }

        [Test]
        public void TestLoadForTotalCall()
        {
            _response
                .TestLoad("load for total call", "total-call", x => x > 0)
                .Assert("load for total call");
        }

        [Test]
        [ExpectedException(typeof(AssertException))]
        public void TestLoadforMoreThanTotalCallShouldThrow()
        {
            _response
                .TestLoad("load for more than total call", "total-call", x => x > 3)
                .Assert("load for more than total call");
        }

        [Test]
        public void TestV4ValidSchema()
        {
            _response
                .Schema(Resource.V4ValidSchema);
        }

        [Test]
        public void WriteAssertions()
        {
            _response.WriteAssertions();
        }
    }
}
