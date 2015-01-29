using System;

namespace RA.Extensions
{
    public static class UriExtension
    {
        public static string SchemeAndHost(this Uri source)
        {
            return source.Scheme + "://" + source.Host;
        }
    }
}