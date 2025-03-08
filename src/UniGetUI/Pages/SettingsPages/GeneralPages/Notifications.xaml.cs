using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using UniGetUI.Core.Tools;
using UniGetUI.Core.SettingsEngine;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UniGetUI.Pages.SettingsPages.GeneralPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Notifications : Page, ISettingsPage
    {
        public Notifications()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (Settings.Get("DisableSystemTray"))
            {
                ToolbarText.Visibility = Visibility.Visible;
                DisableNotifications.IsEnabled = false;
                DisableUpdatesNotifications.IsEnabled = false;
                DisableErrorNotifications.IsEnabled = false;
                DisableSuccessNotifications.IsEnabled = false;
                DisableProgressNotifications.IsEnabled = false;
            }
            else
            {
                ToolbarText.Visibility = Visibility.Collapsed;
                DisableNotifications.IsEnabled = true;
                DisableUpdatesNotifications.IsEnabled = true;
                DisableErrorNotifications.IsEnabled = true;
                DisableSuccessNotifications.IsEnabled = true;
                DisableProgressNotifications.IsEnabled = true;
            }
            base.OnNavigatedTo(e);
        }

        public bool CanGoBack => true;
        public string ShortTitle => CoreTools.Translate("Notification preferences");

        public event EventHandler? RestartRequired;
        public event EventHandler<Type>? NavigationRequested;
    }
}
