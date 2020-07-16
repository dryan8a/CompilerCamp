using System;
using System.Collections.Generic;
using System.Linq;
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
        public ParseTreeNodeType NodeType;
        public SyntaxUnit Unit;
        public string Lexeme;
        public TokenTypes TokenType;

        
        public ParseTreeNode(ParseTreeNodeType nodeType)
        {
            NodeType = nodeType;
        }

        public bool SetValue(SyntaxUnit unit)
        {
            if (NodeType != ParseTreeNodeType.SyntaxUnit) return false;
            Unit = unit;
            return true;
        }
        public bool SetValue(string lexeme)
        {
            if (NodeType != ParseTreeNodeType.Lexeme) return false;
            Lexeme = lexeme;
            return true;
        }
        public bool SetValue(TokenTypes tokenType)
        {
            if (NodeType != ParseTreeNodeType.TokenType) return false;
            TokenType = tokenType;
            return true;
        }

    }
    public enum ParseTreeNodeType
    {
        SyntaxUnit,
        TokenType,
        Lexeme,
    }

    public enum SyntaxUnit
    {
        CompilationUnit,
        Expression,
        EqualsValueClause,
        Value,
        StringLiteral,
        CharLiteral,
        AddExpression,
        SubtractExpression,
        MultiplyExpression,
        DivideExpression,
        ParethesisBoundMathExpression,
        NegativeExpression
        
    }

    public class MathProduction
    {
        public static bool TryParse(List<Token> tokenStream, out ParseTreeNode node)
        {
            var token = tokenStream.FirstOrDefault(a => a.TokenType == TokenTypes.BinaryMathOperand);
            int tokenIndex = tokenStream.IndexOf(token);
            if (token != null && (token.Lexeme == "+" || token.Lexeme == "-"))
            {
                ParseTreeNode leftExpression;
                if (TryParse(tokenStream.GetRange(0, tokenIndex), out leftExpression))
                {
                    ParseTreeNode rightExpression;
                    if(TryParse(tokenStream.GetRange(tokenIndex + 1, tokenStream.Count - tokenIndex + 2), out rightExpression))
                    {
                        node = new ParseTreeNode(ParseTreeNodeType.SyntaxUnit);
                        if (token.Lexeme == "+") node.Unit = SyntaxUnit.AddExpression;
                        else node.Unit = SyntaxUnit.SubtractExpression;
                        node.Children.Add(leftExpression);
                        node.Children.Add(rightExpression);
                        return true;
                    }
                }
            }
            if (token != null && (token.Lexeme == "*"|| token.Lexeme == "/" || token.Lexeme == "%"))
            {
                ParseTreeNode leftExpression;
                if (TryParse(tokenStream.GetRange(0, tokenIndex), out leftExpression))
                {
                    ParseTreeNode rightExpression;
                    if (TryParse(tokenStream.GetRange(tokenIndex + 1, tokenStream.Count - tokenIndex + 2), out rightExpression))
                    {
                        node = new ParseTreeNode(ParseTreeNodeType.SyntaxUnit) { Unit = SyntaxUnit.AddExpression };
                        node.Children.Add(leftExpression);
                        node.Children.Add(rightExpression);
                        return true;
                    }
                }
            }
            var openParenToken = tokenStream.FirstOrDefault(a => a.TokenType == TokenTypes.OpenParenthesis);
            var closeParenToken = tokenStream.FirstOrDefault(a => a.TokenType == TokenTypes.CloseParenthesis);
            int openParenIndex = tokenStream.IndexOf(openParenToken);
            int closeParenIndex = tokenStream.IndexOf(closeParenToken);
            if (openParenToken != null & closeParenToken != null && openParenIndex < closeParenIndex)
            {
                ParseTreeNode expression;
                if(TryParse(tokenStream.GetRange(openParenIndex+1,closeParenIndex-openParenIndex+2), out expression))
                {
                    node = new ParseTreeNode(ParseTreeNodeType.SyntaxUnit) { Unit = SyntaxUnit.ParethesisBoundMathExpression };
                    node.Children.Add(expression);
                }
            }
            node = default;
            return false;
        }
    }




    public class EqualsValueClauseProduction
    {
        public static bool TryParse(ParseTreeNode parent, List<Token> tokenStream)
        {
            throw new NotImplementedException();
        }
    }
    public class ValueProduction
    {
        public static bool TryParse(ParseTreeNode parent, List<Token> tokenStream)
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