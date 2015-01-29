using NUnit.Framework;

namespace RA.Tests
{
    [TestFixture]
    public class StorefrontSuite
    {
        private ResponseContext _response;

        public StorefrontSuite()
        {
            _response = new RestAssured()
                .Given()
                .Name("storefront GET by GUID")
                .Header("Content-Type", "application/json")
                .Header("Accept-Encoding", "gzip,deflate")
                .When()
                .Get(
                    "http://qa.services.theknot.com/local-partners-search/v1/storefronts/E90A470B-8E5C-DF11-849B-0014C258F21E?apikey=ca7f6e91ee8134de9717707d86b29100")
                .Then();
        }

        [Test]
        public void CheckId()
        {
            _response
                .Test("id match", x => x.id == "E90A470B-8E5C-DF11-849B-0014C258F21E".ToLower())
                .Assert("id match");
        }

        [Test]
        public void CheckName()
        {
            _response
                .Test("has name", x => !string.IsNullOrEmpty(x.name.ToString()))
                .Assert("has name");
        }

        [Test]
        public void WriteAssertions()
        {
            _response.WriteAssertions();
        }
    }
}