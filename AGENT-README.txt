================================================================================
AGENT-README: CodeBrix.SkiaSvg
A Comprehensive Guide for AI Coding Agents
================================================================================

OVERVIEW
--------
CodeBrix.SkiaSvg is an SVG loading and rendering library for .NET 10.0+,
built on SkiaSharp. It loads SVG documents (and Android VectorDrawables)
and renders them to SkiaSharp surfaces. It also provides hit testing,
retained scene graph, animation, native composition, pointer interaction,
and export to multiple image and document formats.

CodeBrix.SkiaSvg consolidates several companion packages from the upstream
Svg.Skia ecosystem into a single library.

CodeBrix.SkiaSvg is a fork of the Svg.Skia project (v4.2.0). All namespaces
use "CodeBrix.SkiaSvg" instead of "Svg.Skia" or "ShimSkiaSharp". Do NOT use
Svg.Skia or ShimSkiaSharp namespaces.


INSTALLATION
------------
NuGet Package: CodeBrix.SkiaSvg.MitLicenseForever
Dependencies:
  - CodeBrix.SvgParse.MsplLicenseForever
  - SkiaSharp
  - HarfBuzzSharp

    dotnet add package CodeBrix.SkiaSvg.MitLicenseForever

IMPORTANT: The NuGet package name is CodeBrix.SkiaSvg.MitLicenseForever
(NOT CodeBrix.SkiaSvg). The primary namespace is CodeBrix.SkiaSvg.

Requirements: .NET 10.0 or higher
License: MIT License


PINNED SKIASHARP DEPENDENCY (TEMPORARY)
---------------------------------------
CodeBrix.SkiaSvg is currently pinned to SkiaSharp 3.119.3-preview.1.1
rather than the latest stable SkiaSharp release. This is intentional:
the previous stable release (SkiaSharp 3.119.2) is missing native
libraries for ARM64 and RISC-V 64 platforms, so consuming CodeBrix.SkiaSvg
from those platforms with the stable release fails at load time on the
missing native assets.

When a stable SkiaSharp release ships that includes the ARM64 and
RISC-V 64 native assets, CodeBrix.SkiaSvg will move back to a stable
SkiaSharp reference. Update SkiaSharp (and SkiaSharp.NativeAssets.Linux
in the test project) together -- both projects must stay in lock-step.


KEY NAMESPACES
--------------
    using CodeBrix.SkiaSvg;                      // Main API (SKSvg, etc.)
    using CodeBrix.SkiaSvg.TypefaceProviders;     // Font resolution
    using CodeBrix.SkiaSvg.ShimSkiaSharp;         // SkiaSharp wrappers
    using CodeBrix.SkiaSvg.Interaction;           // Pointer events


================================================================================

CORE API REFERENCE
==================

SKSvg CLASS - MAIN ENTRY POINT
-------------------------------
The SKSvg class is the primary public API for loading and rendering SVGs.
It implements IDisposable - always use 'using' patterns.

Static factory methods (preferred):
    SKSvg.CreateFromFile(string path, SvgParameters? parameters = null)
    SKSvg.CreateFromStream(Stream stream, SvgParameters? parameters = null)
    SKSvg.CreateFromXmlReader(XmlReader reader)
    SKSvg.CreateFromSvg(string svg)
    SKSvg.CreateFromSvgDocument(SvgDocument svgDocument)
    SKSvg.CreateFromVectorDrawable(string path, ...)
    SKSvg.CreateFromVectorDrawable(Stream stream, ...)
    SKSvg.CreateFromVectorDrawable(XmlReader reader)

Key properties:
    SKSvgSettings Settings              // Configuration
    SvgDocument SourceDocument          // The loaded SVG DOM
    SkiaSharp.SKPicture Picture         // Rendered picture (SkiaSharp)
    ISvgAssetLoader AssetLoader         // Handles image/font loading

