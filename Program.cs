using System;
using System.Collections.Generic;
using System.Windows.Forms;

static class Program
{
    /// <summary>
    /// The random source for this program.
    /// </summary>
    public static Random Random = new Random();

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Window window = new Window();
        window.Show();
        

        DateTime last = DateTime.Now;
        while (window.Visible)
        {
            window.Update(1.0 / 60.0);
            Application.DoEvents();
        }
    }
}