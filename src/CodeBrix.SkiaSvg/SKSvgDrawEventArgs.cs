// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg; //Was previously: namespace Svg.Skia;

public class SKSvgDrawEventArgs : EventArgs
{
    public SkiaSharp.SKCanvas Canvas { get; }

    internal SKSvgDrawEventArgs(SkiaSharp.SKCanvas canvas)
    {
        Canvas = canvas;
    }
}
