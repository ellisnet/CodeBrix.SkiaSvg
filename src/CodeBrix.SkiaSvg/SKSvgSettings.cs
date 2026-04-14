// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Collections.Generic;
using CodeBrix.SkiaSvg.TypefaceProviders;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg; //Was previously: namespace Svg.Skia;

/// <summary>Configuration settings for SVG rendering.</summary>
public class SKSvgSettings
{
    /// <summary>Gets or sets the alpha type used for rendering.</summary>
    public SkiaSharp.SKAlphaType AlphaType { get; set; }

    /// <summary>Gets or sets the color type used for rendering.</summary>
    public SkiaSharp.SKColorType ColorType { get; set; }

    /// <summary>Gets or sets the linear sRGB color space.</summary>
    public SkiaSharp.SKColorSpace SrgbLinear { get; set; }

    /// <summary>Gets or sets the sRGB color space.</summary>
    public SkiaSharp.SKColorSpace Srgb { get; set; }

    /// <summary>Gets or sets the list of typeface providers used for font resolution.</summary>
    public IList<ITypefaceProvider> TypefaceProviders { get; set; }

    /// <summary>Gets or sets the standalone viewport rectangle, or <c>null</c> to use the document viewport.</summary>
    public SkiaSharp.SKRect? StandaloneViewport { get; set; }

    /// <summary>Gets or sets a value indicating whether SVG font elements are enabled.</summary>
    public bool EnableSvgFonts { get; set; }

    /// <summary>Gets or sets a value indicating whether text reference elements are enabled.</summary>
    public bool EnableTextReferences { get; set; }

    /// <summary>Initializes a new instance of the <see cref="SKSvgSettings"/> class with default values.</summary>
    public SKSvgSettings()
    {
        AlphaType = SkiaSharp.SKAlphaType.Unpremul;

        ColorType = SkiaSharp.SKImageInfo.PlatformColorType;

        SrgbLinear = SkiaSharp.SKColorSpace.CreateRgb(SkiaSharp.SKColorSpaceTransferFn.Linear, SkiaSharp.SKColorSpaceXyz.Srgb); // SkiaSharp.SKColorSpace.CreateSrgbLinear();

        Srgb = SkiaSharp.SKColorSpace.CreateRgb(SkiaSharp.SKColorSpaceTransferFn.Srgb, SkiaSharp.SKColorSpaceXyz.Srgb); // SkiaSharp.SKColorSpace.CreateSrgb();

        TypefaceProviders = new List<ITypefaceProvider>
        {
            new FontManagerTypefaceProvider(),
            new DefaultTypefaceProvider()
        };

        StandaloneViewport = null;
        EnableSvgFonts = true;
        EnableTextReferences = true;
    }
}
