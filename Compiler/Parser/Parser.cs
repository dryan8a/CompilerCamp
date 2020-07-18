using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TokenizerNamespace;

namespace ParserNamespace
{
    public static class Parser
    {
        public static bool CompilationUnitProductionParse(List<Token> tokenStream, out ParseTreeNode node)
        {
            node = new ParseTreeNode(SyntaxUnit.CompilationUnit);
            if (tokenStream == null) return false;
            while (tokenStream.Count > 0)
            {
                if (!NamespaceProductionParse(ref tokenStream, out ParseTreeNode tempNode)) return false;
                node.Children.Add(tempNode);
            }
            return true;
        }
        public static bool NamespaceProductionParse(ref List<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream[0].TokenType != TokenTypes.Namespace || tokenStream[1].TokenType != TokenTypes.Identifier || tokenStream[2].TokenType != TokenTypes.OpenRegion) return false;
            int rightBracketIndex = FindCorrespondingRightBracket(tokenStream, 2);
            node = new ParseTreeNode(SyntaxUnit.NamespaceDeclaration);
            node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[1] });
            
            List<Token> namespaceTokens = tokenStream.GetRange(3, rightBracketIndex - 3 < 0 ? 0 : rightBracketIndex - 3);
            RemoveUsedTokens(ref tokenStream, rightBracketIndex);
            while (namespaceTokens.Count > 0)
            {
                if (!ClassProductionParse(ref namespaceTokens, out ParseTreeNode tempNode)) return false;
                node.Children.Add(tempNode);
            }

            return true;
        }
        public static bool ClassProductionParse(ref List<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream[0].TokenType != TokenTypes.Class) return false;
            node = new ParseTreeNode(SyntaxUnit.ClassDecleration);
            int LeftBracketIndex = 2;
            if(tokenStream[1].TokenType == TokenTypes.AccessModifier)
            {
                LeftBracketIndex = 3;
                node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[1] });
            }
            if (tokenStream[LeftBracketIndex - 1].TokenType != TokenTypes.Identifier || tokenStream[LeftBracketIndex].TokenType != TokenTypes.OpenRegion) return false;
            node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[LeftBracketIndex - 1] });
            int rightBracketIndex = FindCorrespondingRightBracket(tokenStream, LeftBracketIndex);

            List<Token> classTokens = tokenStream.GetRange(LeftBracketIndex + 1, rightBracketIndex - (LeftBracketIndex + 1) < 0 ? 0 : rightBracketIndex - (LeftBracketIndex + 1));
            RemoveUsedTokens(ref tokenStream, rightBracketIndex);
            while (classTokens.Count > 0)
            {
                if (!VariableDeclarationProductionParse(ref classTokens, out ParseTreeNode tempNode,false)) return false;
                node.Children.Add(tempNode);
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

        public static bool VariableDeclarationProductionParse(ref List<Token> tokenStream, out ParseTreeNode node, bool isInFunc)
        {
            node = default;
            if (tokenStream.Count < 4 || tokenStream[0].TokenType != TokenTypes.VariableInitialization) return false;
            node = new ParseTreeNode(SyntaxUnit.VariableDeclaration);
            int typeIndex = 1;
            if(tokenStream[1].TokenType == TokenTypes.AccessModifier && !isInFunc)
            {
                typeIndex = 2;
                node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[1] });
            }
            if (tokenStream[typeIndex].TokenType != TokenTypes.Type || tokenStream[typeIndex + 1].TokenType != TokenTypes.Identifier || tokenStream[typeIndex + 2].TokenType != TokenTypes.Semicolon) return false;
            node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[typeIndex] });
            node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[typeIndex+1] });
            RemoveUsedTokens(ref tokenStream, typeIndex + 2);
            return true;
        }

        public static void RemoveUsedTokens(ref List<Token> tokenStream, int lastIndex)
        {
            if (lastIndex == tokenStream.Count - 1) tokenStream = new List<Token>();
            else tokenStream = tokenStream.GetRange(lastIndex + 1, tokenStream.Count - (lastIndex + 1));
        }

        public static bool VariableAccessProductionParse(List<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream.Count == 0 || (tokenStream[0].TokenType != TokenTypes.Identifier && tokenStream[0].TokenType != TokenTypes.ThisKeyword)) return false;
            if(tokenStream.Count == 1)
            {
                node = new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] };
                return true;
            }
            if(tokenStream[1].TokenType == TokenTypes.MemberAccess)
            {
                if (!VariableAccessProductionParse(tokenStream.GetRange(2, tokenStream.Count - 2), out ParseTreeNode tempNode)) return false;
                node = new ParseTreeNode(SyntaxUnit.VariableAccess);
                node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] });
                node.Children.Add(tempNode);
                return true;
            }
            return false;
        }

        public static bool MathProductionParse(List<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream.Count == 0) return false;

            var token = FindBinaryOp(tokenStream, new string[] { "+", "-" });
            int tokenIndex = tokenStream.IndexOf(token);
            if (token != null && (token.Lexeme == "+" || token.Lexeme == "-"))
            {
                if (MathProductionParse(tokenStream.GetRange(0, tokenIndex), out ParseTreeNode leftExpression) && MathProductionParse(tokenStream.GetRange(tokenIndex + 1, tokenStream.Count - (tokenIndex + 1)), out ParseTreeNode rightExpression))
                {
                    node = new ParseTreeNode();
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
                if (MathProductionParse(tokenStream.GetRange(0, tokenIndex), out ParseTreeNode leftExpression) && MathProductionParse(tokenStream.GetRange(tokenIndex + 1, tokenStream.Count - (tokenIndex + 1)), out ParseTreeNode rightExpression))
                {
                    node = new ParseTreeNode();
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
                if (MathProductionParse(tokenStream.GetRange(1, tokenStream.Count - 2), out ParseTreeNode expression))
                {
                    node = new ParseTreeNode(SyntaxUnit.ParethesisBoundMathExpression);
                    node.Children.Add(expression);
                    return true;
                }
            }

            if (tokenStream[0].TokenType == TokenTypes.UnaryMathOperand && tokenStream[0].Lexeme == "(-)")
            {
                if (MathProductionPrimeParse(tokenStream.GetRange(1, tokenStream.Count - 1), out ParseTreeNode expression))
                {
                    node = new ParseTreeNode(SyntaxUnit.NegativeExpression);
                    node.Children.Add(expression);
                    return true;
                }
            }
            if (MathProductionPrimeParse(tokenStream, out ParseTreeNode expressionPrime))
            {
                node = expressionPrime;
                return true;
            }
            return false;
        }
        public static bool MathProductionPrimeParse(List<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream[0].TokenType == TokenTypes.IntLiteral)
            {
                if (tokenStream.Count != 1) return false;
                node = new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] };
                return true;
            }
            if (!VariableAccessProductionParse(tokenStream,out ParseTreeNode tempNode)) return false;
            node = new ParseTreeNode(SyntaxUnit.VariableAccess);
            node.Children.Add(tempNode);
            return true;
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