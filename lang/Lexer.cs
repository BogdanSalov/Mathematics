using System;
using System.Collections.Generic;

namespace Lang
{	
	class Lexer
	{
		private string source;
		private int position;
		
		public int line;
		
		private Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType> {
			{"if", TokenType.If}, {"then", TokenType.Then}, {"else", TokenType.Else}, {"for", TokenType.For},
			{"to", TokenType.To}, {"next", TokenType.Next}, {"end",  TokenType.End}, {"or", TokenType.Or},
			{"and", TokenType.And}, {"not", TokenType.Not}, {"function", TokenType.Function}, {"while", TokenType.While},
			{"true", TokenType.True}, {"false", TokenType.False}, {"break", TokenType.Break},
		};
		
		private Dictionary<string, TokenType> operators = new Dictionary<string, TokenType> {
			{":", TokenType.Colon}, {";", TokenType.Semicolon}, {",", TokenType.Comma}, {"=", TokenType.Equal},
			{"+", TokenType.Plus}, {"-", TokenType.Minus}, {"/", TokenType.Slash}, {"*", TokenType.Asterisk},
			{"^", TokenType.Caret}, {"(", TokenType.LParen}, {")", TokenType.RParen}, {"[", TokenType.LSquare},
			{"]", TokenType.RSquare}, {"!=", TokenType.NotEqual}, {"<=", TokenType.LessEqual}, {"<", TokenType.Less},
			{">=", TokenType.MoreEqual}, {">", TokenType.More}, {"!", TokenType.Not}, {".", TokenType.Dot},
		};
		
		public Lexer()
		{
			this.position = 0;
			this.line = 1;
		}
		
		public List<Token> GetAllToken(string source)
		{
			List<Token> tokens = new List<Token>();
			
			this.source = source;
			this.position = 0;
			this.line = 0;
			
			while(true)
			{
				Token token = GetToken();
				
				tokens.Add(token);
				
				if(token == Token.Eof)
				{
					return tokens;
				}
			}
		}
		
		char GetChar()
		{
			if(position >= source.Length)
			{
				return '\0';
			}
			
			char symbol = source[position++];
			
			if(symbol == '\n')
			{
				line++;
			}
			
			return symbol;
		}
		
		char CurrentChar()
		{
			if(position >= source.Length)
			{
				return '\0';
			}
			
			return source[position];
		}
		
		char PeekChar()
		{
			if(position >= source.Length)
			{
				return '\0';
			}
			
			return source[position + 1];
		}
		
		Token GetOperator()
		{
			char current = GetChar();
			char next = CurrentChar();
			
			string oper = current.ToString() + next.ToString();
			
			if(operators.ContainsKey(oper))
			{
				GetChar();
				return new Token(operators[oper]);
			}
			
			return new Token(operators[current.ToString()]);
		}
		
		Token GetIdentifier()
		{
			string identifier = GetChar().ToString();
			
			while(true)
			{
				char symbol = CurrentChar();
				
				if(char.IsLetterOrDigit(symbol))
				{
					GetChar();
					identifier += symbol;
				}
				else
				{
					break;
				}
			}
			
			if(keywords.ContainsKey(identifier))
			{
				return new Token(keywords[identifier], identifier);
			}
			
			return new Token(TokenType.Identifier, identifier);
		}
		
		Token GetString()
		{
			string line = "";

			while(true)
			{
				char symbol = GetChar();
				
				if(symbol == '\\')
				{
					switch(GetChar())
					{
						case 'n': line += '\n'; break;
						case 't': line += '\t'; break;
						case '\\': line += '\\'; break;
						case '"': line += '"'; break;
					}
				}
				else if(symbol == '"' || symbol == '\'')
				{
					break;
				}
				else
				{
					line += symbol;
				}
			}
			
			return new Token(TokenType.Value, line);
		}
		
		Token GetNumber()
		{
			string number = "";
			
			while(true)
			{
				char symbol = CurrentChar();
				
				if(char.IsDigit(symbol) || symbol == '.')
				{
					GetChar();
					number += symbol;
				}
				else
				{
					break;
				}
			}

			return new Token(TokenType.Value, number);
		}
		
		Token GetToken()
		{
			while(true)
			{
				char symbol = CurrentChar();
				
				if(symbol == ' ' || symbol == '\t' || symbol == '\r')
				{
					GetChar();
					continue;
				}
				else if(symbol == '\n')
				{
					GetChar();
					return Token.NewLine;
				}
				else if(symbol == '"' || symbol == '\'')
				{
					GetChar();
					return GetString();
				}
				else if(char.IsLetter(symbol))
				{
					return GetIdentifier();
				}
				else if(char.IsDigit(symbol))
				{
					return GetNumber();
				}
				else if(operators.ContainsKey(symbol.ToString()))
				{
					return GetOperator();
				}
				else if(symbol == '#')
				{
					while(true)
					{
						if(GetChar() == '\n')
						{
							return Token.NewLine;
						}
					}
				}
				else if(symbol == '\0')
				{
					return Token.Eof;
				}
				else
				{
					return Token.Unknown;
				}
			}
		}
	}
}