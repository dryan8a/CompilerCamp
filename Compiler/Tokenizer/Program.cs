using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
/*
1. Comment: \/\/.*\n?
2. Namespace: namespace
3. Function: method
4. Class: class
5. Variable Initialization: var
6. Access Modifier: (\[public\]|\[private\])
7. Entry Point: entrypoint
8. Types: (int|bool|string|char|void)
9. Open Region: \{
10. Close Region: \}
11. Semi-colon: ;
12. Member Access: \.
13. Open Parenthesis: \(
14. Close Parenthesis: \)
15. If Statement: if
16. While Loop: while
17. White Space: \s
18. Comparer: (==|!=|<=|>=|<|>)
19. Set Variable: =
20. Return: return
21. Break: break
22. Continue: continue
23. Null: null
24. New: new
25. Comma: ,
26. Array Open Bracket: \[
27. Array Close Bracket: \] 
28. Bool literal: (true|false)
29. Int literal: (\(-\))?\d+
30. Increment/Decrement: (\+\+|--)
31. Logical Operand: (&&|\|\||\^\^|!)
32. Math Operand: (+|-|*|/|%|&|\||\^|~)
33. Char literal: '\\?.'
34. String Literals: \".*?\"
35. Indentifier: [A-Za-z_]\w* 
*/
namespace Tokenizer
{
	public enum TokenTypes
    {
		Comment,
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
		WhileLoop,
		WhiteSpace,
		Comparer,
		SetVariable,
		Return,
		Break,
		Continue,
		Null,
		New,
		Comma,
		ArrayOpenBracket,
		ArrayCloseBracket,
		BoolLiteral,
		IntLiteral,
		IncrementOrDecrement,
		LogicalOperand,
		MathOperand,
		CharLiteral,
		StringLiteral,
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

    public class Tokenizer
    {
		Dictionary<Regex, TokenTypes> regexes = new Dictionary<Regex, TokenTypes>();
		public Tokenizer()
        {
			regexes.Add(new Regex("\\/\\/.*\n?"),TokenTypes.Comment);
			regexes.Add(new Regex("namespace"), TokenTypes.Namespace);
			regexes.Add(new Regex("function"), TokenTypes.Function);
			regexes.Add(new Regex("class"), TokenTypes.Class);
			regexes.Add(new Regex("var"), TokenTypes.VariableInitialization);
			regexes.Add(new Regex(""), TokenTypes.AccessModifier);
			regexes.Add(new Regex(""), TokenTypes.EntryPointMarker);
			regexes.Add(new Regex(""), TokenTypes.Type);
			regexes.Add(new Regex(""), TokenTypes.OpenRegion);
			regexes.Add(new Regex(""), TokenTypes.CloseRegion);
			regexes.Add(new Regex(""), TokenTypes.Semicolon);
			regexes.Add(new Regex(""), TokenTypes.MemberAccess);
			regexes.Add(new Regex(""), TokenTypes.OpenParenthesis);
			regexes.Add(new Regex(""), TokenTypes.CloseParenthesis);
			regexes.Add(new Regex(""), TokenTypes.IfStatement); 
			regexes.Add(new Regex(""), TokenTypes.WhileLoop); 
			regexes.Add(new Regex(""), TokenTypes.WhiteSpace); 
			regexes.Add(new Regex(""), TokenTypes.Comparer); 
			regexes.Add(new Regex(""), TokenTypes.SetVariable); 
			regexes.Add(new Regex(""), TokenTypes.Return);
			regexes.Add(new Regex(""), TokenTypes.Break);
			regexes.Add(new Regex(""), TokenTypes.Continue);
			regexes.Add(new Regex(""), TokenTypes.Null);
			regexes.Add(new Regex(""), TokenTypes.New);
			regexes.Add(new Regex(""), TokenTypes.Comma);
			regexes.Add(new Regex(""), TokenTypes.ArrayOpenBracket);
			regexes.Add(new Regex(""), TokenTypes.ArrayCloseBracket);
			regexes.Add(new Regex(""), TokenTypes.BoolLiteral);
			regexes.Add(new Regex(""), TokenTypes.IntLiteral);
			regexes.Add(new Regex(""), TokenTypes.IncrementOrDecrement);
			regexes.Add(new Regex(""), TokenTypes.LogicalOperand);
			regexes.Add(new Regex(""), TokenTypes.MathOperand);
			regexes.Add(new Regex(""), TokenTypes.CharLiteral);
			regexes.Add(new Regex(""), TokenTypes.StringLiteral);
		}
		public static List<Token> Tokenize(string Program)
        {

        }

    }
    class Program
    {
        static void Main(string[] args)
        {
			 
        }
    }
}
