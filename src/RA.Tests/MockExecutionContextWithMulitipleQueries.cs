using System;
using System.IO;
using NUnit.Framework;

namespace RA.Tests
{
    [TestFixture]
    public class MockExecutionContextWithMulitipleQueries
    {
        private ExecutionContext _execution;
        private string _key;
        private string _val;
        private string _val2;

        public MockExecutionContextWithMulitipleQueries()
        {
            _key = "foo";
            _val = "bar";
            _val2 = "baz";

            var _setup = new SetupContext()
                .Query(_key, _val)
                .Query(_key, _val2);
            _execution = new HttpActionContext(_setup)
                .Get("http://test.com");

        }

        [Test]
        public void UrlContainsAllQuiries()
        {
            var stout = Console.Out;
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                _execution.Debug();
                Console.SetOut(stout);

                StringAssert.Contains(string.Format("{0}={1}", _key, _val), sw.ToString());
                StringAssert.Contains(string.Format("{0}={1}", _key, _val2), sw.ToString());
            }
        }
    }
}