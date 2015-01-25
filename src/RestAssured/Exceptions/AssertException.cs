using System;

namespace RestAssured.Exceptions
{
    public class AssertException : Exception
    {
        public AssertException(string message = null) : base(message)
        { }
    }
}
