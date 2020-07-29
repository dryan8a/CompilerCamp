using System;
using System.IO;
using TokenizerNamespace;
using ParserNamespace;
using ValidatorNamespace;
using System.Collections.Generic;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string program = File.ReadAllText("Example.dyl");
            var tokens = Tokenizer.Tokenize(program.AsSpan());
            ParseTreeNode head;
            if(!Parser.CompilationUnitProductionParse(tokens.ToArray().AsSpan(), out head)) throw new Exception("Parser Fail");
            var symbolsTreeHead = Validator.Validate(head);
            //Convert to IL here
        }
    }
}
