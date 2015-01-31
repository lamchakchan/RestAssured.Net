using System;
using System.Collections.Generic;

namespace RA.Extensions
{
    public static class EnumerationExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action.Invoke(item);
            }
        } 
    }

    public static class ConsoleExtensions
    {
        public static void WriteHeader(this string source, params object[] objects)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(source, objects);
            Console.ResetColor();
        }

        public static void WriteLine(this string source, params object[] objects)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("- " + source, objects);
            Console.ResetColor();
        }

        public static void Write(this string source, params object[] objects)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(source, objects);
            Console.ResetColor();
        }

        public static void WriteTest(this KeyValuePair<string, bool> source)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("- {0} : ", source.Key);
            Console.ResetColor();

            if (source.Value)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Passed");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed");
            }

            Console.ResetColor();
        }

        public static void WritePassedTest()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("- Passed");
            Console.ResetColor();
        }

        public static void WriteFailedTest()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("- Failed");
            Console.ResetColor();
        }
    }
}