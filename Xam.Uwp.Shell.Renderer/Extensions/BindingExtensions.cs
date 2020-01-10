namespace Xam.Uwp.Shell.Renderer.Extensions
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Data;

    internal static class BindingExtensions
    {
        public static void SetBinding(this FrameworkElement self, DependencyProperty property, string path)
        {
            self.SetBinding(property, new Binding {Path = new PropertyPath(path)});
        }

        public static void SetBinding(this FrameworkElement self, DependencyProperty property, string path,
            IValueConverter converter)
        {
            self.SetBinding(property, new Binding {Path = new PropertyPath(path), Converter = converter});
        }
    }
}