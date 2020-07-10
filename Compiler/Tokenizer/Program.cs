using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.IO;
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
32.	Unary Math Operand: (\(-\)|~)
33. Binary Math Operand: (+|-|*|/|%|&|\||\^)
34. Char literal: '\\?.'
35. String Literals: \".*?\"
36. Indentifier: [A-Za-z_]\w* 
*/
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

    public static class Tokenizer
    {
		public static List<Token> Tokenize(string Program)
        {
			if (Program.Length == 0) return null;
			int initialLength = Program.Length;
			Dictionary<Regex, TokenTypes> regexes = new Dictionary<Regex, TokenTypes>();
			{
				regexes.Add(new Regex("\\/\\/.*\n?"), TokenTypes.Comment);
				regexes.Add(new Regex("\".*?\""), TokenTypes.StringLiteral);
				regexes.Add(new Regex("'\\?.'"), TokenTypes.CharLiteral);
				regexes.Add(new Regex("namespace"), TokenTypes.Namespace);
				regexes.Add(new Regex("function"), TokenTypes.Function);
				regexes.Add(new Regex("class"), TokenTypes.Class);
				regexes.Add(new Regex("var"), TokenTypes.VariableInitialization);
				regexes.Add(new Regex("(\\[public\\]|\\[private\\])"), TokenTypes.AccessModifier);
				regexes.Add(new Regex("entrypoint"), TokenTypes.EntryPointMarker);
				regexes.Add(new Regex("(int|bool|string|char|void)"), TokenTypes.Type);
				regexes.Add(new Regex("\\{"), TokenTypes.OpenRegion);
				regexes.Add(new Regex("\\}"), TokenTypes.CloseRegion);
				regexes.Add(new Regex(";"), TokenTypes.Semicolon);
				regexes.Add(new Regex("\\."), TokenTypes.MemberAccess);
				regexes.Add(new Regex("\\("), TokenTypes.OpenParenthesis);
				regexes.Add(new Regex("\\)"), TokenTypes.CloseParenthesis);
				regexes.Add(new Regex("if"), TokenTypes.IfStatement);
				regexes.Add(new Regex("else"), TokenTypes.ElseStatement);
				regexes.Add(new Regex("while"), TokenTypes.WhileLoop);
				regexes.Add(new Regex("for"), TokenTypes.ForLoop);
				regexes.Add(new Regex("\\s"), TokenTypes.WhiteSpace);
				regexes.Add(new Regex("(==|!=|<=|>=|<|>)"), TokenTypes.Comparer);
				regexes.Add(new Regex("="), TokenTypes.SetVariable);
				regexes.Add(new Regex("return"), TokenTypes.Return);
				regexes.Add(new Regex("break"), TokenTypes.Break);
				regexes.Add(new Regex("continue"), TokenTypes.Continue);
				regexes.Add(new Regex("null"), TokenTypes.Null);
				regexes.Add(new Regex("new"), TokenTypes.New);
				regexes.Add(new Regex(","), TokenTypes.Comma);
				regexes.Add(new Regex("\\["), TokenTypes.ArrayOpenBracket);
				regexes.Add(new Regex("\\]"), TokenTypes.ArrayCloseBracket);
				regexes.Add(new Regex("(true|false)"), TokenTypes.BoolLiteral);
				regexes.Add(new Regex("(\\(-\\))?\\d+"), TokenTypes.IntLiteral);
				regexes.Add(new Regex("(\\+\\+|--)"), TokenTypes.IncrementOrDecrement);
				regexes.Add(new Regex("(&&|\\|\\||\\^\\^|!)"), TokenTypes.LogicalOperand);
				regexes.Add(new Regex("(\\(-\\)|~)"), TokenTypes.UnaryMathOperand);
				regexes.Add(new Regex("(\\+|-|\\*|/|%|&|\\||\\^)"), TokenTypes.BinaryMathOperand);
			}
			List<Token> tokens = new List<Token>();
			for (int i = 0; i < initialLength; i++)
			{
				foreach (var pair in regexes)
				{
					var match = pair.Key.Match(Program.Substring(0,i-(initialLength-Program.Length)+1));
					if (!match.Success || match.Index != 0) continue;
					tokens.Add(new Token(pair.Value, match.Value));
					Program = Program.Substring(i - (initialLength - Program.Length));
					break;
				}
			}
			return tokens;
        }

    }
    class Program
    {
        static void Main(string[] args)
        {
			string program = File.ReadAllText(@"Example.dyl");
			var tokens = Tokenizer.Tokenize(program);
        }
    }
}
