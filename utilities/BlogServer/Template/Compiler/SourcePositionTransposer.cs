using System;
using System.Collections;
using System.Text;
using System.Diagnostics;

namespace DynamicTemplate.Compiler
{
    public class PositionTransposer
    {
        private readonly ArrayList _mappings;
        private bool dirty = false;

        public PositionTransposer()
        {
            _mappings = new ArrayList();
        }

        public void AddMapping(Position from, Position to)
        {
            dirty = true;
            _mappings.Add(new PositionMapping(from, to));
        }

        public Position TranslatePosition(Position pos)
        {
            try
            {
                if (pos.Equals(Position.Unknown))
                    return Position.Unknown;

                if (dirty)
                {
                    _mappings.Sort();
                    dirty = false;
                }

                if (_mappings.Count > 0)
                {
                    int idx = _mappings.BinarySearch(new PositionMapping(pos, Position.Unknown));
                    if (idx >= 0)
                    {
                        return ((PositionMapping) _mappings[idx]).Value;
                    }
                    else
                    {
                        int closestIndex = (~idx) - 1;
                        PositionMapping matchedMapping = (PositionMapping) _mappings[closestIndex];
                        if (!matchedMapping.Value.Equals(Position.Unknown))
                        {
                            if (matchedMapping.Key.Line == pos.Line)
                            {
                                return new Position(
                                    matchedMapping.Value.Line,
                                    matchedMapping.Value.Column + (pos.Column - matchedMapping.Key.Column));
                            }
                            else
                            {
                                return new Position(
                                    matchedMapping.Value.Line + (pos.Line - matchedMapping.Key.Line),
                                    pos.Column);
                            }
                        }
                    }
                }

                return Position.Unknown;
            }
            catch (Exception e)
            {
                Trace.Fail(e.ToString());
                return Position.Unknown;
            }
        }

        struct PositionMapping : IComparable
        {
            public PositionMapping(Position key, Position value)
            {
                Key = key;
                Value = value;
            }

            public readonly Position Key;
            public readonly Position Value;
        	
        	public static implicit operator Position(PositionMapping pm)
        	{
        		return pm.Key;
        	}

        	public int CompareTo(object obj)
        	{
        		if (obj is PositionMapping)
        		{
        			return Key.CompareTo(((PositionMapping) obj).Key);
        		}
        		else if (obj is Position)
        		{
        			return Key.CompareTo((Position) obj);
        		}
        		else
        			throw new InvalidCastException();
        	}
        }

    }
}
