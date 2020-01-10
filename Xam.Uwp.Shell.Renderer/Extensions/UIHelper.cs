namespace Xam.Uwp.Shell.Renderer.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Media;

    internal static class UIHelper
    {
        #region Public Methods

        public static IEnumerable<DependencyObject> GetDescendantsOfType<T>(this DependencyObject start)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                var child = VisualTreeHelper.GetChild(start, i);

                if (child is T)
                {
                    yield return child;
                }

                foreach (var item in GetDescendantsOfType<T>(child))
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<FrameworkElement> GetDescendantsOfType<T>(this DependencyObject start, string name)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(start); i++)
            {
                var child = VisualTreeHelper.GetChild(start, i);

                if (child is T && child is FrameworkElement element && element.Name.Equals(name))
                {
                    yield return element;
                }

                foreach (var item in GetDescendantsOfType<T>(child, name))
                {
                    yield return item;
                }
            }
        }

        public static DependencyObject GetDescendantOfType<T>(this DependencyObject start)
        {
            return start.GetDescendantsOfType<T>().FirstOrDefault();
        }

        public static FrameworkElement GetDescendantOfType<T>(this DependencyObject start, string name)
        {
            return start.GetDescendantsOfType<T>(name).FirstOrDefault();
        }

        public static T GetParent<T>(this DependencyObject element)
        {
            if (element == null)
            {
                return default(T);
            }

            if (element is T)
            {
                return (T)System.Convert.ChangeType(element, typeof(T));
            }

            var parent = (element as FrameworkElement)?.Parent ?? VisualTreeHelper.GetParent(element);

            return parent.GetParent<T>();
        }

        public static T GetParent<T>(this DependencyObject element, string name)
        {
            if (element == null)
            {
                return default(T);
            }

            if (element is T && element is FrameworkElement frameworkElement && frameworkElement.Name.Equals(name))
            {
                return (T)Convert.ChangeType(element, typeof(T));
            }
            
            var parent = (element as FrameworkElement)?.Parent ?? VisualTreeHelper.GetParent(element);

            return parent.GetParent<T>();
        }

        public static bool HasParent(this DependencyObject element)
        {
            if (element == null)
            {
                return false;
            }

            var parent = (element as FrameworkElement)?.Parent ?? VisualTreeHelper.GetParent(element);

            return parent != null;
        }

        #endregion
    }
}
