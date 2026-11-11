using System;
using System.IO;
using System.Windows.Forms;

namespace PharmacyInventoryManagementSystem;

internal static class Program
{
    private static readonly string SessionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.txt");

    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            Database.Initialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to initialize the database:\n" + ex.Message,
                "Startup error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return;
        }

        while (true)
        {
            string loggedInName = "";
            string loggedInRole = "";
            bool sessionExists = false;

            if (File.Exists(SessionPath))
            {
                try
                {
                    var lines = File.ReadAllLines(SessionPath);
                    if (lines.Length >= 2)
                    {
                        loggedInName = lines[0];
                        loggedInRole = lines[1];
                        sessionExists = true;
                    }
                }
                catch { }
            }

            if (sessionExists)
            {
                using (var splash = new LoadingForm("Loading dashboard..."))
                {
                    splash.ShowDialog();
                }
            }
            else
            {
                using (var splash = new LoadingForm("Initializing system..."))
                {
                    splash.ShowDialog();
                }

                using (var login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK)
                        return;

                    loggedInName = login.LoggedInName;
                    loggedInRole = login.LoggedInRole;
                }

                using (var splash = new LoadingForm("Preparing dashboard..."))
                {
                    splash.ShowDialog();
                }
            }

            bool logoutRequested;

            using (var main = new MainForm(loggedInName, loggedInRole))
            {
                Application.Run(main);
                logoutRequested = main.LogoutRequested;
            }

            if (logoutRequested)
            {
                try
                {
                    if (File.Exists(SessionPath)) File.Delete(SessionPath);
                }
                catch { }
            }
            else
            {
                return;
            }
        }
    }
}