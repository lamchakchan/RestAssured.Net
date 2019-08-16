using Headspring;

namespace RA.Enums
{
    public class LoadValueTypes : Enumeration<LoadValueTypes, string>
    {
        public static LoadValueTypes TotalCall = new LoadValueTypes("Total-Call", "Total Call");
        public static LoadValueTypes TotalSucceeded = new LoadValueTypes("Total-Succeeded", "Total Succeeded");
        public static LoadValueTypes TotalLost = new LoadValueTypes("Total-Lost", "Total Lost");
        public static LoadValueTypes SuccessPercent = new LoadValueTypes("Success-Percent", "Success Percent");
        public static LoadValueTypes AverageTTLMs = new LoadValueTypes("Average-TTL-Ms", "Average TTL Ms");
        public static LoadValueTypes MaximumTTLMs = new LoadValueTypes("Maximum-TTL-Ms", "Maximum TTL Ms");
        public static LoadValueTypes MinimumTTLMs = new LoadValueTypes("Minimum-TTL-Ms", "Minimum TTL Ms");

        public LoadValueTypes(string value, string displayName) : base(value, displayName)
        {
        }
    }
}