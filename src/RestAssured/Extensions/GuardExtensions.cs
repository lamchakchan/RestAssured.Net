using System;

namespace RestAssured.Extensions
{
    public static class GuardExtensions
    {
        public static void Null(this string source, string message = "must have a value")
        {
            if (string.IsNullOrEmpty(source))
                throw new Exception(message);
        }

        public static string FixProtocol(this string source)
        {
            if (!source.StartsWith("http://") && !source.StartsWith("https://"))
                return "http://" + source;

            return source;
        }
    }
}