Instance loading methods:
    SKPicture Load(string path, SvgParameters? parameters = null)
    SKPicture Load(Stream stream, SvgParameters? parameters = null, Uri? baseUri = null)
    SKPicture Load(XmlReader reader)
    SKPicture LoadVectorDrawable(string path, SvgParameters? parameters = null)
    SKPicture LoadVectorDrawable(Stream stream, SvgParameters? parameters = null)
    SKPicture LoadVectorDrawable(XmlReader reader)
    void FromSvg(string svg)
    void FromVectorDrawable(string xml)
    void FromSvgDocument(SvgDocument document)
    void ReLoad(SvgParameters? parameters = null)

Drawing:
    void Draw(SkiaSharp.SKCanvas canvas)
    void RebuildFromModel()

Cloning:
    SKSvg Clone()

Wireframe debug:
    bool Wireframe                      // Toggle wireframe rendering
    void ClearWireframePicture()


SAVING AND EXPORT
------------------
Save rendered SVG to various formats.

Direct save methods on SKSvg:
    bool Save(Stream stream, SKColor background, SKEncodedImageFormat format,
              int quality, float scaleX, float scaleY)
    bool Save(string path, SKColor background, SKEncodedImageFormat format,
              int quality, float scaleX, float scaleY)

Extension methods on SKPicture (SKPictureExtensions):
    void Draw(SKColor background, float scaleX, float scaleY, SKCanvas canvas)
    SKBitmap ToBitmap(SKColor background, float scaleX, float scaleY,
                      SKColorType colorType, SKAlphaType alphaType,
                      SKColorSpace colorSpace)
    bool ToImage(Stream stream, SKColor background, SKEncodedImageFormat format,
                 int quality, float scaleX, float scaleY, ...)
    bool ToSvg(string path, SKColor background, float scaleX, float scaleY)
    bool ToSvg(Stream stream, SKColor background, float scaleX, float scaleY)
    bool ToPdf(string path, SKColor background, float scaleX, float scaleY)
    bool ToPdf(Stream stream, SKColor background, float scaleX, float scaleY)
    bool ToXps(string path, SKColor background, float scaleX, float scaleY)
    bool ToXps(Stream stream, SKColor background, float scaleX, float scaleY)

Supported export formats:
    Raster: PNG, JPEG, BMP, GIF, TIFF (via SKEncodedImageFormat)
    Vector: SVG, PDF, XPS (via extension methods)


================================================================================

HIT TESTING
============

Hit test SVG elements by point or rectangle. Supports canvas matrix
transformation for hit testing in transformed coordinate spaces.

Element hit testing:
    IEnumerable<SvgElement> HitTestElements(SKPoint point)
    IEnumerable<SvgElement> HitTestElements(SKRect rect)
    IEnumerable<SvgElement> HitTestElements(SKPoint point, SKMatrix canvasMatrix)
    IEnumerable<SvgElement> HitTestElements(SKRect rect, SKMatrix canvasMatrix)

Topmost element:
    SvgElement HitTestTopmostElement(SKPoint point)
    SvgElement HitTestTopmostElement(SKPoint point, SKMatrix canvasMatrix)

Scene node hit testing (retained mode):
    IEnumerable<SvgSceneNode> HitTestSceneNodes(SKPoint point)
    IEnumerable<SvgSceneNode> HitTestSceneNodes(SKRect rect)
    IEnumerable<SvgSceneNode> HitTestSceneNodes(SKPoint point, SKMatrix canvasMatrix)
    IEnumerable<SvgSceneNode> HitTestSceneNodes(SKRect rect, SKMatrix canvasMatrix)
    SvgSceneNode HitTestTopmostSceneNode(SKPoint point)
    SvgSceneNode HitTestTopmostSceneNode(SKPoint point, SKMatrix canvasMatrix)

Coordinate conversion:
    bool TryGetPicturePoint(SKPoint point, SKMatrix canvasMatrix,
                            out SKPoint picturePoint)
    bool TryGetPictureRect(SKRect rect, SKMatrix canvasMatrix,
                           out SKRect pictureRect)

Example:
    using var svg = SKSvg.CreateFromFile("interactive.svg");

    var point = new SKPoint(100, 50);
    var element = svg.HitTestTopmostElement(point);
    if (element != null)
    {
        Console.WriteLine($"Hit: {element.ElementName}");
    }

    // With canvas transform
    var elements = svg.HitTestElements(point, canvas.TotalMatrix);
    foreach (var el in elements)
    {
        Console.WriteLine($"  {el.ElementName}");
    }


