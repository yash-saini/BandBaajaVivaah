using System.Windows;

namespace BandBaajaVivaah.WPF.Services
{
    public class ThemeService
    {
        public void SetTheme(bool isDarkMode)
        {
            var appResources = Application.Current.Resources.MergedDictionaries;
            var themeDictionary = appResources.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Themes/"));

            if (themeDictionary != null)
            {
                string themeSource = isDarkMode ? "/Resources/Styles/Themes/Dark.xaml" : "/Resources/Styles/Themes/Light.xaml";
                themeDictionary.Source = new Uri(themeSource, UriKind.Relative);
            }
        }
    }
}
