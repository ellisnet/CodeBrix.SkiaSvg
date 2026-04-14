using System;
using System.Collections.Generic;
using CodeBrix.SkiaSvg.ShimSkiaSharp;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg; //Was previously: namespace Svg.Skia;

public partial class SKSvg
{
    /// <summary>
    /// Returns all scene nodes whose bounds contain the specified point.
    /// </summary>
    /// <param name="point">The point to test in picture coordinates.</param>
    /// <returns>An enumerable of scene nodes that intersect the point.</returns>
    public IEnumerable<SvgSceneNode> HitTestSceneNodes(SKPoint point)
    {
        if (TryEnsureRetainedSceneGraph(out var sceneDocument) && sceneDocument is not null)
        {
            foreach (var node in sceneDocument.HitTest(point))
            {
                yield return node;
            }
        }
    }

    /// <summary>
    /// Returns all scene nodes whose bounds intersect the specified rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to test in picture coordinates.</param>
    /// <returns>An enumerable of scene nodes that intersect the rectangle.</returns>
    public IEnumerable<SvgSceneNode> HitTestSceneNodes(SKRect rect)
    {
        if (TryEnsureRetainedSceneGraph(out var sceneDocument) && sceneDocument is not null)
        {
            foreach (var node in sceneDocument.HitTest(rect))
            {
                yield return node;
            }
        }
    }

    /// <summary>
    /// Returns the topmost scene node whose bounds contain the specified point.
    /// </summary>
    /// <param name="point">The point to test in picture coordinates.</param>
    /// <returns>The topmost scene node at the point, or <see langword="null"/> if none.</returns>
    public SvgSceneNode HitTestTopmostSceneNode(SKPoint point)
    {
        if (!TryEnsureRetainedSceneGraph(out var sceneDocument) || sceneDocument is null)
        {
            return null;
        }

        return sceneDocument.HitTestTopmostNode(point);
    }

    /// <summary>
    /// Returns the topmost scene node at the specified point after transforming through a canvas matrix.
    /// </summary>
    /// <param name="point">The point to test in canvas coordinates.</param>
    /// <param name="canvasMatrix">The canvas transformation matrix used to map to picture coordinates.</param>
    /// <returns>The topmost scene node at the transformed point, or <see langword="null"/> if none.</returns>
    public SvgSceneNode HitTestTopmostSceneNode(SKPoint point, SKMatrix canvasMatrix)
    {
        return TryGetPicturePoint(point, canvasMatrix, out var picturePoint)
            ? HitTestTopmostSceneNode(picturePoint)
            : null;
    }

    /// <summary>
    /// Returns the topmost SVG element whose scene node bounds contain the specified point.
    /// </summary>
    /// <param name="point">The point to test in picture coordinates.</param>
    /// <returns>The topmost SVG element at the point, or <see langword="null"/> if none.</returns>
    public SvgElement HitTestTopmostElement(SKPoint point)
    {
        return HitTestTopmostSceneNode(point)?.HitTestTargetElement;
    }

    /// <summary>
    /// Returns the topmost SVG element at the specified point after transforming through a canvas matrix.
    /// </summary>
    /// <param name="point">The point to test in canvas coordinates.</param>
    /// <param name="canvasMatrix">The canvas transformation matrix used to map to picture coordinates.</param>
    /// <returns>The topmost SVG element at the transformed point, or <see langword="null"/> if none.</returns>
    public SvgElement HitTestTopmostElement(SKPoint point, SKMatrix canvasMatrix)
    {
        return TryGetPicturePoint(point, canvasMatrix, out var picturePoint)
            ? HitTestTopmostElement(picturePoint)
            : null;
    }
}
