using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC_Lab3.TreeDir
{
    public class TreeNode
    {
        public string Value { get; }
        public string Attribute { get; set; }
        public TreeNode[] ChildNodes { get; }

        public TreeNode(string value, string attribute,
            TreeNode[]? childNodes = null)
        {
            Value = value;
            Attribute = attribute.Trim(' ');
            ChildNodes = childNodes ?? Array.Empty<TreeNode>();
        }
    }
}
