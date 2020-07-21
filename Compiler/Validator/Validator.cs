using System;
using System.Collections.Generic;
using ParserNamespace;
using TokenizerNamespace;

namespace ValidatorNamespace
{
    public static class Validator
    {
        public static bool Validate(ParseTreeNode head)
        {
            var classNode = head.Children[0].Children[1];
            foreach(var method in classNode.Children)
            {
                if (method.Unit != SyntaxUnit.MethodDeclaration) continue;
                foreach(var expression in method.Children.Find(a => a.Unit == SyntaxUnit.MethodBody).Children)
                {
                    //Expression go here
                }
            }
            return true;
        }
    }

    //public class Scope : Stack<Dictionary<>>
    //{

    //}
}
