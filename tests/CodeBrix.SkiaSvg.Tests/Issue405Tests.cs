using CodeBrix.SkiaSvg.ShimSkiaSharp;
using CodeBrix.SkiaSvg;
using Xunit;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg.Tests; //Was previously: namespace Svg.Skia.UnitTests;

public class Issue405Tests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SansSerifBold_ResolvesSingleTypefaceSpan(bool enableSvgFonts)
    {
        var settings = new SKSvgSettings { EnableSvgFonts = enableSvgFonts };
        var assetLoader = new SkiaSvgAssetLoader(new SkiaModel(settings));
        var paint = new SKPaint
        {
            Typeface = SKTypeface.FromFamilyName(
                "sans-serif",
                SKFontStyleWeight.Bold,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };

        var spans = assetLoader.FindTypefaces("Bold Text 20px", paint);

        Assert.Single(spans);
        var span = spans[0];
        Assert.NotNull(span.Typeface);
        Assert.True(span.Typeface!.FontWeight >= SKFontStyleWeight.SemiBold,
            "Expected resolved typeface to be semi-bold or heavier.");
    }

    // ----------------------------------------------------------------------------------
    // CS0618 MIGRATION NOTE (SkiaSharp 3.x):
    //
    // Previously, ApplyTypefaceAdjustments() set paint.FakeBoldText = true on the
    // SKPaint when the resolved typeface weight was lighter than the desired weight.
    // After the CS0618 migration, font/text properties were moved from SKPaint to
    // SKFont. FakeBoldText has been replaced by SKFont.Embolden, and the adjustment
    // is now applied via model.ToSKFont() instead of model.ToSKPaint().
    //
    // This test was updated to call model.ToSKFont() and assert font.Embolden rather
    // than localPaint.FakeBoldText.
    // ----------------------------------------------------------------------------------
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FakeBoldMatchesDesiredWeight(bool enableSvgFonts)
    {
        var settings = new SKSvgSettings { EnableSvgFonts = enableSvgFonts };
        var model = new SkiaModel(settings);
        var paint = new SKPaint
        {
            Typeface = SKTypeface.FromFamilyName(
                "sans-serif",
                SKFontStyleWeight.ExtraBlack,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };

        // ToSKFont now carries text/font properties, including the Embolden flag
        // that replaced FakeBoldText on SKPaint.
        using var font = model.ToSKFont(paint);
        Assert.NotNull(font);

        var desiredWeight = (int)SkiaSharp.SKFontStyleWeight.ExtraBlack;
        var actualWeight = font!.Typeface?.FontWeight ?? 0;
        var shouldFakeBold = actualWeight < desiredWeight;

        Assert.Equal(shouldFakeBold, font.Embolden);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WhitespaceStaysAttachedToResolvedTypefaceSpan(bool enableSvgFonts)
    {
        var settings = new SKSvgSettings { EnableSvgFonts = enableSvgFonts };
        var assetLoader = new SkiaSvgAssetLoader(new SkiaModel(settings));
        var paint = new SKPaint();

        var spans = assetLoader.FindTypefaces("ښ ښښښ", paint);

        var span = Assert.Single(spans);
        Assert.Equal("ښ ښښښ", span.Text);
        Assert.NotNull(span.Typeface);
    }
}

