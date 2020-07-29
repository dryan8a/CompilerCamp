using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace EmitterNamespace
{
    /* Things to remember:
     * ILDASM
     * PEVERIFY
     */
    class EmitILTest
    {
        static void Main(string[] args)
        {
            var assemblyName = new AssemblyName("Assembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule", "HelloWorld.exe");
            var typeBuilder = moduleBuilder.DefineType("Program", TypeAttributes.Public);
            var fieldBuilder = typeBuilder.DefineField("PrintString", typeof(string), FieldAttributes.Public);
            var methodBuilder = typeBuilder.DefineMethod("Main", MethodAttributes.Static | MethodAttributes.Public);
            var ilGen = methodBuilder.GetILGenerator();

            ilGen.Emit(OpCodes.Ldstr, "Hello World");
            ilGen.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));
            ilGen.Emit(OpCodes.Ret);

            typeBuilder.CreateType();

            assemblyBuilder.SetEntryPoint(methodBuilder as MethodInfo);
            assemblyBuilder.Save("HelloWorld.exe");
        }
    }
}
