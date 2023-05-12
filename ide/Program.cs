using System;
using System.Windows;
using System.IO;
using System.Text;

namespace ide
{
	public partial class Program
	{
		[STAThread]
		public static void Main()
		{
			Application app = new Application();
			
			app.Run(new MainWindow());
		}
	}
}