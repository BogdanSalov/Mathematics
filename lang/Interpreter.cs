using System;
using System.Collections.Generic;

namespace Lang
{
	public class Interpreter
	{
		private Token prevToken;
		private Token lastToken;
		
		private Dictionary<string, Value> variables;
		private Dictionary<string, int> loops;
		private List<Token> tokens;
		private Lexer lexer;
		
		private int index;
		private int blockCounter;
		
		private Dictionary<string, Library> library = new Dictionary<string, Library>();
		
		Dictionary<TokenType, int> precedens = new Dictionary<TokenType, int>()
		{
			{ TokenType.Or, 0 }, { TokenType.And, 0 },
			{ TokenType.Equal, 1 }, { TokenType.NotEqual, 1 },
			{ TokenType.Less, 1 }, { TokenType.More, 1 },
			{ TokenType.LessEqual, 1 },	 { TokenType.MoreEqual, 1 },
			{ TokenType.Plus, 2 }, { TokenType.Minus, 2 },
			{ TokenType.Asterisk, 3 }, {TokenType.Slash, 3 },
			{ TokenType.Caret, 4 }
		};

		public Interpreter()
		{
			this.Initialized();
		}
		
		public void Initialized()
		{
			this.variables = new Dictionary<string, Value>();
			this.loops = new Dictionary<string, int>();
			this.lexer = new Lexer();
			
			this.library.Add("std", new StdLibrary());
			this.library.Add("math", new MathLibrary());
			
			this.blockCounter = 0;
			this.index = 0;
			
			this.lastToken = Token.Unknown;
			this.prevToken = Token.Unknown;
		}
		
		public void ClearState()
		{
			this.variables.Clear();
			this.loops.Clear();
			
			this.blockCounter = 0;
			this.index = 0;
			
			this.lastToken = Token.Unknown;
			this.prevToken = Token.Unknown;
		}

		public void SetVariable(string name, Value value)
		{
			if(variables.ContainsKey(name))
			{
				variables[name] = value;
			}
			else
			{
				variables.Add(name, value);
			}
		}
		
		public Value GetVariable(string name)
		{
			if(variables.ContainsKey(name))
			{
				return variables[name];
			}
			
			throw new Exception("Variable with name " + name + " does not exist.");
		}
		
		public Token NextToken()
		{
			prevToken = lastToken;
			lastToken = tokens[index++];

			if(lastToken == Token.Eof && prevToken == Token.Eof)
			{
				Error("Unexpected end of file");
			}

			return lastToken;
		}
		
		public void Execute(string source)
		{
			this.tokens = lexer.GetAllToken(source);
			this.index = 0;
			
			this.NextToken();
			
			while(true)
			{
				if(lastToken == Token.Eof)
				{
					return;
				}
				else if(lastToken == Token.NewLine)
				{
					NextToken();
					continue;
				}
				
				Statement(lastToken);
				
				if(lastToken != Token.NewLine && lastToken != Token.Eof)
				{
					Error("Expect new line got " + lastToken.type.ToString());
				}
			}
		}
		
		void Error(string text)
		{
			throw new Exception(text);
		}

		void Match(TokenType token)
		{
			if(lastToken != token)
			{
				Error("Expect " + token.ToString() + " got " + lastToken.type.ToString());
			}
		}
		
		void Statement(Token token)
		{
			NextToken();
			
			switch (token.type)
			{
				case TokenType.If: If(); break;
				case TokenType.Else: Else(); break;
				case TokenType.End: break;
				case TokenType.Break: Break(); break;
				case TokenType.For: For(); break;
				case TokenType.Next: Next(); break;
				case TokenType.Identifier:
					if (lastToken == TokenType.Equal) Let(token.value);
					else if (lastToken == TokenType.LParen) Call("std", token.value);
					else if (lastToken == TokenType.Dot) LibraryCall(token.value);
					else if (lastToken == TokenType.LSquare) SetArray(token.value);
					else goto default;
					break;
				case TokenType.EOF:
					return;
				default:
					Error("Expect keyword got " + token.type.ToString());
					break;
			}
		}
		
