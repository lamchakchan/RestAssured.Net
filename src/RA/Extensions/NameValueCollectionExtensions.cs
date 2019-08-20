using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace RA.Extensions
{
    public static class NameValueCollectionExtensions
    {
        public static string ToQueryString(this NameValueCollection nvc)
        {
            int count = nvc.Count;
            if (count == 0)
                return "";
            StringBuilder sb = new StringBuilder();
            string[] keys = nvc.AllKeys;

            var items = nvc.AllKeys.SelectMany(nvc.GetValues, (k, v) => new { key = k, value = v });
            foreach (var item in items)
                sb.AppendFormat("{0}={1}&", item.key, HttpUtility.UrlEncode(item.value, Encoding.UTF8));

            if (sb.Length > 0)
                sb.Length--;
            return sb.ToString();
        }


    }
}
