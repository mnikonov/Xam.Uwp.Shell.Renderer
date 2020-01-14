## Xamarin.Forms Shell Renderer for UWP 

UWP shell renderer that supports 

* auto menu layout
* back navigaton
* bindable elements colors

### Setup

1. Add the Xam.Uwp.Shell.Renderer.csproj project to your solution and reference it in UWP project

2. Add the Xam.Uwp.Shell.Renderer Resources to your App.xaml.

namespace declaration
```xaml
xmlns:resources="using:Xam.Uwp.Shell.Renderer.Resources"
```

resource dictionary
```xaml
<Application.Resources>
   <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
         <resources:XamUwpShellRendererResources />
      </ResourceDictionary.MergedDictionaries>
   </ResourceDictionary>
</Application.Resources>
```
