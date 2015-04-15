using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meka.Syntax_Tree;

namespace Meka
{
    public class Parser
    {
        string tokenizedStr;
        Scanner scanner;

        //TODO Parse the output from the tokenizer into an abstract syntax tree
        public Parser(Stream tokenized)
        {
            using (StreamReader reader = new StreamReader(tokenized))
            {
                tokenizedStr = reader.ReadToEnd();
                scanner = new Scanner(tokenizedStr);
            }
        }

        #region Parser
        void AccessorDepthApply(string tag, AbstractObjectKind kind, bool applyAccessors = false)
        {
            string Name = tag.Replace(kind.ToString() + " ", string.Empty);

            //Create the collection entry for the AbstractSyntaxKind, adding it to the global node and making it the current deepest
            var tmp = new AbstractSyntaxCollection()
            {
                Name = Name.Trim(),
                ObjectKind = kind,
                Parent = DeepestNode
            };
            if (applyAccessors) tmp.SyntaxObjects = Accessors.ToList();
            DeepestNode.Children.Add(tmp);
            DeepestNode = tmp;
            collectionHeirarchy.Push(Name);
            collectionHeirarchyKind.Push(kind); //Specify the kind of object we're in
        }
        void DepthUnApply(AbstractObjectKind kind)
        {
            //Check the tag and make sure that the tag matches up, otherwise there is a syntax error which needs to be reported, if kind is 'None' we can ignore the tag
            if (collectionHeirarchyKind.Peek() == kind || kind == AbstractObjectKind.None)
            {
                DeepestNode = DeepestNode.Parent;
                collectionHeirarchy.Pop();
                collectionHeirarchyKind.Pop();
            }
            else
            {
                //TODO throw missing brace exception
            }
        }

        AbstractSyntaxTree syntaxTree = new AbstractSyntaxTree();
        AbstractSyntaxCollection DeepestNode = new AbstractSyntaxCollection();  //Initially represents the global root node
        AbstractSyntaxCollection PreviousNode = new AbstractSyntaxCollection(); //the previous node

        Stack<string> collectionHeirarchy = new Stack<string>();
        Stack<AbstractObjectKind> collectionHeirarchyKind = new Stack<AbstractObjectKind>();
        List<SyntaxObject> Accessors = new List<SyntaxObject>();
        int depthCount = 0, startPosition = 0;

        List<string> Enums = new List<string>();
        List<string> Classes = new List<string>();
        List<string> Functions = new List<string>();
        Dictionary<string, string> Variables = new Dictionary<string, string>();


