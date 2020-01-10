namespace Xam.Uwp.Shell.Renderer.Converters
{
    #region Usings

    using System;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    #endregion

    internal class ShellContentItemsCountToVisibilityConvert : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count && count > 1)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}