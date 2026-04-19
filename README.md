# CodeBrix.SkiaSvg

An SVG loading and rendering library for .NET, built on SkiaSharp.
CodeBrix.SkiaSvg is provided as a .NET 10 library and associated `CodeBrix.SkiaSvg.MitLicenseForever` NuGet package.

CodeBrix.SkiaSvg supports applications and assemblies that target Microsoft .NET version 10.0 and later.
Microsoft .NET version 10.0 is a Long-Term Supported (LTS) version of .NET, and was released on Nov 11, 2025; and will be actively supported by Microsoft until Nov 14, 2028.
Please update your C#/.NET code and projects to the latest LTS version of Microsoft .NET.

CodeBrix.SkiaSvg is a fork of the code of the open source Svg.Skia library (and several of its companion packages) - see below for licensing details.

## Pinned SkiaSharp dependency (temporary)

The `CodeBrix.SkiaSvg.MitLicenseForever` NuGet package is currently pinned to **SkiaSharp 3.119.3-preview.1.1** rather than the latest stable SkiaSharp release. This is intentional: the previous stable release (SkiaSharp 3.119.2) is missing native libraries for **ARM64** and **RISC-V 64** platforms, so depending on the stable release fails at load time on those platforms. When a stable SkiaSharp release ships that includes the missing native assets, CodeBrix.SkiaSvg will move back to a stable SkiaSharp reference.

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
using SkiaSharp;

using var svg = SKSvg.CreateFromFile("interactive.svg");

var point = new SKPoint(100, 100);
var element = svg.HitTestTopmostElement(point);
if (element != null)
{
    Console.WriteLine($"Hit: {element.ElementName} (ID: {element.ID})");
}
```

### Load from SVG String

```csharp
using CodeBrix.SkiaSvg;

var svgContent = "<svg xmlns='http://www.w3.org/2000/svg' width='100' height='100'>" +
                 "<circle cx='50' cy='50' r='40' fill='blue'/></svg>";

using var svg = SKSvg.CreateFromSvg(svgContent);
canvas.DrawPicture(svg.Picture);
```

Note that additional sample code and usage examples are available in the `CodeBrix.SkiaSvg.Tests` project.

## License

The project is licensed under the MIT License. see: https://en.wikipedia.org/wiki/MIT_License

All code originating from Svg.Skia was included as allowed by the MIT License permissible open source software license - as of Svg.Skia version 4.2.0.
This project (CodeBrix.SkiaSvg) complies with all provisions of the source code license of Svg.Skia v4.2.0 (MIT License).