        public AbstractSyntaxTree Parse()
        {
            char? currentChar = scanner.GetNextCharacter();
            while (currentChar != null)
            {
                if (currentChar == '<')
                {
                    //This trick allows us to not worry about the statements having the '<' in them
                    if (++depthCount == 1)    //This means we have entered a valid tag
                    {
                        //Save the position so we can later extract the substring
                        startPosition = scanner.Position + 1;
                    }
                }
                else if (currentChar == '>')
                {
                    //We expect this to go down to zero for each valid tag
                    if (--depthCount == 0)
                    {
                        //Get the tag from the start position to here
                        string tag = scanner.Source.Substring(startPosition, scanner.Position - startPosition);

                        #region Import
                        if (tag.StartsWith("Import "))
                        {
                            //Parse the import information
                            syntaxTree.Imports.Add(new Import()
                            {
                                Item = tag.Replace("Import ", string.Empty).Split(':')[0],
                                BaseLibrary = tag.Replace("Import ", string.Empty).Split(':')[1]
                            });
                        }
                        #endregion
                        #region Uses Accessors
                        else if (tag.StartsWith("Namespace") ||
                            tag.StartsWith("Class") ||
                            tag.StartsWith("Interface") ||
                            tag.StartsWith("Enum ") ||
                            tag.StartsWith("Struct ") ||
                            tag.StartsWith("Delegate ") ||
                            tag.StartsWith("Constraint ") ||
                            tag.StartsWith("Function ") ||
                            tag.StartsWith("Variable ") ||
                            tag.StartsWith("Type "))
                        //This handles tags which can legally have accessors before them
                        {
                            if (collectionHeirarchyKind.Count > 0 && collectionHeirarchyKind.Peek() == AbstractObjectKind.Function && Accessors.Count > 0)
                            {
                                //Throw exception, accessors are not allowed here
                            }

                            //Things inside functions can not have accessors
                            if (Accessors.Count == 0 && collectionHeirarchyKind.Count > 0)   //Default to private for anything which requires an accessor but hasn't been given one
                            {
                                Accessors.Add(new SyntaxObject()
                                {
                                    instruction = Instruction.Accessor,
                                    Value = "private"
                                });
                            }

                            #region Namespace
                            if (tag.StartsWith("Namespace "))
                            {
                                AccessorDepthApply(tag, AbstractObjectKind.Namespace, true);
                            }
                            #endregion
                            #region Class
                            else if (tag.StartsWith("Class "))
                            {
                                AccessorDepthApply(tag, AbstractObjectKind.Class, true);
                            }
                            #endregion
                            #region Interface
                            else if (tag.StartsWith("Interface "))
                            {
                                AccessorDepthApply(tag, AbstractObjectKind.Interface, true);
                            }
                            #endregion
                            #region Enum
                            else if (tag.StartsWith("Enum "))
                            {
                                AccessorDepthApply(tag, AbstractObjectKind.Enum, true);
                            }
                            #endregion
                            #region Struct
                            else if (tag.StartsWith("Struct "))
                            {
                                AccessorDepthApply(tag, AbstractObjectKind.Struct, true);
                            }
                            #endregion
                            #region Delegate
                            else if (tag.StartsWith("Delegate "))
                            {
                                AccessorDepthApply(tag, AbstractObjectKind.Delegate, true);
                            }
                            #endregion
                            #region Constraint
                            else if (tag.StartsWith("Constraint "))
                            {
                                AccessorDepthApply(tag, AbstractObjectKind.Constraint, true);
                            }
                            #endregion
                            #region Function
                            else if (tag.StartsWith("Function "))
                            {
                                AccessorDepthApply(tag, AbstractObjectKind.Function, true);
                            }
                            #endregion
                            #region Variable
                            else if (tag.StartsWith("Variable "))
                            {
                                string varName = tag.Replace("Variable ", string.Empty);
                                DeepestNode.Children.Add(new AbstractSyntaxCollection()
                                {
                                    Parent = DeepestNode,
                                    ObjectKind = AbstractObjectKind.Variable,
                                    Name = varName,
                                    SyntaxObjects = Accessors.ToList()
                                });
                                //TODO figure out how to deal with separating variables from userdefined types and from function calls
                            }
                            else if (tag.StartsWith("Type "))
                            {
                                string typeName = tag.Replace("Type ", string.Empty);
                                DeepestNode.Children.Add(new AbstractSyntaxCollection()
                                {
                                    Parent = DeepestNode,
                                    ObjectKind = AbstractObjectKind.Type,
                                    Name = typeName,
                                    SyntaxObjects = Accessors.ToList()
                                });
                            }
                            #endregion
                            Accessors.Clear();  //All accessor's should have been applied so clear out the list
                        }
                        #endregion
                        #region Accessor
                        else if (tag.StartsWith("Accessor "))
                        {
                            //Parse the accessor and add it to the accessor list, to be pulled out by the next valid group
                            string accessor = tag.Replace("Accessor ", string.Empty);
                            Accessors.Add(new SyntaxObject()
                            {
                                instruction = Instruction.Accessor,
                                Value = accessor
                            });
                        }
                        #endregion
                        #region No Accessors
                        else if (Accessors.Count == 0)   //If any accessors are set, these AbstractSyntaxKinds are not legal
                        {
                            //If accessors were placed in an illegal place, throw an exception
                            if (tag.StartsWith("/Class"))
                            {
                                DepthUnApply(AbstractObjectKind.Class);
                            }
                            else if (tag.StartsWith("/Namespace"))
                            {
                                DepthUnApply(AbstractObjectKind.Namespace);
                            }
                            else if (tag.StartsWith("/Interface"))
                            {
                                DepthUnApply(AbstractObjectKind.Interface);
                            }
                            else if (tag.StartsWith("/Function"))
                            {
                                DepthUnApply(AbstractObjectKind.Function);
                            }
                            else if (tag.StartsWith("/Enum"))
                            {
                                DepthUnApply(AbstractObjectKind.Enum);
                            }
                            else if (tag.StartsWith("/Struct"))
                            {
                                DepthUnApply(AbstractObjectKind.Struct);
                            }
                            #region Operators
                            else if (tag.StartsWith("Operator "))
                            {
                                //TODO Generate extra nodes for +=, -=, *=, /=, %=, !=, ^=, |=, &=
                                DeepestNode.Children.Add(new AbstractSyntaxCollection()
                                {
                                    ObjectKind = AbstractObjectKind.Operator,
                                    Name = tag.Replace("Operator ", string.Empty).Replace("'", string.Empty),
                                    Parent = DeepestNode
                                });
                            }
                            #endregion
                            #region EnumMember
                            else if (tag.StartsWith("EnumMember"))
                            {
                                DeepestNode.Children.Add(new AbstractSyntaxCollection()
                                {
                                    ObjectKind = AbstractObjectKind.EnumMember,
                                    Name = tag.Replace("EnumMember ", string.Empty),
                                    Parent = DeepestNode
                                });
                            }
                            #endregion
                            #region Brackets
                            else if (tag.StartsWith("OpeningBracket"))
                            {
                                AccessorDepthApply(tag, AbstractObjectKind.None);
                            }
                            else if (tag.StartsWith("ClosingBracket"))
                            {
                                DepthUnApply(AbstractObjectKind.None);
                            }
                            #endregion
                            #region End Statement
                            else if (tag.StartsWith("EndStatement"))
                            {
                                if (collectionHeirarchyKind.Peek() == AbstractObjectKind.Constraint || collectionHeirarchyKind.Peek() == AbstractObjectKind.Delegate)
                                {
                                    DeepestNode = DeepestNode.Parent;
                                    collectionHeirarchy.Pop();
                                    collectionHeirarchyKind.Pop();
                                }
                                else
                                {
                                    DeepestNode.Children.Add(new AbstractSyntaxCollection()
                                    {
                                        Parent = DeepestNode,
                                        Name = "EndStatement",
                                        ObjectKind = AbstractObjectKind.EndStatement
                                    });
                                }
                            }
                            #endregion
                            #region String
                            else if (tag.StartsWith("String "))
                            {
                                DeepestNode.Children.Add(new AbstractSyntaxCollection()
                                {
                                    ObjectKind = AbstractObjectKind.String,
                                    Name = tag.Replace("String ", string.Empty),
                                    Parent = DeepestNode
                                });
                            }
                            #endregion
                            #region Integer Constant
                            else if (tag.StartsWith("Integer "))
                            {
                                DeepestNode.Children.Add(new AbstractSyntaxCollection()
                                {
                                    ObjectKind = AbstractObjectKind.Integer,
                                    Name = tag.Replace("Integer ", string.Empty),
                                    Parent = DeepestNode
                                });
                            }
                            #endregion
                            #region Attribute
                            else if (tag.StartsWith("Attribute "))
                            {
                                DeepestNode.Children.Add(new AbstractSyntaxCollection()
                                {
                                    Parent = DeepestNode,
                                    Name = tag.Replace("Attribute ", string.Empty),
                                    ObjectKind = AbstractObjectKind.Attribute
                                });
                                PreviousNode = DeepestNode.Children.Last();
                            }
                            #endregion
                            #region Keyword
                            else if (tag.StartsWith("Keyword "))
                            {
                                DeepestNode.Children.Add(new AbstractSyntaxCollection()
                                {
                                    Parent = DeepestNode,
                                    Name = tag.Replace("Keyword ", string.Empty),
                                    ObjectKind = AbstractObjectKind.Keyword
                                });
                                PreviousNode = DeepestNode.Children.Last();
                            }
                            #endregion
                        }
                        #endregion
                        #region OpeningTag
                        else if (tag.StartsWith("OpeningTag"))
                        {

                        }
                        #endregion
                    }
                }

                currentChar = scanner.GetNextCharacter();
            }
            syntaxTree.Nodes.Add(DeepestNode);  //TODO we might want to check to make sure that the heirarchy is empty to make sure the global root is set
            return SimplifyTree(syntaxTree);
        }
        #endregion

