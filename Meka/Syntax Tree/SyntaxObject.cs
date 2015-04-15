using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meka.Syntax_Tree
{
    public enum Instruction
    {
        Accessor,
        ParameterType,
        ParameterValue,
        Attribute,
        EnumMemberValue
    }
    public class SyntaxObject
    {
        public Instruction instruction;
        public string Value;
    }
}
