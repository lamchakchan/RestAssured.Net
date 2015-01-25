using System;

namespace RestAssured.Extensions
{
    public static class UriExtension
    {
        public static string SchemeAndHost(this Uri source)
        {
            return source.Scheme + "://" + source.Host;
        }
    }
}