        #region Pretty Printer
        public string PrettyPrint(AbstractSyntaxTree tree)
        {
            for (int i = 0; i < tree.Nodes.Count; i++)
            {
                RecursePrint(tree.Nodes[i]);
            }
            return builder.ToString();
        }
        StringBuilder builder = new StringBuilder();
        int depth = 0;
        private void RecursePrint(AbstractSyntaxCollection collection)
        {
            string tabs = "";
            depth++;
            for (int d = 0; d < depth - 1; d++)
            {
                tabs += "\t";
            }

            for (int i = 0; i < collection.Children.Count; i++)
            {
                builder.Append(tabs + collection.Children[i].ObjectKind + " : " + collection.Children[i].Name);
                for (int x = 0; x < collection.Children[i].SyntaxObjects.Count; x++)
                {
                    builder.Append(", " + collection.Children[i].SyntaxObjects[x].instruction + " : " + collection.Children[i].SyntaxObjects[x].Value);
                }
                builder.AppendLine();
                RecursePrint(collection.Children[i]);
            }
            depth--;
        }
        #endregion

        private AbstractSyntaxTree SimplifyTree(AbstractSyntaxTree tree)
        {
            for (int i = 0; i < tree.Nodes.Count; i++)
            {
                tree.Nodes[i] = CorrectTree(tree.Nodes[i]);
            }
            return tree;
        }

