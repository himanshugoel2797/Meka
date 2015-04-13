using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

namespace Meka
{
    public class Tokenizer
    {
        Scanner scanner;

        public Tokenizer(string file)
        {
            scanner = new Scanner(file);
            str = new MemoryStream();
            writer = new StreamWriter(str);
            ObjectKindStack = new Stack<AbstractObjectKind>();
        }


        void FillTagTillChar(params char[] tag)
        {
            char? nxChar = scanner.GetNextCharacter();
            while (nxChar != null && !tag.Contains((char)nxChar))   //Read up till the first { which marks the start of the namespace body
            {
                if (!char.IsWhiteSpace((char)nxChar))
                {
                    writer.Write(nxChar);
                }
                nxChar = scanner.GetNextCharacter();
            }
            scanner.Position--;
        }
        void GenerateTag(AbstractObjectKind tagName, params char[] delims)
        {
            ObjectKind = tagName;
            scanner.Position += tagName.ToString().Length - 1;
            FillTagTillChar(delims);
            ObjectKind = AbstractObjectKind.None;
            writer.Write(">");
        }

        MemoryStream str;
        StreamWriter writer;
        Stack<AbstractObjectKind> ObjectKindStack;
        AbstractObjectKind objKind;
        AbstractObjectKind ObjectKind
        {
            get
            {
                return objKind;
            }
            set
            {
                objKind = value;
                if (objKind != AbstractObjectKind.None)
                {
                    writer.Write("<" + objKind.ToString() + " ");
                }
            }
        }           //ObjectKind is setup as a property to minimize code repeat for writing out the tags

