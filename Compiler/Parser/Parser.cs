using System;
using System.Collections.Generic;
using Tokenizer;

namespace Parser
{
    public static class Parser
    {


    }

    public class ParseTreeNode
    {
        public ParseTreeNode Parent;
        public List<ParseTreeNode> Children;
        public SyntaxUnit Unit;
        
        public ParseTreeNode(SyntaxUnit unit)
        {
            Unit = unit;
        }
    }

    public enum SyntaxUnit
    {
        CompilationUnit,
        Expression,
        EqualsValueClause,
        Value,
        StringLiteral,
        CharLiteral,
        NonNewlineWhiteSpace
    }

    public interface IProduction
    {
        static bool TryParse(ParseTreeNode parent, List<Token> tokenStream) { return true; }
    }
    public class NonNewlineWhiteSpace : IProduction
    {
        public static bool TryParse(ParseTreeNode parent, List<Token> tokenStream)
        {
            if(tokenStream[0].TokenType == TokenTypes.WhiteSpace && tokenStream[0].Lexeme != "\n")
            {
                return true;
            }
            return false;
        }
        public static void RemoveLeadingWhiteSpace(List<Token> tokenStream)
        {
            int spaceCount = 0;
            while(true)
            {
                if(TryParse(null, tokenStream))
                {
                    spaceCount++;
                    continue;
                }
                tokenStream.RemoveRange(0, spaceCount);
                return;
            }
        }
    }
    public class EqualsValueClause : IProduction
    {
        public static bool TryParse(ParseTreeNode parent, List<Token> tokenStream)
        {
            NonNewlineWhiteSpace.RemoveLeadingWhiteSpace(tokenStream);
            //if(tokenStream[0] == )
        }
    }
    
    public class StringLiteral : IProduction
    {
        public bool TryParse(ParseTreeNode parent, List<Token> tokenStream)
        {
            throw new NotImplementedException();
        }
    }
    //public class CompilationUnit : IProduction
    //{
    //    public bool TryParse(ParseTreeNode parent)
    //    {

    //    }
    //}

    //public class Expression : IProduction
    //{
    //    public bool TryParse(ParseTreeNode parent)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}