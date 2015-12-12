// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// XPP uses a little language to express conditionals.
    ///
    /// Literals:
    /// strings - 'like this'
    /// regex - /like this/
    /// versions - like this: 12.0.518.4124
    ///
    /// Unary Operator:
    /// not - boolean NOT
    ///
    /// Binary Operators:
    /// is - equal
    /// eq - equal
    /// neq - not equal
    /// lt - less than
    /// lte - less than or equal to
    /// gt - greater than
    /// gte - greater than or equal to
    /// and - boolean AND
    /// or - boolean OR
    /// =~ - match the string on the left with the regex on the right
    /// !~ - inverse of =~.
    ///
    /// You can use parenthesis to group expressions.
    ///
    /// The caller can define variables by Add()ing them to the
    /// XmlPreprocessor instance.
    ///
    /// Keywords are case insensitive, but identifiers are not.
    /// Identifiers must begin with a letter and can contain
    /// letters, numbers, and underscore.
    ///
    /// Examples:
    ///
    /// 'hello' is 'hello' and 'foo' neq 'bar'
    ///
    /// </summary>
    public class XmlPreprocessor : RuntimeValues
    {
        private const string NS = "http://writer.live.com/xmlpp/2007";

        public void Munge(XmlDocument doc)
        {
            Munge(doc.DocumentElement);
        }

        private void Munge(XmlElement el)
        {
            XmlAttribute cond = GetConditional(el);
            if (cond != null)
            {
                if (!new XppParser(cond.Value).Evaluate(this))
                {
                    el.ParentNode.RemoveChild(el);
                    return;
                }
                else
                {
                    el.RemoveAttributeNode(cond);
                }
            }

            XmlNodeList nodes = el.ChildNodes;
            XmlNode[] children = new XmlNode[nodes.Count];
            for (int i = 0; i < children.Length; i++)
                children[i] = nodes[i];

            foreach (XmlNode child in children)
            {
                if (child is XmlElement)
                    Munge((XmlElement)child);
            }
        }

        private XmlAttribute GetConditional(XmlElement el)
        {
            return el.GetAttributeNode("if", NS);
        }

        public bool Test(string expression)
        {
            XppParser parser = new XppParser(expression);
            return parser.Evaluate(this);
        }
    }

    internal enum XppTokenType
    {
        EOF,
        Identifier, // [a-zA-Z][a-zA-Z0-9]*
        StringLiteral, // ".*?"
        VersionLiteral, // [0-9]+(\.[0-9]+(\.[0-9]+(\.[0-9]+)?)?)?
        RegexLiteral, // /.*?/
        Lparen,    // (
        Rparen,    // )
        NOT,
        AND,
        OR,
        LT,
        LTE,
        EQ,
        NEQ,
        GT,
        GTE,
        REGEX,     // =~
        NOTREGEX,  // !~
        TRUE,
        FALSE,

        Production = 99,
        ERROR
    }

    internal class XppParserSymbol
    {
        public readonly XppTokenType TokenType;
        public readonly string Value;
        public readonly XppAstNode Node;

        public XppParserSymbol(XppTokenType tokenType, string value, XppAstNode node)
        {
            TokenType = tokenType;
            Value = value;
            Node = node;
        }

        public bool IsExpr
        {
            get { return TokenType == XppTokenType.Production && Node is XppAstExpr; }
        }

        public object GetRuntimeValue(RuntimeValues runtimeEnv)
        {
            if (TokenType == XppTokenType.Identifier)
                return runtimeEnv.Get(Value);
            else if (TokenType == XppTokenType.StringLiteral)
                return Value;
            else if (TokenType == XppTokenType.VersionLiteral)
                return new Version(Value);
            else if (TokenType == XppTokenType.RegexLiteral)
                return new Regex(Value, XppRegexExpr.DEFAULT_REGEX_OPTIONS);
            else
                throw new ArgumentException("Program error: Cannot get value for something that is not an identifier, string, or version");
        }

    }

    internal class XppToken
    {
        public readonly XppTokenType TokenType;
        public readonly string Value;
        public readonly int Offset;
        public readonly int Length;

        public XppToken(XppTokenType tokenType, string value, int offset, int length)
        {
            TokenType = tokenType;
            Value = value;
            Offset = offset;
            Length = length;
        }
    }

    internal abstract class XppAstNode
    {
    }

    internal abstract class XppAstExpr : XppAstNode
    {
        public abstract bool Evaluate(RuntimeValues runtimeEnv);
    }

    internal class XppAstNotExpr : XppAstExpr
    {
        private readonly XppAstExpr expr;

        public XppAstNotExpr(XppAstExpr expr)
        {
            this.expr = expr;
        }

        public override bool Evaluate(RuntimeValues runtimeEnv)
        {
            return !expr.Evaluate(runtimeEnv);
        }
    }

    public class RuntimeValues
    {
        private readonly Hashtable values;

        public RuntimeValues()
        {
            values = new Hashtable();
        }

        public void Add(string identifier, object val)
        {
            Debug.Assert(Regex.IsMatch(identifier, "[a-z][a-z0-9_]*", RegexOptions.IgnoreCase));
            values.Add(identifier, val);
        }

        public object Get(string identifier)
        {
            if (!values.ContainsKey(identifier))
                throw new ArgumentException("Unknown value " + identifier);
            return values[identifier];
        }
    }

    internal class XppParser
    {
        private readonly XppLexer lexer;
        private readonly ArrayList nodeStack;

        public XppParser(string data)
        {
            lexer = new XppLexer(data);
            nodeStack = new ArrayList();
        }

        public bool Evaluate(RuntimeValues runtimeEnv)
        {
            return Parse().Evaluate(runtimeEnv);
        }

        public XppAstExpr Parse()
        {
            while (true)
            {
                XppToken token = lexer.NextToken();
                if (token.TokenType == XppTokenType.EOF)
                {
                    if (nodeStack.Count == 1)
                    {
                        XppAstExpr expr = Pop().Node as XppAstExpr;
                        if (expr != null)
                            return expr;
                    }
                    Trace.Fail("Parse error, reached EOF in an invalid state. Use a debugger to see what's left on the stack.");
                    throw new ArgumentException(
                        "Parse error, reached EOF in an invalid state. Use a debugger to see what's left on the stack.");
                }
                Push(new XppParserSymbol(token.TokenType, token.Value, null));
                EvaluateStack();
            }
        }

        private XppParserSymbol Push(XppParserSymbol o)
        {
            nodeStack.Add(o);
            return o;
        }

        private XppParserSymbol Pop()
        {
            XppParserSymbol o = Peek(0);
            nodeStack.RemoveAt(nodeStack.Count - 1);
            return o;
        }

        private XppParserSymbol Peek(int depth)
        {
            int index = nodeStack.Count - 1 - depth;
            if (index < 0)
                return new XppParserSymbol(XppTokenType.ERROR, null, null);
            return (XppParserSymbol)nodeStack[index];
        }

        private void EvaluateStack()
        {
            bool repeat;
            do
            {
                repeat = false;
                XppParserSymbol top = Peek(0);
                switch (top.TokenType)
                {
                    case XppTokenType.Production:
                        if (top.IsExpr)
                        {
                            XppTokenType tt1 = Peek(1).TokenType;
                            if (tt1 == XppTokenType.NOT)
                            {
                                XppParserSymbol expr = Pop();
                                Pop();
                                Push(new XppParserSymbol(XppTokenType.Production, null, new XppAstNotExpr((XppAstExpr)expr.Node)));
                                repeat = true;
                                continue;
                            }
                            else if (tt1 == XppTokenType.AND || tt1 == XppTokenType.OR)
                            {
                                if (Peek(2).IsExpr)
                                {
                                    XppParserSymbol right = Pop();
                                    Pop(); // operand
                                    XppParserSymbol left = Pop();
                                    Push(
                                        new XppParserSymbol(XppTokenType.Production, null,
                                                            new XppCompositeExpr(tt1 == XppTokenType.OR, (XppAstExpr)left.Node, (XppAstExpr)right.Node)));
                                }
                            }
                        }
                        break;
                    case XppTokenType.Rparen:
                        if (Peek(1).IsExpr)
                        {
                            if (Peek(2).TokenType == XppTokenType.Lparen)
                            {
                                Pop();
                                XppParserSymbol expr = Pop();
                                Pop();
                                Push(expr);
                                repeat = true;
                                continue;
                            }
                        }
                        throw new ArgumentException("Didn't expect a right parenthesis here");
                    case XppTokenType.Identifier:
                    case XppTokenType.StringLiteral:
                    case XppTokenType.VersionLiteral:
                        switch (Peek(1).TokenType)
                        {
                            case XppTokenType.LT:
                            case XppTokenType.LTE:
                            case XppTokenType.EQ:
                            case XppTokenType.NEQ:
                            case XppTokenType.GT:
                            case XppTokenType.GTE:
                                XppTokenType tt2 = Peek(2).TokenType;
                                if (tt2 == XppTokenType.StringLiteral || tt2 == XppTokenType.Identifier || tt2 == XppTokenType.VersionLiteral)
                                {
                                    XppParserSymbol right = Pop();
                                    XppParserSymbol op = Pop();
                                    XppParserSymbol left = Pop();
                                    Push(new XppParserSymbol(XppTokenType.Production, null, new XppCompareExpr(op.TokenType, left, right)));
                                    repeat = true;
                                    continue;
                                }
                                break;
                        }

                        if (top.TokenType == XppTokenType.Identifier && (Peek(1).TokenType == XppTokenType.REGEX || Peek(1).TokenType == XppTokenType.NOTREGEX))
                        {
                            XppTokenType tt2 = Peek(2).TokenType;
                            if (tt2 == XppTokenType.StringLiteral || tt2 == XppTokenType.Identifier)
                            {
                                bool not = Peek(1).TokenType == XppTokenType.NOTREGEX;

                                XppParserSymbol pattern = Pop();
                                Pop();
                                XppParserSymbol val = Pop();
                                Push(new XppParserSymbol(XppTokenType.Production, null, new XppRegexExpr(val, pattern, not)));
                            }
                        }
                        break;
                    case XppTokenType.RegexLiteral:
                        if (Peek(1).TokenType == XppTokenType.REGEX || Peek(1).TokenType == XppTokenType.NOTREGEX)
                        {
                            bool not = Peek(1).TokenType == XppTokenType.NOTREGEX;

                            XppTokenType tt2 = Peek(2).TokenType;
                            if (tt2 == XppTokenType.StringLiteral || tt2 == XppTokenType.Identifier)
                            {
                                XppParserSymbol pattern = Pop();
                                Pop();
                                XppParserSymbol val = Pop();
                                Push(new XppParserSymbol(XppTokenType.Production, null, new XppRegexExpr(val, pattern, not)));
                                repeat = true;
                                continue;
                            }
                        }
                        throw new ArgumentException("Didn't expect a regex literal here");
                    case XppTokenType.TRUE:
                    case XppTokenType.FALSE:
                        Pop();
                        Push(new XppParserSymbol(XppTokenType.Production, null, new XppBoolLiteralExpr(top.TokenType == XppTokenType.TRUE)));
                        repeat = true;
                        continue;
                }
            } while (repeat);
        }
    }

    internal class XppCompositeExpr : XppAstExpr
    {
        private readonly bool _or;
        private readonly XppAstExpr _left;
        private readonly XppAstExpr _right;

        public XppCompositeExpr(bool or, XppAstExpr left, XppAstExpr right)
        {
            _or = or;
            _left = left;
            _right = right;
        }

        public override bool Evaluate(RuntimeValues runtimeEnv)
        {
            if (_or)
                return _left.Evaluate(runtimeEnv) || _right.Evaluate(runtimeEnv);
            else
                return _left.Evaluate(runtimeEnv) && _right.Evaluate(runtimeEnv);
        }
    }

    internal class XppRegexExpr : XppAstExpr
    {
        private readonly XppParserSymbol val;
        private readonly XppParserSymbol pattern;
        private readonly bool not;

        public const RegexOptions DEFAULT_REGEX_OPTIONS = RegexOptions.IgnoreCase;

        public XppRegexExpr(XppParserSymbol val, XppParserSymbol pattern, bool not)
        {
            this.val = val;
            this.pattern = pattern;
            this.not = not;
        }

        public override bool Evaluate(RuntimeValues runtimeEnv)
        {
            Regex regex;
            object patternObj = pattern.GetRuntimeValue(runtimeEnv);
            if (patternObj is string)
            {
                regex = new Regex((string)patternObj, DEFAULT_REGEX_OPTIONS);
            }
            else if (patternObj is Regex)
            {
                regex = (Regex)patternObj;
            }
            else
            {
                throw new ArgumentException("Right-hand side of regex operator wasn't a string or regex");
            }

            return not ^ regex.IsMatch(val.GetRuntimeValue(runtimeEnv).ToString());
        }
    }

    internal class XppBoolLiteralExpr : XppAstExpr
    {
        private readonly bool val;

        public XppBoolLiteralExpr(bool val)
        {
            this.val = val;
        }

        public override bool Evaluate(RuntimeValues runtimeEnv)
        {
            return val;
        }
    }

    internal class XppCompareExpr : XppAstExpr
    {
        private readonly XppTokenType opToken;
        private readonly XppParserSymbol left;
        private readonly XppParserSymbol right;

        public XppCompareExpr(XppTokenType opToken, XppParserSymbol left, XppParserSymbol right)
        {
            this.opToken = opToken;
            this.left = left;
            this.right = right;
        }

        public override bool Evaluate(RuntimeValues runtimeEnv)
        {
            object leftVal = left.GetRuntimeValue(runtimeEnv);
            object rightVal = right.GetRuntimeValue(runtimeEnv);

            IComparer comparer = new CaseInsensitiveComparer(CultureInfo.InvariantCulture);

            int compare;
            if (leftVal is IComparable)
                compare = comparer.Compare(leftVal, rightVal);
            else if (rightVal is IComparable)
                compare = -comparer.Compare(rightVal, leftVal);
            else
            {
                // Only do this if neither version is comparable.
                // The reason is IComparable objects can change the
                // meaning of Equals in surprising ways if necessary,
                // such as for version matching.
                if (opToken == XppTokenType.EQ)
                    return Equals(leftVal, rightVal);
                if (opToken == XppTokenType.NEQ)
                    return !Equals(leftVal, rightVal);

                throw new ArgumentException("Can't perform a compare on two objects that don't implement IComparable");
            }

            switch (opToken)
            {
                case XppTokenType.LT:
                    return compare < 0;
                case XppTokenType.LTE:
                    return compare <= 0;
                case XppTokenType.EQ:
                    return compare == 0;
                case XppTokenType.NEQ:
                    return compare != 0;
                case XppTokenType.GT:
                    return compare > 0;
                case XppTokenType.GTE:
                    return compare >= 0;
                default:
                    throw new ArgumentException("Program error: Unexpected comparison operator in XppCompareExpr");
            }
        }
    }

    internal class XppLexer
    {
        private int startPos;
        private int pos = 0;
        private readonly string str;

        public XppLexer(string str)
        {
            this.str = str;
        }

        private int Peek()
        {
            if (EOF)
                return -1;
            return str[pos];
        }

        private int Read()
        {
            if (EOF)
                return -1;
            return str[pos++];
        }

        private char ReadNoEOF()
        {
            if (EOF)
                ThrowEOF();
            return str[pos++];
        }

        private void ThrowEOF()
        {
            throw new ArgumentException("Premature end of data, starting at offset " + startPos);
        }

        private bool EOF
        {
            get { return pos >= str.Length; }
        }

        private void Match(char c)
        {
            int readChar;
            if ((readChar = ReadNoEOF()) != c)
                throw new ArgumentException("Unexpected character, was looking for '" + c + "' and got '" + readChar + "'");
        }

        public XppToken NextToken()
        {
        restart:
            startPos = pos;

            while (!EOF)
            {
                int c;
                switch (c = Read())
                {
                    case -1:
                        return null;
                    case '(':
                        return new XppToken(XppTokenType.Lparen, "(", startPos, 1);
                    case ')':
                        return new XppToken(XppTokenType.Rparen, ")", startPos, 1);
                    case '\'': // string literal
                        {
                            StringBuilder tokenValue = new StringBuilder();
                            while (true)
                            {
                                switch (Peek())
                                {
                                    case -1:
                                        ThrowEOF();
                                        break;
                                    case '\\':
                                        Read();
                                        tokenValue.Append(ReadNoEOF());
                                        break;
                                    case '\'':
                                        Read();
                                        return new XppToken(XppTokenType.StringLiteral, tokenValue.ToString(), startPos, pos - startPos);
                                    default:
                                        tokenValue.Append((char)Read());
                                        break;
                                }
                            }
                        }
                    case '!':
                        Match('~');
                        return new XppToken(XppTokenType.NOTREGEX, "!~", startPos, 2);
                    case '=':
                        if (Peek() == '=')
                            throw new ArgumentException("Invalid sequence '=='; if you're looking for equal, try 'EQ' instead.");
                        Match('~');
                        return new XppToken(XppTokenType.REGEX, "=~", startPos, 2);
                    case '/':
                        {
                            StringBuilder tokenValue = new StringBuilder();
                            while (true)
                            {
                                switch (Peek())
                                {
                                    case -1:
                                        ThrowEOF();
                                        break;
                                    case '\\':
                                        Read();
                                        char nextChar = ReadNoEOF();
                                        if (nextChar != '/')
                                            tokenValue.Append('\\');
                                        tokenValue.Append(nextChar);
                                        break;
                                    case '/':
                                        Read();
                                        return new XppToken(XppTokenType.RegexLiteral, tokenValue.ToString(), startPos, pos - startPos);
                                    default:
                                        tokenValue.Append((char)Read());
                                        break;
                                }
                            }
                        }
                    case ' ':
                    case '\t':
                        goto restart;
                    default:
                        if (IsLetter(c))
                        {
                            // identifier or keyword
                            while ((c = Peek()) != -1)
                            {
                                if (IsLetter(c) || IsDigit(c) || c == '_')
                                    Read();
                                else
                                    break;
                            }
                            string val = str.Substring(startPos, pos - startPos);
                            return new XppToken(ClassifyString(val), val, startPos, pos - startPos);
                        }
                        else if (IsDigit(c))
                        {
                            while ((c = Peek()) != -1)
                            {
                                if (IsDigit(c) || c == '.')
                                    Read();
                                else
                                    break;
                            }
                            string val = str.Substring(startPos, pos - startPos);
                            return new XppToken(XppTokenType.VersionLiteral, val, startPos, pos - startPos);
                        }
                        else
                            throw new ArgumentException("Unexpected character '" + c + "' at offset " + startPos);
                }
            }
            return new XppToken(XppTokenType.EOF, "", startPos, 0);
        }

        private XppTokenType ClassifyString(string val)
        {
            // All alphabetic keywords should be defined here
            switch (val.ToUpper(CultureInfo.InvariantCulture))
            {
                case "NOT":
                    return XppTokenType.NOT;
                case "AND":
                    return XppTokenType.AND;
                case "OR":
                    return XppTokenType.OR;
                case "LT":
                    return XppTokenType.LT;
                case "LTE":
                    return XppTokenType.LTE;
                case "IS":
                case "EQ":
                    return XppTokenType.EQ;
                case "NEQ":
                    return XppTokenType.NEQ;
                case "GT":
                    return XppTokenType.GT;
                case "GTE":
                    return XppTokenType.GTE;
                case "TRUE":
                    return XppTokenType.TRUE;
                case "FALSE":
                    return XppTokenType.FALSE;
                default:
                    return XppTokenType.Identifier;
            }
        }

        private static bool IsLetter(int c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private static bool IsDigit(int c)
        {
            return (c >= '0' && c <= '9');
        }
    }
}
