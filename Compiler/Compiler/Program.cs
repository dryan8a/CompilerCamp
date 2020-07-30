using System;
using System.IO;
using TokenizerNamespace;
using ParserNamespace;
using ValidatorNamespace;
using EmitterNamespace;
using System.Collections.Generic;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string programName = "Example.dyl";
            string program = File.ReadAllText(programName);
            var tokens = Tokenizer.Tokenize(program.AsSpan());
            ParseTreeNode compilationUnit;
            if(!Parser.CompilationUnitProductionParse(tokens.ToArray().AsSpan(), out compilationUnit)) throw new Exception("Parser Fail");
            var symbolsTreeHead = Validator.Validate(compilationUnit);
            Emitter.EmitIL(compilationUnit, symbolsTreeHead, programName);
        }
    }
}
