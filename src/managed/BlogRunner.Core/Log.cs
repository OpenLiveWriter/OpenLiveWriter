// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core
{
    using System;

    using JetBrains.Annotations;

    /// <summary>
    /// Class Log.
    /// </summary>
    public class Log
    {
        /// <summary>
        /// The indent level
        /// </summary>
        [ThreadStatic]
        private static int indentLevel;

        /// <summary>
        /// Delegate Action
        /// </summary>
        public delegate void Action();

        /// <summary>
        /// Gets the indent.
        /// </summary>
        /// <value>The indent.</value>
        private static string Indent => new string(' ', indentLevel * 2);

        /// <summary>
        /// Sections the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        public static void Section([CanBeNull] string name, [CanBeNull] Action action)
        {
            WriteLine($"/== {name} ====");
            indentLevel++;
            try
            {
                action?.Invoke();
            }
            finally
            {
                indentLevel--;
            }

            WriteLine($@"\== {name} ====");
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WriteLine([CanBeNull] string message)
        {
            if (indentLevel > 0)
            {
                message = Indent + message?.Replace(Environment.NewLine, Indent);
            }

            Console.WriteLine(message);
        }
    }
}