		void SetArray(string name)
		{
			int idx = (int)ParseArgument(TokenType.LSquare, TokenType.RSquare)[0].GetInteger();
			
			NextToken();
			
			GetVariable(name).GetArray()[idx] = Expr();
		}
		
		Value LibraryCall(string name)
		{
			string function = NextToken().value;
			NextToken();
			return Call(name, function);
		}
		
		Value Call(string nameLibrary, string nameFunction)
		{
			if(lastToken == TokenType.LParen)
			{
				return library[nameLibrary].Execute(nameFunction, ParseArgument(TokenType.LParen, TokenType.RParen));
			}
			
			return library[nameLibrary].Execute(nameFunction, null);
		}

		void If()
		{
			bool result = Expr() == Value.False;
			
			Match(TokenType.Then);
			NextToken();

			if(result)
			{
				int i = blockCounter;
				
				while(true)
				{
					if(lastToken == TokenType.If)
					{
						i++;
					}
					else if(lastToken == TokenType.Else)
					{
						if(i == blockCounter)
						{
							NextToken();
							return;
						}
					}
					else if(lastToken == TokenType.End)
					{
						if(i == blockCounter)
						{
							NextToken();
							return;
						}
						
						i--;
					}
					
					NextToken();
				}
			}
		}

		void Else()
		{
			int i = blockCounter;
			
			while(true)
			{
				if(lastToken == TokenType.If)
				{
					i++;
				}
				else if (lastToken == TokenType.End)
				{
					if (i == blockCounter)
					{
						NextToken();
						return;
					}
					
					i--;
				}
				
				NextToken();
			}
		}

		void Let(string name)
		{
			if (lastToken != TokenType.Equal)
			{
				Match(TokenType.Identifier);
				NextToken();
				Match(TokenType.Equal);
			}
			
			NextToken();
			
			if(lastToken == TokenType.LSquare)
			{
				SetVariable(name, new Array(ParseArgument(TokenType.LSquare, TokenType.RSquare)));
			}
			else
			{
				SetVariable(name, Expr());
			}
		}
		
		void For()
		{
			int temp = index - 2;
			Match(TokenType.Identifier);
			string var = lastToken.value;
			NextToken();
			
			if(lastToken == TokenType.Equal)
			{
				ForIndexer(var, temp);
			}
			else if(lastToken == TokenType.Colon)
			{
				Foreach(var, temp);
			}
			/*
			Match(TokenType.Equal);
			NextToken();
			Value v = Expr();
			
			if (loops.ContainsKey(var))
			{
				loops[var] = temp;
			}
			else
			{
				SetVariable(var, v);
				loops.Add(var, temp);
			}
			
			Match(TokenType.To);
			NextToken();
			
			if (Expr().GetInteger() ==  0)
			{
				while (true)
				{
					while (!(NextToken() == TokenType.Identifier && prevToken == TokenType.Next));
					if (lastToken.value == var)
					{
						loops.Remove(var);
						NextToken();
						Match(TokenType.NewLine);
						break;
					}
				}
			}*/
		}
		
		void ForIndexer(string var, int temp)
		{
			Match(TokenType.Equal);
			NextToken();
			Value v = Expr();
			
			if (loops.ContainsKey(var))
			{
				loops[var] = temp;
			}
			else
			{
				SetVariable(var, v);
				loops.Add(var, temp);
			}
			
			Match(TokenType.To);
			NextToken();
			
			if (Expr().GetInteger() ==  0)
			{
				while (true)
				{
					while (!(NextToken() == TokenType.Identifier && prevToken == TokenType.Next));
					if (lastToken.value == var)
					{
						loops.Remove(var);
						NextToken();
						Match(TokenType.NewLine);
						break;
					}
				}
			}
		}
		
