using System.Configuration;
using System.Data;
using System.Windows;

namespace Lehrstellensuchassistenz
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var service = new Lehrstellensuchassistenz.Services.FileService();
            var settings = service.LoadSettings();

            var culture = new System.Globalization.CultureInfo(settings.Language);
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            base.OnStartup(e);
        }
    }

}
