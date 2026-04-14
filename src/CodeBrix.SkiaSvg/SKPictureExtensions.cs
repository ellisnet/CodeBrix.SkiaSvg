// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.IO;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg; //Was previously: namespace Svg.Skia;

/// <summary>Provides extension methods for <see cref="SkiaSharp.SKPicture"/> rendering and export.</summary>
public static class SKPictureExtensions
{
    /// <summary>Draws the picture onto the specified canvas with a background color and scale.</summary>
    /// <param name="skPicture">The picture to draw.</param>
    /// <param name="background">The background color.</param>
    /// <param name="scaleX">The horizontal scale factor.</param>
    /// <param name="scaleY">The vertical scale factor.</param>
    /// <param name="skCanvas">The canvas to draw on.</param>
    public static void Draw(this SkiaSharp.SKPicture skPicture, SkiaSharp.SKColor background, float scaleX, float scaleY, SkiaSharp.SKCanvas skCanvas)
    {
        skCanvas.Clear(background);
        skCanvas.Save();
        skCanvas.Scale(scaleX, scaleY);
        skCanvas.DrawPicture(skPicture);
        skCanvas.Restore();
    }

    /// <summary>Converts the picture to a bitmap with the specified settings.</summary>
    /// <param name="skPicture">The picture to convert.</param>
    /// <param name="background">The background color.</param>
    /// <param name="scaleX">The horizontal scale factor.</param>
    /// <param name="scaleY">The vertical scale factor.</param>
    /// <param name="skColorType">The color type for the bitmap.</param>
    /// <param name="skAlphaType">The alpha type for the bitmap.</param>
    /// <param name="skColorSpace">The color space for the bitmap.</param>
    /// <returns>The bitmap, or <c>null</c> if dimensions are invalid.</returns>
    public static SkiaSharp.SKBitmap ToBitmap(this SkiaSharp.SKPicture skPicture, SkiaSharp.SKColor background, float scaleX, float scaleY, SkiaSharp.SKColorType skColorType, SkiaSharp.SKAlphaType skAlphaType, SkiaSharp.SKColorSpace skColorSpace)
    {
        var width = skPicture.CullRect.Width * scaleX;
        var height = skPicture.CullRect.Height * scaleY;
        if (!(width > 0) || !(height > 0))
        {
            return null;
        }
        var skImageInfo = new SkiaSharp.SKImageInfo((int)width, (int)height, skColorType, skAlphaType, skColorSpace);
        var skBitmap = new SkiaSharp.SKBitmap(skImageInfo);
        using var skCanvas = new SkiaSharp.SKCanvas(skBitmap);
        Draw(skPicture, background, scaleX, scaleY, skCanvas);
        return skBitmap;
    }

    /// <summary>Encodes the picture as an image and writes it to a stream.</summary>
    /// <param name="skPicture">The picture to encode.</param>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="background">The background color.</param>
    /// <param name="format">The image encoding format.</param>
    /// <param name="quality">The encoding quality.</param>
    /// <param name="scaleX">The horizontal scale factor.</param>
    /// <param name="scaleY">The vertical scale factor.</param>
    /// <param name="skColorType">The color type.</param>
    /// <param name="skAlphaType">The alpha type.</param>
    /// <param name="skColorSpace">The color space.</param>
    /// <returns><c>true</c> if the image was written successfully; otherwise, <c>false</c>.</returns>
    public static bool ToImage(this SkiaSharp.SKPicture skPicture, Stream stream, SkiaSharp.SKColor background, SkiaSharp.SKEncodedImageFormat format, int quality, float scaleX, float scaleY, SkiaSharp.SKColorType skColorType, SkiaSharp.SKAlphaType skAlphaType, SkiaSharp.SKColorSpace skColorSpace)
    {
        using var skBitmap = skPicture.ToBitmap(background, scaleX, scaleY, skColorType, skAlphaType, skColorSpace);
        if (skBitmap is null)
        {
            return false;
        }
        using var skImage = SkiaSharp.SKImage.FromBitmap(skBitmap);
        using var skData = skImage.Encode(format, quality);
        if (skData is { })
        {
            skData.SaveTo(stream);
            return true;
        }
        return false;
    }

    /// <summary>Exports the picture as SVG to the specified file path.</summary>
    /// <param name="skPicture">The picture to export.</param>
    /// <param name="path">The file path to write to.</param>
    /// <param name="background">The background color.</param>
    /// <param name="scaleX">The horizontal scale factor.</param>
    /// <param name="scaleY">The vertical scale factor.</param>
    /// <returns><c>true</c> if the SVG was written successfully; otherwise, <c>false</c>.</returns>
    public static bool ToSvg(this SkiaSharp.SKPicture skPicture, string path, SkiaSharp.SKColor background, float scaleX, float scaleY)
    {
        var width = skPicture.CullRect.Width * scaleX;
        var height = skPicture.CullRect.Height * scaleY;
        if (width <= 0 || height <= 0)
        {
            return false;
        }
        using var skFileWStream = new SkiaSharp.SKFileWStream(path);
        using var skCanvas = SkiaSharp.SKSvgCanvas.Create(SkiaSharp.SKRect.Create(0, 0, width, height), skFileWStream);
        Draw(skPicture, background, scaleX, scaleY, skCanvas);
        return true;
    }

