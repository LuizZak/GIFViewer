using System;
using System.Linq;
using System.Windows.Forms;
using GIF_Viewer.Utils;
using Microsoft.VisualBasic.ApplicationServices;

namespace GIF_Viewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Setup and run the application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FormMain form = new FormMain(args);

            // Load the program settings
            Settings.Instance.LoadSettings();

            // Create the application base.
            WindowsApplicationBase applicationBase = new WindowsApplicationBase(form, Settings.Instance.SingleInstance);
            applicationBase.StartupNextInstance += (sender, eventArgs) =>
            {
                if (!Settings.Instance.SingleInstance)
                    return;

                eventArgs.BringToForeground = true;
                form.TopMost = true;
                form.ProcessCommandLine(eventArgs.CommandLine.ToArray());
                form.BringToFront();
                form.TopMost = false;
                form.Focus();
            };

            applicationBase.Run(args);
        }
    }

    /// <summary>
    /// Class used to manage singleton instances of the application
    /// </summary>
    public class WindowsApplicationBase : WindowsFormsApplicationBase
    {
        /// <summary>
        /// Initializes a new instance of the WindowsApplicationBase class
        /// </summary>
        /// <param name="form">The target form to open</param>
        /// <param name="singleInstance">Whether the application should be single-instance</param>
        public WindowsApplicationBase(Form form, bool singleInstance)
        {
            IsSingleInstance = singleInstance;
            MainForm = form;
        }
    }
}