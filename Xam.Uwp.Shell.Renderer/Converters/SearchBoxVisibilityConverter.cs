namespace Xam.Uwp.Shell.Renderer.Converters
{
    #region Usings

    using System;

    using Windows.UI.Xaml;

    using Xamarin.Forms;
    
    using IValueConverter = Windows.UI.Xaml.Data.IValueConverter;

    #endregion

    internal class SearchBoxVisibilityConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SearchBoxVisibility visibility)
            {
                switch (visibility)
                {
                    case SearchBoxVisibility.Hidden:
                        return Visibility.Collapsed;
                    case SearchBoxVisibility.Collapsible:
                        return Visibility.Visible;
                    case SearchBoxVisibility.Expanded:
                        // TODO: Expand search
                        return Visibility.Visible;
                }
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