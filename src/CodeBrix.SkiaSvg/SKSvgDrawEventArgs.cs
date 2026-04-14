// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg; //Was previously: namespace Svg.Skia;

/// <summary>
/// Provides data for the <see cref="SKSvg"/> draw event.
/// </summary>
public class SKSvgDrawEventArgs : EventArgs
{
    /// <summary>
    /// Gets the SkiaSharp canvas on which drawing is performed.
    /// </summary>
    public SkiaSharp.SKCanvas Canvas { get; }

    internal SKSvgDrawEventArgs(SkiaSharp.SKCanvas canvas)
    {
        Canvas = canvas;
    }
}
