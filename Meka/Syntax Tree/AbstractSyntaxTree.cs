using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meka.Syntax_Tree
{
    public class AbstractSyntaxTree
    {
        public List<Import> Imports;
        public List<AbstractSyntaxCollection> Nodes;  //Represents each collection of items

        public AbstractSyntaxTree()
        {
            Imports = new List<Import>();
            Nodes = new List<AbstractSyntaxCollection>();
        }
    }
}