================================================================================

RETAINED SCENE GRAPH
=====================

The retained scene graph provides a compiled, queryable representation of
the rendered SVG. It enables efficient partial updates (mutations) without
re-rendering the entire document.

Access:
    SvgSceneDocument RetainedSceneGraph     // The compiled scene
    bool HasRetainedSceneGraph              // Check availability
    bool TryEnsureRetainedSceneGraph(out SvgSceneDocument scene)

Node lookup:
    bool TryGetRetainedSceneNode(string addressKey, out SvgSceneNode node)
    bool TryGetRetainedSceneNode(SvgElement element, out SvgSceneNode node)
    bool TryGetRetainedSceneNodes(string addressKey,
                                  out IReadOnlyList<SvgSceneNode> nodes)
    bool TryGetRetainedSceneNodes(SvgElement element,
                                  out IReadOnlyList<SvgSceneNode> nodes)
    bool TryGetRetainedSceneNodeById(string id, out SvgSceneNode node)

Resource lookup:
    bool TryGetRetainedSceneResource(string addressKey,
                                     out SvgSceneResource resource)
    bool TryGetRetainedSceneResourceById(string id,
                                         out SvgSceneResource resource)

Rendering from scene graph:
    SKPicture CreateRetainedSceneGraphModel()
    SkiaSharp.SKPicture CreateRetainedSceneGraphPicture()
    SKPicture CreateRetainedSceneNodeModel(SvgSceneNode node, SKRect? clip = null)
    SkiaSharp.SKPicture CreateRetainedSceneNodePicture(SvgSceneNode node,
                                                       SKRect? clip = null)
    SKPicture CreateRetainedSceneModel(SvgElement element, SKRect? clip = null)
    SkiaSharp.SKPicture CreateRetainedScenePicture(SvgElement element,
                                                    SKRect? clip = null)

Scene mutation (dynamic updates):
    SvgSceneMutationResult ApplyRetainedSceneMutation(
        SvgElement element,
        IReadOnlyCollection<string> changedAttributes = null)
    SvgSceneMutationResult ApplyRetainedSceneMutation(
        string addressKey,
        IReadOnlyCollection<string> changedAttributes = null)
    SvgSceneMutationResult ApplyRetainedSceneMutationById(
        string id,
        IReadOnlyCollection<string> changedAttributes = null)

SvgSceneDocument:
    SvgDocument SourceDocument              // Source SVG DOM
    SvgSceneNode Root                       // Root scene node
    SKRect CullRect                         // Bounding rectangle
    IReadOnlyDictionary<string, SvgSceneNode> NodesById
    IReadOnlyDictionary<string, SvgSceneResource> ResourcesById

SvgSceneNode:
    SvgSceneNodeKind Kind                   // Type of node
    SvgElement Element                      // Source SVG element
    IReadOnlyList<SvgSceneNode> Children
    SKMatrix Transform
    SKPicture LocalModel                    // Rendered content
    SKPath HitTestPath                      // Hit test geometry
    float Opacity

SvgSceneNodeKind enum:
    Unknown, Fragment, Group, Anchor, Use, Switch,
    Image, Text, Marker, Path, Shape, Mask, Container

SvgSceneResource:
    SvgSceneResourceKind Kind
    SvgElement SourceElement
    string Id

SvgSceneResourceKind enum:
    Unknown, ClipPath, Mask, Filter, Gradient, Pattern, Marker, Symbol,
    PaintServer

Example:
    using var svg = SKSvg.CreateFromFile("diagram.svg");

    if (svg.TryEnsureRetainedSceneGraph(out var scene))
    {
        // Find a specific element's scene node
        if (svg.TryGetRetainedSceneNodeById("myRect", out var node))
        {
            // Render just this node
            var picture = svg.CreateRetainedSceneNodePicture(node);
        }

        // Mutate an element and update the scene
        var element = svg.SourceDocument.GetElementById("myRect");
        element.Fill = new SvgColorServer(System.Drawing.Color.Red);
        svg.ApplyRetainedSceneMutationById("myRect");
    }


