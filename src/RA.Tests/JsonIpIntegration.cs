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
                    .Debug()
                    .TestBody("ip exist", x => x.ip != null)
                    .Assert("ip exist");
        }
    }

    [TestFixture]
    public class KnotPostIntegration
    {
        [Test]
        public void TestPost()
        {
            new RestAssured()
                .Given()
                    .Header("Content-Type", "application/json")
                    .Header("Accept-Encoding", "gzip, deflate")
                    .Host("http://qa.services.theknot.com")
                    .Uri("/local-partners-search/v1/storefronts")
                    .Query("apikey", "ca7f6e91ee8134de9717707d86b29100")
                    .Body("{ 'Id': [323920, '3a6b4e0b-8e5c-df11-849b-0014c258f21e'] }")
                .When()
                    .Load(5, 10)
                    .Post()
                .Then()
                    .Debug();
        }
    }
}