using System;

namespace RA.Extensions
{
    public static class GuardExtensions
    {
        public static void Null(this string source, string message = "must have a value")
        {
            if (string.IsNullOrEmpty(source))
                throw new Exception(message);
        }

        public static string FixProtocol(this string source, bool useHttps)
        {
            var defaultPortocol = useHttps ? "https" : "http";
            if (!source.StartsWith("http://") && !source.StartsWith("https://"))
                return $"{defaultPortocol}://" + source;

            return source;
        }
    }
}