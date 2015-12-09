using System;
using System.Text;

namespace DynamicTemplate.Compiler
{
    public struct Position : IComparable
    {
        public Position(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public readonly int Line;
        public readonly int Column;

        public int CompareTo(object obj)
        {
        	Position other = (Position) obj;
            if (Line != other.Line)
            {
                return Line - other.Line;
            }

            return Column - other.Column;
        }

        public override bool Equals(object obj)
        {
            return Line == ((Position)obj).Line && Column == ((Position)obj).Column;
        }

        public override int GetHashCode()
        {
            return Line * 513 + Column;
        }

        public static readonly Position Unknown = new Position(-1, -1);
    }
}
