// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.IO;
using System.Linq;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg.TypefaceProviders; //Was previously: namespace Svg.Skia.TypefaceProviders;

/// <summary>
/// A typeface provider that serves a single custom typeface loaded from a file, stream, or data.
/// </summary>
public sealed class CustomTypefaceProvider : ITypefaceProvider, IDisposable
{
    /// <summary>Characters trimmed from font family names during matching.</summary>
    public static readonly char[] s_fontFamilyTrim = { '\'' };

    /// <summary>Gets or sets the loaded typeface.</summary>
    public SkiaSharp.SKTypeface Typeface { get; set; }

    /// <summary>Gets or sets the family name of the loaded typeface.</summary>
    public string FamilyName { get; set; }

    /// <summary>
    /// Initializes a new <see cref="CustomTypefaceProvider"/> from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the font data.</param>
    /// <param name="index">The index of the face within the font file.</param>
    public CustomTypefaceProvider(Stream stream, int index = 0)
    {
        Typeface = SkiaSharp.SKTypeface.FromStream(stream, index);
        FamilyName = Typeface.FamilyName;
    }

    /// <summary>
    /// Initializes a new <see cref="CustomTypefaceProvider"/> from a SkiaSharp stream asset.
    /// </summary>
    /// <param name="stream">The stream asset containing the font data.</param>
    /// <param name="index">The index of the face within the font file.</param>
    public CustomTypefaceProvider(SkiaSharp.SKStreamAsset stream, int index = 0)
    {
        Typeface = SkiaSharp.SKTypeface.FromStream(stream, index);
        FamilyName = Typeface.FamilyName;
    }

    /// <summary>
    /// Initializes a new <see cref="CustomTypefaceProvider"/> from a file path.
    /// </summary>
    /// <param name="path">The path to the font file.</param>
    /// <param name="index">The index of the face within the font file.</param>
    public CustomTypefaceProvider(string path, int index = 0)
    {
        Typeface = SkiaSharp.SKTypeface.FromFile(path, index);
        FamilyName = Typeface.FamilyName;
    }

    /// <summary>
    /// Initializes a new <see cref="CustomTypefaceProvider"/> from SkiaSharp data.
    /// </summary>
    /// <param name="data">The data containing the font.</param>
    /// <param name="index">The index of the face within the font file.</param>
    public CustomTypefaceProvider(SkiaSharp.SKData data, int index = 0)
    {
        Typeface = SkiaSharp.SKTypeface.FromData(data, index);
        FamilyName = Typeface.FamilyName;
    }

    /// <inheritdoc />
    public SkiaSharp.SKTypeface FromFamilyName(string fontFamily, SkiaSharp.SKFontStyleWeight fontWeight, SkiaSharp.SKFontStyleWidth fontWidth, SkiaSharp.SKFontStyleSlant fontStyle)
    {
        if (Typeface is null)
        {
            return null;
        }
        var skTypeface = default(SkiaSharp.SKTypeface);
        var fontFamilyNames = fontFamily?.Split(',')?.Select(x => x.Trim().Trim(s_fontFamilyTrim))?.ToArray();
        if (fontFamilyNames is { } && fontFamilyNames.Length > 0)
        {
            foreach (var fontFamilyName in fontFamilyNames)
            {
                if (fontFamilyName == FamilyName
                    && Typeface.FontStyle.Width == (int)fontWidth
                    && Typeface.FontStyle.Weight == (int)fontWeight
                    && Typeface.FontStyle.Slant == fontStyle)
                {
                    skTypeface = Typeface;
                    break;
                }
            }
        }
        return skTypeface;
    }

    /// <summary>Releases the loaded typeface.</summary>
    public void Dispose()
    {
        Typeface?.Dispose();
        Typeface = null;
    }
}
