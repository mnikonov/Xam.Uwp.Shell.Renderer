namespace Xam.Uwp.Shell.Renderer.TemplateSelectors
{
    #region Usings

    using Xamarin.Forms;

    using DataTemplate = Windows.UI.Xaml.DataTemplate;
    using DataTemplateSelector = Windows.UI.Xaml.Controls.DataTemplateSelector;

    #endregion

    internal class ShellFlyoutTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate BaseShellItemTemplate { get; set; }

        public DataTemplate MenuItemTemplate { get; set; }

        public DataTemplate SeparatorTemplate { get; set; }

        #endregion

        #region Private Methods

        protected override DataTemplate SelectTemplateCore(object item)
        {
            switch (item)
            {
                case BaseShellItem shellItem:
                    return this.BaseShellItemTemplate;
                case MenuItem menuItem:
                    return this.MenuItemTemplate;
                case null:
                    return this.SeparatorTemplate;
                default:
                    return base.SelectTemplateCore(item);
            }
        }

        #endregion
    }
}