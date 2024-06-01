using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC_Lab3.TreeDir
{
    public class Tree
    {
        public TreeNode RootNode { get; }

        public Tree(TreeNode rootNode)
        {
            RootNode = rootNode;
        }
    }
}
