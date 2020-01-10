namespace Xam.Uwp.Shell.Renderer.Converters
{
    #region Usings

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Windows.UI.Xaml;

    using Xamarin.Forms;

    using IValueConverter = Windows.UI.Xaml.Data.IValueConverter;

    #endregion

    internal class ShellToMenuItemsSourceConverter : IValueConverter
    {
        #region Public Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is IShellController shellController)
            {
                return IterateShellMenuItems(shellController).ToArray();
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Private Methods

        private static IEnumerable<Element> IterateShellMenuItems(IShellController shellController)
        {
            var groups = shellController.GenerateFlyoutGrouping();
            foreach (var group in groups)
            {
                if (group.Count > 0 && group != groups[0])
                {
                    yield return null; // Creates a separator
                }

                foreach (var item in group)
                {
                    yield return item;
                }
            }
        }

        #endregion
    }
}