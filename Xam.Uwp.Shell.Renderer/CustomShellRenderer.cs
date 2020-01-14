#region Usings

using Xam.Uwp.Shell.Renderer;
using Xam.Uwp.Shell.Renderer.Commands;
using Xam.Uwp.Shell.Renderer.Converters;
using Xam.Uwp.Shell.Renderer.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

#endregion

[assembly: ExportRenderer(typeof(Shell), typeof(CustomShellRenderer))]

namespace Xam.Uwp.Shell.Renderer
{
    #region Usings

    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;

    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Core;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Media.Animation;

    using Xamarin.Forms;
    using Xamarin.Forms.Internals;
    using Xamarin.Forms.Platform.UWP;

    using Binding = Windows.UI.Xaml.Data.Binding;
    using Button = Windows.UI.Xaml.Controls.Button;
    using Color = Xamarin.Forms.Color;
    using ContentPresenter = Windows.UI.Xaml.Controls.ContentPresenter;
    using Grid = Windows.UI.Xaml.Controls.Grid;
    using NativeAutomationProperties = Windows.UI.Xaml.Automation.AutomationProperties;
    using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
    using NavigationViewBackRequestedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs;
    using NavigationViewDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode;
    using NavigationViewDisplayModeChangedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewDisplayModeChangedEventArgs;
    using NavigationViewItemInvokedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;
    using NavigationViewPaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode;
    using Page = Xamarin.Forms.Page;
    using SelectionChangedEventArgs = Windows.UI.Xaml.Controls.SelectionChangedEventArgs;
    using Size = Windows.Foundation.Size;

    #endregion

