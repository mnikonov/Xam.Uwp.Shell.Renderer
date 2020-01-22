namespace Xam.Uwp.Shell.Renderer.Extensions
{
    #region Usings

    using System;

    using Windows.UI;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Media;

    #endregion

    public static class ColorExtensions
    {
        #region Public Methods

        public static void ApplyToResource(this Color color, string resourceKey)
        {
            color.ApplyToResource(resourceKey, Application.Current.Resources);
        }

        public static void ApplyToResource(this Color color, string resourceKey, ResourceDictionary dictionary)
        {
            if (dictionary == null || !dictionary.ContainsKey(resourceKey))
            {
                return;
            }
            
            if (dictionary[resourceKey] is Brush brush)
            {
                brush.ChangeBrushColor(color);
            }
            else if (dictionary[resourceKey] is Color simpleColor && simpleColor != color)
            {
                dictionary[resourceKey] = color;
            }
        }

        public static bool ChangeBrushColor(this Brush brush, Color color)
        {
            if (brush is Microsoft.UI.Xaml.Media.AcrylicBrush microsoftAcrylicBrush &&
                (microsoftAcrylicBrush.TintColor != color || microsoftAcrylicBrush.FallbackColor != color))
            {
                microsoftAcrylicBrush.TintColor = color;
                microsoftAcrylicBrush.FallbackColor = color;

                return true;
            }
            
            if (brush is AcrylicBrush windowsAcrylicBrush &&
                     (windowsAcrylicBrush.TintColor != color || windowsAcrylicBrush.FallbackColor != color))
            {
                windowsAcrylicBrush.TintColor = color;
                windowsAcrylicBrush.FallbackColor = color;

                return true;
            }

            if (brush is SolidColorBrush solidBrush && solidBrush.Color != color)
            {
                solidBrush.Color = color;

                return true;
            }

            return false;
        }

        public static Color ChangeColorContrast(this Color color, int presentOfColor)
        {
            var red = (float)color.R;
            var green = (float)color.G;
            var blue = (float)color.B;

            var correctionFactor = (100 - presentOfColor) / 100f;

            if (color.PerceivedBrightness() > 130)
            {
                correctionFactor *= -1;
            }

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = ((255 - red) * correctionFactor) + red;
                green = ((255 - green) * correctionFactor) + green;
                blue = ((255 - blue) * correctionFactor) + blue;
            }

            return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
        }

        public static int PerceivedBrightness(this Color color)
        {
            return (int)Math.Sqrt(color.R * color.R * .299 + color.G * color.G * .587 + color.B * color.B * .114);
        }

        #endregion
    }
}