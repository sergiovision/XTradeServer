using System;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using log4net.Config;

namespace FXMind.WinClient
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            try
            {
                XmlConfigurator.Configure();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                SkinManager.EnableFormSkins();
                UserLookAndFeel.Default.SetSkinStyle("DevExpress Style");

                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public static void LogStatus(string statMessage)
        {
            if (MainForm.statusBar != null) MainForm.statusBar.Caption = statMessage;
        }
    }
}