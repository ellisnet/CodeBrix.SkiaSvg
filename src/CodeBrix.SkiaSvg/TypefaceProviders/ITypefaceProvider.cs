// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg.TypefaceProviders; //Was previously: namespace Svg.Skia.TypefaceProviders;

public interface ITypefaceProvider
{
    SkiaSharp.SKTypeface FromFamilyName(string fontFamily, SkiaSharp.SKFontStyleWeight fontWeight, SkiaSharp.SKFontStyleWidth fontWidth, SkiaSharp.SKFontStyleSlant fontStyle);
}
