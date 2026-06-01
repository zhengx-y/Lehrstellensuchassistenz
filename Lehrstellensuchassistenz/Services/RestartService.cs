using System;
using System.Windows; // Wichtig für Application.Current

namespace Lehrstellensuchassistenz.Services
{
    // Klasse auf public setzen
    public static class RestartService
    {
        // Methode auf public setzen
        public static void RestartApplication()
        {
            try
            {
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath)) exePath = Environment.ProcessPath;

                if (!string.IsNullOrEmpty(exePath))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true
                    });
                    Environment.Exit(0);
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
            catch
            {
                Environment.Exit(0);
            }
        }
    }
}