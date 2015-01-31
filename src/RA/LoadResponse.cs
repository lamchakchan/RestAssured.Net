namespace RA
{
    public class LoadResponse
    {
        public LoadResponse(int statusCode, long ticks)
        {
            Ticks = ticks;
            StatusCode = statusCode;
        }
        public long Ticks { get; set; }
        public int StatusCode { get; set; }
    }
}