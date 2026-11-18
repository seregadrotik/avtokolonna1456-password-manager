using System;
using System.Windows;

namespace Avtokolonna1456.PasswordManager.Services
{
    public sealed class ThemeService
    {
        public void Apply(bool dark)
        {
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            ResourceDictionary theme = null;
            foreach (var dict in dictionaries)
            {
                var source = dict.Source?.OriginalString ?? string.Empty;
                if (source.IndexOf("LightTheme.xaml", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    source.IndexOf("DarkTheme.xaml", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    theme = dict;
                    break;
                }
            }

            var uri = new Uri(
                dark
                    ? "Themes/DarkTheme.xaml"
                    : "Themes/LightTheme.xaml",
                UriKind.Relative);

            var newTheme = new ResourceDictionary { Source = uri };
            if (theme != null)
            {
                var index = dictionaries.IndexOf(theme);
                dictionaries.RemoveAt(index);
                dictionaries.Insert(index, newTheme);
            }
            else
            {
                dictionaries.Insert(0, newTheme);
            }
        }
    }
}
