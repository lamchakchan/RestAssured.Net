namespace RA
{
    public class RestAssured
    {
        public SetupContext Given()
        {
            return new SetupContext();
        }

        /// <summary>
        /// Adds Parser to allow unsupporter response types to be parsed
        /// </summary>
        /// <param name="type">Content-Type to use provided function to parse</param>
        /// <param name="func">Function to parse type</param>
        internal static void AddParser(string type, System.Func<System.String, dynamic> func)
            => ResponseContext.AddParser(type, func);
    }
}
