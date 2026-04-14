using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg; //Was previously: namespace Svg.Skia;

/// <summary>Identifies the type of SVG pointer event.</summary>
public enum SvgPointerEventType
{
    /// <summary>The pointer moved.</summary>
    Move,
    /// <summary>A pointer button was pressed.</summary>
    Press,
    /// <summary>A pointer button was released.</summary>
    Release,
    /// <summary>The pointer entered an element.</summary>
    Enter,
    /// <summary>The pointer left an element.</summary>
    Leave,
    /// <summary>The pointer wheel was scrolled.</summary>
    Wheel,
    /// <summary>A click occurred.</summary>
    Click
}
