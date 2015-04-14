using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meka.Syntax_Tree
{
    public enum Instruction
    {
        Call,
        Definition,
        Assignment,
        Accessor,
        Operation,
        Type
    }
    public class SyntaxObject
    {
        public Instruction instruction;
        public string Value;
    }
}
