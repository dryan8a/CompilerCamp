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

        public static bool IsComparison(SyntaxUnit unit)
        {
            if(unit == SyntaxUnit.LessThanComparison || unit == SyntaxUnit.LessThanEqualToComparison || unit == SyntaxUnit.EqualToComparison || unit == SyntaxUnit.GreaterThanComparison || unit == SyntaxUnit.GreaterThanEqualToComparison || unit == SyntaxUnit.NotEqualToComparison)
            {
                return true;
            }
            return false;
        }
        public static bool IsMathEquals(SyntaxUnit unit)
        {
            if(unit == SyntaxUnit.AddEqualsExpression || unit == SyntaxUnit.SubtractEqualsExpression || unit == SyntaxUnit.MultiplyEqualsExpression || unit == SyntaxUnit.DivideEqualsExpression || unit == SyntaxUnit.ModuloEqualsExpression)
            {
                return true;
            }
            return false;
        }
    }

    public enum SyntaxUnit
    {
        None,
        CompilationUnit,
        NamespaceDeclaration,
        ClassDecleration,
        MethodDeclaration,
        Body,
        MethodCall,
        VariableDeclaration,
        VariableInitialization,
        VariableAssignment,
        NewObject,
        EqualsValueClause,
        Value,
        IntValue,
        StringValue,
        CharValue,
        BoolValue,
        AddExpression,
        SubtractExpression,
        MultiplyExpression,
        DivideExpression,
        ModuloExpression,
        AddEqualsExpression,
        SubtractEqualsExpression,
        MultiplyEqualsExpression,
        DivideEqualsExpression,
        ModuloEqualsExpression,
        ParethesisBoundMathExpression,
        ParenthesisBoundBoolExpression,
        NegativeExpression,
        OrExpression,
        AndExpression,
        XorExpression,
        NotExpression,
        LessThanEqualToComparison,
        LessThanComparison,
        EqualToComparison,
        GreaterThanComparison,
        GreaterThanEqualToComparison,
        NotEqualToComparison,
        Token,
        MemberAccess,
        Parameter,
        ReturnType,
        VariableType,
        ParameterList,
        EmptyStatement,
        ReturnStatement,
        IfStatement,
        Increment,
        Decrement,
    }
}