        private AbstractSyntaxCollection CorrectTree(AbstractSyntaxCollection collection)
        {

            if (collection.Children.Count > 0)
            {
                //TODO Parse variables and separate them from function calls

                #region Attribute Folding
                if (collection.ObjectKind == AbstractObjectKind.Function || collection.ObjectKind == AbstractObjectKind.Class)   //Correct statements which should only be in a function
                {
                    //Folds attributes into the owner's metadata
                    int index = collection.Parent.Children.IndexOf(collection);
                    if (index >= 1 && collection.Parent.Children[index - 1].ObjectKind == AbstractObjectKind.Attribute)
                    {
                        while (collection.Parent.Children[index - 1].ObjectKind == AbstractObjectKind.Attribute)
                        {
                            collection.SyntaxObjects.Add(new SyntaxObject()
                            {
                                instruction = Instruction.Attribute,
                                Value = collection.Parent.Children[index - 1].Name
                            });
                            collection.Parent.Children.RemoveAt(index - 1);
                            index--;
                        }
                    }
                }
                #endregion
                #region Enum Member IDs
                else if (collection.ObjectKind == AbstractObjectKind.Enum)
                {
                    //Parses the IDs of the enum members and sets them as SyntaxObjectMetadata
                }
                #endregion
            }
            else
            {
                if (collection.ObjectKind == AbstractObjectKind.Variable)   //Correct statements which should only be in a function
                {
                    #region Type Folding
                    //Folds attributes into the owner's metadata
                    int index = collection.Parent.Children.IndexOf(collection);
                    if (index >= 1 && (collection.Parent.Children[index - 1].ObjectKind == AbstractObjectKind.Type || collection.Parent.Children[index - 1].ObjectKind == AbstractObjectKind.Variable))
                    {
                        collection.Parent.Children[index - 1].Parent = collection;
                        collection.Parent.Children[index - 1].ObjectKind = AbstractObjectKind.Type; //Everything here is definitely a type
                        collection.Children.Add(collection.Parent.Children[index - 1]);
                        collection.Parent.Children.RemoveAt(index - 1);
                    }
                    #endregion

                    index = collection.Parent.Children.IndexOf(collection);
                    if (index >= 0 && index + 1 < collection.Parent.Children.Count && collection.Parent.ObjectKind != AbstractObjectKind.Variable && collection.Parent.Children[index + 1].ObjectKind == AbstractObjectKind.Operator && collection.Parent.Children[index + 1].Name == "=")
                    {
                        while (index + 1 < collection.Parent.Children.Count && collection.Parent.Children[index + 1].ObjectKind != AbstractObjectKind.EndStatement)
                        {
                            collection.Parent.Children[index + 1].Parent = collection;
                            collection.Children.Add(collection.Parent.Children[index + 1]);
                            collection.Parent.Children.RemoveAt(index + 1);
                        }
                    }
                }
            }

            #region Recursor
            for (int i = 0; i < collection.Children.Count; i++)
            {
                var tmp = CorrectTree(collection.Children[i]);   //Correct all subnodes first
            }
            #endregion

            return collection;
        }
    }
}
