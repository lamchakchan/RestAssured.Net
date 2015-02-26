using System;
using System.Net.Http;

namespace RA.Net
{
    public class HttpResponseMessageWrapper
    {
        public HttpResponseMessage Response { get; set; }
        public TimeSpan ElaspedExecution { get; set; }
    }
}
