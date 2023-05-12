using System;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Lang
{
	class Program
	{
		public static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			Console.InputEncoding = Encoding.UTF8;
			
			Interpreter interpreter = new Interpreter();
			
			while(true)
			{
				try
				{
					while(true)
					{
						string line = Console.ReadLine();
						string[] source = line.Split(' ');
						
						if(string.Compare(source[0], "clear") == 0)
						{
							interpreter.ClearState();
						}
						else if(string.Compare(source[0], "exit") == 0)
						{
							return;
						}
						else if(string.Compare(source[0], "start") == 0)
						{
							using(StreamReader reader = new StreamReader(source[1]))
							{
								interpreter.Execute(reader.ReadToEnd());
							}
						}
						else
						{
							interpreter.Execute(line);
						}
						
						Console.WriteLine();
					}
				}
				catch(Exception exception)
				{
					Console.WriteLine(exception.Message);
					continue;
				}
			}
		}
	}
}