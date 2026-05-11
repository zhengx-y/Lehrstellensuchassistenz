using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace Lehrstellensuchassistenz.Services
{
    public class AutostartService
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "Lehrstellensuchassistenz";

        public void SetAutostart(bool enable)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, true))
                {
                    if (enable)
                    {
                        // Pfad zur aktuellen .exe holen
                        string exePath = Process.GetCurrentProcess().MainModule.FileName;
                        key.SetValue(AppName, $"\"{exePath}\"");
                    }
                    else
                    {
                        // Eintrag entfernen, falls vorhanden
                        if (key.GetValue(AppName) != null)
                        {
                            key.DeleteValue(AppName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Autostart-Fehler: " + ex.Message);
            }
        }

        public bool IsAutostartEnabled()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, false))
            {
                return key.GetValue(AppName) != null;
            }
        }
    }
}