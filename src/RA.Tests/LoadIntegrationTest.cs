using NUnit.Framework;

namespace RA.Tests
{
    [TestFixture]
    public class LoadIntegrationTest
    {
        [Test]
        public void SingleThread()
        {
            new RestAssured()
                .Given()
                    .Name("JsonIP single thread")
                .When()
                //one thread for 15 seconds
                    .Load(1, 10)
                    .Get("http://geoip.nekudo.com/api/")
                .Then()
                    .Debug();
        }

        [Test]
        public void MultiThread()
        {
            new RestAssured()
                .Given()
                    .Name("JsonIP single thread")
                .When()
                //one thread for 15 seconds
                    .Load(6, 10)
                    .Get("http://geoip.nekudo.com/api/")
                .Then()
                    .Debug();
        }
    }
}