    [Windows.UI.Xaml.Data.Bindable]
    public class CustomShellRenderer : ContentControl,
                                       IVisualElementRenderer,
                                       IAppearanceObserver,
                                       IFlyoutBehaviorObserver
    {
        #region Fields

        public static readonly DependencyProperty TitleForegroundProperty = DependencyProperty.Register(
            "TitleForeground",
            typeof(Brush),
            typeof(CustomShellRenderer),
            new PropertyMetadata(null));

        public static readonly DependencyProperty UnselectedColorProperty = DependencyProperty.Register(
            "UnselectedColor",
            typeof(Brush),
            typeof(CustomShellRenderer),
            new PropertyMetadata(null));

        public static readonly DependencyProperty DisabledColorProperty = DependencyProperty.Register(
            "DisabledColor",
            typeof(Brush),
            typeof(CustomShellRenderer),
            new PropertyMetadata(null));

        public static readonly DependencyProperty TabBarBackgroundProperty = DependencyProperty.Register(
            "TabBarBackground",
            typeof(Brush),
            typeof(CustomShellRenderer),
            new PropertyMetadata(null));

        public static readonly DependencyProperty TabBarForegroundProperty = DependencyProperty.Register(
            "TabBarForeground",
            typeof(Brush),
            typeof(CustomShellRenderer),
            new PropertyMetadata(null));

        public static readonly DependencyProperty TabBarUnselectedColorProperty = DependencyProperty.Register(
            "TabBarUnselectedColor",
            typeof(Brush),
            typeof(CustomShellRenderer),
            new PropertyMetadata(null));

        public static readonly DependencyProperty TabBarTitleForegroundProperty = DependencyProperty.Register(
            "TabBarTitleForeground",
            typeof(Brush),
            typeof(CustomShellRenderer),
            new PropertyMetadata(null));
        
        public static readonly DependencyProperty ShellProperty = DependencyProperty.Register(
            "Shell",
            typeof(Shell),
            typeof(CustomShellRenderer),
            new PropertyMetadata(null, CurrentShellPropertyChangedCallback));

        public static readonly DependencyProperty CurrentShellItemProperty = DependencyProperty.Register(
            "CurrentShellItem",
            typeof(ShellItem),
            typeof(CustomShellRenderer),
            new PropertyMetadata(null, CurrentShellItemPropertyChangedCallback));

        public static readonly DependencyProperty CurrentShellItemSectionProperty = DependencyProperty.Register(
            "CurrentShellItemSection",
            typeof(ShellSection),
            typeof(CustomShellRenderer),
            new PropertyMetadata(null, CurrentShellItemSectionChangedCallback));

        public static readonly DependencyProperty CurrentShellItemSectionContentProperty = DependencyProperty.Register(
            "CurrentShellItemSectionContent",
            typeof(ShellContent),
            typeof(CustomShellRenderer),
            new PropertyMetadata(null, CurrentShellItemSectionContentPropertyChangedCallback));

        public static readonly DependencyProperty CurrentPageProperty = DependencyProperty.Register(
            "CurrentPage",
            typeof(Page),
            typeof(CustomShellRenderer),
            new PropertyMetadata(null, CurrentPagePropertyChangedCallback));

        public static readonly DependencyProperty CurrentPageSearchHandlerProperty = DependencyProperty.Register(
            "CurrentPageSearchHandler",
            typeof(SearchHandler),
            typeof(CustomShellRenderer),
            new PropertyMetadata(null));

        private readonly IconImageSourceConverter iconImageSourceConverter = new IconImageSourceConverter();

        private FlyoutBehavior? currentFlyoutBehavior;

        private bool isPaneClosingLocked = false;

        #endregion

        #region Constructors

        public CustomShellRenderer()
        {
            Shell.VerifyShellUWPFlagEnabled(nameof(CustomShellRenderer));

            this.DefaultStyleKey = typeof(CustomShellRenderer);

            this.MenuItemCommand = new RelayCommand<IMenuItemController>(
            menuItem =>
                {
                    menuItem.Activate();
                },
            menuItem => menuItem.IsEnabled); 

            this.SearchBoxTextChangedCommand = new RelayCommand<AutoSuggestBoxTextChangedEventArgs>(
                args =>
                    {
                        if (args.Reason != AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
                        {
                            this.CurrentPageSearchHandler.Query = this.AutoSuggestBox.Text;
                        }
                    },
                shellSection => this.CurrentPageSearchHandler != null);

            this.SearchBoxQuerySubmittedCommand = new RelayCommand<AutoSuggestBoxQuerySubmittedEventArgs>(
                args =>
                    {
                        ((ISearchHandlerController)this.CurrentPageSearchHandler).QueryConfirmed();
                    },
                shellSection => this.CurrentPageSearchHandler != null);

            this.SearchBoxSuggestionChosenCommand = new RelayCommand<AutoSuggestBoxSuggestionChosenEventArgs>(
                args =>
                    {
                        ((ISearchHandlerController)this.CurrentPageSearchHandler).ItemSelected(args.SelectedItem);
                    },
                shellSection => this.CurrentPageSearchHandler != null);

            this.SizeChangedCommand = new RelayCommand<SizeChangedEventArgs>(
                args =>
                {
                    if (this.CurrentPage != null)
                    {
                        this.CurrentPage.ContainerArea = new Rectangle(0, 0, args.NewSize.Width, args.NewSize.Height);
                    }
                });
            
            this.NavigationViewItemInvokedCommand = new RelayCommand<NavigationViewItemInvokedEventArgs>(
                args =>
                    {
                        //var selected = args.InvokedItemContainer?.DataContext as ShellItem;
                        // Temporary workaround, on second click of same menu item InvokedItemContainer contains last last menu element
                        var selected = this.NavigationView.SelectedItem as ShellItem;

                        if (this.CurrentShellItem != selected)
                        {
                            this.OnElementSelected(selected);
                        }
                    });

            this.ShellSectionCommand = new RelayCommand<ShellSection>(
                selected =>
                    {
                        if (this.CurrentShellItemSection != selected)
                        {
                            this.OnElementSelected(selected);
                        }
                    },
                shellSection => shellSection.IsEnabled);

            this.TabBarSelectionChangedCommand = new RelayCommand<SelectionChangedEventArgs>(
                args =>
                    {
                        var selected = args.AddedItems.FirstOrDefault() as ShellContent;

                        if (this.CurrentShellItemSectionContent != selected)
                        {
                            this.OnElementSelected(selected);
                        }
                    });
        }
        
        #endregion

        #region Events

        public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

        #endregion

        #region Properties

        public ICommand MenuItemCommand { get; }

        public ICommand ShellSectionCommand { get; }

        public ICommand SearchBoxTextChangedCommand { get; }

        public ICommand SearchBoxQuerySubmittedCommand { get; }

        public ICommand SearchBoxSuggestionChosenCommand { get; }
        
        public ICommand SizeChangedCommand { get; }

        public ICommand TabBarSelectionChangedCommand { get; }

        public ICommand NavigationViewItemInvokedCommand { get; }

        public FrameworkElement ContainerElement => this;

        public VisualElement Element => this.Shell;

        public Brush TitleForeground
        {
            get => (Brush)this.GetValue(TitleForegroundProperty);
            set => this.SetValue(TitleForegroundProperty, value);
        }

        public Brush UnselectedColor
        {
            get => (Brush)this.GetValue(UnselectedColorProperty);
            set => this.SetValue(UnselectedColorProperty, value);
        }

        public Brush DisabledColor
        {
            get => (Brush)this.GetValue(DisabledColorProperty);
            set => this.SetValue(DisabledColorProperty, value);
        }

        public Brush TabBarBackground
        {
            get => (Brush)this.GetValue(TabBarBackgroundProperty);
            set => this.SetValue(TabBarBackgroundProperty, value);
        }

        public Brush TabBarForeground
        {
            get => (Brush)this.GetValue(TabBarForegroundProperty);
            set => this.SetValue(TabBarForegroundProperty, value);
        }

        public Brush TabBarUnselectedColor
        {
            get => (Brush)this.GetValue(TabBarUnselectedColorProperty);
            set => this.SetValue(TabBarUnselectedColorProperty, value);
        }

        public Brush TabBarTitleForeground
        {
            get => (Brush)this.GetValue(TabBarTitleForegroundProperty);
            set => this.SetValue(TabBarTitleForegroundProperty, value);
        }

        public Shell Shell
        {
            get => (Shell)this.GetValue(ShellProperty);
            private set => this.SetValue(ShellProperty, value);
        }

        public ShellItem CurrentShellItem
        {
            get => (ShellItem)this.GetValue(CurrentShellItemProperty);
            internal set => this.SetValue(CurrentShellItemProperty, value);
        }

        public ShellSection CurrentShellItemSection
        {
            get => (ShellSection)this.GetValue(CurrentShellItemSectionProperty);
            internal set => this.SetValue(CurrentShellItemSectionProperty, value);
        }

        public ShellContent CurrentShellItemSectionContent
        {
            get => (ShellContent)this.GetValue(CurrentShellItemSectionContentProperty);
            internal set => this.SetValue(CurrentShellItemSectionContentProperty, value);
        }
        
        public Page CurrentPage
        {
            get => (Page)this.GetValue(CurrentPageProperty);
            internal set => this.SetValue(CurrentPageProperty, value);
        }

        public SearchHandler CurrentPageSearchHandler
        {
            get => (SearchHandler)this.GetValue(CurrentPageSearchHandlerProperty);
            internal set => this.SetValue(CurrentPageSearchHandlerProperty, value);
        }
        
        protected NavigationView NavigationView { get; private set; }

        protected ContentPresenter ContentPresenter { get; private set; }

        protected Border BottomArea { get; private set; }

        protected CommandBar HeaderCommandBar { get; private set; }

        protected AutoSuggestBox AutoSuggestBox { get; private set; }

        protected HorizontalScrollLoopListBox TabBar { get; private set; }

        protected Grid ButtonHolderGrid { get; private set; }

        protected TextBlock AppTitle { get; private set; }

        #endregion

        #region Public Methods

        public void Dispose()
        {
            this.SetElement(null);
        }

        public UIElement GetNativeElement()
        {
            return this.NavigationView;
        }

        public void SetElement(VisualElement element)
        {
            if (this.Shell != null && element != null)
            {
                throw new NotSupportedException("Reuse of the Shell Renderer is not supported");
            }

            if (element != null)
            {
                this.Shell = (Shell)element;
                this.OnElementSet(this.Shell);
            }
        }

        #endregion

        #region Interfaces Implementation

        void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
        {
            if (appearance != null)
            {
                if (!appearance.BackgroundColor.IsDefault)
                {
                    this.Background.ChangeBrushColor(appearance.BackgroundColor.ToWindowsColor());
                }

                if (!appearance.ForegroundColor.IsDefault)
                {
                    var foregroundColor = appearance.ForegroundColor.ToWindowsColor();
                    this.Foreground.ChangeBrushColor(foregroundColor);
                    
                    foregroundColor.ApplyToResource("TabBarSelectionHighlight");

                    foregroundColor.ApplyToResource("AppBarButtonForegroundPointerOver", this.NavigationView?.Resources);
                    foregroundColor.ApplyToResource("AppBarButtonForegroundPressed", this.NavigationView?.Resources);
                    foregroundColor.ApplyToResource("NavigationViewButtonForegroundPointerOver", this.NavigationView?.Resources);
                    foregroundColor.ApplyToResource("NavigationViewButtonForegroundPressed", this.NavigationView?.Resources);
                }

                if (!appearance.TitleColor.IsDefault)
                {
                    var titleColor = appearance.TitleColor.ToWindowsColor();
                    this.TitleForeground.ChangeBrushColor(titleColor);

                    titleColor.ApplyToResource("TabBarForegroundSelected");
                    titleColor.ApplyToResource("TabBarForegroundSelectedPointerOver");
                    titleColor.ApplyToResource("TabBarForegroundUnselectedPointerOver");
                }

                if (!appearance.DisabledColor.IsDefault)
                {
                    var disabledColor = appearance.DisabledColor.ToWindowsColor();
                    this.DisabledColor.ChangeBrushColor(disabledColor);

                    disabledColor.ApplyToResource("TabBarForegroundDisabled");
                }

                if (!appearance.UnselectedColor.IsDefault)
                {
                    var unselectedColor = appearance.UnselectedColor.ToWindowsColor();
                    this.UnselectedColor.ChangeBrushColor(unselectedColor);

                    unselectedColor.ApplyToResource("TabBarForegroundUnselected");
                }

                if (!appearance.TabBarBackgroundColor.IsDefault)
                {
                    this.TabBarBackground.ChangeBrushColor(appearance.TabBarBackgroundColor.ToWindowsColor());
                }

                if (!appearance.TabBarForegroundColor.IsDefault)
                {
                    this.TabBarForeground.ChangeBrushColor(appearance.TabBarForegroundColor.ToWindowsColor());
                }

                if (!appearance.TabBarUnselectedColor.IsDefault)
                {
                    this.TabBarUnselectedColor.ChangeBrushColor(appearance.TabBarUnselectedColor.ToWindowsColor());
                }

                if (!appearance.TabBarTitleColor.IsDefault)
                {
                    this.TabBarTitleForeground.ChangeBrushColor(appearance.TabBarTitleColor.ToWindowsColor());
                }
            }

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            if (this.Background != null)
            {
                var color = ((SolidColorBrush)this.Background).Color;
                titleBar.BackgroundColor = titleBar.ButtonBackgroundColor = titleBar.InactiveBackgroundColor = titleBar.ButtonInactiveBackgroundColor = color;
                titleBar.ButtonHoverBackgroundColor = color.ChangeColorContrast(80);
                titleBar.ButtonPressedBackgroundColor = color.ChangeColorContrast(60);
            }

            if (this.Foreground != null)
            {
                var color = ((SolidColorBrush)this.Foreground).Color;
                if (color == Windows.UI.Color.FromArgb(255, 255, 255, 255))
                {
                    // So it seems that UWP has a issue with setting the foreground color to exactly white.
                    color = Windows.UI.Color.FromArgb(255, 254, 254, 254);
                }

                titleBar.ForegroundColor = titleBar.ButtonForegroundColor = titleBar.ButtonHoverForegroundColor = titleBar.ButtonPressedForegroundColor = color;
                titleBar.InactiveForegroundColor = titleBar.ButtonInactiveForegroundColor = color.ChangeColorContrast(80);
            }
        }

        void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
        {
            if (this.currentFlyoutBehavior != behavior)
            {
                switch (behavior)
                {
                    case FlyoutBehavior.Disabled:
                        this.NavigationView.IsPaneToggleButtonVisible = false;
                        this.NavigationView.IsPaneVisible = true;
                        this.NavigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
                        this.NavigationView.IsPaneOpen = false;
                        break;

                    case FlyoutBehavior.Flyout:
                        this.NavigationView.IsPaneVisible = true;
                        this.NavigationView.IsPaneToggleButtonVisible = true;
                        var shouldOpen = this.Shell.FlyoutIsPresented;
                        this.NavigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
                        this.NavigationView.IsPaneOpen = shouldOpen;
                        break;

                    case FlyoutBehavior.Locked:
                        this.NavigationView.IsPaneVisible = true;
                        this.NavigationView.IsPaneToggleButtonVisible = false;
                        this.NavigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
                        break;
                }

                this.currentFlyoutBehavior = behavior;
            }
        }

        SizeRequest IVisualElementRenderer.GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            var constraint = new Size(widthConstraint, heightConstraint);

            var oldWidth = this.Width;
            var oldHeight = this.Height;

            this.Height = double.NaN;
            this.Width = double.NaN;

            this.Measure(constraint);
            var result = new Xamarin.Forms.Size(
                Math.Ceiling(this.DesiredSize.Width),
                Math.Ceiling(this.DesiredSize.Height));

            this.Width = oldWidth;
            this.Height = oldHeight;

            return new SizeRequest(result);
        }

        #endregion

        #region Protected Methods

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.NavigationView = this.GetTemplateChild("ShellNavigationView") as NavigationView;
            this.ContentPresenter = this.GetTemplateChild("ContentPresenter") as ContentPresenter;
            this.BottomArea = this.GetTemplateChild("BottomArea") as Border;
            this.HeaderCommandBar = this.GetTemplateChild("HeaderCommandBar") as CommandBar;
            this.AutoSuggestBox = this.GetTemplateChild("AutoSuggestBox") as AutoSuggestBox;
            this.TabBar = this.GetTemplateChild("TabBar") as HorizontalScrollLoopListBox;
            this.AppTitle = this.GetTemplateChild("AppTitle") as TextBlock;

            if (this.NavigationView != null)
            {
                this.NavigationView.IsSettingsVisible = false;
                this.NavigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.Auto;
                this.NavigationView.IsPaneOpen = false;
                this.NavigationView.IsBackEnabled = false;

                this.NavigationView.BackRequested += this.OnNavigationViewBackRequested;
                this.NavigationView.PaneClosing += (s, e) => this.OnPaneClosed();
                this.NavigationView.PaneOpening += (s, e) => this.OnPaneOpening();
                this.NavigationView.Loaded += this.OnNavigationViewLoaded;
                this.NavigationView.DisplayModeChanged += this.OnNavigationViewDisplayModeChanged;
                
                this.OnElementSet(this.Shell);
            }

            if (this.AppTitle != null)
            {
                this.AppTitle.Text = Package.Current.DisplayName;
               
                var titleBar = CoreApplication.GetCurrentView().TitleBar;
                if (!titleBar.ExtendViewIntoTitleBar)
                {
                    this.AppTitle.Visibility = Visibility.Collapsed;
                }
            }
        }

