using System;
using System.Collections.Generic;
using TokenizerNamespace;

namespace ParserNamespace
{
    public static class Parser
    {
        public static bool CompilationUnitProductionParse(ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = new ParseTreeNode(SyntaxUnit.CompilationUnit);
            if (tokenStream == null) return false;
            while (tokenStream.Length > 0)
            {
                if (!NamespaceProductionParse(ref tokenStream, out ParseTreeNode tempNode)) return false;
                node.Children.Add(tempNode);
            }
            return true;
        }
        public static bool NamespaceProductionParse(ref ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream[0].TokenType != TokenTypes.Namespace || tokenStream[1].TokenType != TokenTypes.Identifier || tokenStream[2].TokenType != TokenTypes.OpenRegion) return false;
            int rightBracketIndex = FindCorrespondingRightBracket(tokenStream, 2,BracketTypes.Curly);
            if (rightBracketIndex < 0) return false;
            node = new ParseTreeNode(SyntaxUnit.NamespaceDeclaration);
            node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[1] });
            
            ReadOnlySpan<Token> namespaceTokens = tokenStream.Slice(3, rightBracketIndex - 3 < 0 ? 0 : rightBracketIndex - 3);
            RemoveUsedTokens(ref tokenStream, rightBracketIndex);
            while (namespaceTokens.Length > 0)
            {
                if (!ClassProductionParse(ref namespaceTokens, out ParseTreeNode tempNode)) return false;
                node.Children.Add(tempNode);
            }

            return true;
        }
        public static bool ClassProductionParse(ref ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
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
            int rightBracketIndex = FindCorrespondingRightBracket(tokenStream, LeftBracketIndex,BracketTypes.Curly);
            if (rightBracketIndex < 0) return false;

            ReadOnlySpan<Token> classTokens = tokenStream.Slice(LeftBracketIndex + 1, rightBracketIndex - (LeftBracketIndex + 1) < 0 ? 0 : rightBracketIndex - (LeftBracketIndex + 1));
            RemoveUsedTokens(ref tokenStream, rightBracketIndex);
            while (classTokens.Length > 0)
            {
                ParseTreeNode tempNode;
                if (!VariableDeclarationProductionParse(ref classTokens, out tempNode,false) && !MethodProductionParse(ref classTokens, out tempNode)) return false;
                node.Children.Add(tempNode);
            }

            return true;
        }
        public static bool MethodProductionParse(ref ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream[0].TokenType != TokenTypes.Function) return false;
            node = new ParseTreeNode(SyntaxUnit.MethodDeclaration);
            int TypeIndex = 1;
            if(tokenStream[TypeIndex].TokenType == TokenTypes.AccessModifier)
            {
                node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[TypeIndex] });
                TypeIndex++;
            }
            if (tokenStream[TypeIndex].TokenType == TokenTypes.EntryPointMarker)
            {
                node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[TypeIndex] });
                TypeIndex++;
            }
            if (!ReturnTypeProductionParse(tokenStream[TypeIndex], out ParseTreeNode TypeNode) || tokenStream[TypeIndex + 1].TokenType != TokenTypes.Identifier || tokenStream[TypeIndex + 2].TokenType != TokenTypes.OpenParenthesis) return false;
            node.Children.Add(TypeNode);
            node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[TypeIndex + 1] });
            int rightParenIndex = FindCorrespondingRightBracket(tokenStream, TypeIndex + 2, BracketTypes.Parenthesis);
            if (!ParameterListProductionParse(tokenStream.Slice(TypeIndex + 3, rightParenIndex - (TypeIndex + 3) < 0 ? 0 : rightParenIndex - (TypeIndex + 3)), out ParseTreeNode ParamsNode)) return false;
            node.Children.Add(ParamsNode);

            if (tokenStream[rightParenIndex + 1].TokenType != TokenTypes.OpenRegion) return false;
            int rightBracketIndex = FindCorrespondingRightBracket(tokenStream, rightParenIndex + 1, BracketTypes.Curly);
            if (rightBracketIndex < 0) return false;

            ReadOnlySpan<Token> methodTokens = tokenStream.Slice(rightParenIndex + 2, rightBracketIndex - (rightParenIndex + 2) < 0 ? 0 : rightBracketIndex - (rightParenIndex + 2));
            RemoveUsedTokens(ref tokenStream, rightBracketIndex);
            while (methodTokens.Length > 0)
            {
                //ParseTreeNode tempNode;
                //if () return false;
                //node.Children.Add(tempNode);
            }

            return true; 
        }

        public static int FindCorrespondingRightBracket(ReadOnlySpan<Token> tokenStream, int leftBracketIndex, BracketTypes bracketType)
        {
            if (tokenStream.Length < 2) return -1;
            int bracketCount = 1;
            for(int i = leftBracketIndex + 1;i<tokenStream.Length;i++)
            {
                switch(bracketType)
                {
                    case BracketTypes.Curly:
                        if (tokenStream[i].TokenType == TokenTypes.OpenRegion) bracketCount++;
                        else if (tokenStream[i].TokenType == TokenTypes.CloseRegion) bracketCount--;
                        break;
                    case BracketTypes.Parenthesis:
                        if (tokenStream[i].TokenType == TokenTypes.OpenParenthesis) bracketCount++;
                        else if (tokenStream[i].TokenType == TokenTypes.CloseParenthesis) bracketCount--;
                        break;
                    case BracketTypes.Square:
                        if (tokenStream[i].TokenType == TokenTypes.ArrayOpenBracket) bracketCount++;
                        else if (tokenStream[i].TokenType == TokenTypes.ArrayCloseBracket) bracketCount--;
                        break;
                }
                if(bracketCount == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public static bool VariableDeclarationProductionParse(ref ReadOnlySpan<Token> tokenStream, out ParseTreeNode node, bool isInFunc)
        {
            node = default;
            if (tokenStream.Length < 4 || tokenStream[0].TokenType != TokenTypes.VariableInitialization) return false;
            node = new ParseTreeNode(SyntaxUnit.VariableDeclaration);
            int typeIndex = 1;
            if(tokenStream[1].TokenType == TokenTypes.AccessModifier && !isInFunc)
            {
                typeIndex = 2;
                node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[1] });
            }
            ParseTreeNode typeNode;
            if (!VariableTypeProductionParse(tokenStream[typeIndex], out typeNode) || tokenStream[typeIndex + 1].TokenType != TokenTypes.Identifier || tokenStream[typeIndex + 2].TokenType != TokenTypes.Semicolon) return false;
            node.Children.Add(typeNode);
            node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[typeIndex+1] });
            RemoveUsedTokens(ref tokenStream, typeIndex + 2);
            return true;
        }

        public static bool ParameterListProductionParse(ReadOnlySpan<Token> tokenStream,out ParseTreeNode node)
        {
            node = new ParseTreeNode(SyntaxUnit.ParameterList);
            return true;
        }

        public static bool ParameterProductionParse(ref ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream.Length < 4 || tokenStream[0].TokenType != TokenTypes.VariableInitialization) return false;
            node = new ParseTreeNode(SyntaxUnit.Parameter);
            int typeIndex = 1;
            ParseTreeNode typeNode;
            if (!VariableTypeProductionParse(tokenStream[typeIndex], out typeNode) || tokenStream[typeIndex + 1].TokenType != TokenTypes.Identifier || (tokenStream[typeIndex + 2].TokenType != TokenTypes.Comma && tokenStream[typeIndex + 2].TokenType != TokenTypes.CloseParenthesis)) return false;
            node.Children.Add(typeNode);
            node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[typeIndex + 1] });
            RemoveUsedTokens(ref tokenStream, typeIndex + 2);
            return true;
        }

        public static bool ReturnTypeProductionParse(Token token, out ParseTreeNode node)
        {
            node = default;
            if (token.TokenType != TokenTypes.Type && token.TokenType != TokenTypes.Identifier) return false;
            node = new ParseTreeNode(SyntaxUnit.ReturnType);
            node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = token });
            return true;
        }

        public static bool VariableTypeProductionParse(Token token, out ParseTreeNode node)
        {
            node = default;
            if ((token.TokenType != TokenTypes.Type && token.TokenType != TokenTypes.Identifier) || token.Lexeme == "void") return false;
            node = new ParseTreeNode(SyntaxUnit.VariableType);
            node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = token });
            return true;
        }

        public static void RemoveUsedTokens(ref ReadOnlySpan<Token> tokenStream, int lastIndex)
        {
            if (lastIndex == tokenStream.Length - 1) tokenStream = new ReadOnlySpan<Token>();
            else tokenStream = tokenStream.Slice(lastIndex + 1, tokenStream.Length - (lastIndex + 1));
        }

        //Currently cannot call functions, split into variable access and method call after creating that production
        public static bool MemberAccessProductionParse(ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream.Length == 0 || (tokenStream[0].TokenType != TokenTypes.Identifier && tokenStream[0].TokenType != TokenTypes.ThisKeyword)) return false;
            if(tokenStream.Length == 1)
            {
                node = new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] };
                return true;
            }
            if(tokenStream[1].TokenType == TokenTypes.MemberAccess)
            {
                if (!MemberAccessProductionParse(tokenStream.Slice(2, tokenStream.Length - 2), out ParseTreeNode tempNode)) return false;
                node = new ParseTreeNode(SyntaxUnit.MemberAccess);
                node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] });
                node.Children.Add(tempNode);
                return true;
            }
            return false;
        }

        public static bool MathProductionParse(ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream.Length == 0) return false;

            var operation = FindBinaryOp(tokenStream, new string[] { "+", "-" });
            if (operation.Token != null)
            {
                if (MathProductionParse(tokenStream.Slice(0, operation.Index), out ParseTreeNode leftExpression) && MathProductionParse(tokenStream.Slice(operation.Index + 1, tokenStream.Length - (operation.Index + 1)), out ParseTreeNode rightExpression))
                {
                    node = new ParseTreeNode();
                    if (operation.Token.Lexeme == "+") node.Unit = SyntaxUnit.AddExpression;
                    else node.Unit = SyntaxUnit.SubtractExpression;
                    node.Children.Add(leftExpression);
                    node.Children.Add(rightExpression);
                    return true;
                }
            }

            operation = FindBinaryOp(tokenStream, new string[] { "*", "/", "%" });
            if (operation.Token != null)
            {
                if (MathProductionParse(tokenStream.Slice(0, operation.Index), out ParseTreeNode leftExpression) && MathProductionParse(tokenStream.Slice(operation.Index + 1, tokenStream.Length - (operation.Index + 1)), out ParseTreeNode rightExpression))
                {
                    node = new ParseTreeNode();
                    if (operation.Token.Lexeme == "*") node.Unit = SyntaxUnit.MultiplyExpression;
                    else if (operation.Token.Lexeme == "/") node.Unit = SyntaxUnit.DivideExpression;
                    else node.Unit = SyntaxUnit.ModuloExpression;
                    node.Children.Add(leftExpression);
                    node.Children.Add(rightExpression);
                    return true;
                }
            }

            if (tokenStream[0].TokenType == TokenTypes.OpenParenthesis && tokenStream[tokenStream.Length - 1].TokenType == TokenTypes.CloseParenthesis)
            {
                if (MathProductionParse(tokenStream.Slice(1, tokenStream.Length - 2), out ParseTreeNode expression))
                {
                    node = new ParseTreeNode(SyntaxUnit.ParethesisBoundMathExpression);
                    node.Children.Add(expression);
                    return true;
                }
            }

            if (tokenStream[0].TokenType == TokenTypes.UnaryMathOperand && tokenStream[0].Lexeme == "(-)")
            {
                if (MathProductionPrimeParse(tokenStream.Slice(1, tokenStream.Length - 1), out ParseTreeNode expression))
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
        public static bool MathProductionPrimeParse(ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream[0].TokenType == TokenTypes.IntLiteral)
            {
                if (tokenStream.Length != 1) return false;
                node = new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] };
                return true;
            }
            if (!MemberAccessProductionParse(tokenStream,out ParseTreeNode tempNode)) return false;
            node = new ParseTreeNode(SyntaxUnit.MemberAccess);
            node.Children.Add(tempNode);
            return true;
        }
        private static (Token Token , int Index) FindBinaryOp(ReadOnlySpan<Token> tokenStream, string[] validLexemes)
        {
            int unmatchedLeftParens = 0;
            int currentIndex = 0;
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
                        return (token,currentIndex);
                    }
                    
                }
                currentIndex++;
            }
            return (default,-1);
        }
    
    }

    

}