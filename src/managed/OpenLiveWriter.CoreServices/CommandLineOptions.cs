// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace OpenLiveWriter.CoreServices
{
    public class CommandLineOptions
    {
        private static int errorCode = 100;
        private static readonly ErrorType ERROR_UNPARSEABLE_ARG = new ErrorType(errorCode++, "The argument \"{0}\" was not understood.");
        private static readonly ErrorType ERROR_UNRECOGNIZED_ARG = new ErrorType(errorCode++, "The argument \"{0}\" was not understood.");
        private static readonly ErrorType ERROR_ARG_REQUIRED = new ErrorType(errorCode++, "The /{1} argument is required.");
        private static readonly ErrorType ERROR_VALUE_REQUIRED = new ErrorType(errorCode++, "The /{1} argument needs a value.");
        private static readonly ErrorType ERROR_FLAG_HAD_VALUE = new ErrorType(errorCode++, "The /{1} argument does not take a value.");
        private static readonly ErrorType ERROR_NOT_AN_INT = new ErrorType(errorCode++, "The argument value \"{2}\" was not a valid integer.");
        private static readonly ErrorType ERROR_NOT_HEX_INT = new ErrorType(errorCode++, "The argument value \"{2}\" was not a valid hex value.");
        private static readonly ErrorType ERROR_TOO_FEW_UNNAMED_ARGS = new ErrorType(errorCode++, "Not enough arguments were provided.");
        private static readonly ErrorType ERROR_TOO_MANY_UNNAMED_ARGS = new ErrorType(errorCode++, "The argument \"{0}\" was unexpected--too many arguments.");

        private readonly bool _caseSensitive;
        private readonly int _minUnnamedArgCount;
        private readonly int _maxUnnamedArgCount;
        private ArrayList _required = new ArrayList();
        private Hashtable _args = new Hashtable();

        private ArrayList _unnamedArgs = new ArrayList();
        private Hashtable _values = new Hashtable();
        private HashSet _argsPresent = new HashSet();
        private ArrayList _errors = new ArrayList();

        public CommandLineOptions(params ArgSpec[] args) : this(false, 0, int.MaxValue, args)
        {
        }

        public CommandLineOptions(bool caseSensitive, int minUnnamedArgCount, int maxUnnamedArgCount, params ArgSpec[] args)
        {
            _caseSensitive = caseSensitive;
            _maxUnnamedArgCount = maxUnnamedArgCount;
            _minUnnamedArgCount = minUnnamedArgCount;
            foreach (ArgSpec arg in args)
            {
                string name = arg.Name;
                NormalizeName(ref name);
                ValidateArgName(name);
                if (arg.IsRequired)
                    _required.Add(name);
                _args.Add(name, arg);
            }
        }

        public bool IsArgPresent(string name)
        {
            NormalizeName(ref name);
            return _values.ContainsKey(name);
        }

        public bool GetFlagValue(string name, bool defaultValue)
        {
            return (bool)GetValue(name, defaultValue);
        }

        public long GetIntegerValue(string name, int defaultValue)
        {
            return (long)GetValue(name, defaultValue);
        }

        public object[] GetValues(string name)
        {
            NormalizeName(ref name);
            ArgSpec argSpec = (ArgSpec)_args[name];
            Type t = argSpec.IsInteger ? typeof(long) :
                argSpec.IsHexNumber ? typeof(long) :
                typeof(string);
            return (object[])((ArrayList)GetValue(name, new ArrayList())).ToArray(t);
        }

        public object GetValue(string name, object defaultValue)
        {
            NormalizeName(ref name);
            if (!_values.ContainsKey(name))
                return defaultValue;
            else
                return _values[name];
        }

        public string[] UnnamedArgs
        {
            get
            {
                return (string[])_unnamedArgs.ToArray(typeof(string));
            }
        }

        public int UnnamedArgCount
        {
            get
            {
                return _unnamedArgs.Count;
            }
        }

        public string GetUnnamedArg(int index, string defaultValue)
        {
            if (index < _unnamedArgs.Count)
                return (string)_unnamedArgs[index];
            else
                return defaultValue;
        }

        /// <summary>
        /// Parse the arguments, returning true if everything was kosher
        /// and false if not.
        /// </summary>
        /// <param name="args">Command line arguments, as passed to Main(string[] args) or Environment.GetCommandLineArgs()</param>
        /// <param name="stopOnFirstError">Return false at the first error encountered.</param>
        /// <returns>True if arguments were valid and all required arguments were provided.</returns>
        public bool Parse(string[] args, bool stopOnFirstError)
        {
            _values.Clear();
            _errors.Clear();
            _argsPresent.Clear();
            _unnamedArgs.Clear();

            bool success = true;
            for (int i = 0; i < args.Length && (!stopOnFirstError || success); i++)
                success &= ParseArg(args[i]);
            if (stopOnFirstError && !success)
                return false;

            if (_minUnnamedArgCount > _unnamedArgs.Count)
                success &= AddError(ERROR_TOO_FEW_UNNAMED_ARGS, null, null, null);
            if (stopOnFirstError && !success)
                return false;

            for (int i = 0; i < _required.Count && (!stopOnFirstError || success); i++)
            {
                string requiredArg = (string)_required[i];
                if (!_argsPresent.Contains(requiredArg))
                    success &= AddError(ERROR_ARG_REQUIRED, null, requiredArg, null);
            }
            if (stopOnFirstError && !success)
                return false;

            return success;
        }

        /// <summary>
        /// The errors that were found during the parse.
        /// </summary>
        public Error[] Errors
        {
            get
            {
                return (Error[])_errors.ToArray(typeof(Error));
            }
        }

        /// <summary>
        /// List of errors that were found during the parse.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                Error[] errs = Errors;
                if (errs.Length == 0)
                    return null;
                return StringHelper.Join(errs, "\r\n");
            }
        }

        /// <summary>
        /// Parse an individual argument; returns false if error.
        /// </summary>
        private bool ParseArg(string arg)
        {
            arg = arg.Trim();
            if (arg.Length == 0)
                return true;

            if (!arg.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                if (_unnamedArgs.Count >= _maxUnnamedArgCount)
                    return AddError(ERROR_TOO_MANY_UNNAMED_ARGS, arg, null, null);
                _unnamedArgs.Add(arg);
                return true;
            }
            else
            {
                return ParseNamedArg(arg);
            }
        }

        private bool ParseNamedArg(string arg)
        {
            Match m = Regex.Match(arg, @"^/(-)?([a-z0-9]+)(?:\:(.+))?$");
            if (!m.Success)
                return AddError(ERROR_UNPARSEABLE_ARG, arg, null, null);
            bool unset = m.Groups[1].Success;
            string argName = m.Groups[2].Value;
            string argValue = (m.Groups[3].Success) ? m.Groups[3].Value : null;

            NormalizeName(ref argName);

            _argsPresent.Add(argName);

            ArgSpec argSpec = (ArgSpec)_args[argName];
            ErrorType err =
                (argSpec == null) ? ERROR_UNRECOGNIZED_ARG :
                (argSpec.IsFlag && argValue != null) ? ERROR_FLAG_HAD_VALUE :
                (argSpec.IsRequired && argValue == null) ? ERROR_VALUE_REQUIRED :
                (!argSpec.IsUnsettable && unset) ? ERROR_UNRECOGNIZED_ARG :
                null;
            if (err != null)
                return AddError(err, arg, argName, argValue);

            object parsedValue = null;
            if (argSpec.IsFlag)
            {
                parsedValue = !unset;
            }
            else if (argSpec.IsInteger)
            {
                try { parsedValue = long.Parse(argValue, CultureInfo.InvariantCulture); }
                catch { err = ERROR_NOT_AN_INT; }
            }
            else if (argSpec.IsHexNumber)
            {
                try { parsedValue = long.Parse(argValue, NumberStyles.AllowHexSpecifier | NumberStyles.HexNumber, CultureInfo.InvariantCulture); }
                catch { err = ERROR_NOT_HEX_INT; }
            }
            else
            {
                parsedValue = argValue;
            }
            if (err != null)
                return AddError(err, arg, argName, argValue);

            if (!argSpec.IsMultiple)
            {
                _values[argName] = parsedValue;
            }
            else
            {
                ArrayList values = (ArrayList)_values[argName];
                if (values == null)
                {
                    values = new ArrayList();
                    _values[argName] = values;
                }
                values.Add(parsedValue);
            }
            return true;
        }

        private bool AddError(ErrorType error, string arg, string argName, string argValue)
        {
            _errors.Add(new Error(error, arg, argName, argValue));
            return false;
        }

        private void NormalizeName(ref string name)
        {
            if (!_caseSensitive)
                name = name.ToLower(CultureInfo.InvariantCulture);
        }

        private void ValidateArgName(string name)
        {
            if (!Regex.IsMatch(name, "^[a-z0-9]+$", RegexOptions.IgnoreCase))
                throw new ArgumentException("The argument name \"" + name + "\" is invalid; only letters and numbers may be used.");
        }

        public class ErrorType
        {
            private int _errorCode;
            private string _messageFormat;

            public ErrorType(int errorCode, string message)
            {
                _errorCode = errorCode;
                _messageFormat = message;
            }

            public int ErrorCode
            {
                get { return _errorCode; }
            }

            public string MessageFormat
            {
                get { return _messageFormat; }
            }

            public override bool Equals(object obj)
            {
                ErrorType other = obj as ErrorType;
                return (object)other != null && other._errorCode == _errorCode;
            }

            public override int GetHashCode()
            {
                return _errorCode;
            }

            public static bool operator ==(ErrorType e1, ErrorType e2)
            {
                if (null == (object)e1 ^ null == (object)e2)
                    return false;
                if ((object)e1 == null)
                    return true;
                return e1.Equals(e2);
            }

            public static bool operator !=(ErrorType e1, ErrorType e2)
            {
                return !(e1 == e2);
            }
        }

        public class Error
        {
            private readonly ErrorType _errorType;
            private readonly string _arg;
            private readonly string _argName;
            private readonly string _argValue;

            public Error(ErrorType errorType, string arg, string argName, string argValue)
            {
                _errorType = errorType;
                _arg = arg;
                _argName = argName;
                _argValue = argValue;
            }

            public ErrorType ErrorType
            {
                get { return _errorType; }
            }

            public string Message
            {
                get
                {
                    return string.Format(CultureInfo.CurrentCulture, _errorType.MessageFormat, _arg, _argName, _argValue);
                }
            }

            public override string ToString()
            {
                return Message;
            }

        }
    }

    /// <summary>
    /// Describes a named argument--its name, options, and description.
    ///
    /// Examples:
    ///
    /// /foo
    /// /bar:10
    /// /baz:C30BD034
    /// /blurdyboop:hello
    /// /-recursive
    /// </summary>
    public class ArgSpec
    {
        [Flags]
        public enum Options
        {
            Default = 0,  // optional, string value required, not unsettable, single (last-one-wins)
            Required = 1,  // argument is required
            Flag = 2,  // value is not allowed
            ValueOptional = 4,  // value is not required
            Unsettable = 8,  // "-" prefix is allowed (e.g. /-foo), meaning negation
            Integer = 0x10,  // value must be a decimal integer
            HexNumber = 0x20,  // value must be a hex value
            Multiple = 0x40  // may be used multiple times in one command line
        }

        private readonly string _name;
        private readonly Options _options;
        private readonly string _description;

        public ArgSpec(string name, Options options, string description)
        {
            _name = name;
            _options = options;
            _description = description;
            ValidateOptions();
        }

        [Conditional("DEBUG")]
        private void ValidateOptions()
        {
            if (IsFlag && (IsValueOptional || IsInteger || IsHexNumber || IsMultiple))
                throw new ArgumentException("Invalid combination of arg options: Flag cannot be combined with ValueOptional, Integer, HexNumber, or Multiple");
            if (!IsFlag && IsUnsettable)
                throw new ArgumentException("Invalid combination of arg options: Flag cannot be combined with Unsettable");
            if (IsInteger && IsHexNumber)
                throw new ArgumentException("Invalid combination of arg options: Integer cannot be combined with HexNumber");
        }

        public string Name
        {
            get { return _name; }
        }

        public string Description
        {
            get { return _description; }
        }

        public bool IsRequired { get { return Test(Options.Required); } }
        public bool IsFlag { get { return Test(Options.Flag); } }
        public bool IsValueOptional { get { return Test(Options.ValueOptional); } }
        public bool IsUnsettable { get { return Test(Options.Unsettable); } }
        public bool IsInteger { get { return Test(Options.Integer); } }
        public bool IsHexNumber { get { return Test(Options.HexNumber); } }
        public bool IsMultiple { get { return Test(Options.Multiple); } }

        private bool Test(Options options)
        {
            return (_options & options) == options;
        }
    }
}
