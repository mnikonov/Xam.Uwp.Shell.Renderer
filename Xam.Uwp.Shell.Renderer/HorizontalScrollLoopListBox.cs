namespace Xam.Uwp.Shell.Renderer
{
    #region Usings

    using System.Collections;

    using Extensions;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Input;

    #endregion

    public class HorizontalScrollLoopListBox : ListBox
    {
        private enum ScrollItemDirection
        {
            Left,

            Right
        }

        #region Fields

        private RepeatButton scrollLeftButton;

        private RepeatButton scrollRightButton;

        private ScrollViewer scrollViewer;

        #endregion

        #region Constructors

        public HorizontalScrollLoopListBox()
        {
            this.DefaultStyleKey = typeof(HorizontalScrollLoopListBox);
        }

        #endregion

        #region Private Methods

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.scrollLeftButton = this.GetTemplateChild("ScrollLeftButton") as RepeatButton;
            this.scrollRightButton = this.GetTemplateChild("ScrollRightButton") as RepeatButton;
            this.scrollViewer = this.GetTemplateChild("ScrollViewer") as ScrollViewer;

            if (this.scrollViewer != null)
            {
                if (this.scrollLeftButton != null)
                {
                    this.scrollLeftButton.Click += (sender, args) => { this.ScrollToElement(ScrollItemDirection.Left); };
                } 
                
                if (this.scrollRightButton != null)
                {
                    this.scrollRightButton.Click += (sender, args) => { this.ScrollToElement(ScrollItemDirection.Right); };
                }

                this.PointerEntered += this.OnScrollViewerPointerEntered;
                this.PointerExited += this.OnScrollViewerPointerExited;
            }
        }

        private void OnScrollViewerPointerExited(object sender, PointerRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
        }

        private void OnScrollViewerPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (this.scrollViewer.ScrollableWidth > 0)
            {
                VisualStateManager.GoToState(this, "ScrollButtonsVisible", true);
            }
        }

        private void ScrollToElement(ScrollItemDirection direction)
        {
            var item = this.GetClosetToHorizontalScrollPositionItem();

            if (this.ItemsSource is IList list)
            {
                var index = list.IndexOf(item);

                if (index == -1)
                {
                    return;                   
                }

                object nextElement = null;

                var selection = this.SelectedItem;

                switch (direction)
                {
                    case ScrollItemDirection.Right:
                    {
                        var closestToEnd = this.GetClosetToHorizontalScrollPositionItem(true);

                        var endElementIndex = list.IndexOf(closestToEnd);

                        if (index + 1 != list.Count)
                        {
                            nextElement = list[index + 1];
                        }

                        if (endElementIndex + 1 == list.Count)
                        {
                            var firstElement = list[0];

                            list.Remove(firstElement);
                            list.Insert(list.Count, firstElement);

                            this.SelectedItem = selection;

                            this.UpdateLayout();
                        }

                        if (nextElement == null)
                        {
                            nextElement = list[index];
                        }

                        break;
                    }

                    case ScrollItemDirection.Left:
                    {
                        if (index - 1 != -1)
                        {
                            nextElement = list[index - 1];
                        }
                        else
                        {
                            nextElement = list[list.Count - 1];
                            list.Remove(nextElement);
                            list.Insert(0, nextElement);

                            this.SelectedItem = selection;

                            this.UpdateLayout();
                        }

                        break;
                    }
                }
                
                if (nextElement != null)
                {
                    var position = this.ItemHorizontalScrollPosition(nextElement);

                    this.scrollViewer.ChangeView(position, null, null);
                }
            }
        }

        #endregion
    }
}