using System;
using System.Windows.Forms;

namespace XGRAB;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		try
		{
			Application.SetHighDpiMode(HighDpiMode.SystemAware);
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			Application.Run(new auth());
		}
		catch (Exception ex)
		{
			MessageBox.Show("Error loading DLL: " + ex.Message);
		}
	}
}
