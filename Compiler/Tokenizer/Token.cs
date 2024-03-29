﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TokenizerNamespace
{
	public enum BracketTypes
	{
		None,
		Curly,
		Parenthesis,
		Square,
	}

	public enum TokenTypes
	{
		None,
		Comment,
		StringLiteral,
		CharLiteral,
		Namespace,
		Function,
		Class,
		VariableInitialization,
		AccessModifier,
		EntryPointMarker,
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
		Identifier
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
