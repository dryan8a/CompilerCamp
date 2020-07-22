using System;
using System.Collections.Generic;
using ParserNamespace;
using TokenizerNamespace;
using System.Linq;

namespace ValidatorNamespace
{
    public static class Validator
    {
        public static bool Validate(ParseTreeNode head)
        {
            Scope scope = new Scope();
            var classNode = head.Children[0].Children[1];

            //Queue<ParseTreeNode> toVisit = new Queue<ParseTreeNode>();
            //List<ParseTreeNode> visited = new List<ParseTreeNode>();
            //toVisit.Enqueue(classNode);
            //ParseTreeNode tempNode;
            //while (toVisit.Count > 0)
            //{
            //    tempNode = toVisit.Dequeue();
            //    visited.Add(tempNode);
            //    foreach (var child in tempNode.Children)
            //    {
            //        if (!toVisit.Contains(child) && !visited.Contains(child))
            //        {
            //            toVisit.Enqueue(child);
            //        }
            //    }
            //}

            var validTypes = new List<string>
            {
                "void",
                "int",
                "string",
                "char",
                "bool"
            };
            var classScope = new ScopeStack();
            classScope.Push(new Scope());
            var toVisit = new Queue<(ParseTreeNode,ScopeStack)>();
            toVisit.Enqueue((classNode,classScope));
            while(toVisit.Count > 0)
            {
                var (node,scopeStack) = toVisit.Dequeue();
                foreach(var expression in node.Children)
                {
                    if (expression.Unit == SyntaxUnit.MethodDeclaration)
                    {
                        string name = expression.Children.First(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier).Token.Lexeme;
                        string type = expression.Children.First(a => a.Unit == SyntaxUnit.ReturnType).Children[0].Token.Lexeme;
                        bool isEntryPoint = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.EntryPointMarker) != null;
                        var parameters = expression.Children.First(a => a.Unit == SyntaxUnit.ParameterList).Children;
                        var paramTypes = new string[parameters.Count];
                        for (int i = 0; i < parameters.Count; i++)
                        {
                            paramTypes[i] = parameters[i].Children[0].Children[0].Token.Lexeme;
                        }
                        scopeStack.Peek(0).Add(name, new Object(name, type, paramTypes, isEntryPoint));
                        var newScopeStack = scopeStack.GetCopy();
                        newScopeStack.Push(new Scope());
                        toVisit.Enqueue((expression.Children.Find(a => a.Unit == SyntaxUnit.Body), newScopeStack));
                        continue;
                    }
                    if(expression.Unit == SyntaxUnit.VariableDeclaration || expression.Unit == SyntaxUnit.VariableInitialization)
                    {
                        //Add in variable declaration so that we can at least get the types of any variable or method
                    }
                }
            }
            return true;
        }
    }

    public class ScopeStack
    {
        List<Scope> Data;
        public ScopeStack()
        {
            Data = new List<Scope>();
        }
        private ScopeStack(List<Scope> data)
        {
            Data = data;
        }

        public void Push(Scope scope)
        {
            Data.Add(scope);
        }
        public Scope Pop()
        {
            var returnVal = Data[^1];
            Data.RemoveAt(Data.Count - 1);
            return returnVal;
        }
        public Scope Peek(int depth)
        {
            return Data[Data.Count - depth - 1];
        }
        public ScopeStack GetCopy()
        {
            return new ScopeStack(Data.GetRange(0,Data.Count));
        }
        public void AddToScope(int depth, string key, Object value)
        {
            Data[Data.Count - depth - 1].Add(key, value);
        }
    }

    public class Scope : Dictionary<string,Object>
    {
    }

    public class Object
    {
        public string Name;
        public string Type;
        public bool IsEntryPoint;
        public string[] ParamTypes;

        public Object(string name, string type)
        {
            Name = name;
            Type = type;
            ParamTypes = new string[0];
            IsEntryPoint = false;
        }
        public Object(string name, string type, string[] paramTypes, bool isEntryPoint)
        {
            Name = name;
            Type = type;
            ParamTypes = paramTypes;
            IsEntryPoint = isEntryPoint;
        }
    }
}