================================================================================

ANIMATION
==========

SVG SMIL animation support with time-based control.

Properties:
    SvgAnimationController AnimationController
    bool HasAnimations
    TimeSpan AnimationTime
    TimeSpan AnimationMinimumRenderInterval
    bool HasPendingAnimationFrame
    int LastAnimationDirtyTargetCount

Control methods:
    void SetAnimationTime(TimeSpan time)
    void AdvanceAnimation(TimeSpan delta)
    void ResetAnimation()
    bool FlushPendingAnimationFrame()
    bool NotifyPointerEvent(SvgElement element,
                            SvgPointerEventType eventType)

Events:
    event EventHandler<SvgAnimationFrameChangedEventArgs> AnimationInvalidated

Example:
    using var svg = SKSvg.CreateFromFile("animated.svg");

    if (svg.HasAnimations)
    {
        // Seek to 2 seconds
        svg.SetAnimationTime(TimeSpan.FromSeconds(2));
        canvas.DrawPicture(svg.Picture);

        // Advance by 100ms
        svg.AdvanceAnimation(TimeSpan.FromMilliseconds(100));
        if (svg.HasPendingAnimationFrame)
        {
            svg.FlushPendingAnimationFrame();
            canvas.DrawPicture(svg.Picture);
        }
    }


================================================================================

NATIVE COMPOSITION
===================

Layer-based decomposition for optimized animation rendering. Instead of
re-rendering the entire SVG each frame, only animated layers are updated.

Properties and methods:
    bool SupportsNativeComposition
    bool TryCreateNativeCompositionScene(
             out SvgNativeCompositionScene scene)
    bool TryCreateNativeCompositionFrame(
             out SvgNativeCompositionFrame frame)

SvgNativeCompositionScene:
    SKRect SourceBounds
    IReadOnlyList<SvgNativeCompositionLayer> Layers

SvgNativeCompositionFrame:
    SKRect SourceBounds
    IReadOnlyList<SvgNativeCompositionLayer> Layers

SvgNativeCompositionLayer:
    int DocumentChildIndex
    bool IsAnimated
    SKPicture Picture
    SKPoint Offset
    SKSize Size
    float Opacity
    bool IsVisible


================================================================================

INTERACTION
============

Pointer/mouse event dispatching for interactive SVGs.

SvgPointerEventType enum:
    PointerDown, PointerUp, PointerMove, PointerEnter, PointerLeave, etc.

SvgPointerInput:
    SKPoint PicturePoint
    SvgPointerDeviceType PointerDeviceType  // Mouse, Touch, Pen
    SvgMouseButton Button                   // Left, Middle, Right, etc.
    int ClickCount
    int WheelDelta
    bool AltKey, ShiftKey, CtrlKey
    string SessionId

SvgPointerEventArgs:
    SvgPointerEventType EventType
    SvgElement Element
    SvgElement TargetElement
    SvgElement RelatedElement
    SvgPointerEventRoutePhase RoutePhase    // Tunnel, Target, Bubble
    SvgPointerInput Input
    string Cursor


================================================================================

CONFIGURATION: SKSvgSettings
==============================

Properties:
    SKAlphaType AlphaType                   // Premul or Unpremul
    SKColorType ColorType                   // Color format
    SKColorSpace SrgbLinear                 // Linear color space
    SKColorSpace Srgb                       // Standard RGB
    IList<ITypefaceProvider> TypefaceProviders  // Font resolution chain
    SKRect? StandaloneViewport              // Override viewport
    bool EnableSvgFonts                     // Support @font-face in SVG
    bool EnableTextReferences               // Support <tref> elements

Default typeface providers:
    1. FontManagerTypefaceProvider()        // System fonts via SkiaSharp
    2. DefaultTypefaceProvider()            // Fallback to default

Example:
    var svg = new SKSvg();
    svg.Settings.TypefaceProviders.Insert(0,
        new CustomTypefaceProvider());
    svg.Settings.EnableSvgFonts = true;
    svg.Load("fonts.svg");


================================================================================

TYPEFACE PROVIDERS
===================