        protected virtual void OnElementSet(Shell shell)
        {
            if (this.NavigationView != null && shell != null)
            {
                this.NavigationView.PaneCustomContent = new ShellHeaderRenderer(shell);
                this.NavigationView.IsPaneOpen = this.Shell.FlyoutIsPresented;

                this.CurrentShellItem = shell.CurrentItem;

                this.UpdateNavigationViewBackground();
            }
        }

        #endregion

        #region Private Static Dependency Properties Callback Handlers

        private static void CurrentShellPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomShellRenderer shellRenderer)
            {
                var oldShell = e.OldValue as Shell;

                if (oldShell != null)
                {
                    oldShell.SizeChanged -= shellRenderer.OnElementSizeChanged;
                    oldShell.PropertyChanged -= shellRenderer.OnShellPropertyChanged;

                    ((IShellController)oldShell).RemoveAppearanceObserver(shellRenderer);
                    ((IShellController)oldShell).RemoveFlyoutBehaviorObserver(shellRenderer);
                }

                if (e.NewValue is Shell newShell)
                {
                    newShell.SizeChanged += shellRenderer.OnElementSizeChanged;
                    newShell.PropertyChanged += shellRenderer.OnShellPropertyChanged;

                    ((IShellController)newShell).AddAppearanceObserver(shellRenderer, newShell);
                    ((IShellController)newShell).AddFlyoutBehaviorObserver(shellRenderer);

                    shellRenderer.ElementChanged?.Invoke(shellRenderer, new VisualElementChangedEventArgs(oldShell, newShell));
                }
            }
        }
        
        private static void CurrentShellItemPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldShellItem = e.OldValue as ShellItem;
            var newShellItem = e.NewValue as ShellItem;

            if (d is CustomShellRenderer shellRenderer && newShellItem != oldShellItem)
            {
                if (oldShellItem != null)
                {
                    oldShellItem.PropertyChanged -= shellRenderer.OnCurrentShellItemPropertyChanged;
                }

                if (newShellItem != null)
                {
                    newShellItem.PropertyChanged += shellRenderer.OnCurrentShellItemPropertyChanged;
                    shellRenderer.CurrentShellItemSection = newShellItem.CurrentItem;
                }
                else
                {
                    shellRenderer.CurrentShellItemSection = null;
                }
            }
        }

        private static void CurrentShellItemSectionChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldShellSection = e.OldValue as ShellSection;
            var newShellSection = e.NewValue as ShellSection;

            if (d is CustomShellRenderer shellRenderer && newShellSection != oldShellSection)
            {
                if (oldShellSection != null)
                {
                    oldShellSection.PropertyChanged -= shellRenderer.OnCurrentShellItemSectionPropertyChanged;
                    ((IShellSectionController)oldShellSection).NavigationRequested -= shellRenderer.OnNavigationRequested;
                }

                if (newShellSection != null)
                {
                    newShellSection.PropertyChanged += shellRenderer.OnCurrentShellItemSectionPropertyChanged;
                    ((IShellSectionController)newShellSection).NavigationRequested += shellRenderer.OnNavigationRequested;

                    shellRenderer.CurrentShellItemSectionContent = newShellSection.CurrentItem;
                }
                else
                {
                    shellRenderer.CurrentShellItemSectionContent = null;
                }
            }
        }

        private static void CurrentShellItemSectionContentPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldShellContent = e.OldValue as ShellContent;
            var newShellContent = e.NewValue as ShellContent;

            if (d is CustomShellRenderer shellRenderer && newShellContent != oldShellContent)
            {
                if (oldShellContent != null)
                {
                    if (shellRenderer.CurrentPage != null)
                    {
                        ((IShellContentController)oldShellContent).RecyclePage(shellRenderer.CurrentPage);
                    }
                }

                if (newShellContent != null)
                {
                    shellRenderer.TabBar.SelectedItem = newShellContent;
                    shellRenderer.CurrentPage = newShellContent.Navigation.NavigationStack.Count > 1 ?
                                                    newShellContent.Navigation.NavigationStack.Last() : 
                                                    ((IShellContentController)newShellContent).GetOrCreateContent();
                }
                else
                {
                    shellRenderer.CurrentPage = null;
                }
            }
        }

        private static void CurrentPagePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldPage = e.OldValue as Page;
            var newPage = e.NewValue as Page;

            if (d is CustomShellRenderer shellRenderer && newPage != oldPage)
            {
                if (oldPage != null)
                {
                    oldPage.PropertyChanged -= shellRenderer.OnPagePropertyChanged;
                }
                
                if (newPage != null)
                {
                    newPage.PropertyChanged += shellRenderer.OnPagePropertyChanged;
                    newPage.ContainerArea = new Rectangle(0, 0, shellRenderer.ContentPresenter.ActualWidth, shellRenderer.ContentPresenter.ActualHeight);

                    shellRenderer.Content = ((ContentPage)newPage).CreateFrameworkElement();
                }

                shellRenderer.UpdateBottomBarVisibility(newPage);
                shellRenderer.UpdatePageSearchHandler(newPage);
                shellRenderer.UpdateNavBarVisibility(newPage);
                shellRenderer.UpdateToolBar(newPage);

                shellRenderer.ApplyIsBackEnabled();
            }
        }

        #endregion

        #region Private Notify Property Changed Handlers

        private void UpdateNavigationViewBackground()
        {
            if (this.Shell.FlyoutBackgroundColor != Color.Default)
            {
                var flyoutBackgroundColor = this.Shell.FlyoutBackgroundColor.ToWindowsColor();

                flyoutBackgroundColor.ApplyToResource("NavigationViewDefaultPaneBackground");
                flyoutBackgroundColor.ApplyToResource("NavigationViewTopPaneBackground");
                flyoutBackgroundColor.ApplyToResource("NavigationViewExpandedPaneBackground");
            }
        }

        private void OnShellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Shell.CurrentItemProperty.PropertyName)
            {
                this.ApplyContentTransition(new EntranceThemeTransition { FromHorizontalOffset = 0, FromVerticalOffset = 80 }, true);
                this.CurrentShellItem = this.Shell.CurrentItem;
            }
            else if (e.PropertyName == Shell.FlyoutIsPresentedProperty.PropertyName)
            {
                if (this.isPaneClosingLocked && this.NavigationView.IsPaneOpen && !this.Shell.FlyoutIsPresented)
                {
                    this.Shell.FlyoutIsPresented = true;
                }
                else
                {
                    this.NavigationView.IsPaneOpen = this.Shell.FlyoutIsPresented;
                }

                this.isPaneClosingLocked = false;

            }
            else if (e.PropertyName == Shell.FlyoutBackgroundColorProperty.PropertyName)
            {
                this.UpdateNavigationViewBackground();
            }
        }

        private void OnCurrentShellItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ShellItem.CurrentItemProperty.PropertyName)
            {
                this.ApplyContentTransition(new EntranceThemeTransition { FromHorizontalOffset = 0, FromVerticalOffset = 80 }, true);
                this.CurrentShellItemSection = this.CurrentShellItem.CurrentItem;
            }
        }

        private void OnCurrentShellItemSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
            {
                var transition = this.CurrentShellItemSection.Items.IndexOf(this.CurrentShellItemSectionContent)
                                 > this.CurrentShellItemSection.Items.IndexOf(this.CurrentShellItemSection.CurrentItem)
                                     ? new EntranceThemeTransition { FromHorizontalOffset = 80, FromVerticalOffset = 0 }
                                     : new EntranceThemeTransition { FromHorizontalOffset = -80, FromVerticalOffset = 0 };
                this.ApplyContentTransition(transition, true);

                this.CurrentShellItemSectionContent = this.CurrentShellItemSection.CurrentItem;
            }
        }

        private void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Shell.TabBarIsVisibleProperty.PropertyName)
            {
                this.UpdateBottomBarVisibility(this.CurrentPage);
            }
            else if (e.PropertyName == Shell.NavBarIsVisibleProperty.PropertyName)
            {
                this.UpdateNavBarVisibility(this.CurrentPage);
            }
            else if (e.PropertyName == Shell.SearchHandlerProperty.PropertyName)
            {
                this.UpdatePageSearchHandler(this.CurrentPage);
            }
        }

        #endregion 

        #region Private Methods
        
        private void OnNavigationViewLoaded(object sender, RoutedEventArgs e)
        {
            this.NavigationView.Loaded -= this.OnNavigationViewLoaded;

            this.ButtonHolderGrid = this.NavigationView.GetDescendantOfType<Grid>("ButtonHolderGrid") as Grid;
            
            if (this.ButtonHolderGrid != null)
            {
                this.ApplyButtonsHolderBackground();

                if (this.ButtonHolderGrid.GetDescendantOfType<Button>("NavigationViewBackButton") is Button backButton)
                {
                    backButton.SetBinding(Control.ForegroundProperty, new Binding { Path = new PropertyPath("Foreground"), Source = this, RelativeSource = new RelativeSource { Mode = RelativeSourceMode.TemplatedParent } });
                }

                if (this.ButtonHolderGrid.GetDescendantOfType<Button>("TogglePaneButton") is Button togglePaneButton)
                {
                    togglePaneButton.SetBinding(Control.ForegroundProperty, new Binding { Path = new PropertyPath("Foreground"), Source = this, RelativeSource = new RelativeSource { Mode = RelativeSourceMode.TemplatedParent } });
                }

                if (this.ButtonHolderGrid.GetDescendantOfType<Button>("NavigationViewCloseButton") is Button closePaneButton)
                {
                    closePaneButton.SetBinding(Control.ForegroundProperty, new Binding { Path = new PropertyPath("Foreground"), Source = this, RelativeSource = new RelativeSource { Mode = RelativeSourceMode.TemplatedParent } });
                }
            }
        }

        private void OnNavigationViewBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            this.Shell.Navigation.PopAsync(true);
        }
        
        private void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
        {
            switch (e.RequestType)
            {
                case NavigationRequestType.Unknown:
                    break;
                case NavigationRequestType.Push:
                    this.ApplyContentTransition(new EdgeUIThemeTransition { Edge = EdgeTransitionLocation.Right }, e.Animated);
                    this.CurrentPage = e.Page;
                    break;
                case NavigationRequestType.Pop:
                    this.ApplyContentTransition(new EdgeUIThemeTransition { Edge = EdgeTransitionLocation.Left }, e.Animated);
                    this.CurrentPage = this.Shell.Navigation.NavigationStack.Count > 1 ? 
                                           this.Shell.Navigation.NavigationStack[this.Shell.Navigation.NavigationStack.Count - 1] : 
                                           ((IShellContentController)this.CurrentShellItemSectionContent).GetOrCreateContent();
                    break;
                case NavigationRequestType.PopToRoot:
                    this.ApplyContentTransition(new EdgeUIThemeTransition { Edge = EdgeTransitionLocation.Left }, e.Animated);
                    this.CurrentPage = ((IShellContentController)this.CurrentShellItemSectionContent).GetOrCreateContent();
                    break;
                case NavigationRequestType.Insert:
                    break;
                case NavigationRequestType.Remove:
                    break;
            }
        }

        private void ApplyContentTransition(Transition transition, bool animate)
        {
            this.ContentPresenter.ContentTransitions.Clear();

            if (animate)
            {
                this.ContentPresenter.ContentTransitions.Add(transition);
            }
        }
        
        private void ApplyIsBackEnabled()
        {
            this.NavigationView.IsBackEnabled = this.Shell.Navigation.NavigationStack.Count > 1;
        }

        private void OnElementSizeChanged(object sender, EventArgs e)
        {
            this.InvalidateMeasure();
        }

        private void OnPaneClosed()
        {
            if (this.Shell != null)
            {
                this.Shell.FlyoutIsPresented = false;
            }

            this.UpdateTopBarAppearance();
        }

        private void OnPaneOpening()
        {
            if (this.Shell != null)
            {
                this.Shell.FlyoutIsPresented = true;
            }

            this.UpdateTopBarAppearance();
        }

        private void OnNavigationViewDisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            this.UpdateTopBarAppearance();
        }

        private void UpdateTopBarAppearance()
        {
            if (this.AppTitle != null && this.NavigationView != null)
            {
                switch (this.NavigationView.DisplayMode)
                {
                    case NavigationViewDisplayMode.Minimal:
                        this.AppTitle.Margin = new Windows.UI.Xaml.Thickness(12, 8, 0, 0);
                        this.ApplyButtonsHolderBackground();

                        break; 
                    case NavigationViewDisplayMode.Compact:
                    case NavigationViewDisplayMode.Expanded:
                        this.AppTitle.Margin = this.NavigationView.IsPaneOpen ? new Windows.UI.Xaml.Thickness(12, 8, 0, 0) : new Windows.UI.Xaml.Thickness(56, 8, 0, 0);
                        this.ApplyButtonsHolderBackground();
                        break;
                }
            }
        }

        private void ApplyButtonsHolderBackground()
        {
            if (this.ButtonHolderGrid != null)
            {
                if (this.ButtonHolderGrid.Background == null)
                {
                    this.ButtonHolderGrid.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
                }

                if (this.Background is SolidColorBrush brush && this.NavigationView.DisplayMode == NavigationViewDisplayMode.Minimal && !this.NavigationView.IsPaneOpen)
                {
                    this.ButtonHolderGrid?.Background.ChangeBrushColor(brush.Color);
                }
                else
                {
                    this.ButtonHolderGrid.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
                }
            }
        }

        private void UpdatePageSearchHandler(Page page)
        {
            this.CurrentPageSearchHandler = page != null ? Shell.GetSearchHandler(page) : null;
        }

        private void UpdateBottomBarVisibility(Page page)
        {
            this.BottomArea.Visibility = (page == null || Shell.GetTabBarIsVisible(page)) && this.Shell.CurrentItem.Items.Count > 1
                                                          ? Visibility.Visible
                                                          : Visibility.Collapsed;
        }

        private void UpdateNavBarVisibility(Page page)
        {
            if (page == null || Shell.GetNavBarIsVisible(page))
            {
                Shell.SetFlyoutBehavior(this.Shell, FlyoutBehavior.Flyout);
            }
            else
            {
                Shell.SetFlyoutBehavior(this.Shell, FlyoutBehavior.Disabled);
            }
        }

        private void UpdateToolBar(Page page)
        {
            this.HeaderCommandBar.PrimaryCommands.Clear();
            this.HeaderCommandBar.SecondaryCommands.Clear();

            foreach (var item in this.CurrentPage.ToolbarItems.OrderBy(p => p.Priority))
            {
                var button = new AppBarButton
                {
                    DataContext = item
                };

                button.SetBinding(AppBarButton.LabelProperty, nameof(item.Text));
                button.SetBinding(AppBarButton.IconProperty, nameof(item.IconImageSource), this.iconImageSourceConverter);

                button.SetBinding(Control.IsEnabledProperty, nameof(item.IsEnabled));
                button.SetValue(NativeAutomationProperties.AutomationIdProperty, item.AutomationId);
                button.SetAutomationPropertiesName(item);
                button.SetAutomationPropertiesAccessibilityView(item);
                button.SetAutomationPropertiesHelpText(item);
                button.SetAutomationPropertiesLabeledBy(item);
                
                button.SetBinding(Control.ForegroundProperty, new Binding { Path = new PropertyPath("Foreground"), Source = this, RelativeSource = new RelativeSource { Mode = RelativeSourceMode.TemplatedParent } });

                button.Command = this.MenuItemCommand;
                button.CommandParameter = item;

                var order = item.Order == ToolbarItemOrder.Default ? ToolbarItemOrder.Primary : item.Order;
                if (order == ToolbarItemOrder.Primary)
                {
                    this.HeaderCommandBar.PrimaryCommands.Add(button);
                }
                else
                {
                    this.HeaderCommandBar.SecondaryCommands.Add(button);
                }
            }
        }

        private async void OnElementSelected(Element element)
        {
            if (element != null)
            {
                if (this.Shell.FlyoutBehavior == FlyoutBehavior.Flyout)
                {
                    this.isPaneClosingLocked = true;
                }

                await ((IShellController)this.Shell).OnFlyoutItemSelectedAsync(element);
            }
        }

        #endregion
    }
}