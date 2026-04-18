using System.Collections.Generic;
using System.Runtime.InteropServices;
using CodeBrix.SkiaSvg.ShimSkiaSharp;
using Xunit;

namespace CodeBrix.SkiaSvg.Tests;

/// <summary>
/// Tests covering the SkiaSharp text, font, and filter-quality API surface after the
/// CS0618 migration. Text and font properties (TextSize, Typeface, SubpixelText,
/// LcdRenderText) are now on <c>SKFont</c> via <c>ToSKFont()</c>, and filter quality
/// is represented as <c>SKSamplingOptions</c> via <c>ToSKSamplingOptions()</c>.
/// <c>ToSKPaint()</c> continues to carry visual/stroke properties only.
///
/// These tests originally locked down the pre-migration baseline (properties on
/// <c>SKPaint</c>) and were updated to verify the new locations after the migration.
/// </summary>
public class SkiaModelTextApiTests
{
    private static SkiaModel CreateModel()
    {
        return new SkiaModel(new SKSvgSettings());
    }

    private static SkiaSvgAssetLoader CreateAssetLoader()
    {
        var model = CreateModel();
        return new SkiaSvgAssetLoader(model);
    }

    // ---------------------------------------------------------------
    // 1. ToSKFont transfers all text-related properties
    // ---------------------------------------------------------------
    //
    // CS0618 MIGRATION NOTE (SkiaSharp 3.x):
    //
    // Previously, text-related properties (TextSize, TextAlign, Typeface,
    // LcdRenderText, SubpixelText, TextEncoding) were all set on SKPaint by
    // ToSKPaint(). After the CS0618 migration, these properties have been
    // moved to SKFont, which is now created by the new ToSKFont() method.
    //
    // Property mapping from old SKPaint to new SKFont:
    //   SKPaint.TextSize       → SKFont.Size
    //   SKPaint.Typeface       → SKFont.Typeface
    //   SKPaint.SubpixelText   → SKFont.Subpixel
    //   SKPaint.LcdRenderText  → SKFont.Edging (SubpixelAntialias when true)
    //
    // SKPaint.TextAlign is no longer stored on either object — it is passed
    // directly to SkiaSharp canvas draw methods as a separate parameter.
    //
    // SKPaint.TextEncoding has no SKFont equivalent; SkiaSharp 3.x always
    // uses UTF-8 internally.
    //
    // ToSKPaint() still carries visual properties (Color, Style, Shader, etc.).
    // ---------------------------------------------------------------

