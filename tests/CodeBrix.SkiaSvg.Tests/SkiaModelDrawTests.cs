using System.Collections.Generic;
using CodeBrix.SkiaSvg.ShimSkiaSharp;
using Xunit;

namespace CodeBrix.SkiaSvg.Tests;

/// <summary>
/// Tests covering the <see cref="SkiaModel.Draw"/> method's handling of
/// canvas commands during rendering.
/// </summary>
public class SkiaModelDrawTests
{
    private static SkiaModel CreateModel()
    {
        return new SkiaModel(new SKSvgSettings());
    }

    // ---------------------------------------------------------------
    // SetMatrixCanvasCommand applies transformation during Draw
    // ---------------------------------------------------------------
    //
    // This test exercises the code path in SkiaModel.Draw() that handles
    // SetMatrixCanvasCommand. That code converts a shim SKMatrix to a
    // SkiaSharp SKMatrix via ToSKMatrix() and then concatenates it onto
    // the SkiaSharp canvas via skCanvas.Concat().
    //
    // The test creates a picture that:
    //   1. Sets a translation matrix (shifts content 80px to the right)
    //   2. Draws a small filled rectangle at the origin
    //
    // After rendering, the rectangle should appear at the translated
    // position (~x=80) rather than at the origin (~x=0), proving that
    // the matrix was correctly applied to the canvas.
    // ---------------------------------------------------------------

    [Fact]
    public void Draw_SetMatrixCommand_AppliesTranslation()
    {
        var model = CreateModel();

        // A small 10×10 filled rectangle at the origin
        var rectPath = new SKPath();
        rectPath.AddRect(SKRect.Create(0, 0, 10, 10));

        var paint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0xFF, 0x00, 0x00, 0xFF)
        };

        // Translate 80px to the right
        var translateMatrix = SKMatrix.CreateTranslation(80, 0);

        var commands = new List<CanvasCommand>
        {
            new SetMatrixCanvasCommand(translateMatrix, translateMatrix),
            new DrawPathCanvasCommand(rectPath, paint)
        };
        var picture = new SKPicture(new SKRect(0, 0, 200, 50), commands);

        using var bitmap = new SkiaSharp.SKBitmap(200, 50);
        using var canvas = new SkiaSharp.SKCanvas(bitmap);
        canvas.Clear(SkiaSharp.SKColors.White);
        model.Draw(picture, canvas);
        canvas.Flush();

        // The rectangle should NOT appear at the origin (x=0..9)
        var hasPixelsAtOrigin = false;
        for (var y = 0; y < 10; y++)
        {
            for (var x = 0; x < 10; x++)
            {
                if (bitmap.GetPixel(x, y) != SkiaSharp.SKColors.White)
                {
                    hasPixelsAtOrigin = true;
                }
            }
        }

        Assert.False(hasPixelsAtOrigin,
            "No drawn pixels should appear at the origin (0,0)-(10,10) " +
            "because the translation matrix should have shifted them.");

        // The rectangle SHOULD appear at the translated position (x=80..89)
        var hasPixelsAtTranslated = false;
        for (var y = 0; y < 10; y++)
        {
            for (var x = 80; x < 90; x++)
            {
                if (bitmap.GetPixel(x, y) != SkiaSharp.SKColors.White)
                {
                    hasPixelsAtTranslated = true;
                }
            }
        }

        Assert.True(hasPixelsAtTranslated,
            "Drawn pixels should appear at the translated position (80,0)-(90,10) " +
            "after the SetMatrixCanvasCommand applied an 80px horizontal translation.");
    }

    [Fact]
    public void Draw_SetMatrixCommand_AppliesScaling()
    {
        var model = CreateModel();

        // A small 10×10 filled rectangle at (5,5)
        var rectPath = new SKPath();
        rectPath.AddRect(SKRect.Create(5, 5, 10, 10));

        var paint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor(0x00, 0x00, 0xFF, 0xFF)
        };

        // Scale 2× in both directions — the rect at (5,5)-(15,15) becomes (10,10)-(30,30)
        var scaleMatrix = SKMatrix.CreateScale(2, 2);

        var commands = new List<CanvasCommand>
        {
            new SetMatrixCanvasCommand(scaleMatrix, scaleMatrix),
            new DrawPathCanvasCommand(rectPath, paint)
        };
        var picture = new SKPicture(new SKRect(0, 0, 100, 100), commands);

        using var bitmap = new SkiaSharp.SKBitmap(100, 100);
        using var canvas = new SkiaSharp.SKCanvas(bitmap);
        canvas.Clear(SkiaSharp.SKColors.White);
        model.Draw(picture, canvas);
        canvas.Flush();

        // After 2× scaling, the center of the original rect (10,10) maps to (20,20).
        // Check that pixels appear in the scaled region.
        var hasPixelsInScaledRegion = false;
        for (var y = 15; y < 25; y++)
        {
            for (var x = 15; x < 25; x++)
            {
                if (bitmap.GetPixel(x, y) != SkiaSharp.SKColors.White)
                {
                    hasPixelsInScaledRegion = true;
                }
            }
        }

        Assert.True(hasPixelsInScaledRegion,
            "Drawn pixels should appear in the scaled region after the " +
            "SetMatrixCanvasCommand applied a 2× scale transformation.");
    }
}
