using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using ValidatorNamespace;
using ParserNamespace;
using TokenizerNamespace;


namespace EmitterNamespace
{
    public static class Emitter
    {
        public static void EmitIL(ParseTreeNode compilationUnit, SymbolsTreeNode head, string programName)
        {
            var assemblyName = new AssemblyName(programName);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            var namespaceNode = compilationUnit.Children[0];
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(namespaceNode.Children[0].Token.Lexeme, $"{programName}.exe");
            foreach(var type in namespaceNode.Children.GetRange(1,namespaceNode.Children.Count-1))
            {
                string typeName = type.Children.First(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.Identifier).Token.Lexeme;
                var accessModifier = type.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.AccessModifier);
                TypeAttributes typeAttributes = accessModifier != null && accessModifier.Token.Lexeme == "[public]" ? TypeAttributes.Class | TypeAttributes.Public : TypeAttributes.Class | TypeAttributes.NotPublic;
                var typeBuilder = moduleBuilder.DefineType(typeName, typeAttributes);
                foreach(var member in type.Children.First(a => a.Unit == SyntaxUnit.Body).Children)
                {
                    if(member.Unit == SyntaxUnit.MethodDeclaration)
                    {
                        int TypeIndex = 0;
                        bool isEntryPoint = false;
                        if(member.Children.Exists(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.EntryPointMarker))
                        {
                            isEntryPoint = true;
                            TypeIndex++;
                        }
                        accessModifier = member.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.AccessModifier);
                        if(accessModifier != null) TypeIndex++;
                        MethodAttributes methodAttributes = accessModifier != null && accessModifier.Token.Lexeme == "[public]" ? MethodAttributes.Public : MethodAttributes.Private;
                        Type methodType = GetTypeFromString(member.Children[TypeIndex].Children[0].Token.Lexeme);
                        string methodName = member.Children[TypeIndex + 1].Token.Lexeme;
                        Type[] paramTypes = new Type[member.Children[TypeIndex + 2].Children.Count];
                        int index = 0;
                        foreach(var param in member.Children[TypeIndex + 2].Children)
                        {
                            paramTypes[index] = GetTypeFromString(param.Children[0].Children[0].Token.Lexeme);
                            index++;
                        }
                        //Add method body to queue for ILGen
                        var methodBuilder = typeBuilder.DefineMethod(methodName, methodAttributes, methodType, paramTypes);
                        if(isEntryPoint) assemblyBuilder.SetEntryPoint(methodBuilder);
                        var ilGen = methodBuilder.GetILGenerator();
                        ilGen.Emit(OpCodes.Ret);
                    }
                    if(member.Unit == SyntaxUnit.VariableDeclaration)
                    {
                        int TypeIndex = 0;
                        accessModifier = member.Children.FirstOrDefault(a => a.Unit == SyntaxUnit.Token && a.Token.TokenType == TokenTypes.AccessModifier);
                        if (accessModifier != null) TypeIndex++;
                        FieldAttributes fieldAttributes = accessModifier != null && accessModifier.Token.Lexeme == "[public]" ? FieldAttributes.Public : FieldAttributes.Private;
                        Type fieldType = GetTypeFromString(member.Children[TypeIndex].Children[0].Token.Lexeme);
                        string fieldName = member.Children[TypeIndex + 1].Token.Lexeme;
                        var fieldBuilder = typeBuilder.DefineField(fieldName, fieldType, fieldAttributes);
                    }

                }
                typeBuilder.CreateType();
            }
            
            assemblyBuilder.Save($"{programName}.exe");
        }

        public static Type GetTypeFromString(string type)
        {
            switch(type)
            {
                case "int":
                    return typeof(int);
                case "string":
                    return typeof(string);
                case "bool":
                    return typeof(bool);
                case "char":
                    return typeof(char);
                case "void":
                    return typeof(void);
                default:
                    return null;
            }
        }
    }
}