Interface:
    public interface ITypefaceProvider
    {
        SKTypeface FromFamilyName(
            string fontFamily,
            SKFontStyleWeight fontWeight,
            SKFontStyleWidth fontWidth,
            SKFontStyleSlant fontStyle);
    }

Built-in implementations:
    FontManagerTypefaceProvider      // System fonts via SKFontManager
    DefaultTypefaceProvider          // SkiaSharp default typeface
    CustomTypefaceProvider           // User-defined custom fonts

Example - add a custom font:
    var provider = new CustomTypefaceProvider();
    // Register custom fonts as needed
    svg.Settings.TypefaceProviders.Insert(0, provider);


================================================================================

SHIMSKIASHARP NAMESPACE
========================

Provides internal representation types that bridge between the SVG model
and SkiaSharp rendering. These types are used internally but some are
publicly accessible.

Value types:
    SKPoint, SKPointI, SKPoint3, SKSize, SKSizeI, SKRect, SKMatrix,
    SKColor, SKColorF

Drawing types:
    SKPaint, SKPath, SKCanvas, SKPicture, SKPictureRecorder, SKDrawable,
    SKImage

Effects:
    SKColorFilter, SKImageFilter, SKShader

Enumerations:
    SKPaintStyle, SKStrokeCap, SKStrokeJoin, SKPathFillType,
    SKTextAlign, SKFontStyleWeight, SKBlendMode, SKShaderTileMode, etc.

Interfaces:
    IDeepCloneable<T>               // Deep clone support
    ICanvasCommandVisitor           // Visitor for canvas commands


================================================================================

COMPLETE EXAMPLES
==================

Example 1: Load and Render an SVG to Canvas
---------------------------------------------
    using CodeBrix.SkiaSvg;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromFile("logo.svg");

    // Draw on a canvas (e.g., in a SkiaSharp surface)
    canvas.Clear(SKColors.White);
    canvas.DrawPicture(svg.Picture);


Example 2: Convert SVG to PNG
-------------------------------
    using CodeBrix.SkiaSvg;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromFile("icon.svg");

    // Save as PNG at 2x scale
    svg.Save("icon.png", SKColors.Transparent,
             SKEncodedImageFormat.Png, 100, 2f, 2f);


Example 3: Convert SVG to PDF
-------------------------------
    using CodeBrix.SkiaSvg;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromFile("document.svg");
    svg.Picture.ToPdf("document.pdf", SKColors.White, 1f, 1f);


Example 4: Hit Testing
------------------------
    using CodeBrix.SkiaSvg;
    using CodeBrix.SvgParse;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromFile("map.svg");

    // Find all elements at a point
    var elements = svg.HitTestElements(new SKPoint(150, 75));
    foreach (var el in elements)
    {
        Console.WriteLine($"Element: {el.ElementName}");
        if (el is SvgVisualElement visual)
        {
            Console.WriteLine($"  Fill: {visual.Fill}");
        }
    }

    // Find topmost element
    var top = svg.HitTestTopmostElement(new SKPoint(150, 75));
    Console.WriteLine($"Topmost: {top?.ElementName}");


Example 5: Animation Playback
-------------------------------
    using CodeBrix.SkiaSvg;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromFile("animation.svg");

    if (svg.HasAnimations)
    {
        // Render frames at 30fps for 5 seconds
        var frameDuration = TimeSpan.FromMilliseconds(1000.0 / 30);

        for (int frame = 0; frame < 150; frame++)
        {
            svg.SetAnimationTime(frameDuration * frame);
            canvas.Clear(SKColors.White);
            canvas.DrawPicture(svg.Picture);
            // ... present frame
        }
    }


Example 6: Retained Scene Graph with Mutation
-----------------------------------------------
    using CodeBrix.SkiaSvg;
    using CodeBrix.SvgParse;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromFile("dashboard.svg");

    if (svg.TryEnsureRetainedSceneGraph(out var scene))
    {
        // Modify an element in the DOM
        var bar = svg.SourceDocument.GetElementById<SvgRectangle>("bar1");
        bar.Height = new SvgUnit(SvgUnitType.Pixel, 150);
        bar.Fill = new SvgColorServer(System.Drawing.Color.Green);

        // Apply mutation to update just the affected part
        svg.ApplyRetainedSceneMutationById("bar1");

        // Re-render
        canvas.DrawPicture(svg.Picture);
    }


