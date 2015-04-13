using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meka
{
    public enum AbstractObjectKind
    {
        None,       //No Abstract Object        X
        Comment,    //A single line comment     X
        Operator,   //An operator               X
        Variable,   //A variable                
        Type,       //A type/object             
        Function,   //A function                
        Class,      //A class                   X
        Namespace,  //A namespace               X
        Interface,  //An interface              X
        Attribute,  //An attribute              
        Struct,     //A struct                  X
        Enum,       //An enum                   X
        EnumMember, //An enum member            X
        Accessor,   //An accessor               X
        Property,   //A property                
        Import,     //An import                 X
        Alias,      //An alias                  
        Definition, //A variable definition     
        Constraint, //A constraint type         
        Delegate,   //A delegate type           
        Keyword,    //A keyword                 
        Integer,    //An integer constant       
        String,     //A string literal          X
        Float       //A float constant          
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
