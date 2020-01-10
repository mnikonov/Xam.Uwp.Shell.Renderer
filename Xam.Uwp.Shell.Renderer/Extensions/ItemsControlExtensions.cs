namespace Xam.Uwp.Shell.Renderer.Extensions
{
    #region

    using Windows.Foundation;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;

    #endregion

    internal static class ItemsControlExtensions
    {
        #region Public Methods and Operators

        public static ScrollViewer GetScrollViewer(this ItemsControl control)
        {
            return (ScrollViewer)control.GetDescendantOfType<ScrollViewer>();
        }

        public static TModel GetClosetToVerticalScrollPositionItem<TModel>(this ItemsControl control, bool closestToEnd = false)
        {
            var scroller = control.GetScrollViewer();

            if (scroller == null || control.ItemsPanelRoot == null)
            {
                return default(TModel);
            }

            object prevElementContent = null;

            for (var i = 0; i < control.ItemsPanelRoot.Children.Count; i++)
            {
                if (!(control.ItemsPanelRoot.Children[i] is SelectorItem item))
                {
                    continue;
                }

                var transform = item.TransformToVisual(scroller);
                var absolutePosition = transform.TransformPoint(new Point(0, 0));

                if (absolutePosition.Y >= (closestToEnd ? scroller.ViewportHeight - item.ActualHeight : 0) - 0.01)
                {
                    return (TModel)item.Content;
                }

                prevElementContent = item.Content;
            }

            return (TModel)prevElementContent;
        }

        public static object GetClosetToHorizontalScrollPositionItem(this ItemsControl control, bool closestToEnd = false)
        {
            var scroller = control.GetScrollViewer();

            if (scroller == null || control.ItemsPanelRoot == null)
            {
                return null;
            }

            object prevElementContent = null;

            for (var i = 0; i < control.ItemsPanelRoot.Children.Count; i++)
            {
                if (!(control.ItemsPanelRoot.Children[i] is SelectorItem item))
                {
                    continue;
                }

                var transform = item.TransformToVisual(scroller);
                var absolutePosition = transform.TransformPoint(new Point(0, 0));

                if (absolutePosition.X >= (closestToEnd ? scroller.ViewportWidth - item.ActualWidth : 0) - 0.01)
                {
                    return item.Content;
                }

                prevElementContent = item.Content;
            }

            return prevElementContent;
        }

        public static double ItemVerticalScrollPosition(this ItemsControl control, object item)
        {
            if (item == null || control.ItemsPanelRoot == null)
            {
                return 0;
            }

            var scroller = control.GetScrollViewer();

            if (scroller == null)
            {
                return 0;
            }

            for (var i = 0; i < control.ItemsPanelRoot.Children.Count; i++)
            {
                if (control.ItemsPanelRoot.Children[i] is SelectorItem listItem && listItem.Content == item)
                {
                    var offsetTransform = listItem.TransformToVisual(scroller);
                    var offset = offsetTransform.TransformPoint(new Point(0, 0));

                    return scroller.VerticalOffset + offset.Y;
                }
            }

            return 0;
        }

        public static double ItemHorizontalScrollPosition(this ItemsControl control, object item)
        {
            if (item == null || control.ItemsPanelRoot == null)
            {
                return 0;
            }

            var scroller = control.GetScrollViewer();

            if (scroller == null)
            {
                return 0;
            }

            for (var i = 0; i < control.ItemsPanelRoot.Children.Count; i++)
            {
                if (control.ItemsPanelRoot.Children[i] is SelectorItem listItem && listItem.Content == item)
                {
                    var offsetTransform = listItem.TransformToVisual(scroller);
                    var offset = offsetTransform.TransformPoint(new Point(0, 0));

                    return scroller.HorizontalOffset + offset.X;
                }
            }

            return 0;
        }

        public static bool? IsItemOnView(this ListViewBase control, object item)
        {
            if (control.ItemsPanelRoot == null)
            {
                return null;
            }

            for (var i = 0; i < control.ItemsPanelRoot.Children.Count; i++)
            {
                if (control.ItemsPanelRoot.Children[i] is SelectorItem listItem && listItem.Content == item)
                {
                    return true;
                }
            }

            return false;
        }

        public static DependencyObject FindChildControl<TChildType>(this ListViewBase listControl, object item)
        {
            if (listControl.ItemsPanelRoot == null)
            {
                return null;
            }

            foreach (var t in listControl.ItemsPanelRoot.Children)
            {
                if (t is SelectorItem listItem && listItem.Content == item)
                {
                    return listItem.GetDescendantOfType<TChildType>();
                }
            }

            return null;
        }

        public static TChildType FindChildControl<TChildType>(this ListViewBase listControl, object item, string controlName)
            where TChildType : FrameworkElement
        {
            if (listControl.ItemsPanelRoot == null)
            {
                return null;
            }

            foreach (var control in listControl.ItemsPanelRoot.Children)
            {
                if (control is SelectorItem listItem && listItem.Content == item)
                {
                    return (TChildType)listItem.GetDescendantOfType<TChildType>(controlName);
                }
            }

            return null;
        }

        public static SelectorItem GetListViewSelectorItem(this ListViewBase listControl, object item)
        {
            if (listControl.ItemsPanelRoot == null || item == null)
            {
                return null;
            }

            foreach (var control in listControl.ItemsPanelRoot.Children)
            {
                if (control is SelectorItem listItem && listItem.Content == item)
                {
                    return listItem;
                }
            }

            return null;
        }

        #endregion
    }
}
