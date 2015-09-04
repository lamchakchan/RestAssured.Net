using Headspring;

namespace RA.Enums
{
    public class HeaderType : Enumeration<HeaderType, string>
    {
        public static HeaderType ContentType = new HeaderType("Content-Type", "Content Type");
        public static HeaderType Accept = new HeaderType("Accept", "Accept");
        public static HeaderType AcceptEncoding = new HeaderType("Accept-Encoding", "Accept Encoding");
        public static HeaderType AcceptCharset = new HeaderType("Accept-Charset", "Accept Charset");
        public static HeaderType Authorization = new HeaderType("Authorization", "Authorization");

        public HeaderType(string value, string displayName) : base(value, displayName)
        {
        }
    }
}