    [Fact]
    public void ToSKFont_TransfersTextProperties()
    {
        var model = CreateModel();
        var shimPaint = new SKPaint
        {
            TextSize = 24f,
            TextAlign = SKTextAlign.Center,
            LcdRenderText = true,
            SubpixelText = true,
            TextEncoding = SKTextEncoding.Utf16,
            Typeface = SKTypeface.FromFamilyName(
                "sans-serif",
                SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };

        using var font = model.ToSKFont(shimPaint);

        Assert.NotNull(font);
        // TextSize is now SKFont.Size
        Assert.Equal(24f, font.Size);
        // Typeface is now on SKFont
        Assert.NotNull(font.Typeface);
        // SubpixelText is now SKFont.Subpixel
        Assert.True(font.Subpixel);
        // LcdRenderText=true maps to SKFontEdging.SubpixelAntialias
        Assert.Equal(SkiaSharp.SKFontEdging.SubpixelAntialias, font.Edging);
    }

    // ---------------------------------------------------------------
    // 2. ToSKSamplingOptions maps all SKFilterQuality values correctly
    // ---------------------------------------------------------------
    //
    // CS0618 MIGRATION NOTE (SkiaSharp 3.x):
    //
    // Previously, filter quality was set as SKPaint.FilterQuality by
    // ToSKPaint(). SkiaSharp 3.x deprecated FilterQuality in favor of
    // SKSamplingOptions, which provides finer-grained control over image
    // sampling. The new ToSKSamplingOptions() method performs this mapping:
    //
    //   SKFilterQuality.None   → SKSamplingOptions(SKFilterMode.Nearest)
    //   SKFilterQuality.Low    → SKSamplingOptions(SKFilterMode.Linear)
    //   SKFilterQuality.Medium → SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear)
    //   SKFilterQuality.High   → SKSamplingOptions(SKCubicResampler.Mitchell)
    //
    // ToSKPaint() no longer sets FilterQuality on the returned SKPaint.
    // Callers that need sampling options should use ToSKSamplingOptions()
    // and pass the result to canvas draw methods (e.g., DrawImage).
    // ---------------------------------------------------------------

    [Fact]
    public void ToSKSamplingOptions_None_UsesNearestFilter()
    {
        var model = CreateModel();
        var options = model.ToSKSamplingOptions(SKFilterQuality.None);

        Assert.False(options.UseCubic);
        Assert.Equal(SkiaSharp.SKFilterMode.Nearest, options.Filter);
    }

    [Fact]
    public void ToSKSamplingOptions_Low_UsesLinearFilter()
    {
        var model = CreateModel();
        var options = model.ToSKSamplingOptions(SKFilterQuality.Low);

        Assert.False(options.UseCubic);
        Assert.Equal(SkiaSharp.SKFilterMode.Linear, options.Filter);
    }

    [Fact]
    public void ToSKSamplingOptions_Medium_UsesLinearFilterWithMipmap()
    {
        var model = CreateModel();
        var options = model.ToSKSamplingOptions(SKFilterQuality.Medium);

        Assert.False(options.UseCubic);
        Assert.Equal(SkiaSharp.SKFilterMode.Linear, options.Filter);
        Assert.Equal(SkiaSharp.SKMipmapMode.Linear, options.Mipmap);
    }

    [Fact]
    public void ToSKSamplingOptions_High_UsesCubicResampler()
    {
        var model = CreateModel();
        var options = model.ToSKSamplingOptions(SKFilterQuality.High);

        // High quality uses Mitchell cubic resampler instead of linear filtering
        Assert.True(options.UseCubic);
    }

    // ---------------------------------------------------------------
    // 3. GetFontMetrics returns plausible non-default metrics
    // ---------------------------------------------------------------

    [Theory]
    [InlineData(0.5f)]
    public void GetFontMetrics_ReturnsNonDefaultMetrics(float linuxDifferenceTolerance)
    {
        var assetLoader = CreateAssetLoader();
        var paint = new SKPaint
        {
            TextSize = 20f,
            Typeface = SKTypeface.FromFamilyName(
                "sans-serif",
                SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };

        var metrics = assetLoader.GetFontMetrics(paint);

        //On Linux, "sans-serif" resolves to DejaVu Sans (or similar), whose Top
        //  and Ascent are reported essentially equal (e.g. -21.34 vs -21.38 at
        //  20pt) -- within rounding of each other rather than satisfying the
        //  strict Top<=Ascent ordering that holds on Windows/macOS. So we allow
        //  a small tolerance for that specific platform, but require exact
        //  ordering on all other platforms (so far).
        //  Other platforms can have exceptions too, in the future - if we
        //  encounter the same minor calculated difference.
        var tolerance = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? linuxDifferenceTolerance
            : 0f;

        // Ascent and Top should be negative; Descent and Bottom positive
        Assert.True(metrics.Ascent < 0, $"Expected negative Ascent, got {metrics.Ascent}");
        Assert.True(metrics.Descent > 0, $"Expected positive Descent, got {metrics.Descent}");
        Assert.True(metrics.Top <= metrics.Ascent + tolerance,
            $"Top ({metrics.Top}) should be <= Ascent ({metrics.Ascent}) (tolerance {tolerance})");
        Assert.True(metrics.Bottom >= metrics.Descent - tolerance,
            $"Bottom ({metrics.Bottom}) should be >= Descent ({metrics.Descent}) (tolerance {tolerance})");
    }

    // ---------------------------------------------------------------
    // 4. MeasureText returns positive width and non-empty bounds
    // ---------------------------------------------------------------

    [Fact]
    public void MeasureText_ReturnsPositiveWidthAndBounds()
    {
        var assetLoader = CreateAssetLoader();
        var paint = new SKPaint
        {
            TextSize = 16f,
            Typeface = SKTypeface.FromFamilyName(
                "sans-serif",
                SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };
        var bounds = new SKRect();

        var width = assetLoader.MeasureText("Hello, SVG!", paint, ref bounds);

        Assert.True(width > 0, $"Expected positive width, got {width}");
        Assert.True(bounds.Right > bounds.Left, "Bounds width should be positive");
    }

    // ---------------------------------------------------------------
    // 5. GetTextPath returns a non-empty path for renderable text
    // ---------------------------------------------------------------

    [Fact]
    public void GetTextPath_ReturnsNonEmptyPath()
    {
        var assetLoader = CreateAssetLoader();
        var paint = new SKPaint
        {
            TextSize = 20f,
            Typeface = SKTypeface.FromFamilyName(
                "sans-serif",
                SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };

        var path = assetLoader.GetTextPath("Test", paint, 0f, 20f);

        Assert.NotNull(path);
        Assert.NotNull(path.Commands);
        Assert.NotEmpty(path.Commands);
    }

    // ---------------------------------------------------------------
    // 6. Small text sizes produce proportionally scaled measurements
    // ---------------------------------------------------------------

    [Theory]
    [InlineData(4f)]
    [InlineData(8f)]
    [InlineData(12f)]
    public void MeasureText_SmallTextSize_ReturnsProportionalWidth(float textSize)
    {
        var assetLoader = CreateAssetLoader();
        var paint = new SKPaint
        {
            TextSize = textSize,
            Typeface = SKTypeface.FromFamilyName(
                "sans-serif",
                SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };
        var paintLarge = new SKPaint
        {
            TextSize = 32f,
            Typeface = SKTypeface.FromFamilyName(
                "sans-serif",
                SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };
        var bounds = new SKRect();
        var boundsLarge = new SKRect();

        var widthSmall = assetLoader.MeasureText("Scaling", paint, ref bounds);
        var widthLarge = assetLoader.MeasureText("Scaling", paintLarge, ref boundsLarge);

        Assert.True(widthSmall > 0, $"Expected positive width for TextSize={textSize}");
        Assert.True(widthLarge > 0, "Expected positive width for TextSize=32");

        // Width should be roughly proportional to text size.
        // Allow a generous tolerance (±30%) because hinting and rounding affect small sizes.
        var expectedRatio = textSize / 32f;
        var actualRatio = widthSmall / widthLarge;
        Assert.True(
            actualRatio > expectedRatio * 0.7 && actualRatio < expectedRatio * 1.3,
            $"Width ratio {actualRatio:F3} should be near expected {expectedRatio:F3} for TextSize={textSize}");
    }

    // ---------------------------------------------------------------
    // 7. DrawWireframe produces stroke-style output using text properties
    // ---------------------------------------------------------------

    [Fact]
    public void DrawWireframe_DrawsTextCommands_WithoutError()
    {
        var model = CreateModel();
        var paint = new SKPaint
        {
            TextSize = 16f,
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName(
                "sans-serif",
                SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright),
            Color = new SKColor(0xFF, 0x00, 0x00, 0xFF)
        };

        var commands = new List<CanvasCommand>
        {
            new DrawTextCanvasCommand("Wireframe", 50f, 50f, paint)
        };
        var picture = new SKPicture(new SKRect(0, 0, 200, 100), commands);

        using var bitmap = new SkiaSharp.SKBitmap(200, 100);
        using var canvas = new SkiaSharp.SKCanvas(bitmap);
        canvas.Clear(SkiaSharp.SKColors.White);

        // Should not throw and should produce some drawn pixels
        model.DrawWireframe(picture, canvas);
        canvas.Flush();

        // Verify at least one non-white pixel was drawn (wireframe uses gray color)
        var hasDrawnPixels = false;
        for (var y = 0; y < bitmap.Height && !hasDrawnPixels; y++)
        {
            for (var x = 0; x < bitmap.Width && !hasDrawnPixels; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                if (pixel != SkiaSharp.SKColors.White)
                {
                    hasDrawnPixels = true;
                }
            }
        }

        Assert.True(hasDrawnPixels, "DrawWireframe should have produced at least one non-white pixel");
    }

    // ---------------------------------------------------------------
    // 8. Positioned text blob caching is stable for identical paint
    // ---------------------------------------------------------------

    [Fact]
    public void DrawTextBlob_CachesPositionedBlob_WhenPaintMatches()
    {
        var model = CreateModel();
        var paint = new SKPaint
        {
            TextSize = 14f,
            Typeface = SKTypeface.FromFamilyName(
                "sans-serif",
                SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };

        var textBlob = SKTextBlob.CreatePositioned("AB", new[]
        {
            new SKPoint(0, 0),
            new SKPoint(10, 0)
        });

        var commands = new List<CanvasCommand>
        {
            new DrawTextBlobCanvasCommand(textBlob, 0, 0, paint)
        };
        var picture = new SKPicture(new SKRect(0, 0, 100, 50), commands);

        using var bitmap1 = new SkiaSharp.SKBitmap(100, 50);
        using var canvas1 = new SkiaSharp.SKCanvas(bitmap1);
        canvas1.Clear(SkiaSharp.SKColors.White);
        model.Draw(picture, canvas1);
        canvas1.Flush();

        // Draw the same picture a second time to exercise the cache path
        using var bitmap2 = new SkiaSharp.SKBitmap(100, 50);
        using var canvas2 = new SkiaSharp.SKCanvas(bitmap2);
        canvas2.Clear(SkiaSharp.SKColors.White);
        model.Draw(picture, canvas2);
        canvas2.Flush();

        // Both renders should produce identical output
        Assert.Equal(bitmap1.Width, bitmap2.Width);
        Assert.Equal(bitmap1.Height, bitmap2.Height);
        for (var y = 0; y < bitmap1.Height; y++)
        {
            for (var x = 0; x < bitmap1.Width; x++)
            {
                Assert.Equal(bitmap1.GetPixel(x, y), bitmap2.GetPixel(x, y));
            }
        }
    }

    // ---------------------------------------------------------------
    // 9. DrawText fallback renders non-blank output
    // ---------------------------------------------------------------

    [Fact]
    public void DrawText_FallbackPath_RendersNonBlankOutput()
    {
        var model = CreateModel();
        var paint = new SKPaint
        {
            TextSize = 20f,
            Color = new SKColor(0x00, 0x00, 0x00, 0xFF),
            Typeface = SKTypeface.FromFamilyName(
                "sans-serif",
                SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };

        var commands = new List<CanvasCommand>
        {
            new DrawTextCanvasCommand("Hello", 10f, 30f, paint)
        };
        var picture = new SKPicture(new SKRect(0, 0, 200, 50), commands);

        using var bitmap = new SkiaSharp.SKBitmap(200, 50);
        using var canvas = new SkiaSharp.SKCanvas(bitmap);
        canvas.Clear(SkiaSharp.SKColors.White);
        model.Draw(picture, canvas);
        canvas.Flush();

        var hasDrawnPixels = false;
        for (var y = 0; y < bitmap.Height && !hasDrawnPixels; y++)
        {
            for (var x = 0; x < bitmap.Width && !hasDrawnPixels; x++)
            {
                if (bitmap.GetPixel(x, y) != SkiaSharp.SKColors.White)
                {
                    hasDrawnPixels = true;
                }
            }
        }

        Assert.True(hasDrawnPixels, "DrawText should have rendered visible text pixels");
    }

    // ---------------------------------------------------------------
    // 10. DrawTextOnPath renders text along a path
    // ---------------------------------------------------------------

    [Fact]
    public void DrawTextOnPath_RendersTextAlongPath()
    {
        var model = CreateModel();
        var paint = new SKPaint
        {
            TextSize = 16f,
            Color = new SKColor(0x00, 0x00, 0x00, 0xFF),
            Typeface = SKTypeface.FromFamilyName(
                "sans-serif",
                SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright)
        };

        // Create a simple horizontal line path
        var shimPath = new SKPath();
        shimPath.MoveTo(10f, 40f);
        shimPath.LineTo(190f, 40f);

        var commands = new List<CanvasCommand>
        {
            new DrawTextOnPathCanvasCommand("Path text", shimPath, 0f, 0f, paint)
        };
        var picture = new SKPicture(new SKRect(0, 0, 200, 80), commands);

        using var bitmap = new SkiaSharp.SKBitmap(200, 80);
        using var canvas = new SkiaSharp.SKCanvas(bitmap);
        canvas.Clear(SkiaSharp.SKColors.White);
        model.Draw(picture, canvas);
        canvas.Flush();

        var hasDrawnPixels = false;
        for (var y = 0; y < bitmap.Height && !hasDrawnPixels; y++)
        {
            for (var x = 0; x < bitmap.Width && !hasDrawnPixels; x++)
            {
                if (bitmap.GetPixel(x, y) != SkiaSharp.SKColors.White)
                {
                    hasDrawnPixels = true;
                }
            }
        }

        Assert.True(hasDrawnPixels, "DrawTextOnPath should have rendered visible text pixels");
    }
}
