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
            int rightBracketIndex = FindCorrespondingRightBracket(tokenStream, 2, BracketTypes.Curly);
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
            if (tokenStream[1].TokenType == TokenTypes.AccessModifier)
            {
                LeftBracketIndex = 3;
                node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[1] });
            }
            if (tokenStream[LeftBracketIndex - 1].TokenType != TokenTypes.Identifier || tokenStream[LeftBracketIndex].TokenType != TokenTypes.OpenRegion) return false;
            node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[LeftBracketIndex - 1] });
            int rightBracketIndex = FindCorrespondingRightBracket(tokenStream, LeftBracketIndex, BracketTypes.Curly);
            if (rightBracketIndex < 0) return false;

            ReadOnlySpan<Token> classTokens = tokenStream.Slice(LeftBracketIndex + 1, rightBracketIndex - (LeftBracketIndex + 1) < 0 ? 0 : rightBracketIndex - (LeftBracketIndex + 1));
            RemoveUsedTokens(ref tokenStream, rightBracketIndex);
            while (classTokens.Length > 0)
            {
                ParseTreeNode tempNode;
                if (!VariableDeclarationProductionParse(ref classTokens, out tempNode, false) && !MethodProductionParse(ref classTokens, out tempNode)) return false;
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
            if (tokenStream[TypeIndex].TokenType == TokenTypes.AccessModifier)
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
            if (!ParameterListProductionParse(tokenStream.Slice(TypeIndex + 3, rightParenIndex - (TypeIndex + 2) < 0 ? 0 : rightParenIndex - (TypeIndex + 2)), out ParseTreeNode ParamsNode)) return false;
            node.Children.Add(ParamsNode);

            if (tokenStream[rightParenIndex + 1].TokenType != TokenTypes.OpenRegion) return false;
            int rightBracketIndex = FindCorrespondingRightBracket(tokenStream, rightParenIndex + 1, BracketTypes.Curly);
            if (rightBracketIndex < 0) return false;

            ReadOnlySpan<Token> methodBodyTokens = tokenStream.Slice(rightParenIndex + 2, rightBracketIndex - (rightParenIndex + 2) < 0 ? 0 : rightBracketIndex - (rightParenIndex + 2));
            ParseTreeNode methodBody;
            if (!BodyProductionParse(methodBodyTokens, out methodBody)) return false;
            node.Children.Add(methodBody);
            RemoveUsedTokens(ref tokenStream, rightBracketIndex);
            return true;
        }
        public static bool BodyProductionParse(ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = new ParseTreeNode(SyntaxUnit.Body);
            while (tokenStream.Length > 0)
            {
                if (!ExpressionProductionParse(ref tokenStream, out ParseTreeNode tempNode)) return false;
                node.Children.Add(tempNode);
            }
            return true;
        }

        public static bool ExpressionProductionParse(ref ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            if (IfProductionParse(ref tokenStream, out node))
            {
                return true;
            }
            if(WhileProductionParse(ref tokenStream, out node))
            {
                return true;
            }
            if (VariableDeclarationProductionParse(ref tokenStream, out node, true))
            {
                return true;
            }
            if (VariableInitializationProductionParse(ref tokenStream, out node))
            {
                return true;
            }
            if(MathAsignmentProductionParse(ref tokenStream, out node))
            {
                return true;
            }
            if(IncrementDecrementProductionParse(ref tokenStream, out node))
            {
                return true;
            }
            if(tokenStream[0].TokenType == TokenTypes.Semicolon)
            {
                tokenStream = tokenStream.Slice(0, 1);
                node = new ParseTreeNode(SyntaxUnit.EmptyStatement);
                return true;
            }
            if(VariableAssignmentProductionParse(ref tokenStream, out node))
            {
                return true;
            }
            if(ReturnProductionParse(ref tokenStream, out node))
            {
                return true;
            }

            return false;
        }

        public static bool IfProductionParse(ref ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream[0].TokenType != TokenTypes.IfStatement || tokenStream[1].TokenType != TokenTypes.OpenParenthesis) return false;
            int rightParenIndex = FindCorrespondingRightBracket(tokenStream, 2, BracketTypes.Parenthesis);//FindNextToken(tokenStream, 2, TokenTypes.CloseParenthesis);
            if (rightParenIndex < 0) return false;
            if (!BoolValueProductionParse(tokenStream.Slice(2, rightParenIndex - 2), out ParseTreeNode conditionNode) || tokenStream[rightParenIndex + 1].TokenType != TokenTypes.OpenRegion) return false;
            node = new ParseTreeNode(SyntaxUnit.IfStatement);
            var boolValueNode = new ParseTreeNode(SyntaxUnit.BoolValue);
            boolValueNode.Children.Add(conditionNode);
            node.Children.Add(boolValueNode);
            int closeRegionIndex = FindCorrespondingRightBracket(tokenStream, rightParenIndex + 1, BracketTypes.Curly);//FindNextToken(tokenStream, rightParenIndex + 1, TokenTypes.CloseRegion);
            if (closeRegionIndex < 0) return false;
            if (!BodyProductionParse(tokenStream.Slice(rightParenIndex + 2, closeRegionIndex - (rightParenIndex + 2)), out ParseTreeNode bodyNode)) return false;
            node.Children.Add(bodyNode);
            RemoveUsedTokens(ref tokenStream, closeRegionIndex);
            return true;
        }
        public static bool WhileProductionParse(ref ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream[0].TokenType != TokenTypes.WhileLoop || tokenStream[1].TokenType != TokenTypes.OpenParenthesis) return false;
            int rightParenIndex = FindCorrespondingRightBracket(tokenStream, 2, BracketTypes.Parenthesis);//FindNextToken(tokenStream, 2, TokenTypes.CloseParenthesis);
            if (rightParenIndex < 0) return false;
            if (!BoolValueProductionParse(tokenStream.Slice(2, rightParenIndex - 2), out ParseTreeNode conditionNode) || tokenStream[rightParenIndex + 1].TokenType != TokenTypes.OpenRegion) return false;
            node = new ParseTreeNode(SyntaxUnit.WhileLoop);
            var boolValueNode = new ParseTreeNode(SyntaxUnit.BoolValue);
            boolValueNode.Children.Add(conditionNode);
            node.Children.Add(boolValueNode);
            int closeRegionIndex = FindCorrespondingRightBracket(tokenStream, rightParenIndex + 1, BracketTypes.Curly);//FindNextToken(tokenStream, rightParenIndex + 1, TokenTypes.CloseRegion);
            if (closeRegionIndex < 0) return false;
            if (!BodyProductionParse(tokenStream.Slice(rightParenIndex + 2, closeRegionIndex - (rightParenIndex + 2)), out ParseTreeNode bodyNode)) return false;
            node.Children.Add(bodyNode);
            RemoveUsedTokens(ref tokenStream, closeRegionIndex);
            return true;
        }

        public static bool ReturnProductionParse(ref ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream[0].TokenType != TokenTypes.Return) return false;
            int semicolonIndex = FindNextToken(tokenStream, 1, TokenTypes.Semicolon);
            if (semicolonIndex < 0) return false;
            node = new ParseTreeNode(SyntaxUnit.ReturnStatement);
            if (semicolonIndex == 1)
            {
                RemoveUsedTokens(ref tokenStream, semicolonIndex);
                return true;
            }
            if (!ValueProductionParse(tokenStream.Slice(1, semicolonIndex - 1), out ParseTreeNode valueNode)) return false;
            node.Children.Add(valueNode);
            RemoveUsedTokens(ref tokenStream, semicolonIndex);
            return true;
        }

        public static bool VariableAssignmentProductionParse(ref ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            int semicolonIndex = FindNextToken(tokenStream, 0, TokenTypes.Semicolon);
            int operatorIndex = FindNextToken(tokenStream, 0, TokenTypes.SetVariable);
            if (operatorIndex < 0 || semicolonIndex < 0) return false;
            if (!MemberAccessProductionParse(tokenStream.Slice(0, operatorIndex), out ParseTreeNode variableNode)) return false;
            if (!ValueProductionParse(tokenStream.Slice(operatorIndex + 1, semicolonIndex - (operatorIndex + 1)), out ParseTreeNode valueNode)) return false;
            node = new ParseTreeNode(SyntaxUnit.VariableAssignment);
            node.Children.Add(variableNode);
            node.Children.Add(valueNode);
            RemoveUsedTokens(ref tokenStream, semicolonIndex);
            return true;
        }

        public static bool IncrementDecrementProductionParse(ref ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            int semicolonIndex = FindNextToken(tokenStream, 0, TokenTypes.Semicolon);
            int operatorIndex = FindNextToken(tokenStream, 0, TokenTypes.IncrementOrDecrement);
            if (semicolonIndex < 0 || operatorIndex < 0 || operatorIndex + 1 != semicolonIndex) return false;
            if (!MemberAccessProductionParse(tokenStream.Slice(0, operatorIndex), out ParseTreeNode variableNode)) return false;
            var unit = tokenStream[operatorIndex].Lexeme == "++" ? SyntaxUnit.Increment : SyntaxUnit.Decrement;
            node = new ParseTreeNode(unit);
            node.Children.Add(variableNode);
            RemoveUsedTokens(ref tokenStream, semicolonIndex);
            return true;
        }

        public static bool MathAsignmentProductionParse(ref ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            int semicolonIndex = FindNextToken(tokenStream, 0, TokenTypes.Semicolon);
            int operatorIndex = FindNextToken(tokenStream, 0, TokenTypes.AssignmentMathOperand);
            if (semicolonIndex < 0 || operatorIndex < 0) return false;
            if (!MemberAccessProductionParse(tokenStream.Slice(0, operatorIndex), out ParseTreeNode variableNode)) return false;
            if (!MathProductionParse(tokenStream.Slice(operatorIndex + 1, semicolonIndex - (operatorIndex + 1)), out ParseTreeNode valueNode)) return false;
            SyntaxUnit unit = default;
            switch(tokenStream[operatorIndex].Lexeme)
            {
                case "+=":
                    unit = SyntaxUnit.AddEqualsExpression;
                    break;
                case "-=":
                    unit = SyntaxUnit.SubtractEqualsExpression;
                    break;
                case "*=":
                    unit = SyntaxUnit.MultiplyEqualsExpression;
                    break;
                case "/=":
                    unit = SyntaxUnit.DivideEqualsExpression;
                    break;
                case "%=":
                    unit = SyntaxUnit.ModuloEqualsExpression;
                    break;
            }
            node = new ParseTreeNode(unit);
            node.Children.Add(variableNode);
            var intValueNode = new ParseTreeNode(SyntaxUnit.IntValue);
            intValueNode.Children.Add(valueNode);
            node.Children.Add(intValueNode);
            RemoveUsedTokens(ref tokenStream, semicolonIndex);
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
        public static int FindNextToken(ReadOnlySpan<Token> tokenStream, int startIndex, TokenTypes tokenType)
        {
            if (startIndex >= tokenStream.Length) return -1;
            for(int i = startIndex; i < tokenStream.Length;i++)
            {
                if (tokenStream[i].TokenType == tokenType) return i;
            }
            return -1;
        }

        public static bool VariableInitializationProductionParse(ref ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream.Length < 4 || tokenStream[0].TokenType != TokenTypes.VariableInitialization) return false;
            node = new ParseTreeNode(SyntaxUnit.VariableInitialization);
            int typeIndex = 1;
            ParseTreeNode typeNode;
            if (!VariableTypeProductionParse(tokenStream[typeIndex], out typeNode) || tokenStream[typeIndex + 1].TokenType != TokenTypes.Identifier || tokenStream[typeIndex + 2].TokenType != TokenTypes.SetVariable) return false;
            node.Children.Add(typeNode);
            node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[typeIndex + 1] });
            int SemicolonIndex = FindNextToken(tokenStream,typeIndex + 3, TokenTypes.Semicolon);
            if (SemicolonIndex < 0) return false;
            if (!ValueProductionParse(tokenStream.Slice(typeIndex + 3, SemicolonIndex - (typeIndex + 3)), out ParseTreeNode ValueNode)) return false;
            node.Children.Add(ValueNode);
            RemoveUsedTokens(ref tokenStream, SemicolonIndex);
            return true;
        }

        public static bool ValueProductionParse(ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            ParseTreeNode tempNode;
            if(tokenStream.Length == 1 && tokenStream[0].TokenType == TokenTypes.Null)
            {
                node = new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] };
                return true;
            }
            if (FunctionCallProductionParse(tokenStream, out tempNode))
            {
                node = tempNode;
                return true;
            }
            if (MemberAccessProductionParse(tokenStream, out tempNode))
            {
                node = tempNode;
                return true;
            }
            if(NewObjectProductionParse(tokenStream,out tempNode))
            {
                node = tempNode;
                return true;
            }
            if (MathProductionParse(tokenStream,out tempNode))
            {
                node = new ParseTreeNode(SyntaxUnit.IntValue);
                node.Children.Add(tempNode);
                return true;
            }
            if(StringValueProductionParse(tokenStream, out tempNode))
            {
                node = new ParseTreeNode(SyntaxUnit.StringValue);
                node.Children.Add(tempNode);
                return true;
            }
            if(CharValueProductionParse(tokenStream, out tempNode))
            {
                node = new ParseTreeNode(SyntaxUnit.CharValue);
                node.Children.Add(tempNode);
                return true;
            }
            if(BoolValueProductionParse(tokenStream, out tempNode))
            {
                node = new ParseTreeNode(SyntaxUnit.BoolValue);
                node.Children.Add(tempNode);
                return true;
            }
            node = default;
            return false;
        }

        //Cannot call functions on their own
        public static bool FunctionCallProductionParse(ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream.Length < 3) return false;
            node = new ParseTreeNode(SyntaxUnit.MethodCall);
            if (tokenStream[0].TokenType != TokenTypes.Identifier || tokenStream[1].TokenType != TokenTypes.OpenParenthesis) return false;
            node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] });
            int rightParenIndex = FindCorrespondingRightBracket(tokenStream, 1, BracketTypes.Parenthesis);
            if (rightParenIndex < 0) return false;
            var Params = new ParseTreeNode(SyntaxUnit.ParameterList);
            if (tokenStream[2].TokenType == TokenTypes.CloseParenthesis)
            {
                node.Children.Add(Params);
                return true;
            }
            var ParamTokens = tokenStream.Slice(2, rightParenIndex - 1);
            while (ParamTokens.Length > 0)
            {
                int commaIndex = FindNextToken(ParamTokens, 0, TokenTypes.Comma);
                ParseTreeNode tempNode;
                if (commaIndex < 0)
                {
                    if (!ValueProductionParse(ParamTokens.Slice(0, ParamTokens.Length - 1), out tempNode)) return false;
                    RemoveUsedTokens(ref ParamTokens, ParamTokens.Length - 1);
                }
                else
                {
                    if (!ValueProductionParse(ParamTokens.Slice(0, commaIndex), out tempNode)) return false;
                    RemoveUsedTokens(ref ParamTokens, commaIndex);
                }
                Params.Children.Add(tempNode);
            }
            node.Children.Add(Params);
            return true;
        }
    

        public static bool NewObjectProductionParse(ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream.Length < 4) return false;
            if (tokenStream[0].TokenType != TokenTypes.New || tokenStream[1].TokenType != TokenTypes.Identifier || tokenStream[2].TokenType != TokenTypes.OpenParenthesis) return false;
            node = new ParseTreeNode(SyntaxUnit.NewObject);
            node.Children.Add(new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[1] });
            int rightParenIndex = FindCorrespondingRightBracket(tokenStream, 2, BracketTypes.Parenthesis);
            if (rightParenIndex < 0) return false;
            var Params = new ParseTreeNode(SyntaxUnit.ParameterList);
            if (tokenStream[3].TokenType == TokenTypes.CloseParenthesis)
            {
                node.Children.Add(Params);
                return true;
            }
            var ParamTokens = tokenStream.Slice(3,rightParenIndex - 2);
            while(ParamTokens.Length > 0)
            {
                int commaIndex = FindNextToken(ParamTokens, 0, TokenTypes.Comma);
                ParseTreeNode tempNode;
                if (commaIndex < 0)
                {
                    if (!ValueProductionParse(ParamTokens.Slice(0, ParamTokens.Length - 1), out tempNode)) return false;
                    RemoveUsedTokens(ref ParamTokens, ParamTokens.Length - 1);
                }
                else
                {
                    if (!ValueProductionParse(ParamTokens.Slice(0, commaIndex), out tempNode)) return false;
                    RemoveUsedTokens(ref ParamTokens, commaIndex);
                }
                Params.Children.Add(tempNode);
            }
            node.Children.Add(Params);
            return true;
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
            if(tokenStream.Length == 1 && tokenStream[0].TokenType == TokenTypes.CloseParenthesis) return true;
            while(tokenStream.Length > 0)
            {
                if(!ParameterProductionParse(ref tokenStream, out ParseTreeNode tempNode)) return false;
                node.Children.Add(tempNode);
            }
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
            if(tokenStream[1].TokenType == TokenTypes.OpenParenthesis)
            {
                int dotIndex = FindNextToken(tokenStream, 3, TokenTypes.MemberAccess);
                if(dotIndex < 0)
                {
                    if (!FunctionCallProductionParse(tokenStream, out ParseTreeNode funcNode)) return false;
                    node = funcNode;
                    return true;
                }
                else
                {
                    if (!FunctionCallProductionParse(tokenStream.Slice(0,dotIndex), out ParseTreeNode funcNode)) return false;
                    if (!MemberAccessProductionParse(tokenStream.Slice(dotIndex+1, tokenStream.Length - (dotIndex+1)), out ParseTreeNode tempNode)) return false;
                    node = new ParseTreeNode(SyntaxUnit.MemberAccess);
                    node.Children.Add(funcNode);
                    node.Children.Add(tempNode);
                    return true;
                }
            }
            return false;
        }

        //Note for all Math Productions, all binary operations are not implemented for anything other than bools
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
            if (tokenStream.Length == 0) return false;
            if (tokenStream.Length == 1 && tokenStream[0].TokenType == TokenTypes.IntLiteral)
            {
                if (tokenStream.Length != 1) return false;
                node = new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] };
                return true;
            }
            if (tokenStream.Length == 1 && tokenStream[0].TokenType == TokenTypes.Null)
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

        private static (Token Token , int Index) FindBinaryOp(ReadOnlySpan<Token> tokenStream, string[] validLexemes, TokenTypes tokenType = TokenTypes.BinaryMathOperand)
        {
            int unmatchedLeftParens = 0;
            bool allLexemes = validLexemes.Length == 0;
            for(int i = 0; i < tokenStream.Length;i++)
            {
                if (tokenStream[i].TokenType == TokenTypes.OpenParenthesis) unmatchedLeftParens++;
                if (tokenStream[i].TokenType == TokenTypes.CloseParenthesis) unmatchedLeftParens--;
                if (unmatchedLeftParens != 0) continue;
                if (tokenStream[i].TokenType != tokenType) continue;
                if(allLexemes)
                {
                    return (tokenStream[i],i);
                }
                foreach (string lexeme in validLexemes)
                {
                    if (tokenStream[i].Lexeme == lexeme)
                    {
                        return (tokenStream[i], i);
                    }
                }
            }
            return (default,-1);
        }

        //String concatination not yet implemented
        public static bool StringValueProductionParse(ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream.Length == 0) return false;
            if (tokenStream.Length == 1 && tokenStream[0].TokenType == TokenTypes.StringLiteral)
            {
                if (tokenStream.Length != 1) return false;
                node = new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] };
                return true;
            }
            if (tokenStream.Length == 1 && tokenStream[0].TokenType == TokenTypes.Null)
            {
                if (tokenStream.Length != 1) return false;
                node = new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] };
                return true;
            }
            if (!MemberAccessProductionParse(tokenStream, out ParseTreeNode tempNode)) return false;
            node = new ParseTreeNode(SyntaxUnit.MemberAccess);
            node.Children.Add(tempNode);
            return true;
        }

        public static bool CharValueProductionParse(ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream.Length == 0) return false;
            if (tokenStream.Length == 1 && tokenStream[0].TokenType == TokenTypes.CharLiteral)
            {
                if (tokenStream.Length != 1) return false;
                node = new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] };
                return true;
            }
            if (tokenStream.Length == 1 && tokenStream[0].TokenType == TokenTypes.Null)
            {
                if (tokenStream.Length != 1) return false;
                node = new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] };
                return true;
            }
            if (!MemberAccessProductionParse(tokenStream, out ParseTreeNode tempNode)) return false;
            node = new ParseTreeNode(SyntaxUnit.MemberAccess);
            node.Children.Add(tempNode);
            return true;
        }

        public static bool BoolValueProductionParse(ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream.Length == 0) return false;

            var operation = FindBinaryOp(tokenStream, new string[] { "||", "&&", "^^" },TokenTypes.LogicalOperand);
            if (operation.Token != null)
            {
                if (BoolValueProductionParse(tokenStream.Slice(0, operation.Index), out ParseTreeNode leftExpression) && BoolValueProductionParse(tokenStream.Slice(operation.Index + 1, tokenStream.Length - (operation.Index + 1)), out ParseTreeNode rightExpression))
                {
                    node = new ParseTreeNode();
                    if (operation.Token.Lexeme == "||") node.Unit = SyntaxUnit.OrExpression;
                    else if (operation.Token.Lexeme == "&&") node.Unit = SyntaxUnit.AndExpression;
                    else node.Unit = SyntaxUnit.XorExpression;
                    node.Children.Add(leftExpression);
                    node.Children.Add(rightExpression);
                    return true;
                }
            }

            operation = FindBinaryOp(tokenStream, new string[] {}, TokenTypes.Comparer);
            if (operation.Token != null)
            {
                if (MathProductionParse(tokenStream.Slice(0, operation.Index), out ParseTreeNode leftExpression) && MathProductionParse(tokenStream.Slice(operation.Index + 1, tokenStream.Length - (operation.Index + 1)), out ParseTreeNode rightExpression))
                {
                    node = new ParseTreeNode();
                    if (operation.Token.Lexeme == "<") node.Unit = SyntaxUnit.LessThanComparison;
                    else if (operation.Token.Lexeme == "<=") node.Unit = SyntaxUnit.LessThanEqualToComparison;
                    else if (operation.Token.Lexeme == "==") node.Unit = SyntaxUnit.EqualToComparison;
                    else if (operation.Token.Lexeme == ">=") node.Unit = SyntaxUnit.GreaterThanEqualToComparison;
                    else if (operation.Token.Lexeme == ">") node.Unit = SyntaxUnit.GreaterThanComparison;
                    else node.Unit = SyntaxUnit.NotEqualToComparison;
                    leftExpression.Parent = node;
                    rightExpression.Parent = node;
                    node.Children.Add(leftExpression);
                    node.Children.Add(rightExpression);
                    return true;
                }
            }

            if (tokenStream[0].TokenType == TokenTypes.OpenParenthesis && tokenStream[tokenStream.Length - 1].TokenType == TokenTypes.CloseParenthesis)
            {
                if (BoolValueProductionParse(tokenStream.Slice(1, tokenStream.Length - 2), out ParseTreeNode expression))
                {
                    node = new ParseTreeNode(SyntaxUnit.ParenthesisBoundBoolExpression);
                    node.Children.Add(expression);
                    return true;
                }
            }

            if (tokenStream[0].TokenType == TokenTypes.LogicalOperand && tokenStream[0].Lexeme == "!")
            {
                if (BoolValueProductionPrimeParse(tokenStream.Slice(1, tokenStream.Length - 1), out ParseTreeNode expression))
                {
                    node = new ParseTreeNode(SyntaxUnit.NotExpression);
                    node.Children.Add(expression);
                    return true;
                }
            }
            if (BoolValueProductionPrimeParse(tokenStream, out ParseTreeNode expressionPrime))
            {
                node = expressionPrime;
                return true;
            }
            return false;
        }
        public static bool BoolValueProductionPrimeParse(ReadOnlySpan<Token> tokenStream, out ParseTreeNode node)
        {
            node = default;
            if (tokenStream.Length == 0) return false;
            if (tokenStream.Length == 1 && tokenStream[0].TokenType == TokenTypes.BoolLiteral)
            {
                if (tokenStream.Length != 1) return false;
                node = new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] };
                return true;
            }
            if (tokenStream.Length == 1 && tokenStream[0].TokenType == TokenTypes.Null)
            {
                if (tokenStream.Length != 1) return false;
                node = new ParseTreeNode(SyntaxUnit.Token) { Token = tokenStream[0] };
                return true;
            }
            if (!MemberAccessProductionParse(tokenStream, out ParseTreeNode tempNode)) return false;
            node = tempNode;
            return true;
        }
    }
}