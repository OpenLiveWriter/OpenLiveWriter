using System;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;

namespace DynamicTemplate.Compiler
{
    public delegate string TemplateOperation(object[] parameters);

    abstract class LanguageProvider
    {
        public abstract string Name { get; }

        public abstract bool IsValidIdentifier(string identifier, out string errorMessage);
        public abstract string NormalizeIdentifier(string identifier);

        public abstract void Start(ArgumentDescription[] argDescs);
        public abstract void Code(string code, Position startPos);
        public abstract void Expression(string expr, Position startPos);
        public abstract void Literal(string literal, Position startPos);
        public abstract TemplateOperation End();
    }
}
