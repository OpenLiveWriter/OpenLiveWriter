using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BlogRunner.Core
{
    public class Log
    {
        public delegate void Action();

        [ThreadStatic] private static int indentLevel;

        public static void WriteLine(string message)
        {
            if (indentLevel > 0)
                message = Indent + message.Replace("\n", Indent);
            Console.WriteLine(message);
        }

        public static void Section(string name, Action action)
        {
            WriteLine("/== " + name + " ====");
            indentLevel++;
            try
            {
                action();
            }
            finally
            {
                indentLevel--;
            }
            WriteLine(@"\== " + name + " ====");
        }

        private static string Indent
        {
            get
            {
                return new string(' ', indentLevel * 2);
            }
        }
    }
}
