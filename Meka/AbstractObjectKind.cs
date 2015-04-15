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
        Variable,   //A variable                X
        Indexer,    //An indexer acces          X
        Type,       //A type/object             X
        Function,   //A function                X
        Class,      //A class                   X
        Namespace,  //A namespace               X
        Interface,  //An interface              X
        Attribute,  //An attribute              X
        Struct,     //A struct                  X
        Enum,       //An enum                   X
        EnumMember, //An enum member            X
        Accessor,   //An accessor               X
        Property,   //A property                X
        Import,     //An import                 X
        Alias,      //An alias                  X
        Constraint, //A constraint type         X
        Delegate,   //A delegate type           X
        Keyword,    //A keyword                 X
        Integer,    //An integer constant       X
        String,     //A string literal          X
        EndStatement//End of statement          X
    }
}
