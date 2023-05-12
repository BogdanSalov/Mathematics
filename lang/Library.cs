using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lang
{
	class Function
	{
		public delegate Value BasicFunction(List<Value> args);
		
		public BasicFunction function;
		public int count;
		
		public Function(BasicFunction function, int count)
		{
			this.function = function;
			this.count = count;
		}
	}
	
	abstract class Library
	{
		protected Dictionary<string, Function> functions;
		protected Dictionary<string, Value> variables;
		
		public Library()
		{
			this.functions = new Dictionary<string, Function>();
			this.variables = new Dictionary<string, Value>();
			
			this.Install();
		}
		
		public void Add(string name, Function function)
		{
			functions.Add(name, function);
		}
		
		public void Add(string name, Value value)
		{
			variables.Add(name, value);
		}
		
		public bool IsFunction(string name)
		{
			return functions.ContainsKey(name);
		}
		
		public bool IsVariable(string name)
		{
			return variables.ContainsKey(name);
		}
		
		public Value Execute(string name, List<Value> args)
		{
			if(functions.ContainsKey(name))
			{
				Function function = functions[name];
				int count = args.Count;
				
				if(function.count == -1 || count == function.count)
				{
					return function.function(args);
				}
				else
				{
					throw new ArgumentException("Wrong number of function parameters");
				}
			}
			else if(variables.ContainsKey(name))
			{
				return variables[name];
			}
			else
			{
				throw new Exception("Such a function does not exist");
			}
		}
		
		public abstract void Install();
	}
	
	class StdLibrary : Library
	{
		public override void Install()
		{
			this.Add("print", new Function(Print, -1));
			this.Add("input", new Function(Input, -1));
			this.Add("count", new Function(Count, 1));
			
			this.Add("add", new Function(Add, -1));
			this.Add("sort", new Function(Sort, 1));
			this.Add("array", new Function(CreateArray, -1));
			this.Add("vector", new Function(CreateVector, 2));
		}
		
		public Value CreateArray(List<Value> args)
		{
			return new Array(args);
		}
		
		public Value CreateVector(List<Value> args)
		{
			return new Vector(args[0].GetFloat(), args[1].GetFloat());
		}
		
		public Value Sort(List<Value> args)
		{
			List<Value> array = args[0].GetArray();
			
			array.Sort();
			
			return Value.True;
		}
		
		public Value Print(List<Value> args)
		{
			string str = "";

			for(int i = 0; i < args.Count; i++)
			{
				str += args[i].ToString();
			}

			Console.Write(str);
			
			return Value.Zero;
		}

		public Value Input(List<Value> args)
		{
			string t = "";
			
			for(int i = 0; i < args.Count; i++)
			{
				t += args[i].ToString();
			}
			
			Console.WriteLine(t);
			
			string input = Console.ReadLine();
			
			long integer = 0;
			
			if(long.TryParse(input, out integer))
			{
				return new Integer(integer);
			}
			
			double real = 0.0;
			
			if(double.TryParse(input, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"), out real))
			{
				return new Float(real);
			}
			
			return new String(input);
		}

		public Value Count(List<Value> args)
		{
			if(args[0].type == ValueType.Array)
			{
				return new Integer(args[0].GetArray().Count);
			}

			throw new Exception("не масив");
		}
		
		public Value Add(List<Value> args)
		{
			List<Value> array = args[0].GetArray();
			
			for(int i = 1; i < args.Count; i++)
			{
				array.Add(args[i]);
			}
			
			return Value.Zero;
		}
	}
	
	class MathLibrary : Library
	{
		public override void Install()
		{
			this.Add("abs", new Function(Abs, 1));
			this.Add("min", new Function(Min, 2));
			this.Add("max", new Function(Max, 2));
			this.Add("not", new Function(Not, 1));

			this.Add("sin", new Function(Sin, 1));
			this.Add("cos", new Function(Cos, 1));
			this.Add("tan", new Function(Tan, 1));
			this.Add("cot", new Function(Cot, 1));
			this.Add("sqrt", new Function(Sqrt, 1));
			this.Add("exp", new Function(Exp, 1));
			this.Add("log", new Function(Log, 1));
			
			this.Add("mod", new Function(Mod, 2));
			this.Add("fact", new Function(Factorial, 1));
			this.Add("mean", new Function(Mean, 1));
			
			this.Add("pi", new Float(Math.PI));
			this.Add("e", new Float(Math.E));
		}
		
		public Value Abs(List<Value> args)
		{
			return new Float(Math.Abs(args[0].GetFloat()));
		}
		
		public Value Log(List<Value> args)
		{
			return new Float(Math.Log(args[0].GetFloat()));
		}

		public Value Min(List<Value> args)
		{
			return new Float(Math.Min(args[0].GetFloat(), args[1].GetFloat()));
		}

		public Value Max(List<Value> args)
		{
			return new Float(Math.Max(args[0].GetFloat(), args[1].GetFloat()));
		}

		public Value Not(List<Value> args)
		{
			return new Float(args[0].GetFloat() == 0 ? 1 : 0);
		}

		public Value Sin(List<Value> args)
		{
			return new Float(Math.Sin(args[0].GetFloat()));
		}

		public Value Cos(List<Value> args)
		{
			return new Float(Math.Cos(args[0].GetFloat()));
		}

		public Value Tan(List<Value> args)
		{
			return new Float(Math.Tan(args[0].GetFloat()));
		}

		public Value Cot(List<Value> args)
		{
			return new Float(Math.Tan(Math.PI / 2 - args[0].GetFloat()));
		}

		public Value Sqrt(List<Value> args)
		{
			return new Float(Math.Sqrt(args[0].GetFloat()));
		}

		public Value Exp(List<Value> args)
		{
			return new Float(Math.Exp(args[0].GetFloat()));
		}
		
		public Value Mod(List<Value> args)
		{
			return new Float(args[0].GetFloat() % args[1].GetFloat());
		}
		
		public Value Factorial(List<Value> args)
		{
			long value = args[0].GetInteger();
			long result = 1;
			
			for(long i = value; i > 0; i--)
			{
				result *= i;
			}
			
			return new Integer(result);
		}
		
		public Value Mean(List<Value> args)
		{
			double result = 0.0;
			List<Value> array = args[0].GetArray();
			
			for(int i = 0; i < array.Count; i++)
			{
				result += array[i].GetFloat();
			}
			
			return new Float(result / array.Count);
		}
	}
}