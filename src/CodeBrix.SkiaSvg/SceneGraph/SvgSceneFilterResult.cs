using CodeBrix.SkiaSvg.ShimSkiaSharp;
using CodeBrix.SvgParse.Primitives;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg; //Was previously: namespace Svg.Skia;

internal sealed class SvgSceneFilterResult
{
    public SvgSceneFilterResult(string key, SKImageFilter filter, SvgColorInterpolation colorSpace)
    {
        Key = key;
        Filter = filter;
        ColorSpace = colorSpace;
    }

    public string Key { get; }

    public SKImageFilter Filter { get; }

    public SvgColorInterpolation ColorSpace { get; }
}
