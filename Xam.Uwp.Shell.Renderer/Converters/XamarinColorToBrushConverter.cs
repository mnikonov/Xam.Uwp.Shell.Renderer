namespace Xam.Uwp.Shell.Renderer.Converters
{
    #region Usings

    using System;

    using Windows.UI.Xaml;

    using Xamarin.Forms;
    using Xamarin.Forms.Platform.UWP;

    using IValueConverter = Windows.UI.Xaml.Data.IValueConverter;

    #endregion

    internal class XamarinColorToBrushConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Color color)
            {
                return color.ToBrush();
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}