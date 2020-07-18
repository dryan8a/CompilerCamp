using System;
using System.IO;
using TokenizerNamespace;
using ParserNamespace;
using System.Collections.Generic;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string program = File.ReadAllText("Example.dyl");
            var tokens = Tokenizer.Tokenize(program);
            ParseTreeNode head;
            bool didSucceed = Parser.CompilationUnitProductionParse(tokens.ToArray().AsSpan(), out head);
            
        }
    }
}
