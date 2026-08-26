# CodeBrix.SkiaSvg

An SVG loading and rendering library for .NET, built on SkiaSharp.
CodeBrix.SkiaSvg is provided as a .NET 10 library and associated `CodeBrix.SkiaSvg.MitLicenseForever` NuGet package.

CodeBrix.SkiaSvg supports applications and assemblies that target Microsoft .NET version 10.0 and later.
Microsoft .NET version 10.0 is a Long-Term Supported (LTS) version of .NET, and was released on Nov 11, 2025; and will be actively supported by Microsoft until Nov 14, 2028.
Please update your C#/.NET code and projects to the latest LTS version of Microsoft .NET.

CodeBrix.SkiaSvg is a fork of the code of the open source Svg.Skia library (and several of its companion packages) - see below for licensing details.

## Installation

```
dotnet add package CodeBrix.SkiaSvg.MitLicenseForever
```

Note that the NuGet package ID and the namespace are different - there is no package named plain `CodeBrix.SkiaSvg`:

* NuGet package ID: `CodeBrix.SkiaSvg.MitLicenseForever`
* Assembly and primary namespace: `CodeBrix.SkiaSvg` - i.e. `using CodeBrix.SkiaSvg;`

XML documentation (IntelliSense) ships alongside the assembly.

## Dependencies

The package pulls in the following automatically; no version pinning is needed in the consuming project:

* `CodeBrix.SvgParse.MsplLicenseForever` - the SVG document object model and parser. Note that this package is licensed under the Microsoft Public License (Ms-PL) rather than MIT.
* **SkiaSharp 4.151.0** - the rendering engine
* **HarfBuzzSharp 14.2.1.1** plus its Linux, macOS and Win32 native-asset packages - text shaping

SkiaSharp and HarfBuzzSharp are a matched stable release pair, with native assets covering **ARM64** as well as x64.

### Native assets: one package your application must add

The HarfBuzz native binaries arrive transitively with this package, but **the SkiaSharp native binaries do not**. A consuming application must add the SkiaSharp native-asset package for each platform it runs on - for example, a Linux console or service app adds:

```
dotnet add package SkiaSharp.NativeAssets.Linux
```

Use the `SkiaSharp.NativeAssets.macOS` or `SkiaSharp.NativeAssets.Win32` variant per platform. Without the matching package the project still compiles, and then fails at run time on the first SkiaSharp call with a native-library load error.

## CodeBrix.SkiaSvg supports:

* SVG loading from files, streams, strings, and XmlReaders
* SVG rendering to SkiaSharp SKPicture and SKCanvas
* Android VectorDrawable loading and rendering
* Export to PNG, JPEG, BMP, GIF, TIFF, SVG, PDF, and XPS
* Hit testing (point and rectangle) on SVG elements
* Retained scene graph for efficient rendering and mutation
* SVG animation support with time-based control
* Native composition layer decomposition for optimized animation
* Pointer/mouse interaction dispatching
* Custom typeface/font provider support
* Text shaping via HarfBuzz
* Wireframe debug rendering
* Many more...

## Sample Code

### Load and Render an SVG

```csharp
using CodeBrix.SkiaSvg;

using var svg = SKSvg.CreateFromFile("image.svg");
canvas.DrawPicture(svg.Picture);
```

### Save SVG as a PNG

```csharp
using CodeBrix.SkiaSvg;
using SkiaSharp;

using var svg = SKSvg.CreateFromFile("image.svg");
svg.Save("output.png", SKColors.White, SKEncodedImageFormat.Png, 100, 1f, 1f);
```

### Hit Test an SVG Element

```csharp
using CodeBrix.SkiaSvg;
using CodeBrix.SkiaSvg.ShimSkiaSharp; // hit testing uses this SKPoint, not SkiaSharp's

using var svg = SKSvg.CreateFromFile("interactive.svg");

var point = new SKPoint(100, 100);
var element = svg.HitTestTopmostElement(point);
if (element != null)
{
    // There is no public element.ElementName; identify elements by CLR type.
    Console.WriteLine($"Hit: {element.GetType().Name} (ID: {element.ID})");
}
```

Note that the hit-testing, coordinate-conversion and scene-graph APIs take the `CodeBrix.SkiaSvg.ShimSkiaSharp` `SKPoint`, `SKRect` and `SKMatrix` types, while `SKSvg.Picture` and the drawing calls take the real SkiaSharp types. Both namespaces declare a type named `SKPoint`, so importing the wrong one produces a confusing conversion error.

### Load from SVG String

```csharp
using CodeBrix.SkiaSvg;

var svgContent = "<svg xmlns='http://www.w3.org/2000/svg' width='100' height='100'>" +
                 "<circle cx='50' cy='50' r='40' fill='blue'/></svg>";

using var svg = SKSvg.CreateFromSvg(svgContent);
canvas.DrawPicture(svg.Picture);
```

## Documentation

The NuGet package includes `AGENT-README.txt`, a complete API reference and usage guide written for AI coding agents - point your agent at that file when it is writing code against this library.

This library renders and inspects SVG; it does not author it. The SVG document object model - `SvgDocument`, `SvgElement` and the element types - comes from the `CodeBrix.SvgParse.MsplLicenseForever` package, and DOM-level manipulation belongs there. Read that package's own `AGENT-README.txt` for the element and attribute model.

Additional sample code and usage examples are available in the `CodeBrix.SkiaSvg.Tests` project:
https://github.com/ellisnet/CodeBrix.SkiaSvg/tree/main/tests/CodeBrix.SkiaSvg.Tests

## License

The project is licensed under the MIT License. see: https://en.wikipedia.org/wiki/MIT_License

All code originating from Svg.Skia was included as allowed by the MIT License permissible open source software license - as of Svg.Skia version 4.2.0.
This project (CodeBrix.SkiaSvg) complies with all provisions of the source code license of Svg.Skia v4.2.0 (MIT License).
