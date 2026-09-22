using System.Windows;
using LoreCompanion.Utilities;

namespace LoreCompanion.Views.Helpers
{
    public static class AdminHelper
    {
        public static readonly Visibility AdminModeVisibility =
            AppHelper.IsAdminMode ? Visibility.Visible : Visibility.Collapsed;
    }
}