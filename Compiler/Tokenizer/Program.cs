using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

namespace Tokenizer
{
	
    public static class Tokenizer
    {
		public static List<Token> Tokenize(ReadOnlySpan<char> Program)
		{
			if (Program.Length == 0) return null;
			int currentLineNumber = 1;
			(int Paren, int Curly, int Square) BracketCount = (0, 0, 0);
			//Reminder to change dictionary into a list of tuples
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
				bool didAdd = false;
				foreach (var pair in regexes)
				{
					var match = pair.Key.Match(Program.ToString());
					if(match.Success)
                    {
						var lexeme = match.Value;
						var tokenType = pair.Value;
						if (tokenType == TokenTypes.Comment || (tokenType == TokenTypes.WhiteSpace && lexeme.StartsWith('\n')))
						{
							currentLineNumber++;
						}
						BracketCount = AddToBracketCount(BracketCount, tokenType);
						tokens.Add(new Token(tokenType, lexeme));
						Program = Program.Slice(lexeme.Length);
						didAdd = true;
						break;
					}
				}
				if (!didAdd)
				{
					if(Program.Length > 0) throw new Exception($"Unknown expression on line {currentLineNumber}");
					if (BracketCount.Paren != 0) throw new Exception("Unmatched Parenthesis found");
					if (BracketCount.Curly != 0) throw new Exception("Unmatched Curly Bracket found");
					if (BracketCount.Square != 0) throw new Exception("Unmatched Square Bracket found");
					return tokens;
				}
			}
        }

		private static (int,int,int) AddToBracketCount((int Paren,int Curly,int Square) BracketCount, TokenTypes type)
        {
			switch(type)
            {
				case TokenTypes.OpenParenthesis:
					BracketCount.Paren++;
					break;
				case TokenTypes.CloseParenthesis:
					BracketCount.Paren--;
					break;
				case TokenTypes.OpenRegion:
					BracketCount.Curly++;
					break;
				case TokenTypes.CloseRegion:
					BracketCount.Curly--; 
					break;
				case TokenTypes.ArrayOpenBracket:
					BracketCount.Square++;
					break;
				case TokenTypes.ArrayCloseBracket:
					BracketCount.Square--;
					break;
			}
			return BracketCount;
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