Example 7: Load Android VectorDrawable
-----------------------------------------
    using CodeBrix.SkiaSvg;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromVectorDrawable("ic_launcher.xml");
    canvas.DrawPicture(svg.Picture);


Example 8: Custom Typeface Provider
--------------------------------------
    using CodeBrix.SkiaSvg;
    using CodeBrix.SkiaSvg.TypefaceProviders;
    using SkiaSharp;

    var svg = new SKSvg();

    // Add custom typeface provider at highest priority
    var customProvider = new CustomTypefaceProvider();
    svg.Settings.TypefaceProviders.Insert(0, customProvider);

    svg.Load("text-heavy.svg");
    canvas.DrawPicture(svg.Picture);


Example 9: Export to Multiple Formats
---------------------------------------
    using CodeBrix.SkiaSvg;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromFile("diagram.svg");
    var bg = SKColors.White;

    // Raster formats
    svg.Save("output.png", bg, SKEncodedImageFormat.Png, 100, 1f, 1f);
    svg.Save("output.jpg", bg, SKEncodedImageFormat.Jpeg, 90, 1f, 1f);

    // Vector formats
    svg.Picture.ToPdf("output.pdf", bg, 1f, 1f);
    svg.Picture.ToSvg("output.svg", bg, 1f, 1f);
    svg.Picture.ToXps("output.xps", bg, 1f, 1f);


================================================================================

PERFORMANCE TIPS
=================

1. Use static factory methods (SKSvg.CreateFromFile, etc.) for the simplest
   usage. They load and render in one call.

2. Reuse SKSvg instances when rendering the same SVG multiple times. The
   parsed DOM and rendered picture are cached.

3. Use the retained scene graph for interactive or frequently updated SVGs.
   ApplyRetainedSceneMutation() is cheaper than re-rendering the entire
   document.

4. For animation, use native composition (TryCreateNativeCompositionScene)
   when supported. It decomposes the SVG into layers so only animated
   layers need re-rendering.

5. Set AnimationMinimumRenderInterval to throttle animation frame rates.

6. Dispose SKSvg instances when done. They hold SkiaSharp resources that
   need cleanup.

7. Use ToBitmap() with appropriate SKColorType for your platform to avoid
   unnecessary color format conversion.

8. For batch export, load the SVG once and call Save() or the extension
   methods multiple times.

9. Insert custom TypefaceProviders at index 0 for highest priority. The
   provider chain is checked in order.

10. For headless/server environments without system fonts, use
    CustomTypefaceProvider to register fonts explicitly.


================================================================================

COMMON PITFALLS TO AVOID
==========================

1. DO NOT confuse the NuGet package name with the namespace.
   - Package: CodeBrix.SkiaSvg.MitLicenseForever
     Namespace: CodeBrix.SkiaSvg

2. DO NOT use Svg.Skia or ShimSkiaSharp namespaces. Even though this is a
   fork of Svg.Skia, all namespaces are CodeBrix.SkiaSvg.* and
   CodeBrix.SkiaSvg.ShimSkiaSharp.

3. DO NOT target .NET versions below 10.0.

4. DO NOT forget to dispose SKSvg instances. They implement IDisposable
   and hold unmanaged SkiaSharp resources.

5. DO NOT confuse the internal ShimSkiaSharp types (CodeBrix.SkiaSvg.
   ShimSkiaSharp.SKPicture) with the real SkiaSharp types (SkiaSharp.
   SKPicture). The Picture property on SKSvg returns the SkiaSharp type.

6. DO NOT assume thread safety. PDFium and some SkiaSharp operations are
   not thread-safe. Use the Sync lock object when accessing SKSvg from
   multiple threads.

7. DO NOT call ApplyRetainedSceneMutation without first ensuring the
   retained scene graph exists via TryEnsureRetainedSceneGraph.

8. DO NOT expect animation to work automatically. You must drive the
   animation clock manually via SetAnimationTime or AdvanceAnimation.

9. DO NOT forget that hit testing returns elements in rendering order.
   HitTestTopmostElement returns the frontmost (last-rendered) element.

