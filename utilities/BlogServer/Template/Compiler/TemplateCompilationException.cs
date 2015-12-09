using System;
using System.Text;
using System.CodeDom.Compiler;

namespace DynamicTemplate.Compiler
{
    public class TemplateCompilationException : Exception
    {
        private readonly string _message;
        private readonly Position _position;

        internal static TemplateCompilationException CreateFromCompilerResults(CompilerResults results, PositionTransposer transposer)
        {
            foreach (CompilerError error in results.Errors)
            {
                if (!error.IsWarning)
                {
                    string message = error.ErrorNumber + ": " + error.ErrorText;
                    return new TemplateCompilationException(message, transposer.TranslatePosition(new Position(error.Line, error.Column)));
                }
            }
            throw new ArgumentException();
        }

        internal TemplateCompilationException(string message, Position position) : base(message)
        {
            _message = message;
            _position = position;
        }

        public Position Position
        {
            get { return _position; }
        }
    }
}
