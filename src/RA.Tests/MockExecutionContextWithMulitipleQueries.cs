using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using NUnit.Framework;

namespace RA.Tests
{
    [TestFixture]
    public class MockExecutionContextWithMulitipleQueries
    {
        private SetupContext _setupContext;
        private ExecutionContext _executionContext;

        [SetUp]
        public void Setup()
        {
            _setupContext = new SetupContext();
            _executionContext = new HttpActionContext(_setupContext)
                .Get("http://test.com");
        }

        [Test]
        [TestCaseSource(nameof(GenerateTestData))]
        public void UrlContainsAllQueries(NameValueCollection queryStrings, string expectedQuery)
        {
            _setupContext.Queries(queryStrings);

            var stdOut = Console.Out;
            try
            {
                using (StringWriter sw = new StringWriter())
                {
                    Console.SetOut(sw);

                    _executionContext.Debug();
                    StringAssert.Contains(expectedQuery, sw.ToString());
                }
            }
            finally
            {
                Console.SetOut(stdOut);
            }
        }

        private static IEnumerable<TestCaseData> GenerateTestData()
        {
            yield return new TestCaseData(
                new NameValueCollection
                {
                    { "a", "x" },
                    { "b", "y" }
                },
                "a=x&b=y");

            yield return new TestCaseData(
                new NameValueCollection
                {
                    { "foo", "bar" },
                    { "foo", "baz" }
                },
                "foo=bar&foo=baz");

            yield return new TestCaseData(
                new NameValueCollection
                {
                    { null, "foo" },
                    { "bar", "baz" }
                },
                "foo&bar=baz");
        }
    }
}