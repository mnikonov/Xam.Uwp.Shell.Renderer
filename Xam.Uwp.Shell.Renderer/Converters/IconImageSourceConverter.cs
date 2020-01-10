namespace Xam.Uwp.Shell.Renderer.Converters
{
    #region Usings

    using System;

    using Extensions;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    using Xamarin.Forms;

    using IValueConverter = Windows.UI.Xaml.Data.IValueConverter;

    #endregion

    internal class IconImageSourceConverter : IValueConverter
    {
        #region Fields

        private readonly XamarinColorToBrushConverter colorToBrushConverter = new XamarinColorToBrushConverter();

        #endregion

        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch (value)
            {
                case FileImageSource fileImageSource:
                    return new BitmapIcon { UriSource = new Uri("ms-appx:///" + fileImageSource.File) };

                case FontImageSource fontImageSource:

                    var icon = new FontIcon { DataContext = fontImageSource };

                    icon.SetBinding(FontIcon.GlyphProperty, "Glyph");
                    icon.SetBinding(FontIcon.FontFamilyProperty, "FontFamily");
                    icon.SetBinding(FontIcon.FontSizeProperty, "Size");

                    if (!fontImageSource.Color.IsDefault)
                    {
                        icon.SetBinding(IconElement.ForegroundProperty, "Color", this.colorToBrushConverter);
                    }

                    return icon;
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