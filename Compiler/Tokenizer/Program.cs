using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

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
	
    public static class Tokenizer
    {
		public static List<Token> Tokenize(ReadOnlySpan<char> Program)
        {
			int currentLineNumber = 1;
			if (Program.Length == 0) return null;
			int initialLength = Program.Length;
			Dictionary<Regex, TokenTypes> regexes = new Dictionary<Regex, TokenTypes>();
			{
				regexes.Add(new Regex("^(\\/\\/.*\n?|\\/\\*(.|\n)*\\*\\/)"), TokenTypes.Comment);
				regexes.Add(new Regex("^\"(?(?=\\\\)\\\\[ab\\\\0tnfrv\'\"]|[^\"\n\t])+\""), TokenTypes.StringLiteral);
				regexes.Add(new Regex("^\'(?(?=\\\\)\\\\[ab\\\\0tnfrv\'\"]|[^'\n\t])\'"), TokenTypes.CharLiteral);
				regexes.Add(new Regex("^namespace"), TokenTypes.Namespace);
				regexes.Add(new Regex("^method"), TokenTypes.Function);
				regexes.Add(new Regex("^class"), TokenTypes.Class);
				regexes.Add(new Regex("^var"), TokenTypes.VariableInitialization);
				regexes.Add(new Regex("^(\\[public\\]|\\[private\\])"), TokenTypes.AccessModifier);
				regexes.Add(new Regex("^entrypoint"), TokenTypes.EntryPointMarker);
				regexes.Add(new Regex("^static"), TokenTypes.StaticMarker);
				regexes.Add(new Regex("^(int|bool|string|char|void)"), TokenTypes.Type);
				regexes.Add(new Regex("^\\{"), TokenTypes.OpenRegion);
				regexes.Add(new Regex("^\\}"), TokenTypes.CloseRegion);
				regexes.Add(new Regex("^;"), TokenTypes.Semicolon);
				regexes.Add(new Regex("^\\."), TokenTypes.MemberAccess);
				regexes.Add(new Regex("^(==|!=|<=|>=|<|>)"), TokenTypes.Comparer);
				regexes.Add(new Regex("^(\\+|-|\\*|/|%|&|\\||\\^)="), TokenTypes.AssignmentMathOperand);
				regexes.Add(new Regex("^="), TokenTypes.SetVariable);
				regexes.Add(new Regex("^(&&|\\|\\||\\^\\^|!)"), TokenTypes.LogicalOperand);
				regexes.Add(new Regex("^(\\(-\\)|~)"), TokenTypes.UnaryMathOperand);
				regexes.Add(new Regex("^(\\+\\+|--)"), TokenTypes.IncrementOrDecrement);
				regexes.Add(new Regex("^(\\+|-|\\*|/|%|&|\\||\\^)"), TokenTypes.BinaryMathOperand);
				regexes.Add(new Regex("^\\("), TokenTypes.OpenParenthesis);
				regexes.Add(new Regex("^\\)"), TokenTypes.CloseParenthesis);
				regexes.Add(new Regex("^if"), TokenTypes.IfStatement);
				regexes.Add(new Regex("^else"), TokenTypes.ElseStatement);
				regexes.Add(new Regex("^while"), TokenTypes.WhileLoop);
				regexes.Add(new Regex("^for"), TokenTypes.ForLoop);
				regexes.Add(new Regex("^\\s"), TokenTypes.WhiteSpace);
				regexes.Add(new Regex("^return"), TokenTypes.Return);
				regexes.Add(new Regex("^continue|break"), TokenTypes.LoopUtilityKeyword);
				regexes.Add(new Regex("^this"), TokenTypes.ThisKeyword);
				regexes.Add(new Regex("^null"), TokenTypes.Null);
				regexes.Add(new Regex("^new"), TokenTypes.New);
				regexes.Add(new Regex("^,"), TokenTypes.Comma);
				regexes.Add(new Regex("^\\["), TokenTypes.ArrayOpenBracket);
				regexes.Add(new Regex("^\\]"), TokenTypes.ArrayCloseBracket);
				regexes.Add(new Regex("^(true|false)"), TokenTypes.BoolLiteral);
				regexes.Add(new Regex("^\\d+"), TokenTypes.IntLiteral);
				regexes.Add(new Regex("^[A-Za-z_]\\w*"), TokenTypes.Indentifier);
			}
			List<Token> tokens = new List<Token>();
			while(true)
			{ 
				Dictionary<Match,Regex> matches = new Dictionary<Match, Regex>();
				foreach (var pair in regexes)
				{
					var match = pair.Key.Match(Program.ToString());
					if(match.Success)
                    {
						matches.Add(match,pair.Key);
                    }
				}
				if (matches.Count == 0)
				{
					if(Program.Length > 0)
                    {
						throw new Exception($"Unknown expression on line {currentLineNumber}");
                    }
					return tokens;
				}
				var lexeme = matches.Keys.ElementAt(0).Value;
				var tokenType = regexes[matches.Values.ElementAt(0)];
				if(tokenType == TokenTypes.Comment || (tokenType == TokenTypes.WhiteSpace && lexeme.StartsWith('\n')))
                {
					currentLineNumber++;
                }
				tokens.Add(new Token(tokenType, lexeme));
				Program = Program.Slice(lexeme.Length);
			}
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
			string program = File.ReadAllText("LinkedList.dyl");
			var tokens = Tokenizer.Tokenize(program);
			foreach(var token in tokens)
            {
				if (token.TokenType == TokenTypes.WhiteSpace) continue;
				Console.Write($"Token: {token.TokenType}");
				Console.SetCursorPosition(35, Console.CursorTop);
				Console.WriteLine($"Lexeme: {token.Lexeme}");
            }
        }
    }
}
