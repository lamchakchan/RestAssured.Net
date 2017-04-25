using System.Net.Http;
using NUnit.Framework;

namespace RA.Tests
{
    [TestFixture]
    public class InvalidCallTest
    {
        [Test]
        public void BadAddressShouldFail()
        {
            Assert.Throws<HttpRequestException>(() =>
            {
                new RestAssured()
                    .Given()
                    .Name("Bad Call")
                    .When()
                    .Get("http://www.fake-2-address.com")
                    .Then()
                    .Debug();
            });
        }
    }
}