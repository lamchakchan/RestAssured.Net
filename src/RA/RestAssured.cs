using System;

namespace RA
{
    public class RestAssured
    {
        /// <summary>
        /// Adds Parser to allow unsupported response types to be parsed
        /// </summary>
        /// <param name="type">Content-Type to use provided function to parse</param>
        /// <param name="func">Function to parse from body string to dynamic object</param>
        public static void AddParser(string type, Func<String, dynamic> func)
            => ResponseContext.AddParser(type, func);


        public SetupContext Given()
        {
            return new SetupContext();
        }
    }
}
