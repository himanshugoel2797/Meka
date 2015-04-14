using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meka.Syntax_Tree
{
    public class AbstractSyntaxCollection
    {
        public AbstractObjectKind ObjectKind;   //Which type of collection is this
        public List<AbstractSyntaxCollection> Children;   //The children of the collection
        public List<SyntaxObject> SyntaxObjects;
        public string Name; //The name of the collection
        public AbstractSyntaxCollection Parent; //The parent of this collection

        public AbstractSyntaxCollection()
        {
            Children = new List<AbstractSyntaxCollection>();
            SyntaxObjects = new List<SyntaxObject>();
        }

        public AbstractSyntaxCollection GetCollection(string name)
        {
            return Children.Single(x => x.Name == name);
        }
    }
}