10. DO NOT assume system fonts are available in Docker/CI environments.
    Use CustomTypefaceProvider to register fonts explicitly.

11. DO NOT forget that VectorDrawable support is for Android vector
    drawables (XML format), not Android binary resources.

12. DO NOT forget that Save() and export methods need a background color
    parameter. Use SKColors.Transparent for transparent backgrounds
    (PNG only) or SKColors.White for opaque backgrounds.


================================================================================

WHAT THIS LIBRARY DOES NOT DO
===============================

Do NOT attempt to use this library for the following:

  - SVG editing or authoring (this renders existing SVGs; for DOM
    manipulation use CodeBrix.SkiaSvg.Custom directly)
  - SVG optimization or minification
  - Converting HTML/CSS to SVG
  - 3D rendering or WebGL-style effects
  - Video or animated GIF export (renders individual frames only)
  - Browser-compatible SVG rendering (uses SkiaSharp, not a browser engine)
  - PDF text extraction (ToPdf creates a PDF from the SVG rendering)
  - SVG validation against the SVG specification

CodeBrix.SkiaSvg IS for: loading SVG documents and Android VectorDrawables,
rendering them to SkiaSharp surfaces, exporting to raster and vector formats,
hit testing elements, driving SVG animations, and building interactive SVG
applications with retained scene graph and pointer event support.


================================================================================

QUICK REFERENCE CARD
=====================

--- Install ---
dotnet add package CodeBrix.SkiaSvg.MitLicenseForever

--- Namespaces ---
using CodeBrix.SkiaSvg;
using CodeBrix.SkiaSvg.TypefaceProviders;
using SkiaSharp;

--- Load ---
From file:          SKSvg.CreateFromFile("path.svg")
From stream:        SKSvg.CreateFromStream(stream)
From string:        SKSvg.CreateFromSvg(svgText)
From XmlReader:     SKSvg.CreateFromXmlReader(reader)
From document:      SKSvg.CreateFromSvgDocument(doc)
VectorDrawable:     SKSvg.CreateFromVectorDrawable("path.xml")
Reload:             svg.ReLoad(parameters)

--- Render ---
Draw to canvas:     canvas.DrawPicture(svg.Picture)
Get picture:        svg.Picture
Rebuild:            svg.RebuildFromModel()
Wireframe:          svg.Wireframe = true

--- Export ---
Save raster:        svg.Save("out.png", bg, SKEncodedImageFormat.Png, 100, 1f, 1f)
To bitmap:          svg.Picture.ToBitmap(bg, scaleX, scaleY, ...)
To PDF:             svg.Picture.ToPdf("out.pdf", bg, 1f, 1f)
To SVG:             svg.Picture.ToSvg("out.svg", bg, 1f, 1f)
To XPS:             svg.Picture.ToXps("out.xps", bg, 1f, 1f)

--- Hit Testing ---
All elements:       svg.HitTestElements(point)
Topmost element:    svg.HitTestTopmostElement(point)
With transform:     svg.HitTestElements(point, canvasMatrix)
Scene nodes:        svg.HitTestSceneNodes(point)
Topmost node:       svg.HitTestTopmostSceneNode(point)
Convert coords:     svg.TryGetPicturePoint(point, matrix, out result)

--- Scene Graph ---
Ensure scene:       svg.TryEnsureRetainedSceneGraph(out scene)
Find node by ID:    svg.TryGetRetainedSceneNodeById("id", out node)
Find node:          svg.TryGetRetainedSceneNode(element, out node)
Render node:        svg.CreateRetainedSceneNodePicture(node)
Mutate:             svg.ApplyRetainedSceneMutationById("id")

--- Animation ---
Has animations:     svg.HasAnimations
Set time:           svg.SetAnimationTime(TimeSpan.FromSeconds(2))
Advance:            svg.AdvanceAnimation(TimeSpan.FromMilliseconds(100))
Reset:              svg.ResetAnimation()
Flush frame:        svg.FlushPendingAnimationFrame()
Pending:            svg.HasPendingAnimationFrame

