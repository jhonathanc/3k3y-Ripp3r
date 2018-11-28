using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Ripp3r.Controls;
using Ripp3r.Properties;

namespace Ripp3r
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Settings.Default.UpdateSettingsRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettingsRequired = false;
                Settings.Default.Save();
            }

//            UnhandledExceptionDlg dlg = new UnhandledExceptionDlg();
//            dlg.OnShowErrorReport += ShowErrorReport;
//            dlg.OnSendExceptionClick += SendException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static void SendException(object sender, SendExceptionClickEventArgs args)
        {
            // Send the exception to the server
            try
            {
                new ExceptionSender(CreateException(args.UnhandledException)).Send();

                if (!args.RestartApp) return;

                Process.Start(Application.ExecutablePath);
                Application.Exit();
            }
            catch
            {
                // Yes, catch everything
            }
        }

        private static void ShowErrorReport(object sender, SendExceptionClickEventArgs args)
        {
            using (ShowException showException = new ShowException(CreateException(args.UnhandledException)))
            {
                showException.ShowDialog();
            }
        }

        private static string CreateException(Exception ex)
        {
            StringBuilder sb = new StringBuilder(ex.ToString());

            sb.AppendLine().AppendLine().AppendLine("Loaded assemblies: ");

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                sb.AppendLine(new string('-', 50));
                sb.Append("Name: ").AppendLine(assembly.GetName().Name);
                sb.Append("Full name: ").AppendLine(assembly.FullName);
            }
            sb.AppendLine(new string('-', 50));
            return sb.ToString();
        }
    }
}
