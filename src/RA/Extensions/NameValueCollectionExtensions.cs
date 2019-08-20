using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace RA.Extensions
{
    public static class NameValueCollectionExtensions
    {
        public static string ToQueryString(this NameValueCollection nvc)
        {
            if (nvc == null || nvc.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            string[] keys = nvc.AllKeys;
            for (int i = 0; i < nvc.Count; i++)
            {
                string[] values = nvc.GetValues(keys[i]);
                if (values != null)
                {
                    foreach (string value in values)
                    {
                        if (string.IsNullOrEmpty(keys[i]))
                        {
                            sb.AppendFormat("{0}&", HttpUtility.UrlEncode(value));
                        }
                        else
                        {
                            sb.AppendFormat("{0}={1}&", keys[i], HttpUtility.UrlEncode(value));
                        }
                    }
                }
            }

            // trim trailing `&`
            return sb.ToString(0, sb.Length - 1);
        }
    }
}