--- Native Composition ---
Supported:          svg.SupportsNativeComposition
Full scene:         svg.TryCreateNativeCompositionScene(out scene)
Frame only:         svg.TryCreateNativeCompositionFrame(out frame)

--- Settings ---
Custom fonts:       svg.Settings.TypefaceProviders.Insert(0, provider)
SVG fonts:          svg.Settings.EnableSvgFonts = true
Viewport:           svg.Settings.StandaloneViewport = rect

--- Cleanup ---
Dispose:            svg.Dispose()  // or use 'using' pattern
Clone:              var copy = svg.Clone()

Target: .NET 10.0+
License: MIT


================================================================================

DEEPER LEARNING: TEST FILE CROSS-REFERENCES
=============================================

The CodeBrix.SkiaSvg.Tests project contains working examples:

    https://github.com/ellisnet/CodeBrix.SkiaSvg
    Path: tests/CodeBrix.SkiaSvg.Tests/

Feature-to-test-file mapping:

  Core SVG loading and rendering:
    -> tests/CodeBrix.SkiaSvg.Tests/SKSvgTests.cs

  SKSvg settings and configuration:
    -> tests/CodeBrix.SkiaSvg.Tests/SKSvgSettingsTests.cs

  Hit testing (point and rectangle):
    -> tests/CodeBrix.SkiaSvg.Tests/HitTestTests.cs

  Retained scene graph:
    -> tests/CodeBrix.SkiaSvg.Tests/SvgRetainedSceneGraphTests.cs

  Rebuild from model:
    -> tests/CodeBrix.SkiaSvg.Tests/SKSvgRebuildFromModelTests.cs

  Animation clock:
    -> tests/CodeBrix.SkiaSvg.Tests/SvgAnimationClockTests.cs

  Animation controller:
    -> tests/CodeBrix.SkiaSvg.Tests/SvgAnimationControllerTests.cs

  Animation smoke tests:
    -> tests/CodeBrix.SkiaSvg.Tests/AnimationSmokeTests.cs

  Animation host backend:
    -> tests/CodeBrix.SkiaSvg.Tests/SvgAnimationHostBackendResolverTests.cs

  Native composition:
    -> tests/CodeBrix.SkiaSvg.Tests/SKSvgNativeCompositionTests.cs

  Pointer/mouse interaction:
    -> tests/CodeBrix.SkiaSvg.Tests/SvgInteractionDispatcherTests.cs

  VectorDrawable loading:
    -> tests/CodeBrix.SkiaSvg.Tests/VectorDrawableTests.cs

  SVG document compatibility loading:
    -> tests/CodeBrix.SkiaSvg.Tests/SvgDocumentCompatibilityLoaderTests.cs

  SVG marker parsing:
    -> tests/CodeBrix.SkiaSvg.Tests/SvgMarkerParsingTests.cs

  Opacity rendering:
    -> tests/CodeBrix.SkiaSvg.Tests/OpacityRenderingTests.cs

  Struct/fragment rendering:
    -> tests/CodeBrix.SkiaSvg.Tests/StructFragmentRenderingTests.cs

  W3C test suite compliance:
    -> tests/CodeBrix.SkiaSvg.Tests/W3CTestSuiteTests.cs

  resvg compatibility tests:
    -> tests/CodeBrix.SkiaSvg.Tests/resvgTests.cs

  SkiaModel text, font, and filter-quality API (post-CS0618 migration):
    -> tests/CodeBrix.SkiaSvg.Tests/SkiaModelTextApiTests.cs

  SkiaModel Draw method canvas command handling (matrix transforms):
    -> tests/CodeBrix.SkiaSvg.Tests/SkiaModelDrawTests.cs

  Typeface resolution and fake-bold adjustment:
    -> tests/CodeBrix.SkiaSvg.Tests/Issue405Tests.cs

HOW TO USE: Fetch the raw file content from GitHub using a URL like:
    https://raw.githubusercontent.com/ellisnet/CodeBrix.SkiaSvg/main/{path}
For example:
    https://raw.githubusercontent.com/ellisnet/CodeBrix.SkiaSvg/main/tests/CodeBrix.SkiaSvg.Tests/SKSvgTests.cs


================================================================================

END OF AGENT-README
