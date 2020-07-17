using System;
using System.Collections.Generic;
using System.Linq;
using TokenizerNamespace;

namespace ParserNamespace
{
    public static class Parser
    {
        public static bool MathProductionParse(List<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream == null || tokenStream.Count == 0) return false;
            
            var token = tokenStream.FirstOrDefault(a => a.TokenType == TokenTypes.BinaryMathOperand && (a.Lexeme == "+" || a.Lexeme == "-"));
            int tokenIndex = tokenStream.IndexOf(token);
            if (token != null && (token.Lexeme == "+" || token.Lexeme == "-"))
            {
                ParseTreeNode leftExpression;
                ParseTreeNode rightExpression;
                if (MathProductionParse(tokenStream.GetRange(0, tokenIndex), out leftExpression) && MathProductionParse(tokenStream.GetRange(tokenIndex + 1, tokenStream.Count - (tokenIndex + 1)), out rightExpression))
                {
                    node = new ParseTreeNode() {Children = new List<ParseTreeNode>() };
                    if (token.Lexeme == "+") node.Unit = SyntaxUnit.AddExpression;
                    else node.Unit = SyntaxUnit.SubtractExpression;
                    node.Children.Add(leftExpression);
                    node.Children.Add(rightExpression);
                    return true;
                }
            }

            token = tokenStream.FirstOrDefault(a => a.TokenType == TokenTypes.BinaryMathOperand);
            tokenIndex = tokenStream.IndexOf(token);
            if (token != null && (token.Lexeme == "*" || token.Lexeme == "/" || token.Lexeme == "%"))
            {
                ParseTreeNode leftExpression;
                ParseTreeNode rightExpression;
                if (MathProductionParse(tokenStream.GetRange(0, tokenIndex), out leftExpression) && MathProductionParse(tokenStream.GetRange(tokenIndex + 1, tokenStream.Count - (tokenIndex + 1)), out rightExpression))
                {                    
                    node = new ParseTreeNode() { Children = new List<ParseTreeNode>() };
                    if (token.Lexeme == "*") node.Unit = SyntaxUnit.MultiplyExpression;
                    else if (token.Lexeme == "/") node.Unit = SyntaxUnit.DivideExpression;
                    else node.Unit = SyntaxUnit.ModuloExpression;
                    node.Children.Add(leftExpression);
                    node.Children.Add(rightExpression);
                    return true;              
                }
            }

            if (tokenStream[0].TokenType == TokenTypes.OpenParenthesis)
            {
                var closeParenToken = tokenStream.FirstOrDefault(a => a.TokenType == TokenTypes.CloseParenthesis);
                int closeParenIndex = tokenStream.IndexOf(closeParenToken);
                if (closeParenToken != null)
                {
                    ParseTreeNode expression;
                    if (MathProductionParse(tokenStream.GetRange(1, closeParenIndex - 1), out expression))
                    {
                        node = new ParseTreeNode(SyntaxUnit.ParethesisBoundMathExpression) { Children = new List<ParseTreeNode>() };
                        node.Children.Add(expression);
                        return true;
                    }
                }
            }

            if (tokenStream[0].TokenType == TokenTypes.UnaryMathOperand && tokenStream[0].Lexeme == "(-)")
            {
                ParseTreeNode expression;
                if (MathProductionPrimeParse(tokenStream.GetRange(1,tokenStream.Count-1),out expression))
                {
                    node = new ParseTreeNode(SyntaxUnit.NegativeExpression) { Children = new List<ParseTreeNode>()};
                    node.Children.Add(expression);
                    return true;
                }
            }
            ParseTreeNode expressionPrime;
            if(MathProductionPrimeParse(tokenStream,out expressionPrime))
            {
                node = expressionPrime;
                return true;
            }
            return false;
        }
        public static bool MathProductionPrimeParse(List<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream.Count != 1) return false;
            if(tokenStream[0].TokenType == TokenTypes.IntLiteral || tokenStream[0].TokenType == TokenTypes.Identifier)
            {
                node = new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] };
                return true;
            }
            return false;
        }
    }

}