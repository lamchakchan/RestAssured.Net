using NUnit.Framework;

namespace RA.Tests
{
    [TestFixture]
    public class JsonIpIntegration
    {
        [Test]
        public void ResponseShouldShow()
        {
            new RestAssured()
                .Given()
                .Name("JsonIP")
                .When()
                .Get("http://www.telize.com/jsonip")
                .Then()
                .Test("ip exist", x => x.ip != null)
                .Assert("ip exist");
        }
    }
}