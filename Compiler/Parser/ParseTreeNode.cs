using System;
using System.Collections.Generic;
using System.Text;
using TokenizerNamespace;

namespace ParserNamespace
{
    public class ParseTreeNode
    {
        public ParseTreeNode Parent;
        public List<ParseTreeNode> Children;
        public SyntaxUnit Unit;
        public Token Token;

        public ParseTreeNode()
        {
            Children = new List<ParseTreeNode>();
        }
        public ParseTreeNode(SyntaxUnit unit)
        {
            Unit = unit;
            Children = new List<ParseTreeNode>();
        }

        public bool SetValue(Token token)
        {
            if (Unit != SyntaxUnit.Token) return false;
            Token = token;
            return true;
        }

    }

    public enum SyntaxUnit
    {
        None,
        CompilationUnit,
        NamespaceDeclaration,
        ClassDecleration,
        MethodDeclaration,
        VariableDeclaration,
        Expression,
        EqualsValueClause,
        Value,
        StringLiteral,
        CharLiteral,
        AddExpression,
        SubtractExpression,
        MultiplyExpression,
        DivideExpression,
        ModuloExpression,
        ParethesisBoundMathExpression,
        NegativeExpression,
        Token,
        MemberAccess,
        Parameter,
        ReturnType,
        VariableType,
        ParameterList,


    }
}
