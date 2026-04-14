using System.Collections.Generic;
using System.IO;
using CodeBrix.SkiaSvg.ShimSkiaSharp;
using CodeBrix.SkiaSvg.Model;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg.Tests.Model; //Was previously: namespace Svg.Model.UnitTests;

internal sealed class TestAssetLoader : ISvgAssetLoader
{
    public bool EnableSvgFonts => false;

    public SKImage LoadImage(Stream stream)
        => new() { Data = SKImage.FromStream(stream) };

    public List<TypefaceSpan> FindTypefaces(string text, SKPaint paintPreferredTypeface)
        => new();

    public SKFontMetrics GetFontMetrics(SKPaint paint)
        => default;

    public float MeasureText(string text, SKPaint paint, ref SKRect bounds)
        => 0f;

    public SKPath GetTextPath(string text, SKPaint paint, float x, float y)
        => new SKPath();
}