		void Foreach(string var, int temp)
		{
			Match(TokenType.Equal);
			NextToken();
			Value v = Expr();
			
			if (loops.ContainsKey(var))
			{
				loops[var] = temp;
			}
			else
			{
				SetVariable(var, v);
				loops.Add(var, temp);
			}
			
			Match(TokenType.To);
			NextToken();
			
			if (Expr().GetInteger() ==  0)
			{
				while (true)
				{
					while (!(NextToken() == TokenType.Identifier && prevToken == TokenType.Next));
					if (lastToken.value == var)
					{
						loops.Remove(var);
						NextToken();
						Match(TokenType.NewLine);
						break;
					}
				}
			}
		}
		
		void Break()
		{
			while (!(NextToken() == TokenType.Identifier && prevToken == TokenType.Next))
			NextToken();
		}

		void Next()
		{
			Match(TokenType.Identifier);
			string var = lastToken.value;
			
			if(tokens[loops[var] + 2] == TokenType.Colon)
			{
				//Forech
				string arr = tokens[loops[var] + 2].value;
				Console.WriteLine(arr);
			}
			else
			{
				//For
				variables[var] = variables[var].BinaryOperation(TokenType.Plus, Value.True);
			}
			
			index = loops[var];
			lastToken = new Token(TokenType.NewLine);
		}

		Value Expr(int min = 0)
		{
			Value lhs = Primary();

			while (true)
			{
				if (lastToken.type < TokenType.Plus || lastToken.type > TokenType.And || precedens[lastToken.type] < min)
					break;

				Token op = lastToken;
				int prec = precedens[lastToken.type];
				int assoc = 0;
				int nextmin = assoc == 0 ? prec : prec + 1;
				NextToken();
				Value rhs = Expr(nextmin);
				lhs = lhs.BinaryOperation(op.type, rhs);
			}

			return lhs;
		}
		
		List<Value> ParseArgument(TokenType begin, TokenType end, TokenType separator = TokenType.Comma)
		{
			List<Value> args = new List<Value>();
			
			Match(begin);
			
			do
			{
				if(NextToken() != end)
				{
					args.Add(Expr());
				}
			} while(lastToken == separator);
			
			NextToken();
			
			return args;
		}
		
		Value GetValue(string value)
		{
			long integer = 0;
			
			if(long.TryParse(value, out integer))
			{
				return new Integer(integer);
			}
			
			double real = 0.0;
			
			if(double.TryParse(value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"), out real))
			{
				return new Float(real);
			}
			
			return new String(value);
		}
		
		Value Primary()
		{
			Value prim = Value.Zero;

			if(lastToken == TokenType.Value)
			{
				prim = GetValue(lastToken.value);
				NextToken();
			}
			else if(lastToken == TokenType.True)
			{
				NextToken();
				return Value.True;
			}
			else if(lastToken == TokenType.False)
			{
				NextToken();
				return Value.False;
			}
			else if(lastToken == TokenType.Identifier)
			{
				string name = lastToken.value;
				NextToken();
				
				if(lastToken == TokenType.LParen)
				{
					return library["std"].Execute(name, ParseArgument(TokenType.LParen, TokenType.RParen));
				}
				else if(lastToken == TokenType.LSquare)
				{
					List<Value> array = GetVariable(name).GetArray();
					
					int idx = (int)ParseArgument(TokenType.LSquare, TokenType.RSquare)[0].GetInteger();
						
					if(idx >= array.Count || idx < 0)
					{
						throw new Exception("Invalid array index");
					}
					
					return array[idx];
				}
				else if(lastToken == TokenType.Dot)
				{
					return LibraryCall(name);
				}
				else
				{
					return GetVariable(name);
				}
			}
			else if (lastToken == TokenType.Plus || lastToken == TokenType.Minus || lastToken == TokenType.Not)
			{
				Token op = lastToken;
				NextToken();
				prim = Primary().UnaryOperation(op.type);
			}
			else if (lastToken == TokenType.LParen)
			{
				NextToken();
				prim = Expr();
				Match(TokenType.RParen);
				NextToken();
			}
			else
			{
				Error("Неочікуваний токен в первинному");
			}
			
			return prim;
		}
	}
}