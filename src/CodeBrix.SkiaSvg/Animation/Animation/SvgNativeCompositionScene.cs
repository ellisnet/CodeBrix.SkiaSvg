using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CodeBrix.SkiaSvg.ShimSkiaSharp;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg; //Was previously: namespace Svg.Skia;

/// <summary>Represents a single compositing layer in a native composition scene.</summary>
public sealed class SvgNativeCompositionLayer
{
    /// <summary>Initializes a new instance of the <see cref="SvgNativeCompositionLayer"/> class.</summary>
    /// <param name="documentChildIndex">The index of the child in the source document.</param>
    /// <param name="isAnimated">Whether this layer contains animated content.</param>
    /// <param name="picture">The drawing model for this layer.</param>
    /// <param name="offset">The offset position of the layer.</param>
    /// <param name="size">The size of the layer.</param>
    /// <param name="opacity">The opacity of the layer.</param>
    /// <param name="isVisible">Whether the layer is visible.</param>
    public SvgNativeCompositionLayer(
        int documentChildIndex,
        bool isAnimated,
        SKPicture picture,
        SKPoint offset,
        SKSize size,
        float opacity,
        bool isVisible)
    {
        DocumentChildIndex = documentChildIndex;
        IsAnimated = isAnimated;
        Picture = picture;
        Offset = offset;
        Size = size;
        Opacity = opacity;
        IsVisible = isVisible;
    }

    /// <summary>Gets the index of the child element in the source document.</summary>
    public int DocumentChildIndex { get; }

    /// <summary>Gets a value indicating whether this layer contains animated content.</summary>
    public bool IsAnimated { get; }

    /// <summary>Gets the drawing model for this layer.</summary>
    public SKPicture Picture { get; }

    /// <summary>Gets the offset position of the layer.</summary>
    public SKPoint Offset { get; }

    /// <summary>Gets the size of the layer.</summary>
    public SKSize Size { get; }

    /// <summary>Gets the opacity of the layer.</summary>
    public float Opacity { get; }

    /// <summary>Gets a value indicating whether the layer is visible.</summary>
    public bool IsVisible { get; }
}

/// <summary>Represents a complete native composition scene with source bounds and compositing layers.</summary>
public sealed class SvgNativeCompositionScene
{
    /// <summary>Initializes a new instance of the <see cref="SvgNativeCompositionScene"/> class.</summary>
    /// <param name="sourceBounds">The bounds of the source content.</param>
    /// <param name="layers">The compositing layers.</param>
    public SvgNativeCompositionScene(SKRect sourceBounds, IReadOnlyList<SvgNativeCompositionLayer> layers)
    {
        SourceBounds = sourceBounds;
        Layers = new ReadOnlyCollection<SvgNativeCompositionLayer>(layers.ToArray());
    }

    /// <summary>Gets the bounds of the source content.</summary>
    public SKRect SourceBounds { get; }

    /// <summary>Gets the compositing layers in this scene.</summary>
    public IReadOnlyList<SvgNativeCompositionLayer> Layers { get; }
}

/// <summary>Represents a single frame of a native composition animation with updated layers.</summary>
public sealed class SvgNativeCompositionFrame
{
    /// <summary>Initializes a new instance of the <see cref="SvgNativeCompositionFrame"/> class.</summary>
    /// <param name="sourceBounds">The bounds of the source content.</param>
    /// <param name="layers">The compositing layers for this frame.</param>
    public SvgNativeCompositionFrame(SKRect sourceBounds, IReadOnlyList<SvgNativeCompositionLayer> layers)
    {
        SourceBounds = sourceBounds;
        Layers = new ReadOnlyCollection<SvgNativeCompositionLayer>(layers.ToArray());
    }

    /// <summary>Gets the bounds of the source content.</summary>
    public SKRect SourceBounds { get; }

    /// <summary>Gets the compositing layers for this frame.</summary>
    public IReadOnlyList<SvgNativeCompositionLayer> Layers { get; }
}
