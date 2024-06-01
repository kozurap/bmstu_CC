using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilers_Lab1
{
    public class ParseTree
    {
        public ParseTreeNodeTypeEnum Type;
        public char? Data;
        public ParseTree Left;
        public ParseTree Right;

        public ParseTree(ParseTreeNodeTypeEnum _type, char? _data, ParseTree _left, ParseTree _right)
        {
            Type = _type;
            Data = _data;
            Left = _left;
            Right = _right;
        }
    }
}
