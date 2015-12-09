using System;
using System.Diagnostics;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Globalization;

namespace DynamicTemplate.Compiler
{
    class CSharpLanguageProvider : LanguageProvider
    {
        private PositionBuffer _value;
        private PositionTransposer _sourcePositionTransposer = new PositionTransposer();

        public override string Name
        {
            get { return "C#"; }
        }

        public override bool IsValidIdentifier(string identifier, out string errorMessage)
        {
            if (identifier.Length == 0)
            {
                errorMessage = "Variable names must be one or more characters long";
                return false;
            }

            if (identifier[0] == '@')
            {
                string subIdentifier = identifier.Substring(1);
                if (subIdentifier.Length == 0)
                {
                    errorMessage = "Variable name is too short";
                    return false;
                }

                if (!IsValidIdentifierInternal(subIdentifier, out errorMessage))
                    return false;
            }
            else
            {
                if (!IsValidIdentifierInternal(identifier, out errorMessage))
                    return false;
            }

            switch (identifier)
            {
                case "abstract":
                #region C# keywords
                case "as":
                case "base":
                case "bool":
                case "break":
                case "byte":
                case ": case":
                case "catch":
                case "char":
                case "checked":
                case "class":
                case "const":
                case "continue":
                case "decimal":
                case "default":
                case "delegate":
                case "do":
                case "double":
                case "else":
                case "enum":
                case "event":
                case "explicit":
                case "extern":
                case "false":
                case "finally":
                case "fixed":
                case "float":
                case "for":
                case "foreach":
                case "goto":
                case "if":
                case "implicit":
                case "in":
                case "int":
                case "interface":
                case "internal":
                case "is":
                case "lock":
                case "long":
                case "namespace":
                case "new":
                case "null":
                case "object":
                case "operator":
                case "out":
                case "override":
                case "params":
                case "private":
                case "protected":
                case "public":
                case "readonly":
                case "ref":
                case "return":
                case "sbyte":
                case "sealed":
                case "short":
                case "sizeof":
                case "stackalloc":
                case "static":
                case "string":
                case "struct":
                case "switch":
                case "this":
                case "throw":
                case "true":
                case "try":
                case "typeof":
                case "uint":
                case "ulong":
                case "unchecked":
                case "unsafe":
                case "ushort":
                case "using":
                case "virtual":
                case "void":
                case "volatile":
                #endregion
                case "while":
                    errorMessage = "\"" + identifier + "\" is a keyword and cannot be used as a variable name";
                    return false;
            }

            return true;
        }

        private static bool IsValidIdentifierInternal(string identifier, out string errorMessage)
        {
            if (!char.IsLetter(identifier[0]) && identifier[0] != '_')
            {
                errorMessage = "Variable names must start with either a letter or the underscore character";
                return false;
            }

            for (int i = 1; i < identifier.Length; i++)
            {
                switch (char.GetUnicodeCategory(identifier[i]))
                {
                    // letter-character
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.LetterNumber:
                    // combining-character
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    // decimal-digit-character
                    case UnicodeCategory.DecimalDigitNumber:
                    // connecting-character
                    case UnicodeCategory.ConnectorPunctuation:
                    // format-character
                    case UnicodeCategory.Format:
                        break;
                    case UnicodeCategory.SpaceSeparator:
                        errorMessage = "Variable names must not contain spaces";
                        return false;
                    default:
                        errorMessage = "Variable name contains illegal character(s)";
                        return false;
                }
            }

            errorMessage = null;
            return true;
        }

        public override string NormalizeIdentifier(string identifier)
        {
            return identifier[0] == '@' ? identifier.Substring(1) : identifier;
        }

        public override void Start(ArgumentDescription[] args)
        {
            _value = new PositionBuffer();

            _value.Append(
@"using System;
using System.Text;
using System.Web;
using BlogServer;
using BlogServer.Model;

public class Template
{
    private static string HtmlEncode(string val)
    {
        return System.Web.HttpUtility.HtmlEncode(val);
    }

    private static string HtmlAttributeEncode(string val)
    {
        return System.Web.HttpUtility.HtmlAttributeEncode(val);
    }

    private static string HtmlDecode(string val)
    {
        return System.Web.HttpUtility.HtmlDecode(val);
    }

    private static string UrlEncode(string val)
    {
        return System.Web.HttpUtility.UrlEncode(val);
    }

    private static string UrlPathEncode(string val)
    {
        return System.Web.HttpUtility.UrlPathEncode(val);
    }

    private static string UrlDecode(string val)
    {
        return System.Web.HttpUtility.UrlDecode(val);
    }

    public static string Process(object[] parameters)
    {
        StringBuilder ___output = new StringBuilder();
");
            for (int ___i = 0; ___i < args.Length; ___i++)
            {
                ArgumentDescription arg = args[___i];

                string argType = arg.Type.FullName;

                _value.AppendFormat("{0} {1} = ({0})parameters[{2}];", argType, arg.Identifier, ___i);
            }
        }

        public override void Code(string code, Position startPos)
        {
            _sourcePositionTransposer.AddMapping(
                _value.Position,
                startPos);
            _value.Append(code);
            /*
            _sourcePositionTransposer.AddMapping(
                new Position(_value.Line, _value.Column),
                Position.Unknown);
             */
        }

        public override void Expression(string expr, Position startPos)
        {
            _value.Append("___output.Append(");

            _sourcePositionTransposer.AddMapping(
                _value.Position,
                startPos);
            _value.Append(expr);
            /*
            _sourcePositionTransposer.AddMapping(
                new Position(_value.Line, _value.Column + 1),
                Position.Unknown);
             */

            _value.Append(");");
        }

        public override void Literal(string literal, Position startPos)
        {
            _value.Append("___output.Append(@\"");
            foreach (char c in literal)
            {
                switch (c)
                {
                    case '"':
                        _value.Append("\"\"");
                        break;
                    default:
                        _value.Append(c.ToString());
                        break;
                }
            }
            _value.Append("\");");
        }

        public override TemplateOperation End()
        {
            _value.Append(@"
        return ___output.ToString();
    }
}");
            CompilerParameters p = new CompilerParameters();
            p.IncludeDebugInformation = true;
            p.GenerateInMemory = true;
            p.ReferencedAssemblies.Add("System.Web.dll");
        	p.ReferencedAssemblies.Add(Process.GetCurrentProcess().MainModule.FileName);
            CompilerResults results = new CSharpCodeProvider().CreateCompiler().CompileAssemblyFromSource(p, _value.ToString());
            if (results.NativeCompilerReturnValue != 0)
            {
                throw TemplateCompilationException.CreateFromCompilerResults(results, _sourcePositionTransposer);
            }
            Type t = results.CompiledAssembly.GetType("Template");
            MethodInfo m = t.GetMethod("Process", BindingFlags.Static | BindingFlags.Public);
            return new TemplateOperation(new InvokeAdapter(m).Invoke);
        }

        private class InvokeAdapter
        {
            private readonly MethodInfo _method;

            public InvokeAdapter(MethodInfo method)
            {
                _method = method;
            }

            public string Invoke(object[] values)
            {
                return (string)_method.Invoke(null, new object[] { values });
            }
        }
    }
}
