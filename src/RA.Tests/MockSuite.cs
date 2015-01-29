using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using RA.Exceptions;

namespace RA.Tests
{
    [TestFixture]
    public class MockSuite
    {
        private ResponseContext _response;

        public MockSuite()
        {
            var responseContent =
                "{" +
                    "'id':'3a6b4e0b-8e5c-df11-849b-0014c258f21e', " +
                    "'products': [" +
                        "{'id' : '2f355deb-423e-46aa-8d53-071b01018465'}, " +
                        "{'id' : '065983e6-092a-491b-99b0-be3de3fe74c9', 'name' : 'wizzy bang'}" +
                    "]" +
                "}";
            _response = new ResponseContext(HttpStatusCode.OK, "application/json", "", -1, responseContent, new Dictionary<string, string>());
        }

        [Test]
        public void RootIdShouldBeValid()
        {
            _response
                .Test("root id exist", x => x.id.ToString() == "3a6b4e0b-8e5c-df11-849b-0014c258f21e")
                .Assert("root id exist");
        }

        [Test]
        public void ProductCountShouldBeTwo()
        {
            _response
                .Test("there is two products", x => x.products.Count == 2)
                .Assert("there is two products");
        }

        [Test]
        public void SecondProductShouldHaveNameWizzyBang()
        {
            _response
                .Test("second product has a name", x => x.products[1].name == "wizzy bang")
                .Assert("second product has a name");
        }

        [Test]
        [ExpectedException(typeof(AssertException))]
        public void DoesThisBlowUp()
        {
            _response
                .Test("should blow up", x => x.products[0].name == "")
                .Assert("should blow up");
        }

        [Test]
        public void WriteAssertions()
        {
            _response.WriteAssertions();
        }
    }
}
