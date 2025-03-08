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
using UniGetUI.Pages.DialogPages;
using Newtonsoft.Json;
using UniGetUI.Core.Data;
using UniGetUI.Core.Logging;
using UniGetUI.Core.Language;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UniGetUI.Pages.SettingsPages.GeneralPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class General : Page, ISettingsPage
    {
        public General()
        {
            this.InitializeComponent();

            Dictionary<string, string> lang_dict = new(LanguageData.LanguageReference.AsEnumerable());

            foreach (string key in lang_dict.Keys)
            {
                if (key != "en" &&
                    LanguageData.TranslationPercentages.TryGetValue(key, out var translationPercentage))
                {
                    lang_dict[key] = lang_dict[key] + " (" + translationPercentage + ")";
                }
            }

            bool isFirst = true;
            foreach (KeyValuePair<string, string> entry in lang_dict)
            {
                LanguageSelector.AddItem(entry.Value, entry.Key, isFirst);
                isFirst = false;
            }

            LanguageSelector.ShowAddedItems();
        }

        public bool CanGoBack => true;
        public string ShortTitle => CoreTools.Translate("General preferences");

        public event EventHandler? RestartRequired;
        public event EventHandler<Type>? NavigationRequested;

        public void ShowRestartBanner(object sender, EventArgs e)
            => RestartRequired?.Invoke(this, e);

        private void ForceUpdateUniGetUI_OnClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = MainApp.Instance.MainWindow;
            _ = AutoUpdater.CheckAndInstallUpdates(mainWindow, mainWindow.UpdatesBanner, true, false, true);
        }

        private void ManageTelemetrySettings_Click(object sender, EventArgs e)
            => DialogHelper.ShowTelemetryDialog();

        private void ImportSettings(object sender, EventArgs e)
        {
            ExternalLibraries.Pickers.FileOpenPicker picker = new(MainApp.Instance.MainWindow.GetWindowHandle());
            string file = picker.Show(["*.json"]);

            if (file != string.Empty)
            {
                if (Path.GetDirectoryName(file) == CoreData.UniGetUIDataDirectory)
                {
                    Directory.CreateDirectory(Path.Join(CoreData.UniGetUIDataDirectory, "import-temp"));
                    File.Copy(file, Path.Join(CoreData.UniGetUIDataDirectory, "import-temp", Path.GetFileName(file)));
                    file = Path.Join(CoreData.UniGetUIDataDirectory, "import-temp", Path.GetFileName(file));
                }
                ResetWingetUI(sender, e);
                Dictionary<string, string> settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(file)) ?? [];
                foreach (KeyValuePair<string, string> entry in settings)
                {
                    File.WriteAllText(Path.Join(CoreData.UniGetUIDataDirectory, entry.Key), entry.Value);
                }

                if (Directory.Exists(Path.Join(CoreData.UniGetUIDataDirectory, "import-temp")))
                {
                    Directory.Delete(Path.Join(CoreData.UniGetUIDataDirectory, "import-temp"), true);
                }

                ShowRestartBanner(this, new());
            }
        }

        private async void ExportSettings(object sender, EventArgs e)
        {
            try
            {
                ExternalLibraries.Pickers.FileSavePicker picker = new(MainApp.Instance.MainWindow.GetWindowHandle());
                string file = picker.Show(["*.json"], CoreTools.Translate("WingetUI Settings") + ".json");

                if (file != string.Empty)
                {
                    DialogHelper.ShowLoadingDialog(CoreTools.Translate("Please wait..."));

                    string[] IgnoredSettings = ["OperationHistory", "CurrentSessionToken", "OldWindowGeometry"];

                    Dictionary<string, string> settings = [];
                    foreach (string path in Directory.EnumerateFiles(CoreData.UniGetUIDataDirectory))
                    {
                        if (IgnoredSettings.Contains(Path.GetFileName(path)))
                        {
                            continue;
                        }

                        settings.Add(Path.GetFileName(path), await File.ReadAllTextAsync(path));
                    }

                    await File.WriteAllTextAsync(file, JsonConvert.SerializeObject(settings));

                    DialogHelper.HideLoadingDialog();
                }
            }
            catch (Exception ex)
            {
                DialogHelper.HideLoadingDialog();
                Logger.Error("An error occurred when exporting settings");
                Logger.Error(ex);
            }
        }

        private void ResetWingetUI(object sender, EventArgs e)
        {
            try
            {
                foreach (string path in Directory.EnumerateFiles(CoreData.UniGetUIDataDirectory))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error occurred when resetting UniGetUI");
                Logger.Error(ex);
            }
            ShowRestartBanner(this, new());
        }
    }
}
