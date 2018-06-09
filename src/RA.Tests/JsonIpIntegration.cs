using NUnit.Framework;
// using RA.Tests.Data;

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
                    .Get("http://geoip.nekudo.com/api/")
                .Then()
                    .Debug()
                    .TestBody("ip exist", x => x.ip != null)
                    .Assert("ip exist");
        }
    }

    [TestFixture]
    public class PublicIntegration
    {
        [Test]
        public void TestLoad()
        {
            new RestAssured()
                .Given()
                    .Header("Content-Type", "application/json")
                    .Header("Accept-Encoding", "gzip, deflate")
                    .Host("https://jsonplaceholder.typicode.com")
                    .Uri("/posts")
                .When()
                    .Load(5, 10)
                    .Get()
                .Then()
                    .Debug();
        }
        
        [Test]
        public void TestPost()
        {
            new RestAssured()
                .Given()
                    .Header("Content-Type", "application/json")
                    .Header("Accept-Encoding", "gzip, deflate")
                    .Host("https://jsonplaceholder.typicode.com")
                    .Uri("/posts")
                    .Body(@"{""id"":1000}")
                .When()
                    .Post()
                .Then()
                    .Debug();
        }

        [Test]
        public void TestHealthy()
        {
            new RestAssured()
                .Given()
                    .Header("Content-Type", "application/json")
                    .Header("Accept-Encoding", "gzip, deflate")
                    .Host("https://catalog.data.gov/api/3")
                    .Uri("/api/3")
                    //.Uri("/local-partners/marketplace/health/connected")
                .When()
                    .Get()
                .Then()
                    .Debug();            
        }

        [Test]
        public void TestSchemaValidation()
        {
            new RestAssured()
                .Given()
                    .Header("Content-Type", "application/json")
                    .Header("Accept-Encoding", "gzip, deflate")
                    .Host("https://catalog.data.gov/api/3")
                    .Uri("/api/3")
                    //.Uri("/local-partners/marketplace/health/connected")
                .When()
                    .Get()
                .Then()
                    .Schema(@"{""$id"":""http://example.com/example.json"",""type"":""object"",""definitions"":{},""$schema"":""http://json-schema.org/draft-07/schema#"",""properties"":{""version"":{""$id"":""/properties/version"",""type"":""integer"",""title"":""TheVersionSchema"",""default"":0,""examples"":[3]}}}")
                    .Debug()
                    .AssertSchema();
        }
    }
}