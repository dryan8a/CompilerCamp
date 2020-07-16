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
			List<(Regex, TokenTypes)> regexes = new List<(Regex, TokenTypes)>()
			{
				(new Regex("^(\\/\\/.*\n?|\\/\\*(.|\n)*\\*\\/)"), TokenTypes.Comment),
				(new Regex("^\"(?(?=\\\\)\\\\[ab\\\\0tnfrv\'\"]|[^\"\n\t])+\""), TokenTypes.StringLiteral),
				(new Regex("^\'(?(?=\\\\)\\\\[ab\\\\0tnfrv\'\"]|[^'\n\t])\'"), TokenTypes.CharLiteral),
				(new Regex("^namespace"), TokenTypes.Namespace),
				(new Regex("^method"), TokenTypes.Function),
				(new Regex("^class"), TokenTypes.Class),
				(new Regex("^var"), TokenTypes.VariableInitialization),
				(new Regex("^(\\[public\\]|\\[private\\])"), TokenTypes.AccessModifier),
				(new Regex("^entrypoint"), TokenTypes.EntryPointMarker),
				(new Regex("^static"), TokenTypes.StaticMarker),
				(new Regex("^(int|bool|string|char|void)"), TokenTypes.Type),
				(new Regex("^\\{"), TokenTypes.OpenRegion),
				(new Regex("^\\}"), TokenTypes.CloseRegion),
				(new Regex("^;"), TokenTypes.Semicolon),
				(new Regex("^\\."), TokenTypes.MemberAccess),
				(new Regex("^(==|!=|<=|>=|<|>)"), TokenTypes.Comparer),
				(new Regex("^(\\+|-|\\*|/|%|&|\\||\\^)="), TokenTypes.AssignmentMathOperand),
				(new Regex("^="), TokenTypes.SetVariable),
				(new Regex("^(&&|\\|\\||\\^\\^|!)"), TokenTypes.LogicalOperand),
				(new Regex("^(\\(-\\)|~)"), TokenTypes.UnaryMathOperand),
				(new Regex("^(\\+\\+|--)"), TokenTypes.IncrementOrDecrement),
				(new Regex("^(\\+|-|\\*|/|%)"), TokenTypes.BinaryMathOperand),
				(new Regex("^\\("), TokenTypes.OpenParenthesis),
				(new Regex("^\\)"), TokenTypes.CloseParenthesis),
				(new Regex("^if"), TokenTypes.IfStatement),
				(new Regex("^else"), TokenTypes.ElseStatement),
				(new Regex("^while"), TokenTypes.WhileLoop),
				(new Regex("^for"), TokenTypes.ForLoop),
				(new Regex("^\\s"), TokenTypes.WhiteSpace),
				(new Regex("^return"), TokenTypes.Return),
				(new Regex("^continue|break"), TokenTypes.LoopUtilityKeyword),
				(new Regex("^this"), TokenTypes.ThisKeyword),
				(new Regex("^null"), TokenTypes.Null),
				(new Regex("^new"), TokenTypes.New),
				(new Regex("^,"), TokenTypes.Comma),
				(new Regex("^\\["), TokenTypes.ArrayOpenBracket),
				(new Regex("^\\]"), TokenTypes.ArrayCloseBracket),
				(new Regex("^(true|false)"), TokenTypes.BoolLiteral),
				(new Regex("^\\d+"), TokenTypes.IntLiteral),
				(new Regex("^[A-Za-z_]\\w*"), TokenTypes.Indentifier),
			};
			List<Token> tokens = new List<Token>();
			while(true)
			{
				bool didAdd = false;
				foreach (var (regex, tokenType) in regexes)
				{
					var match = regex.Match(Program.ToString());
					if(match.Success)
                    {
						var lexeme = match.Value;
						if (tokenType == TokenTypes.Comment || (tokenType == TokenTypes.WhiteSpace && lexeme.StartsWith('\n')))
						{
							currentLineNumber++;
						}
						TrimUnnecessaryTokenFromEnd(tokens);
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
		private static void TrimUnnecessaryTokenFromEnd(List<Token> tokens)
        {
			if(tokens.Count != 0 && (tokens[tokens.Count-1].TokenType == TokenTypes.Comment || (tokens[tokens.Count-1].TokenType == TokenTypes.WhiteSpace && !tokens[tokens.Count-1].Lexeme.StartsWith("\n"))))
            {
				tokens.RemoveAt(tokens.Count - 1);
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