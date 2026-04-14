// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg.TypefaceProviders; //Was previously: namespace Svg.Skia.TypefaceProviders;

/// <summary>
/// Provides a mechanism for resolving typefaces from font family names and style parameters.
/// </summary>
public interface ITypefaceProvider
{
    /// <summary>
    /// Returns a typeface matching the specified font family name and style, or <c>null</c> if no match is found.
    /// </summary>
    /// <param name="fontFamily">The font family name.</param>
    /// <param name="fontWeight">The desired font weight.</param>
    /// <param name="fontWidth">The desired font width.</param>
    /// <param name="fontStyle">The desired font slant.</param>
    /// <returns>A matching <see cref="SkiaSharp.SKTypeface"/>, or <c>null</c>.</returns>
    SkiaSharp.SKTypeface FromFamilyName(string fontFamily, SkiaSharp.SKFontStyleWeight fontWeight, SkiaSharp.SKFontStyleWidth fontWidth, SkiaSharp.SKFontStyleSlant fontStyle);
}
