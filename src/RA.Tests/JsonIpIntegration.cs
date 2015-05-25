using NUnit.Framework;
using RestAssured.Tests.Data;

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
    public class KnotIntegration
    {
        [Test]
        public void TestPost()
        {
            new RestAssured()
                .Given()
                    .Header("Content-Type", "application/json")
                    .Header("Accept-Encoding", "gzip, deflate")
                    .Host("http://qa.services.theknot.com")
                    .Uri("/local-partners/marketplace/v1/storefronts")
                    .Query("apikey", "ca7f6e91ee8134de9717707d86b29100")
                    .Body("{ 'Id': [323920, '3a6b4e0b-8e5c-df11-849b-0014c258f21e'] }")
                .When()
                    .Load(5, 10)
                    .Post()
                .Then()
                    .Debug();
        }

        [Test]
        public void TestEmptyHealth()
        {
            new RestAssured()
                .Given()
                    .Header("Content-Type", "application/json")
                    .Header("Accept-Encoding", "gzip, deflate")
                    .Host("http://qa.services.theknot.com")
                    .Uri("/local-partners/selfservice/healthy")
                    //.Uri("/local-partners/marketplace/health/connected")
                    .Query("apikey", "ca7f6e91ee8134de9717707d86b29100")
                .When()
                    .Get()
                .Then()
                    .Debug();            
        }

        [Test]
        public void TestConversation()
        {
            new RestAssured()
                .Given()
                    .Header("Content-Type", "application/json")
                    .Host("http://qa.services.theknot.com")
                    .Uri("/local-partners/conversations/bride")
                    .Query("apikey", "ca7f6e91ee8134de9717707d86b29100")
                    .Query("memberId", "5491024343143956")
                    .Query("d", "1")
                .When()
                    .Get()
                .Then()
                    .Schema(Resource.ConversationSchema)
                    .Debug()
                    .AssertSchema();
        }
    }
}