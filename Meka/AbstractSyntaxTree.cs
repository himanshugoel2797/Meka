using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meka
{
    public enum AbstractObjectKind
    {
        None, Comment, Operator, Variable, Type, Function, Class, Namespace, Interface, Attribute, Struct, Enum, Accessor, Property, Import, Alias, Definition, Constraint, Delegate, Keyword, Integer, String, Float
    }

    public class AbstractSyntaxTree
    {
        public AbstractObjectKind ObjectKind;
        public string value;
        public List<AbstractSyntaxTree> Children = new List<AbstractSyntaxTree>();
        public AbstractSyntaxTree Parent;

        public void AddChild(AbstractSyntaxTree child)
        {
            Children.Add(child);
        }
    }
}
