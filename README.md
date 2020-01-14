## Xamarin.Forms Shell Renderer for UWP 

UWP shell renderer that supports 

* auto menu layout
* back navigaton
* changing of elements colors

### Setup

1. Add the Xam.Uwp.Shell.Renderer.csproj project to your solution and reference it in UWP project

2. Add the Xam.Uwp.Shell.Renderer Theme Resources to your App.xaml resources.

```xaml
 <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <resources:XamUwpShellRendererResources />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
```
