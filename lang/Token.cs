namespace Lang
{
	public enum TokenType
	{
		Unknown,
	
		Identifier,
		Value,
		
		If,
		Then,
		Else,
		For,
		To,
		Next,
		Function,
		While,
		End,
		True,
		False,
		Break,
		
		NewLine,
		Colon,
		Semicolon,
		Comma,
		Dot,
	
		Plus,
		Minus,
		Slash,
		Asterisk,
		Caret,
		Equal,
		Less,
		More,
		NotEqual,
		LessEqual,
		MoreEqual,
		Or,
		And,
		Not,
	
		LParen,
		RParen,
		LSquare,
		RSquare,
	
		EOF = -1
	}
	
	public class Token
	{
		public TokenType type;
		public string value;
		
		public static readonly Token Unknown = new Token(TokenType.Unknown);
		public static readonly Token NewLine = new Token(TokenType.NewLine);
		public static readonly Token Eof = new Token(TokenType.EOF);
		
		public Token(TokenType type)
		{
			this.type = type;
		}
		
		public Token(TokenType type, string value)
		{
			this.type = type;
			this.value = value;
		}
		
		public static bool operator==(Token token, TokenType type)
		{
			return token.type == type;
		}
		
		public static bool operator!=(Token token, TokenType type)
		{
			return token.type != type;
		}
		
		public static bool operator==(Token token, Token type)
		{
			return token.type == type.type;
		}
		
		public static bool operator!=(Token token, Token type)
		{
			return token.type != type.type;
		}
	}
}