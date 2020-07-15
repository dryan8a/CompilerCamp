using System;
using System.Collections.Generic;
using System.Text;

namespace Tokenizer
{
	public enum TokenTypes
	{
		Comment,
		StringLiteral,
		CharLiteral,
		Namespace,
		Function,
		Class,
		VariableInitialization,
		AccessModifier,
		EntryPointMarker,
		StaticMarker,
		Type,
		OpenRegion,
		CloseRegion,
		Semicolon,
		MemberAccess,
		OpenParenthesis,
		CloseParenthesis,
		IfStatement,
		ElseStatement,
		WhileLoop,
		ForLoop,
		WhiteSpace,
		Comparer,
		SetVariable,
		AssignmentMathOperand,
		Return,
		LoopUtilityKeyword,
		ThisKeyword,
		Null,
		New,
		Comma,
		ArrayOpenBracket,
		ArrayCloseBracket,
		BoolLiteral,
		IntLiteral,
		IncrementOrDecrement,
		LogicalOperand,
		UnaryMathOperand,
		BinaryMathOperand,
		Indentifier
	}
	public class Token
	{
		public readonly TokenTypes TokenType;
		public readonly string Lexeme;
		public Token(TokenTypes tokenType, string lexeme)
		{
			TokenType = tokenType;
			Lexeme = lexeme;
		}
	}
}
