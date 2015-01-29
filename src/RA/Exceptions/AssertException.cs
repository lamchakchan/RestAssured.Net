using System;

namespace RA.Exceptions
{
    public class AssertException : Exception
    {
        public AssertException(string message = null) : base(message)
        { }
    }
}