        public Stream Parse()
        {
            char? currentChar = scanner.GetNextCharacter();

            //TODO Need to watch out for the fact that a string match for 'class' will pass for both 'class' and 'classmate' which would be incorrect

            //Parse until the end
            while (currentChar != null)
            {
                if (ObjectKind != AbstractObjectKind.String && ObjectKind != AbstractObjectKind.Comment && !char.IsWhiteSpace((char)currentChar))        //If we aren't parsing a string, skip all spaces
                {
                    if (currentChar == '"')   //Check if we're parsing a string
                    {
                        ObjectKind = AbstractObjectKind.String;
                        writer.Write("\"");
                    }
                    #region End Statement
                    else if (currentChar == ';')
                    {
                        writer.Write("<End Statement>");
                    }
                    #endregion
                    #region Closing tag
                    else if (currentChar == '}')
                    {
                        if (ObjectKindStack.Count > 0)
                        {
                            var tmp = ObjectKindStack.Pop();
                            writer.Write("</" + tmp + ">");
                        }
                        else
                        {
                            //TODO Throw an exception because we have too many closing braces
                        }
                    }
                    #endregion
                    #region Opening Tag
                    else if (currentChar == '{')
                    {
                        writer.Write("<Opening Tag>");
                    }
                    #endregion
                    #region Enum Member
                    else if (ObjectKindStack.Count > 0 && ObjectKindStack.Peek() == AbstractObjectKind.Enum) //Check if we're in an enum
                    {
                        ObjectKind = AbstractObjectKind.EnumMember;
                        if (currentChar != ',') writer.Write(scanner.PeekRelativeNthCharacter(0));
                        FillTagTillChar(',', '}');
                        ObjectKind = AbstractObjectKind.None;
                        writer.Write(">");
                    }
                    #endregion
                    #region Operators
                    else if (
                       (currentChar == '\\' && scanner.PeekNextCharacter() != '\\') ||         //Check for division, make sure it isn't a comment
                       currentChar == '+' ||   //Addition
                       currentChar == '-' ||   //Subtraction
                       currentChar == '*' ||   //Multiplication
                       currentChar == '%' ||   //Modulus
                       currentChar == '!' ||   //logical NOT
                       currentChar == '~' ||   //bitwise NOT
                       currentChar == '&' ||   //logical AND
                       currentChar == '^' ||   //logical XOR
                       currentChar == '|' ||   //logical OR
                       currentChar == '=' ||   //Assignment
                       currentChar == '<' ||   //Less than
                       currentChar == '>' ||   //Greater than
                       currentChar == '.' ||
                       scanner.StringMatches("new")    //New operator
                       )
                    {
                        ObjectKind = AbstractObjectKind.Operator;
                        writer.Write("'" + currentChar);

                        //Check for multicharacter operator
                        if (scanner.StringMatches("new"))
                        {
                            //TODO Add code to express the next word as a type
                            writer.Write("ew"); //Finish off the operator
                            scanner.GetNextCharacter();
                            scanner.GetNextCharacter();     //Pull the next two characters from the string, we can skip them
                        }
                        else                    //Test for other multicharacter operators
                        {
                            char? nxChar = scanner.PeekNextCharacter();
                            string op = currentChar.ToString() + nxChar.ToString();
                            if (op == "++" || op == "--" || op == "!=" || op == "^=" || op == "&=" || op == "|=" || op == "<<" || op == ">>" || op == "==" || op == "||" ||
                                op == "??" || op == "&&" || op == "+=" || op == "-=" || op == "*=" || op == "/=" || op == "%=" || op == "=>")
                            {
                                writer.Write(nxChar);
                                scanner.Position++;
                            }
                        }

                        ObjectKind = AbstractObjectKind.None;
                        writer.Write("'>");
                    }
                    #endregion
                    #region Imports
                    else if (scanner.StringMatches("using"))
                    {
                        ObjectKind = AbstractObjectKind.Import;
                        scanner.Position += 4;
                        char? nxChar = scanner.GetNextCharacter();
                        while (nxChar != null && nxChar != ';')
                        {
                            if (!char.IsWhiteSpace((char)nxChar))   //Skip whitespace unless it's the 'from' statement
                            {
                                writer.Write(nxChar);
                            }
                            //Check for 'from' keyword
                            else if (scanner.PeekNextCharacter() == 'f' && scanner.PeekRelativeNthCharacter(2) == 'r' && scanner.PeekRelativeNthCharacter(3) == 'o' && scanner.PeekRelativeNthCharacter(4) == 'm')
                            {
                                writer.Write(":");
                                scanner.Position += 4;
                            }
                            nxChar = scanner.GetNextCharacter();
                        }
                        ObjectKind = AbstractObjectKind.None;
                        writer.Write(">");
                    }
                    #endregion
                    #region Single Line Comment
                    else if (scanner.StringMatches(@"//"))    //Check for a single line comment
                    {
                        scanner.Position++;     //Skip the next '/' character
                        ObjectKind = AbstractObjectKind.Comment;
                        writer.Write("\\*");
                    }
                    #endregion
                    #region Accessors
                    else if (scanner.StringMatchesWord("public") || scanner.StringMatchesWord("private") || scanner.StringMatchesWord("protected") || scanner.StringMatchesWord("internal") || scanner.StringMatchesWord("static"))
                    {
                        ObjectKind = AbstractObjectKind.Accessor;
                        if (scanner.StringMatches("public"))
                        {
                            writer.Write("public");
                            scanner.Position += "public".Length - 1;
                        }
                        else if (scanner.StringMatches("private"))
                        {
                            writer.Write("private");
                            scanner.Position += "private".Length - 1;
                        }
                        else if (scanner.StringMatches("protected"))
                        {
                            writer.Write("protected");
                            scanner.Position += "protected".Length - 1;
                        }
                        else if (scanner.StringMatches("internal"))
                        {
                            writer.Write("internal");
                            scanner.Position += "internal".Length - 1;
                        }
                        else if (scanner.StringMatches("static"))
                        {
                            writer.Write("static");
                            scanner.Position += "static".Length - 1;
                        }
                        ObjectKind = AbstractObjectKind.None;
                        writer.Write(">");
                    }
                    #endregion
                    #region Namespace
                    else if (scanner.StringMatchesWord("namespace"))
                    {
                        GenerateTag(AbstractObjectKind.Namespace, '{');
                        ObjectKindStack.Push(AbstractObjectKind.Namespace); //Manage depth checking by pushing the definition of this namespace on to the stack
                    }
                    #endregion
                    #region Class
                    else if (scanner.StringMatchesWord("class"))
                    {
                        GenerateTag(AbstractObjectKind.Class, '{');
                        ObjectKindStack.Push(AbstractObjectKind.Class); //Push the definition of this class on to the stack
                    }
                    #endregion
                    #region Struct
                    else if (scanner.StringMatchesWord("struct"))
                    {
                        GenerateTag(AbstractObjectKind.Struct, '{');
                        ObjectKindStack.Push(AbstractObjectKind.Struct);
                    }
                    #endregion
                    #region Enum
                    else if (scanner.StringMatchesWord("enum"))
                    {
                        GenerateTag(AbstractObjectKind.Enum, '{');
                        ObjectKindStack.Push(AbstractObjectKind.Enum);
                    }
                    #endregion
                    #region Interface
                    else if (scanner.StringMatchesWord("interface"))
                    {
                        GenerateTag(AbstractObjectKind.Interface, '{');
                        ObjectKindStack.Push(AbstractObjectKind.Interface);
                    }
                    #endregion
                    #region Delegate
                    else if (scanner.StringMatchesWord("delegate"))
                    {
                        GenerateTag(AbstractObjectKind.Delegate, ';');
                    }
                    #endregion
                    #region Property
                    else if (scanner.StringMatchesWord("property"))
                    {
                        GenerateTag(AbstractObjectKind.Property, ';');
                    }
                    #endregion
                    #region Alias
                    else if (scanner.StringMatchesWord("alias"))
                    {
                        GenerateTag(AbstractObjectKind.Alias, ';');
                    }
                    #endregion
                    #region Integer Constant
                    else if (char.IsDigit((char)currentChar))
                    {
                        ObjectKind = AbstractObjectKind.Integer;
                        writer.Write(currentChar);

                        char? nxChar = scanner.GetNextCharacter();
                        while (nxChar != null && char.IsDigit((char)nxChar))   //Read up till the first { which marks the start of the namespace body
                        {
                            writer.Write(nxChar);
                            nxChar = scanner.GetNextCharacter();
                        }
                        scanner.Position--;
                        ObjectKind = AbstractObjectKind.None;
                        writer.Write(">");
                    }
                    #endregion
                    #region Constraint
                    else if (scanner.StringMatchesWord("constraint"))
                    {
                        GenerateTag(AbstractObjectKind.Constraint, ';');
                    }
                    #endregion
                    #region Function
                    else if (scanner.StringMatchesWord("function"))
                    {
                        ObjectKind = AbstractObjectKind.Function;
                        scanner.Position += AbstractObjectKind.Function.ToString().Length - 1;

                        char? nxChar = scanner.GetNextCharacter();
                        while (nxChar != null && nxChar != '{')   //Read up till the first { which marks the start of the namespace body
                        {
                            if (nxChar != '\n' && nxChar != '\r' && nxChar != '\t') writer.Write(nxChar);
                            nxChar = scanner.GetNextCharacter();
                        }
                        scanner.Position--;

                        ObjectKind = AbstractObjectKind.None;
                        writer.Write(">");

                        ObjectKindStack.Push(AbstractObjectKind.Function);
                    }
                    #endregion
                    else if (currentChar == '[')
                    {
                        #region Indexer Access
                        if (ObjectKindStack.Count > 0 &&
                            (ObjectKindStack.Peek() == AbstractObjectKind.Function ||
                             ObjectKindStack.Peek() == AbstractObjectKind.Keyword)) //We're inside a function, this is an array access
                        {
                            ObjectKind = AbstractObjectKind.Indexer;
                            FillTagTillChar(']');
                            ObjectKind = AbstractObjectKind.None;
                            writer.Write(">");
                        }
                        #endregion
                        #region Attribute
                        else
                        {
                            ObjectKind = AbstractObjectKind.Attribute;
                            FillTagTillChar(']');
                            ObjectKind = AbstractObjectKind.None;
                            writer.Write(">");
                        }
                        #endregion
                    }
                    #region Opening Bracket
                    else if (currentChar == '(')
                    {
                        writer.Write("<Opening Bracket>");
                    }
                    #endregion
                    #region Closing Bracket
                    else if (currentChar == ')')
                    {
                        writer.Write("<Closing Bracket>");
                    }
                    #endregion
                    #region Keywords
                    #region If
                    else if (scanner.StringMatchesWordTerminator("if", '('))
                    {
                        ObjectKind = AbstractObjectKind.Keyword;
                        ObjectKindStack.Push(ObjectKind);
                        writer.Write("if>");
                        scanner.Position += "if".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    #endregion
                    #region Else
                    else if (scanner.StringMatchesWord("else"))
                    {
                        ObjectKind = AbstractObjectKind.Keyword;
                        writer.Write("else>");
                        scanner.Position += "else".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    #endregion
                    #region Switch
                    else if (scanner.StringMatchesWordTerminator("switch", '('))
                    {
                        ObjectKind = AbstractObjectKind.Keyword;
                        ObjectKindStack.Push(ObjectKind);
                        writer.Write("switch>");
                        scanner.Position += "switch".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    #endregion
                    #region Case
                    else if (scanner.StringMatchesWord("case"))
                    {
                        ObjectKind = AbstractObjectKind.Keyword;
                        ObjectKindStack.Push(ObjectKind);
                        writer.Write("case>");
                        scanner.Position += "case".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    #endregion
                    #region As
                    else if (scanner.StringMatchesWord("as"))
                    {
                        ObjectKind = AbstractObjectKind.Keyword;
                        writer.Write("as>");
                        scanner.Position += "as".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    #endregion
                    #region While
                    else if (scanner.StringMatchesWordTerminator("while", '('))
                    {
                        ObjectKind = AbstractObjectKind.Keyword;
                        ObjectKindStack.Push(ObjectKind);
                        writer.Write("while>");
                        scanner.Position += "while".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    #endregion
                    #region For
                    else if (scanner.StringMatchesWordTerminator("for", '('))
                    {
                        ObjectKind = AbstractObjectKind.Keyword;
                        ObjectKindStack.Push(ObjectKind);
                        writer.Write("for>");
                        scanner.Position += "for".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    #endregion
                    #region Foreach
                    else if (scanner.StringMatchesWordTerminator("foreach", '('))
                    {
                        ObjectKind = AbstractObjectKind.Keyword;
                        ObjectKindStack.Push(ObjectKind);
                        writer.Write("foreach>");
                        scanner.Position += "foreach".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    #endregion
                    #endregion
                    #region Types
                    else if (scanner.StringMatchesWord("int"))
                    {
                        ObjectKind = AbstractObjectKind.Type;
                        writer.Write("int>");
                        scanner.Position += "int".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    else if (scanner.StringMatchesWord("uint"))
                    {
                        ObjectKind = AbstractObjectKind.Type;
                        writer.Write("uint>");
                        scanner.Position += "uint".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    else if (scanner.StringMatchesWord("long"))
                    {
                        ObjectKind = AbstractObjectKind.Type;
                        writer.Write("long>");
                        scanner.Position += "long".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    else if (scanner.StringMatchesWord("ulong"))
                    {
                        ObjectKind = AbstractObjectKind.Type;
                        writer.Write("ulong>");
                        scanner.Position += "ulong".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    else if (scanner.StringMatchesWord("double"))
                    {
                        ObjectKind = AbstractObjectKind.Type;
                        writer.Write("double>");
                        scanner.Position += "double".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    else if (scanner.StringMatchesWord("byte"))
                    {
                        ObjectKind = AbstractObjectKind.Type;
                        writer.Write("byte>");
                        scanner.Position += "byte".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    else if (scanner.StringMatchesWord("sbyte"))
                    {
                        ObjectKind = AbstractObjectKind.Type;
                        writer.Write("sbyte>");
                        scanner.Position += "sbyte".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    else if (scanner.StringMatchesWord("short"))
                    {
                        ObjectKind = AbstractObjectKind.Type;
                        writer.Write("short>");
                        scanner.Position += "short".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    else if (scanner.StringMatchesWord("ushort"))
                    {
                        ObjectKind = AbstractObjectKind.Type;
                        writer.Write("ushort>");
                        scanner.Position += "ushort".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    else if (scanner.StringMatchesWord("char"))
                    {
                        ObjectKind = AbstractObjectKind.Type;
                        writer.Write("char>");
                        scanner.Position += "char".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    else if (scanner.StringMatchesWord("void"))
                    {
                        ObjectKind = AbstractObjectKind.Type;
                        writer.Write("void>");
                        scanner.Position += "void".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    else if (scanner.StringMatchesWord("object"))
                    {
                        ObjectKind = AbstractObjectKind.Type;
                        writer.Write("object>");
                        scanner.Position += "object".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    else if (scanner.StringMatchesWord("string"))
                    {
                        ObjectKind = AbstractObjectKind.Type;
                        writer.Write("string>");
                        scanner.Position += "string".Length - 1;
                        ObjectKind = AbstractObjectKind.None;
                    }
                    #endregion
                    #region Symbols
                    else if (char.IsLetterOrDigit((char)currentChar) || currentChar == '_')
                    {
                        ObjectKind = AbstractObjectKind.Variable;
                        writer.Write(currentChar);

                        char? nxChar = scanner.GetNextCharacter();
                        while (nxChar != null && (char.IsLetterOrDigit((char)nxChar) || nxChar == '_'))   //Read up till the first { which marks the start of the namespace body
                        {
                            writer.Write(nxChar);
                            nxChar = scanner.GetNextCharacter();
                        }

                        scanner.Position--;

                        ObjectKind = AbstractObjectKind.None;
                        writer.Write(">");
                    }
                    #endregion
                }
                else if (ObjectKind == AbstractObjectKind.String)  //If we are parsing a string
                {
                    #region String Tokenization and Error Checking
                    //Check for escape sequences and the end of the string
                    if (currentChar != '"')
                    {
                        if (currentChar == '\\')    //Check for a valid escape sequence
                        {
                            char? nextChar = scanner.GetNextCharacter();    //Once we're through with this, the next character can be skipped
                            switch (nextChar)
                            {
                                case 'a':
                                    writer.Write("\\a");
                                    break;
                                case 'b':
                                    writer.Write("\\b");
                                    break;
                                case 'f':
                                    writer.Write("\\f");
                                    break;
                                case 'n':
                                    writer.Write("\\n");
                                    break;
                                case 'r':
                                    writer.Write("\\r");
                                    break;
                                case 't':
                                    writer.Write("\\t");
                                    break;
                                case 'v':
                                    writer.Write("\\v");
                                    break;
                                case '\'':
                                case '"':
                                case '\\':
                                    writer.Write("\\" + nextChar);
                                    break;
                                case 'u':
                                    writer.Write("\\u");
                                    for (int i = 0; i < 4; i++)
                                    {
                                        char? nxChar = scanner.GetNextCharacter();
                                        if (nxChar == null || !char.IsLetterOrDigit((char)nxChar))
                                        {
                                            //TODO throw exception because the unicode is incorrect
                                        }
                                        else writer.Write(nxChar);
                                    }
                                    break;
                                default:
                                    //TODO throw exception depending on what kind of error it is
                                    break;
                            }
                        }
                        else
                        {
                            writer.Write(currentChar);
                        }
                    }
                    else
                    {
                        ObjectKind = AbstractObjectKind.None;   //We have reached the end of the string
                        writer.Write("\">");
                    }
                    #endregion
                }
                else if (ObjectKind == AbstractObjectKind.Comment)  //If we are parsing a comment
                {
                    #region Comment Tokenization
                    //Check for comment commands
                    if (currentChar != '\n')  //Check for end of comment (newline)
                    {
                        if (currentChar != '\r') writer.Write(currentChar);
                    }
                    else
                    {
                        writer.Write("*\\>");   //Write the comment terminater tag
                        ObjectKind = AbstractObjectKind.None;
                    }
                    #endregion
                }

                currentChar = scanner.GetNextCharacter();
            }

            writer.Flush();
            str.Position = 0;
            return str;
        }
    }
}
