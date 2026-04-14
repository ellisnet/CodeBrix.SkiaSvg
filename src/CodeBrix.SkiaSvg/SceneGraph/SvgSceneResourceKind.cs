using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg; //Was previously: namespace Svg.Skia;

public enum SvgSceneResourceKind
{
    Unknown,
    ClipPath,
    Mask,
    Filter,
    Gradient,
    Pattern,
    Marker,
    Symbol,
    PaintServer
}
