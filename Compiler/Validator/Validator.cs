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
                if (classSymbols.Contains(className)) throw new Exception($"{className} already exists");
                bool classIsPublic = classNode.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.AccessModifier && a.Token.Lexeme == "[public]") != null;
                classSymbols.Data.Add((new Object(className,"new",classIsPublic),new SymbolsTreeNode()));
                validTypes.Add(className);
                classSymbols.Data[^1].Children.Data = new List<(Object,SymbolsTreeNode)>();
                foreach(var expression in classNode.Children)
                {
                    if (expression.Unit == SyntaxUnit.MethodDeclaration)
                    {
                        string name = expression.Children.First(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier).Token.Lexeme;
                        if (classSymbols.Data[^1].Children.Contains(name)) throw new Exception($"{name} already exists");
                        string type = expression.Children.First(a => a.Unit == SyntaxUnit.ReturnType).Children[0].Token.Lexeme;
                        bool isEntryPoint = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.EntryPointMarker) != null;
                        bool isPublic = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.AccessModifier && a.Token.Lexeme == "[public]") != null;
                        var parametersNodes = expression.Children.First(a => a.Unit == SyntaxUnit.ParameterList).Children;
                        var parameters = new Object[parametersNodes.Count];
                        for (int i = 0; i < parametersNodes.Count; i++)
                        {
                            var paramName = parametersNodes[i].Children[1].Token.Lexeme;
                            if (classSymbols.Data[^1].Children.Contains(paramName)) throw new Exception($"{name} already exists");
                            parameters[i] = new Object(paramName,parametersNodes[i].Children[0].Children[0].Token.Lexeme,false);
                        }
                        classSymbols.Data[^1].Children.Data.Add((new MethodObject(name, type, isPublic, parameters, isEntryPoint),null));
                        toVisit.Enqueue((expression,classIndex));
                        continue;
                    }
                    if(expression.Unit == SyntaxUnit.VariableDeclaration)
                    {
                        string name = expression.Children.First(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier).Token.Lexeme;
                        if (classSymbols.Data[^1].Children.Contains(name)) throw new Exception($"{name} already exists");
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
                var didSucceed = CheckTypes(currentMethod, classIndexInNode, classSymbols);
                if (!didSucceed) return false;
            }


            return true;
        }

        public static bool CheckTypes(ParseTreeNode currentMethod, int classIndexInNode, SymbolsTreeNode classSymbols)
        {
            var scopeStack = new ScopeStack();
            scopeStack.Push(Scope.GetScope(classSymbols.GetRelativeData(classIndexInNode,currentMethod)));
            var didSucceed = CheckTypesInBody(currentMethod.Children.Find(a => a.Unit == SyntaxUnit.Body),currentMethod.Children.Find(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier).Token.Lexeme,scopeStack,classSymbols);
            if (!didSucceed) return false;
            return true;
        }
        private static bool CheckTypesInBody(ParseTreeNode body, string currentMethod, ScopeStack currentStack, SymbolsTreeNode symbols)
        {
            currentStack.Push(new Scope());
            foreach (var expression in body.Children)
            {
                if (expression.Unit == SyntaxUnit.VariableDeclaration || expression.Unit == SyntaxUnit.VariableInitialization)
                {
                    string name = expression.Children.First(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier).Token.Lexeme;
                    if (currentStack.ContainsKey(name)) throw new Exception($"Object {name} already exists in this scope");
                    string type = expression.Children.First(a => a.Unit == SyntaxUnit.VariableType).Children[0].Token.Lexeme;
                    if (!symbols.Contains(type) && !IsBuiltInType(type)) throw new Exception($"{type} is not a valid type");
                    bool isPublic = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.AccessModifier && a.Token.Lexeme == "[public]") != null;
                    var newVar = new Object(name, type, isPublic);
                    currentStack.AddToScope(newVar.Name, newVar);
                }
                if(expression.Unit == SyntaxUnit.ReturnStatement)
                {
                    var methodObject = currentStack.GetObject(currentMethod);
                    if(expression.Children.Count == 0)
                    {
                        if (methodObject.Type == "void") continue;
                        throw new Exception($"Must return object of type {methodObject.Type}");
                    }
                    if(!MatchesType(expression.Children[0],methodObject.Type,currentStack)) throw new Exception($"Must return object of type {methodObject.Type}");
                }
                if (expression.Unit == SyntaxUnit.IfStatement || expression.Unit == SyntaxUnit.WhileLoop)
                {
                    if (!CheckTypesInBody(expression, currentMethod, currentStack,symbols)) return false;
                    var boolValue = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.BoolValue);
                    if (boolValue != null)
                    {
                        foreach (var access in FindMemberAccess(boolValue))
                        {
                            if (!MatchesType(access, "bool", currentStack)) throw new Exception($"{GetFullName(access)} is not correct type");
                        }
                        continue;
                    }
                    boolValue = expression.Children.LastOrDefault(a => a.Unit == SyntaxUnit.MethodCall || a.Unit == SyntaxUnit.MemberAccess || a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier);
                    if (boolValue != null)
                    {
                        if (!MatchesType(boolValue, "bool", currentStack)) throw new Exception($"{GetFullName(boolValue)} is not of type bool");
                        continue;
                    }
                    throw new Exception("Condition in if statement is not of type bool");
                }
                if (expression.Unit == SyntaxUnit.VariableInitialization || expression.Unit == SyntaxUnit.VariableAssignment)
                {
                    string name = "";
                    var variable = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.MemberAccess || (a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier));
                    if(variable == null)
                    {
                        variable = expression.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.MemberAccess);
                        if (variable == null) throw new Exception("something went wrong with the parser");
                    }
                    name = GetFullName(variable);

                    if (!currentStack.ContainsKey(name)) throw new Exception($"Object {name} does not exist in this scope");
                    string variableType = currentStack.GetObject(name).Type;
                    var potentialChildren = expression.Children.GetRange(expression.Children.IndexOf(variable) + 1, expression.Children.Count - expression.Children.IndexOf(variable) - 1);
                    var potentialValue = potentialChildren.LastOrDefault(a => a.Unit == SyntaxUnit.MethodCall || a.Unit == SyntaxUnit.MemberAccess || a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier);
                    if (potentialValue != null)
                    {
                        if (!MatchesType(potentialValue, variableType, currentStack)) throw new Exception($"{GetFullName(potentialValue)} is not of type {variableType}");
                        continue;
                    }
                    potentialValue = potentialChildren.LastOrDefault(a => a.Unit == SyntaxUnit.IntValue);
                    if (variableType == "int" && potentialValue != null)
                    {
                        foreach(var access in FindMemberAccess(potentialValue))
                        {
                            if (!MatchesType(access, variableType,currentStack)) throw new Exception($"{GetFullName(access)} is not of type int");
                        }
                        continue;
                    }
                    if (variableType == "char" && potentialChildren.LastOrDefault(a => a.Unit == SyntaxUnit.CharValue) != null)
                    {
                        continue;
                    }
                    if (variableType == "string" && potentialChildren.LastOrDefault(a => a.Unit == SyntaxUnit.StringValue) != null)
                    {
                        continue;
                    }
                    if(potentialChildren.LastOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Null) != null)
                    {
                        continue;
                    }
                    potentialValue = potentialChildren.LastOrDefault(a => a.Unit == SyntaxUnit.BoolValue);
                    if (variableType == "bool" && potentialValue != null)
                    {
                        foreach (var access in FindMemberAccess(potentialValue))
                        {
                            if (!MatchesType(access, variableType ,currentStack)) throw new Exception($"{GetFullName(access)} is not of type bool");
                        }
                        continue;
                    }
                    potentialValue = potentialChildren.LastOrDefault(a => a.Unit == SyntaxUnit.NewObject);
                    if(potentialValue != null)
                    {
                        var newType = potentialValue.Children[0].Token.Lexeme;
                        if (newType != variableType) throw new Exception($"Cannot implicitely cast {newType} to {variableType}");

                        foreach(var symbol in symbols.Data.First(a => a.Object.Name == newType).Children.Data)
                        {
                            if (!symbol.Object.IsPublic) continue;
                            Object tempObject;
                            if(symbol.Object.GetType() == typeof(MethodObject))
                            {
                                tempObject = ((MethodObject)symbol.Object).GetMethodCopy();
                            }
                            else
                            {
                                tempObject = symbol.Object.GetCopy();
                            }
                            tempObject.Name = name + "." + tempObject.Name;
                            currentStack.AddToScope(tempObject.Name,tempObject);
                        }

                        continue;
                    }
                    throw new Exception("Invalid set statement");
                }
                if(ParseTreeNode.IsMathEquals(expression.Unit))
                {
                    var variable = expression.Children[0];
                    var type = currentStack.GetObject(GetFullName(variable)).Type;
                    if (type != "int") throw new Exception($"{variable} is not of type int");
                    var value = expression.Children[1];
                    if(value.Unit == SyntaxUnit.IntValue)
                    {
                        foreach (var access in FindMemberAccess(value))
                        {
                            if (!MatchesType(access, "int", currentStack)) throw new Exception($"{GetFullName(access)} is not of type int");
                        }
                        continue;
                    }
                    if(value.Unit == SyntaxUnit.Token && value.Token.TokenType == TokenTypes.IntLiteral)
                    {
                        continue;
                    }
                    if (!MatchesType(value, "int", currentStack)) throw new Exception($"{GetFullName(value)} is not of type int");
                }
                if (expression.Unit == SyntaxUnit.Increment || expression.Unit == SyntaxUnit.Decrement)
                {
                    var variable = expression.Children[0];
                    var type = currentStack.GetObject(GetFullName(variable)).Type;
                    if (type != "int") throw new Exception($"{GetFullName(variable)} is not of type int");
                }
            }
            currentStack.Pop();
            return true;
        }

        private static bool IsBuiltInType(string type)
        {
            return type == "int" || type == "char" || type == "string" || type == "bool";
        }
        public static string GetFullName(ParseTreeNode memberAccess)
        {
            if (memberAccess == null) return "";
            
            if(memberAccess.Unit == SyntaxUnit.MethodCall)
            {
                return memberAccess.Children.First(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier).Token.Lexeme;
            }
            if(memberAccess.Unit == SyntaxUnit.Token && memberAccess.Token.TokenType == TokenTypes.Identifier)
            {
                return memberAccess.Token.Lexeme;
            }
            if(memberAccess.Unit == SyntaxUnit.MemberAccess)
            {
                string fullName = GetFullName(memberAccess.Children[0]);
                if(memberAccess.Children.Count == 2)
                {
                    fullName += "." + GetFullName(memberAccess.Children[1]);
                }
                return fullName;
            }
            throw new Exception("GetFullName be broken yall");
        }
        private static List<ParseTreeNode> FindMemberAccess(ParseTreeNode startNode)
        {
            if (startNode == null) return new List<ParseTreeNode>();
            var accesses = new List<ParseTreeNode>();
            if(startNode.Unit == SyntaxUnit.MemberAccess || startNode.Unit == SyntaxUnit.MethodCall || (startNode.Unit == SyntaxUnit.Token && startNode.Token.TokenType == TokenTypes.Identifier))
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
            if (value.Parent != null && ParseTreeNode.IsComparison(value.Parent.Unit)) desiredType = "int";
            string type = SyntaxUnitValueToString(value.Unit);
            if (type != "")
            {
                if(type != desiredType) return false;
                foreach (var access in FindMemberAccess(value))
                {
                    if (!MatchesType(access, type, scopeStack)) throw new Exception($"{GetFullName(access)} is not of type {type}");
                }
                return true;
            }
            string fullName = GetFullName(value);
            var desiredObject = scopeStack.GetObject(fullName);
            if (desiredObject.Type != desiredType) return false;
            if(desiredObject.GetType() == typeof(MethodObject))
            {
                var parameters = FindParameterList(value);
                if (parameters == null || parameters.Children.Count != ((MethodObject)desiredObject).Params.Length) throw new Exception("Invalid amount of parameters entered");
                int index = 0;
                foreach(var param in parameters.Children)
                {
                    type = SyntaxUnitValueToString(param.Unit);
                    if (type != "" && type != ((MethodObject)desiredObject).Params[index].Type) return false;
                    if (type != "") continue;
                    if (!MatchesType(param, ((MethodObject)desiredObject).Params[index].Type, scopeStack)) return false;
                    index++;
                }
            }
            return true;
        }
        private static ParseTreeNode FindParameterList(ParseTreeNode startNode)
        {
            if (startNode == null) return null;
            if(startNode.Unit == SyntaxUnit.MethodCall)
            {
                return startNode.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.ParameterList);
            }
            if(startNode.Unit == SyntaxUnit.MemberAccess)
            {
                return FindParameterList(startNode.Children[1]);
            }
            return null;
        }
        private static string SyntaxUnitValueToString(SyntaxUnit unit)
        {
            switch (unit)
            {
                case SyntaxUnit.IntValue:
                    return "int";
                    
                case SyntaxUnit.BoolValue:
                    return "bool";
                    
                case SyntaxUnit.CharValue:
                    return "char";
                    
                case SyntaxUnit.StringValue:
                    return "string";          
                default:
                    return "";
            }
        }
    }

    public class SymbolsTreeNode
    {
        public List<(Object Object, SymbolsTreeNode Children)> Data;

        public SymbolsTreeNode()
        {
            Data = new List<(Object Object,SymbolsTreeNode Children)>();
        }

        public bool Contains(string name)
        {
            foreach(var datum in Data)
            {
                if (datum.Object.Name == name) return true;
            }
            return false;
        }

        public List<Object> GetRelativeData(int classIndex, ParseTreeNode currentMethod)
        {
            var objects = new List<Object>();
            for (int i = 0; i < Data.Count; i++)
            {
                if(i == classIndex)
                {
                    foreach (var datum in Data[i].Children.Data)
                    {
                        if(datum.Object.Name == currentMethod.Children.First(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier).Token.Lexeme)
                        {
                            foreach(var param in ((MethodObject)datum.Object).Params)
                            {
                                objects.Add(param);
                            }
                        }
                        objects.Add(datum.Object);
                    }
                    continue;
                }
                //foreach(var datum in Data[i].Children.Data)
                //{
                //    var temp = datum.Object.GetCopy();
                //    if (!temp.IsPublic) continue;
                //    temp.Name = $"{Data[i].Object.Name}.{temp.Name}";
                //    objects.Add(temp);
                //}
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

        public Object GetCopy()
        {
            return new Object(Name, Type, IsPublic);
        }
    }

    public class MethodObject : Object
    {
        public bool IsEntryPoint;
        public Object[] Params;

        public MethodObject(string name, string type, bool isPublic)
        {
            Name = name;
            Type = type;
            IsPublic = isPublic;
            Params = new Object[0];
            IsEntryPoint = false;
        }
        public MethodObject(string name, string type, bool isPublic, Object[] parameters, bool isEntryPoint)
        {
            Name = name;
            Type = type;
            IsPublic = isPublic;
            Params = parameters;
            IsEntryPoint = isEntryPoint;
        }

        public MethodObject GetMethodCopy()
        {
            return new MethodObject(Name, Type, IsPublic, Params, IsEntryPoint);
        }
    }
}
