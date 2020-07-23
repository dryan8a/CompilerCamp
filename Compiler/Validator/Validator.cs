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
            var namespaceNode = head.Children[0];

            var validTypes = new List<string>
            {
                "void",
                "int",
                "string",
                "char",
                "bool"
            };

            var toVisit = new Queue<(ParseTreeNode bodyNode,int classIndex)>();
            var classSymbols = new SymbolsTreeNode();
            int classIndex = 0;
            foreach(var classNode in namespaceNode.Children.FindAll(a => a.Unit == SyntaxUnit.ClassDecleration))
            {
                string className = classNode.Children.First(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier).Token.Lexeme;
                bool classIsPublic = classNode.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.AccessModifier && a.Token.Lexeme == "[public]") != null;
                classSymbols.Data.Add((new Object(className,"new",classIsPublic),new SymbolsTreeNode()));
                validTypes.Add(className);
                classSymbols.Data[^1].Children.Data = new List<(Object,SymbolsTreeNode)>();
                foreach(var expression in classNode.Children)
                {
                    if (expression.Unit == SyntaxUnit.MethodDeclaration)
                    {
                        string name = expression.Children.First(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier).Token.Lexeme;
                        string type = expression.Children.First(a => a.Unit == SyntaxUnit.ReturnType).Children[0].Token.Lexeme;
                        bool isEntryPoint = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.EntryPointMarker) != null;
                        bool isPublic = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.AccessModifier && a.Token.Lexeme == "[public]") != null;
                        var parameters = expression.Children.First(a => a.Unit == SyntaxUnit.ParameterList).Children;
                        var paramTypes = new string[parameters.Count];
                        for (int i = 0; i < parameters.Count; i++)
                        {
                            paramTypes[i] = parameters[i].Children[0].Children[0].Token.Lexeme;
                        }
                        classSymbols.Data[^1].Children.Data.Add((new MethodObject(name, type, isPublic, paramTypes, isEntryPoint),null));
                        toVisit.Enqueue((expression.Children.Find(a => a.Unit == SyntaxUnit.Body),classIndex));
                        continue;
                    }
                    if(expression.Unit == SyntaxUnit.VariableDeclaration)
                    {
                        string name = expression.Children.First(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier).Token.Lexeme;
                        string type = expression.Children.First(a => a.Unit == SyntaxUnit.VariableType).Children[0].Token.Lexeme;
                        bool isPublic = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.AccessModifier && a.Token.Lexeme == "[public]") != null;
                        classSymbols.Data[^1].Children.Data.Add((new Object(name, type, isPublic),null));
                        continue;
                    }
                }
                classIndex++;
            }

            while(toVisit.Count > 0)
            {
                (ParseTreeNode currentMethod, int classIndexInNode) = toVisit.Dequeue();
                var scopeStack = new ScopeStack();
                scopeStack.Push(Scope.GetScope(classSymbols.GetRelativeData(classIndexInNode)));
            }


            return true;
        }
    }

    public class SymbolsTreeNode
    {
        public List<(Object Object, SymbolsTreeNode Children)> Data;

        public SymbolsTreeNode()
        {
            Data = new List<(Object Object,SymbolsTreeNode Children)>();
        }

        public List<Object> GetRelativeData(int classIndex)
        {
            //change from list<object,children> to just list<object> by replacing Get and Add Range
            var objects = Data.GetRange(0, Data.Count);
            for (int i = 0; i < Data.Count; i++)
            {
                if(i == classIndex)
                {
                    objects.AddRange(Data[i].Children.Data);
                    continue;
                }
                foreach(var (currentObject,children) in Data[i].Children.Data)
                {

                }
                
            }
            return objects;
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
        public static Scope GetScope(List<Object> objects)
        {
            var scope = new Scope();
            foreach(var currentObject in objects)
            {
                scope.Add(currentObject.Name, currentObject);
            }
            return scope;
        }
    }

    public class Object
    {
        public string Name;
        public string Type;
        public bool IsPublic;

        public Object()
        {
            
        }
        public Object(string name, string type, bool isPublic)
        {
            Name = name;
            Type = type;
            IsPublic = isPublic;
        }
    }

    public class MethodObject : Object
    {
        public bool IsEntryPoint;
        public string[] ParamTypes;

        public MethodObject(string name, string type, bool isPublic)
        {
            Name = name;
            Type = type;
            IsPublic = isPublic;
            ParamTypes = new string[0];
            IsEntryPoint = false;
        }
        public MethodObject(string name, string type, bool isPublic, string[] paramTypes, bool isEntryPoint)
        {
            Name = name;
            Type = type;
            IsPublic = isPublic;
            ParamTypes = paramTypes;
            IsEntryPoint = isEntryPoint;
        }
    }
}
