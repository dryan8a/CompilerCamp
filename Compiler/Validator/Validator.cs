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
                var didSucceed = CheckTypes(currentMethod, classIndex, classSymbols);
                if (!didSucceed) return false;
            }


            return true;
        }

        public static bool CheckTypes(ParseTreeNode currentMethod, int classIndexInNode, SymbolsTreeNode classSymbols)
        {
            var scopeStack = new ScopeStack();
            scopeStack.Push(Scope.GetScope(classSymbols.GetRelativeData(classIndexInNode)));
            var didSucceed = CheckTypesInBody(currentMethod,scopeStack);
            if (!didSucceed) return false;
            return true;
        }
        private static bool CheckTypesInBody(ParseTreeNode body, ScopeStack currentStack)
        {
            currentStack.Push(new Scope());
            foreach (var expression in body.Children)
            {
                if (expression.Unit == SyntaxUnit.VariableDeclaration || expression.Unit == SyntaxUnit.VariableInitialization)
                {
                    string name = expression.Children.First(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier).Token.Lexeme;
                    if (currentStack.ContainsKey(name)) throw new Exception($"Object {name} already exists in this scope");
                    string type = expression.Children.First(a => a.Unit == SyntaxUnit.VariableType).Children[0].Token.Lexeme;
                    if (!type.Contains(type)) throw new Exception($"{type} is not a valid type");
                    bool isPublic = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.AccessModifier && a.Token.Lexeme == "[public]") != null;
                    var newVar = new Object(name, type, isPublic);
                    currentStack.AddToScope(newVar.Name, newVar);
                }
                if(expression.Unit == SyntaxUnit.IfStatement)
                {
                    //I'll write this after I fill in the functions and make sure that variable setting
                    //var boolValue = expression.Children.First(a => a.Unit == SyntaxUnit.BoolValue);
                    if (!CheckTypesInBody(expression.Children.First(a => a.Unit == SyntaxUnit.Body), currentStack)) return false;
                }
                if (expression.Unit == SyntaxUnit.VariableInitialization || expression.Unit == SyntaxUnit.VariableAssignment)
                {
                    string name = "";
                    var variable = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier);
                    if(variable == null)
                    {
                        variable = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.MemberAccess);
                        if (variable == null) throw new Exception("something went wrong with the parser");
                    }
                    name = GetFullName(variable);

                    if (!currentStack.ContainsKey(name)) throw new Exception($"Object {name} does not exist in this scope");
                    string variableType = currentStack.GetObject(name).Type;

                    var potentialValue = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.IntValue);
                    if (variableType == "int" && potentialValue != null)
                    {
                        foreach(var access in FindMemberAccess(potentialValue))
                        {
                            if (!MatchesType(access, variableType,currentStack)) throw new Exception($"{GetFullName(access)} is not of type int");
                        }
                        continue;
                    }
                    if (variableType == "char" && expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.CharValue) != null)
                    {
                        continue;
                    }
                    if (variableType == "string" && expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.StringValue) != null)
                    {
                        continue;
                    }
                    if(expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Null) != null)
                    {
                        continue;
                    }
                    potentialValue = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.BoolValue);
                    if (variableType == "bool" && potentialValue != null)
                    {
                        foreach (var access in FindMemberAccess(potentialValue))
                        {
                            if (!MatchesType(access, variableType,currentStack)) throw new Exception($"{GetFullName(access)} is not of type bool");
                        }
                        continue;
                    }
                    potentialValue = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.MethodCall || a.Unit == SyntaxUnit.MemberAccess || a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.MemberAccess);
                    if(potentialValue != null)
                    {
                        if (!MatchesType(potentialValue, variableType,currentStack)) throw new Exception($"{GetFullName(potentialValue)} is not of type {variableType}");
                        continue;
                    }
                }
            }
            currentStack.Pop();
            return true;
        }

        //Write this function
        public static string GetFullName(ParseTreeNode memberAccess)
        {
            string name = "";
            return name;
        }
        private static List<ParseTreeNode> FindMemberAccess(ParseTreeNode startNode)
        {
            if (startNode == null) return new List<ParseTreeNode>();
            var accesses = new List<ParseTreeNode>();
            if(startNode.Unit == SyntaxUnit.MemberAccess || startNode.Unit == SyntaxUnit.MethodCall || (startNode.Unit == SyntaxUnit.Token && startNode.Token.TokenType == TokenTypes.MemberAccess))
            {
                accesses.Add(startNode);
                return accesses;
            }
            foreach(var child in startNode.Children)
            {
                accesses.AddRange(FindMemberAccess(child));
            }
            return accesses;
            
        }
        private static bool MatchesType(ParseTreeNode value, string desiredType, ScopeStack scopeStack)
        {
            string fullName = GetFullName(value);
            var desiredObject = scopeStack.GetObject(fullName);
            if (desiredObject.Type != desiredType) return false;
            if(desiredObject.GetType() == typeof(MethodObject))
            {
                int index = 0;
                foreach(var param in value.Children.First(a => a.Unit == SyntaxUnit.ParameterList).Children)
                {
                    if (!MatchesType(param, ((MethodObject)desiredObject).ParamTypes[index], scopeStack)) return false;
                    index++;
                }
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
            var objects = new List<Object>();
            foreach(var datum in Data)
            {
                objects.Add(datum.Object);
            }
            for (int i = 0; i < Data.Count; i++)
            {
                if(i == classIndex)
                {
                    foreach (var datum in Data[i].Children.Data)
                    {
                        objects.Add(datum.Object);
                    }
                    continue;
                }
                foreach(var datum in Data[i].Children.Data)
                {
                    var temp = datum.Object;
                    if (!temp.IsPublic) continue;
                    temp.Name = $"{Data[i].Object.Name}.{temp.Name}";
                    objects.Add(temp);
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
        public void AddToScope(string key, Object value, int depth = 0)
        {
            Data[Data.Count - depth - 1].Add(key, value);
        }
        public bool ContainsKey(string key)
        {
            foreach(var scope in Data)
            {
                if (scope.ContainsKey(key)) return true;
            }
            return false;
        }
        public Object GetObject(string name)
        {
            foreach(var scope in Data)
            {
                if(scope.ContainsKey(name))
                {
                    return scope[name];
                }
            }
            return null;
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
