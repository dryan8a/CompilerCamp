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

                //loop through the class body and add methods and other members, add all ilGens to a Queue so that we can loop through and generate the IL
                var fieldBuilder = typeBuilder.DefineField("PrintString", typeof(string), FieldAttributes.Public);
                var methodBuilder = typeBuilder.DefineMethod("Main", MethodAttributes.Static | MethodAttributes.Public);
                assemblyBuilder.SetEntryPoint(methodBuilder);
                var ilGen = methodBuilder.GetILGenerator();



                typeBuilder.CreateType();
            }
            


            
            assemblyBuilder.Save($"{programName}.exe");
        }
    }
}
