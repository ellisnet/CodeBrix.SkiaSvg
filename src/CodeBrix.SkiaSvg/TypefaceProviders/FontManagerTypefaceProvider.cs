// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using System.IO;
using System.Linq;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg.TypefaceProviders; //Was previously: namespace Svg.Skia.TypefaceProviders;

/// <summary>
/// A typeface provider that resolves typefaces using an <see cref="SkiaSharp.SKFontManager"/>.
/// </summary>
public sealed class FontManagerTypefaceProvider : ITypefaceProvider
{
    /// <summary>Characters trimmed from font family names during matching.</summary>
    public static readonly char[] s_fontFamilyTrim = { '\'' };

    private static bool IsGenericFamilyName(string familyName)
    {
        return familyName.Equals("serif", StringComparison.OrdinalIgnoreCase) ||
               familyName.Equals("sans-serif", StringComparison.OrdinalIgnoreCase) ||
               familyName.Equals("monospace", StringComparison.OrdinalIgnoreCase) ||
               familyName.Equals("cursive", StringComparison.OrdinalIgnoreCase) ||
               familyName.Equals("fantasy", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Gets or sets the font manager used for typeface resolution.</summary>
    public SkiaSharp.SKFontManager FontManager { get; set; }

    /// <summary>Initializes a new <see cref="FontManagerTypefaceProvider"/> using the default font manager.</summary>
    public FontManagerTypefaceProvider()
    {
        FontManager = SkiaSharp.SKFontManager.Default;
    }

    /// <summary>
    /// Creates a typeface from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the font data.</param>
    /// <param name="index">The face index within the font file.</param>
    /// <returns>A new typeface, or <c>null</c> if creation failed.</returns>
    public SkiaSharp.SKTypeface CreateTypeface(Stream stream, int index = 0)
    {
        return FontManager.CreateTypeface(stream, index);
    }

    /// <summary>
    /// Creates a typeface from a SkiaSharp stream asset.
    /// </summary>
    /// <param name="stream">The stream asset containing the font data.</param>
    /// <param name="index">The face index within the font file.</param>
    /// <returns>A new typeface, or <c>null</c> if creation failed.</returns>
    public SkiaSharp.SKTypeface CreateTypeface(SkiaSharp.SKStreamAsset stream, int index = 0)
    {
        return FontManager.CreateTypeface(stream, index);
    }

    /// <summary>
    /// Creates a typeface from a file path.
    /// </summary>
    /// <param name="path">The path to the font file.</param>
    /// <param name="index">The face index within the font file.</param>
    /// <returns>A new typeface, or <c>null</c> if creation failed.</returns>
    public SkiaSharp.SKTypeface CreateTypeface(string path, int index = 0)
    {
        return FontManager.CreateTypeface(path, index);
    }

    /// <summary>
    /// Creates a typeface from SkiaSharp data.
    /// </summary>
    /// <param name="data">The data containing the font.</param>
    /// <param name="index">The face index within the font file.</param>
    /// <returns>A new typeface, or <c>null</c> if creation failed.</returns>
    public SkiaSharp.SKTypeface CreateTypeface(SkiaSharp.SKData data, int index = 0)
    {
        return FontManager.CreateTypeface(data, index);
    }

    /// <inheritdoc />
    public SkiaSharp.SKTypeface FromFamilyName(string fontFamily, SkiaSharp.SKFontStyleWeight fontWeight, SkiaSharp.SKFontStyleWidth fontWidth, SkiaSharp.SKFontStyleSlant fontStyle)
    {
        var skTypeface = default(SkiaSharp.SKTypeface);
        var fontFamilyNames = fontFamily?.Split(',')?.Select(x => x.Trim().Trim(s_fontFamilyTrim))?.ToArray();
        if (fontFamilyNames is { } && fontFamilyNames.Length > 0)
        {
            var defaultName = SkiaSharp.SKTypeface.Default.FamilyName;
            var skFontManager = FontManager;
            var skFontStyle = new SkiaSharp.SKFontStyle(fontWeight, fontWidth, fontStyle);

            foreach (var fontFamilyName in fontFamilyNames)
            {
                var skFontStyleSet = skFontManager.GetFontStyles(fontFamilyName);
                if (skFontStyleSet.Count > 0)
                {
                    skTypeface = skFontManager.MatchFamily(fontFamilyName, skFontStyle);
                    if (skTypeface is { })
                    {
                        var requestedExplicitDefault = defaultName.Equals(fontFamilyName, StringComparison.OrdinalIgnoreCase);
                        var resolvedRequestedFamily = skTypeface.FamilyName.Equals(fontFamilyName, StringComparison.OrdinalIgnoreCase);
                        var resolvedExplicitDefault = defaultName.Equals(skTypeface.FamilyName, StringComparison.OrdinalIgnoreCase);
                        var requestedGenericFamily = IsGenericFamilyName(fontFamilyName);
                        if (!resolvedRequestedFamily &&
                            !(requestedExplicitDefault && resolvedExplicitDefault) &&
                            !(requestedGenericFamily && !resolvedExplicitDefault))
                        {
                            skTypeface.Dispose();
                            skTypeface = null;
                            continue;
                        }
                        break;
                    }
                }
            }
        }
        return skTypeface;
    }
}