    /// <summary>Exports the picture as SVG to the specified stream.</summary>
    /// <param name="skPicture">The picture to export.</param>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="background">The background color.</param>
    /// <param name="scaleX">The horizontal scale factor.</param>
    /// <param name="scaleY">The vertical scale factor.</param>
    /// <returns><c>true</c> if the SVG was written successfully; otherwise, <c>false</c>.</returns>
    public static bool ToSvg(this SkiaSharp.SKPicture skPicture, Stream stream, SkiaSharp.SKColor background, float scaleX, float scaleY)
    {
        var width = skPicture.CullRect.Width * scaleX;
        var height = skPicture.CullRect.Height * scaleY;
        if (width <= 0 || height <= 0)
        {
            return false;
        }
        using var skCanvas = SkiaSharp.SKSvgCanvas.Create(SkiaSharp.SKRect.Create(0, 0, width, height), stream);
        Draw(skPicture, background, scaleX, scaleY, skCanvas);
        return true;
    }

    /// <summary>Exports the picture as PDF to the specified file path.</summary>
    /// <param name="skPicture">The picture to export.</param>
    /// <param name="path">The file path to write to.</param>
    /// <param name="background">The background color.</param>
    /// <param name="scaleX">The horizontal scale factor.</param>
    /// <param name="scaleY">The vertical scale factor.</param>
    /// <returns><c>true</c> if the PDF was written successfully; otherwise, <c>false</c>.</returns>
    public static bool ToPdf(this SkiaSharp.SKPicture skPicture, string path, SkiaSharp.SKColor background, float scaleX, float scaleY)
    {
        var width = skPicture.CullRect.Width * scaleX;
        var height = skPicture.CullRect.Height * scaleY;
        if (width <= 0 || height <= 0)
        {
            return false;
        }
        using var skFileWStream = new SkiaSharp.SKFileWStream(path);
        using var skDocument = SkiaSharp.SKDocument.CreatePdf(skFileWStream, SkiaSharp.SKDocument.DefaultRasterDpi);
        using var skCanvas = skDocument.BeginPage(width, height);
        Draw(skPicture, background, scaleX, scaleY, skCanvas);
        skDocument.Close();
        return true;
    }

    /// <summary>Exports the picture as PDF to the specified stream.</summary>
    /// <param name="skPicture">The picture to export.</param>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="background">The background color.</param>
    /// <param name="scaleX">The horizontal scale factor.</param>
    /// <param name="scaleY">The vertical scale factor.</param>
    /// <returns><c>true</c> if the PDF was written successfully; otherwise, <c>false</c>.</returns>
    public static bool ToPdf(this SkiaSharp.SKPicture skPicture, Stream stream, SkiaSharp.SKColor background, float scaleX, float scaleY)
    {
        var width = skPicture.CullRect.Width * scaleX;
        var height = skPicture.CullRect.Height * scaleY;
        if (width <= 0 || height <= 0)
        {
            return false;
        }
        using var skDocument = SkiaSharp.SKDocument.CreatePdf(stream, SkiaSharp.SKDocument.DefaultRasterDpi);
        using var skCanvas = skDocument.BeginPage(width, height);
        Draw(skPicture, background, scaleX, scaleY, skCanvas);
        skDocument.Close();
        return true;
    }

    /// <summary>Exports the picture as XPS to the specified file path.</summary>
    /// <param name="skPicture">The picture to export.</param>
    /// <param name="path">The file path to write to.</param>
    /// <param name="background">The background color.</param>
    /// <param name="scaleX">The horizontal scale factor.</param>
    /// <param name="scaleY">The vertical scale factor.</param>
    /// <returns><c>true</c> if the XPS was written successfully; otherwise, <c>false</c>.</returns>
    public static bool ToXps(this SkiaSharp.SKPicture skPicture, string path, SkiaSharp.SKColor background, float scaleX, float scaleY)
    {
        var width = skPicture.CullRect.Width * scaleX;
        var height = skPicture.CullRect.Height * scaleY;
        if (width <= 0 || height <= 0)
        {
            return false;
        }
        using var skFileWStream = new SkiaSharp.SKFileWStream(path);
        using var skDocument = SkiaSharp.SKDocument.CreateXps(skFileWStream, SkiaSharp.SKDocument.DefaultRasterDpi);
        using var skCanvas = skDocument.BeginPage(width, height);
        Draw(skPicture, background, scaleX, scaleY, skCanvas);
        skDocument.Close();
        return true;
    }

    /// <summary>Exports the picture as XPS to the specified stream.</summary>
    /// <param name="skPicture">The picture to export.</param>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="background">The background color.</param>
    /// <param name="scaleX">The horizontal scale factor.</param>
    /// <param name="scaleY">The vertical scale factor.</param>
    /// <returns><c>true</c> if the XPS was written successfully; otherwise, <c>false</c>.</returns>
    public static bool ToXps(this SkiaSharp.SKPicture skPicture, Stream stream, SkiaSharp.SKColor background, float scaleX, float scaleY)
    {
        var width = skPicture.CullRect.Width * scaleX;
        var height = skPicture.CullRect.Height * scaleY;
        if (width <= 0 || height <= 0)
        {
            return false;
        }
        using var skDocument = SkiaSharp.SKDocument.CreateXps(stream, SkiaSharp.SKDocument.DefaultRasterDpi);
        using var skCanvas = skDocument.BeginPage(width, height);
        Draw(skPicture, background, scaleX, scaleY, skCanvas);
        skDocument.Close();
        return true;
    }
}
