// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg.ShimSkiaSharp.Editing; //Was previously: namespace ShimSkiaSharp.Editing;

/// <summary>
/// Specifies how editing operations are applied to objects.
/// </summary>
public enum EditMode
{
    /// <summary>Modify the object in place.</summary>
    InPlace,
    /// <summary>Clone the object before applying modifications.</summary>
    CloneOnWrite
}
