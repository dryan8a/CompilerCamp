using System;
using System.Collections.Generic;
using System.Linq;
using TokenizerNamespace;

namespace ParserNamespace
{
    public static class Parser
    {
        public static bool CompilationUnitProductionParse(List<Token> tokenStream, out ParseTreeNode node)
        {
            node = new ParseTreeNode(SyntaxUnit.CompilationUnit);
            if (tokenStream == null) return false;
            ParseTreeNode tempNode;
            while (tokenStream.Count > 0)
            {
                if (!NamespaceProductionParse(tokenStream, out tempNode)) return false;
                node.Children.Add(tempNode);
            }
            return true;
        }
        public static bool NamespaceProductionParse(List<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream[0].TokenType != TokenTypes.Namespace || tokenStream[1].TokenType != TokenTypes.Identifier || tokenStream[2].TokenType != TokenTypes.OpenRegion) return false;
            int rightBracketIndex = FindCorrespondingRightBracket(tokenStream, 2);
            List<Token> namespaceTokens = tokenStream.GetRange(3, rightBracketIndex - 4);
            
            if (rightBracketIndex == tokenStream.Count - 1) tokenStream = new List<Token>();
            else tokenStream = tokenStream.GetRange(rightBracketIndex + 1, tokenStream.Count - (rightBracketIndex + 1));

            ParseTreeNode tempNode;
            while(namespaceTokens.Count > 0)
            {

            }

            return true;
        }
        public static int FindCorrespondingRightBracket(List<Token> tokenStream, int leftBracketIndex)
        {
            if (tokenStream.Count < 2) return -1;
            int bracketCount = 1;
            for(int i = leftBracketIndex + 1;i<tokenStream.Count;i++)
            {
                if (tokenStream[i].TokenType == TokenTypes.OpenRegion) bracketCount++;
                else if (tokenStream[i].TokenType == TokenTypes.CloseRegion) bracketCount--;
                if(bracketCount == 0)
                {
                    return i;
                }
            }
            return -1;
        }



        public static bool MathProductionParse(List<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream.Count == 0) return false;

            var token = FindBinaryOp(tokenStream, new string[] { "+", "-" });
            int tokenIndex = tokenStream.IndexOf(token);
            if (token != null && (token.Lexeme == "+" || token.Lexeme == "-"))
            {
                ParseTreeNode leftExpression;
                ParseTreeNode rightExpression;
                if (MathProductionParse(tokenStream.GetRange(0, tokenIndex), out leftExpression) && MathProductionParse(tokenStream.GetRange(tokenIndex + 1, tokenStream.Count - (tokenIndex + 1)), out rightExpression))
                {
                    node = new ParseTreeNode() { Children = new List<ParseTreeNode>() };
                    if (token.Lexeme == "+") node.Unit = SyntaxUnit.AddExpression;
                    else node.Unit = SyntaxUnit.SubtractExpression;
                    node.Children.Add(leftExpression);
                    node.Children.Add(rightExpression);
                    return true;
                }
            }

            token = FindBinaryOp(tokenStream, new string[] { "*", "/", "%" });
            tokenIndex = tokenStream.IndexOf(token);
            if (token != null)
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

            if (tokenStream[0].TokenType == TokenTypes.OpenParenthesis && tokenStream[tokenStream.Count - 1].TokenType == TokenTypes.CloseParenthesis)
            {
                ParseTreeNode expression;
                if (MathProductionParse(tokenStream.GetRange(1, tokenStream.Count - 2), out expression))
                {
                    node = new ParseTreeNode(SyntaxUnit.ParethesisBoundMathExpression) { Children = new List<ParseTreeNode>() };
                    node.Children.Add(expression);
                    return true;
                }
            }

            if (tokenStream[0].TokenType == TokenTypes.UnaryMathOperand && tokenStream[0].Lexeme == "(-)")
            {
                ParseTreeNode expression;
                if (MathProductionPrimeParse(tokenStream.GetRange(1, tokenStream.Count - 1), out expression))
                {
                    node = new ParseTreeNode(SyntaxUnit.NegativeExpression) { Children = new List<ParseTreeNode>() };
                    node.Children.Add(expression);
                    return true;
                }
            }
            ParseTreeNode expressionPrime;
            if (MathProductionPrimeParse(tokenStream, out expressionPrime))
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
            if (tokenStream[0].TokenType == TokenTypes.IntLiteral || tokenStream[0].TokenType == TokenTypes.Identifier)
            {
                node = new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] };
                return true;
            }
            return false;
        }
        private static Token FindBinaryOp(List<Token> tokenStream, string[] validLexemes)
        {
            int unmatchedLeftParens = 0;
            foreach (var token in tokenStream)
            {
                if (token.TokenType == TokenTypes.OpenParenthesis) unmatchedLeftParens++;
                if (token.TokenType == TokenTypes.CloseParenthesis) unmatchedLeftParens--;
                if (unmatchedLeftParens != 0) continue;
                if (token.TokenType != TokenTypes.BinaryMathOperand) continue;
                foreach (string lexeme in validLexemes)
                {
                    if (token.Lexeme == lexeme)
                    {
                        return token;
                    }
                }
            }
            return default;
        }
    
    }
}