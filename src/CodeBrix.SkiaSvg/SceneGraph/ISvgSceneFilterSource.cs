using CodeBrix.SkiaSvg.ShimSkiaSharp;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg; //Was previously: namespace Svg.Skia;

internal interface ISvgSceneFilterSource
{
    SKPicture SourceGraphic(SKRect? clip);
    SKPicture BackgroundImage(SKRect? clip);
    SKPicture FillPaint(SKRect? clip);
    SKPicture StrokePaint(SKRect? clip);
}
