using System;

namespace DynamicTemplate
{
    public class ArgumentDescription
    {
        private Type _type;
        private string _identifier;

        public ArgumentDescription() { }

        public ArgumentDescription(Type type, string identifier)
        {
            _type = type;
            _identifier = identifier;
        }

        public Type Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string Identifier
        {
            get { return _identifier; }
            set { _identifier = value; }
        }
    }
}
