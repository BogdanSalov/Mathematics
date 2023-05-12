using System;
using System.Collections.Generic;

namespace Lang
{
	public enum ValueType
	{
		Integer,
		Float,
		String,
		Array,
		Vector,
	}
	
	public abstract class Value
	{
		public ValueType type;
		
		public static readonly Value Zero = null;
		public static readonly Value True = new Integer(1);
		public static readonly Value False = new Integer(0);
		
		public Value(ValueType type)
		{
			this.type = type;
		}
		
		public virtual Value UnaryOperation(TokenType operation)
		{
			throw new Exception("Унарна операція для цього типу не визначена.");
		}
		
		public virtual Value BinaryOperation(TokenType operation, Value right)
		{
			throw new Exception("Бінарна операція для цього типу не визначена.");
		}
		
		public virtual long GetInteger()
		{
			throw new Exception("Not type");
		}
		
		public virtual double GetFloat()
		{
			throw new Exception("Not type");
		}
		
		public virtual List<Value> GetArray()
		{
			throw new Exception("Not type");
		}
	}
	
	public class Vector : Value
	{
		public Float x;
		public Float y;
		
		public Vector(double x, double y) : base(ValueType.Vector)
		{
			this.x = new Float(x);
			this.y = new Float(y);
		}
		
		public override List<Value> GetArray()
		{
			List<Value> array = new List<Value>();
	
			array.Add(x);
			array.Add(y);
			
			return array;
		}
		
		public override string ToString()
		{
			return string.Format("[{0}, {1}]", x.GetFloat(), y.GetFloat());
		}
	}
	
	public class Integer : Value
	{
		public long value;
		
		public Integer(long value) : base(ValueType.Integer)
		{
			this.value = value;
		}
		
		public override string ToString()
		{
			return value.ToString();
		}
		
		public override long GetInteger()
		{
			return value;
		}
		
		public override double GetFloat()
		{
			return (double)value;
		}
		
		public override Value UnaryOperation(TokenType operation)
		{
			switch(operation)
			{
				case TokenType.Plus: return this;
				case TokenType.Minus: return new Float(-value);
				case TokenType.Not: return value == 0 ? Value.True : Value.False;
			}

			return Zero;
		}
		
		public override Value BinaryOperation(TokenType operation, Value right)
		{
			switch(operation)
			{
				case TokenType.Plus: return new Float(value + right.GetFloat());
				case TokenType.Minus: return new Float(value - right.GetFloat());
				case TokenType.Asterisk: return new Float(value * right.GetFloat());
				case TokenType.Slash: return new Float(value / right.GetFloat());
				case TokenType.Caret: return new Float(Math.Pow(value, right.GetFloat()));
				
				case TokenType.Equal: return value == right.GetFloat() ? True : False;
				case TokenType.NotEqual: return value != right.GetFloat() ? True : False;
				
				case TokenType.Less: return value < right.GetFloat() ? True : False;
				case TokenType.More: return value > right.GetFloat() ? True : False;
				
				case TokenType.LessEqual: return value <= right.GetFloat() ? True : False;
				case TokenType.MoreEqual: return value >= right.GetFloat() ? True : False;
				
				case TokenType.And: return (value != 0) && (right.GetInteger() != 0) ? True : False;
				case TokenType.Or: return (value != 0) || (right.GetInteger() != 0) ? True : False;
			}
			
			return Zero;
		}
	}
	
	public class Float : Value
	{
		public double value;
		
		public Float(double value) : base(ValueType.Float)
		{
			this.value = value;
		}
		
		public override string ToString()
		{
			return value.ToString();
		}
		
		public override Value UnaryOperation(TokenType operation)
		{
			switch (operation)
			{
				case TokenType.Plus: return this;
				case TokenType.Minus: return new Float(-value);
				case TokenType.Not: return value == 0 ? Value.True : Value.False;
			}
			
			return Zero;
		}
		
		public override Value BinaryOperation(TokenType operation, Value right)
		{
			switch(operation)
			{
				case TokenType.Plus: return new Float(value + right.GetFloat());
				case TokenType.Minus: return new Float(value - right.GetFloat());
				case TokenType.Asterisk: return new Float(value * right.GetFloat());
				case TokenType.Slash: return new Float(value / right.GetFloat());
				case TokenType.Caret: return new Float(Math.Pow(value, right.GetFloat()));
				
				case TokenType.Equal: return value == right.GetFloat() ? True : False;
				case TokenType.NotEqual: return value != right.GetFloat() ? True : False;
				
				case TokenType.Less: return value < right.GetFloat() ? True : False;
				case TokenType.More: return value > right.GetFloat() ? True : False;
				
				case TokenType.LessEqual: return value <= right.GetFloat() ? True : False;
				case TokenType.MoreEqual: return value >= right.GetFloat() ? True : False;
				
				case TokenType.And: return (value != 0) && (right.GetFloat() != 0) ? True : False;
				case TokenType.Or: return (value != 0) || (right.GetFloat() != 0) ? True : False;
			}
			
			return Zero;
		}
		
		public override double GetFloat()
		{
			return value;
		}
		
		public override long GetInteger()
		{
			return (long)(value);
		}
	}
	
	public class String : Value
	{
		public string value;
		
		public String(string value) : base(ValueType.String)
		{
			this.value = value;
		}
		
		public override string ToString()
		{
			return value;
		}
		
		public override Value BinaryOperation(TokenType operation, Value right)
		{
			switch(operation)
			{
				case TokenType.Plus: return new String(value + right.ToString());
				case TokenType.Equal: return value == right.ToString() ? True : False;
				case TokenType.NotEqual: return value == right.ToString() ? True : False;
			}
			
			return Zero;
		}
	}
	
	public class Array : Value
	{
		public List<Value> value;

		public Array(List<Value> value) : base(ValueType.Array)
		{
			this.value = value;
		}
		
		public override List<Value> GetArray()
		{
			return value;
		}
		
		public override string ToString()
		{
			string str = "[";

			for(int i = 0; i < value.Count - 1; i++)
			{
				str += value[i] + ", ";
			}

			str += value[value.Count - 1] + "]";

			return str;
		}
	}
}