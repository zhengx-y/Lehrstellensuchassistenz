using System;
using System.Windows.Controls;

namespace Lehrstellensuchassistenz.Services
{
    public class NavigationService
    {
        private readonly Frame _mainFrame;

        public NavigationService(Frame mainFrame)
        {
            _mainFrame = mainFrame ?? throw new ArgumentNullException(nameof(mainFrame));
        }

        public void NavigateTo(Page page, bool clearHistory = false)
        {
            if (page == null) return;
            if (_mainFrame.Content == page) return;

            _mainFrame.Navigate(page);

            if (clearHistory)
            {
                _mainFrame.Navigated += ClearHistoryAfterNavigated;
            }
        }

        private void ClearHistoryAfterNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            _mainFrame.Navigated -= ClearHistoryAfterNavigated;
            while (_mainFrame.CanGoBack)
            {
                _mainFrame.RemoveBackEntry();
            }
        }

        public void GoBack()
        {
            if (_mainFrame.CanGoBack)
            {
                _mainFrame.GoBack();
            }
        }

        // OpenSearchPortal wurde entfernt, da das MainWindow jetzt direkt BrowserService.OpenUrl nutzt.
    }
}