================================================================================
AGENT-README: CodeBrix.SkiaSvg
A Guide for AI Coding Agents - CONSUMING the
CodeBrix.SkiaSvg.MitLicenseForever NuGet package
================================================================================

OVERVIEW
========
CodeBrix.SkiaSvg is an SVG loading and rendering library for .NET 10 or later,
built on SkiaSharp. It loads SVG documents (and Android VectorDrawable XML)
and renders them to SkiaSharp canvases, bitmaps and documents. On top of
plain rendering it provides hit testing, a retained scene graph with
incremental mutation, SMIL animation playback, layer-based native
composition, pointer-event dispatch, an editing/inspection API over the
intermediate drawing model, and export to raster and vector formats.

CodeBrix.SkiaSvg is a fork of the Svg.Skia project, consolidating several
companion packages of that ecosystem into a single library. Every
namespace is rooted at "CodeBrix.SkiaSvg", mapped from upstream like this:

    Svg.Skia                    -> CodeBrix.SkiaSvg
    Svg.Model                   -> CodeBrix.SkiaSvg.Model
    Svg.Model.Services          -> CodeBrix.SkiaSvg.Model.Services
    Svg.Model.Editing           -> CodeBrix.SkiaSvg.Model.Editing
    ShimSkiaSharp               -> CodeBrix.SkiaSvg.ShimSkiaSharp
    ShimSkiaSharp.Editing       -> CodeBrix.SkiaSvg.ShimSkiaSharp.Editing
    Svg.Skia.TypefaceProviders  -> CodeBrix.SkiaSvg.TypefaceProviders

Do NOT use the upstream namespaces - they do not exist in this package.

The SVG DOM itself (SvgDocument, SvgElement, SvgVisualElement, SvgPath,
SvgRectangle, SvgColorServer, SvgUnit and the rest of the parsed object
model) is not defined here: it comes from the CodeBrix.SvgParse package,
which this package depends on and re-exposes through its API surface.


INSTALLATION
============
PackageId:  CodeBrix.SkiaSvg.MitLicenseForever

    dotnet add package CodeBrix.SkiaSvg.MitLicenseForever

IMPORTANT: the NuGet package id is CodeBrix.SkiaSvg.MitLicenseForever (NOT
"CodeBrix.SkiaSvg" - that suffix exists only to make the package license
obvious forever). The primary namespace is CodeBrix.SkiaSvg.

NuGet dependencies (pulled in automatically, no version pinning needed in
the consuming project):
  - CodeBrix.SvgParse.MsplLicenseForever   (the SVG DOM / parser)
  - SkiaSharp                              (the rendering engine)
  - HarfBuzzSharp                          (text shaping)
  - HarfBuzzSharp.NativeAssets.Linux
  - HarfBuzzSharp.NativeAssets.macOS
  - HarfBuzzSharp.NativeAssets.Win32

License: MIT (SPDX: MIT)

Requirements: .NET 10 or later. Any OS that SkiaSharp supports.

NATIVE ASSETS: the HarfBuzz native binaries for Linux, macOS and Windows
arrive transitively with this package, but the SkiaSharp native binaries do
NOT. A consuming application must add the SkiaSharp native-asset package for
each platform it runs on - for example, a Linux console or service app adds:

    dotnet add package SkiaSharp.NativeAssets.Linux

Without it, the first SkiaSharp call fails at run time with a native-library
load error even though everything compiles.

SEE ALSO: the SVG DOM types used throughout this API are documented in the
CodeBrix.SvgParse package (PackageId CodeBrix.SvgParse.MsplLicenseForever);
read that package's own AGENT-README for the element/attribute model.


KEY NAMESPACES / USINGS
=======================
    using CodeBrix.SkiaSvg;                       // SKSvg + the entire
                                                  //   scene-graph, animation
                                                  //   and interaction surface
    using CodeBrix.SkiaSvg.Model;                 // ISvgAssetLoader,
                                                  //   SvgParameters,
                                                  //   DrawAttributes,
                                                  //   GradientMesh, text seam
    using CodeBrix.SkiaSvg.Model.Services;        // SvgService,
                                                  //   GradientMeshService
    using CodeBrix.SkiaSvg.Model.Editing;         // SvgDocumentEditingExtensions
    using CodeBrix.SkiaSvg.ShimSkiaSharp;         // the intermediate drawing
                                                  //   model: SKPoint, SKRect,
                                                  //   SKMatrix, SKPicture,
                                                  //   SKPaint, SKPath, ...
    using CodeBrix.SkiaSvg.ShimSkiaSharp.Editing; // editing extensions +
                                                  //   EditMode
    using CodeBrix.SkiaSvg.TypefaceProviders;     // font resolution
    using CodeBrix.SvgParse;                      // SvgDocument, SvgElement
    using SkiaSharp;                              // the real SkiaSharp types

THERE IS NO "CodeBrix.SkiaSvg.Interaction" NAMESPACE. Interaction/ is only a
source folder: SvgInteractionDispatcher, SvgInteractionDispatchResult,
SvgPointerInput, SvgPointerEventArgs, SvgPointerDeviceType, SvgMouseButton
and SvgPointerEventRoutePhase are all declared in the plain CodeBrix.SkiaSvg
namespace. The same is true of the scene-graph and animation types: despite
living in SceneGraph/ and Animation/ folders, they are in CodeBrix.SkiaSvg.

TWO KINDS OF SKPoint / SKRect / SKMatrix / SKPicture / SKPaint / SKPath
EXIST, and mixing them up is the single most common compile error against
this library:
  * CodeBrix.SkiaSvg.ShimSkiaSharp.*  - the intermediate, immutable-ish
    drawing model this library builds and inspects.
  * SkiaSharp.*                       - the real rendering types.
There is no implicit conversion between them. Hit testing, the scene graph
and the editing API speak the ShimSkiaSharp types; SKSvg.Picture, SKSvg.Draw
and the export extension methods speak the SkiaSharp types. See the pitfalls
section.


================================================================================

CORE API REFERENCE
==================

SKSvg CLASS - MAIN ENTRY POINT
------------------------------
The SKSvg class is the primary public API for loading and rendering SVGs.
It implements IDisposable - always use a 'using' pattern.

Static factory methods (preferred):
    static SKSvg CreateFromFile(string path, SvgParameters? parameters = null)
    static SKSvg CreateFromFile(string path)
    static SKSvg CreateFromStream(Stream stream, SvgParameters? parameters = null)
    static SKSvg CreateFromStream(Stream stream)
    static SKSvg CreateFromXmlReader(XmlReader reader)
    static SKSvg CreateFromSvg(string svg)
    static SKSvg CreateFromSvgDocument(SvgDocument svgDocument)
    static SKSvg CreateFromVectorDrawable(string path,
                                          SvgParameters? parameters = null)
    static SKSvg CreateFromVectorDrawable(Stream stream,
                                          SvgParameters? parameters = null)
    static SKSvg CreateFromVectorDrawable(XmlReader reader)

Static one-shot helpers (no SKSvg instance retained):
    static SkiaSharp.SKPicture ToPicture(SvgFragment svgFragment,
                                         SkiaModel skiaModel,
                                         ISvgAssetLoader assetLoader)
    static void Draw(SkiaSharp.SKCanvas skCanvas, SvgFragment svgFragment,
                     SkiaModel skiaModel, ISvgAssetLoader assetLoader)
    static void Draw(SkiaSharp.SKCanvas skCanvas, string path,
                     SkiaModel skiaModel, ISvgAssetLoader assetLoader)

Static configuration:
    static bool CacheOriginalStream { get; set; }
        When true, the stream a document was loaded from is retained so that
        ReLoad() can re-parse it.

Constructor:
    SKSvg()                                 // then call one of the Load /
                                            //   From... instance methods

Key properties:
    SKSvgSettings Settings { get; }         // configuration (see below)
    SvgDocument SourceDocument { get; }     // the loaded SVG DOM
    SkiaSharp.SKPicture Picture { get; }    // the rendered SkiaSharp picture
    ShimSkiaSharp.SKPicture Model { get; }  // the intermediate drawing model
    ISvgAssetLoader AssetLoader { get; }    // image/font/text-metric loading
    SkiaModel SkiaModel { get; }            // model-to-SkiaSharp converter
    SvgParameters? Parameters { get; }      // parameters of the last load
    DrawAttributes IgnoreAttributes { get; set; }
    object Sync { get; }                    // the instance's lock object

Instance loading methods (each returns the rendered SkiaSharp.SKPicture):
    SkiaSharp.SKPicture Load(string path, SvgParameters? parameters = null)
    SkiaSharp.SKPicture Load(string path)
    SkiaSharp.SKPicture Load(Stream stream, SvgParameters? parameters = null)
    SkiaSharp.SKPicture Load(Stream stream)
    SkiaSharp.SKPicture Load(Stream stream, SvgParameters? parameters,
                             Uri baseUri)
    SkiaSharp.SKPicture Load(XmlReader reader)
    SkiaSharp.SKPicture LoadVectorDrawable(string path,
                                           SvgParameters? parameters = null)
    SkiaSharp.SKPicture LoadVectorDrawable(Stream stream,
                                           SvgParameters? parameters = null)
    SkiaSharp.SKPicture LoadVectorDrawable(XmlReader reader)
    SkiaSharp.SKPicture FromSvg(string svg)
    SkiaSharp.SKPicture FromVectorDrawable(string xml)
    SkiaSharp.SKPicture FromSvgDocument(SvgDocument svgDocument)
    SkiaSharp.SKPicture ReLoad(SvgParameters? parameters)
        NOTE: ReLoad has NO default argument - pass null explicitly to
        re-load with the original parameters.

Drawing and rebuilding:
    void Draw(SkiaSharp.SKCanvas canvas)
    SkiaSharp.SKPicture RebuildFromModel()
        Re-renders Picture from the current intermediate Model - use it
        after editing the Model in place (see EDITING THE DRAWING MODEL).

Events:
    event EventHandler<SKSvgDrawEventArgs> OnDraw
        Raised after each Draw(canvas) completes. SKSvgDrawEventArgs exposes
        one member, SkiaSharp.SKCanvas Canvas { get; } - the canvas that was
        drawn on. The event-args type has no public constructor; consumers
        only subscribe.
    event EventHandler<SvgAnimationFrameChangedEventArgs> AnimationInvalidated
        Raised when an animation frame invalidates the rendering.
        SvgAnimationFrameChangedEventArgs exposes TimeSpan Time { get; }.

Cloning and wireframe debugging:
    SKSvg Clone()
    bool Wireframe { get; set; }
    SkiaSharp.SKPicture WireframePicture { get; protected set; }
    void ClearWireframePicture()

SvgParameters (namespace CodeBrix.SkiaSvg.Model) is a readonly record struct:
    public readonly record struct SvgParameters(
        Dictionary<string, string> Entities, string Css);
Pass it to any load method to inject XML entity values and an extra CSS
stylesheet applied to the document.

DrawAttributes (namespace CodeBrix.SkiaSvg.Model) is a [Flags] enum used by
IgnoreAttributes to skip SVG features during rendering:
    None = 0, Display = 1, Visibility = 2, Opacity = 4, Filter = 8,
    ClipPath = 16, Mask = 32, RequiredFeatures = 64,
    RequiredExtensions = 128, SystemLanguage = 256


SAVING AND EXPORT
-----------------
Direct save methods on SKSvg (background, format, quality and scale all have
defaults):
    bool Save(Stream stream, SkiaSharp.SKColor background,
              SkiaSharp.SKEncodedImageFormat format
                  = SkiaSharp.SKEncodedImageFormat.Png,
              int quality = 100, float scaleX = 1f, float scaleY = 1f)
    bool Save(string path, SkiaSharp.SKColor background,
              SkiaSharp.SKEncodedImageFormat format
                  = SkiaSharp.SKEncodedImageFormat.Png,
              int quality = 100, float scaleX = 1f, float scaleY = 1f)

Extension methods on SkiaSharp.SKPicture (SKPictureExtensions, namespace
CodeBrix.SkiaSvg) - note these extend the REAL SkiaSharp.SKPicture, so they
apply to svg.Picture:
    void Draw(this SkiaSharp.SKPicture skPicture,
              SkiaSharp.SKColor background, float scaleX, float scaleY,
              SkiaSharp.SKCanvas skCanvas)
    SkiaSharp.SKBitmap ToBitmap(this SkiaSharp.SKPicture skPicture,
              SkiaSharp.SKColor background, float scaleX, float scaleY,
              SkiaSharp.SKColorType skColorType,
              SkiaSharp.SKAlphaType skAlphaType,
              SkiaSharp.SKColorSpace skColorSpace)
    bool ToImage(this SkiaSharp.SKPicture skPicture, Stream stream,
              SkiaSharp.SKColor background,
              SkiaSharp.SKEncodedImageFormat format, int quality,
              float scaleX, float scaleY, SkiaSharp.SKColorType skColorType,
              SkiaSharp.SKAlphaType skAlphaType,
              SkiaSharp.SKColorSpace skColorSpace)
    bool ToSvg(this SkiaSharp.SKPicture skPicture, string path,
              SkiaSharp.SKColor background, float scaleX, float scaleY)
    bool ToSvg(this SkiaSharp.SKPicture skPicture, Stream stream,
              SkiaSharp.SKColor background, float scaleX, float scaleY)
    bool ToPdf(this SkiaSharp.SKPicture skPicture, string path,
              SkiaSharp.SKColor background, float scaleX, float scaleY)
    bool ToPdf(this SkiaSharp.SKPicture skPicture, Stream stream,
              SkiaSharp.SKColor background, float scaleX, float scaleY)
    bool ToXps(this SkiaSharp.SKPicture skPicture, string path,
              SkiaSharp.SKColor background, float scaleX, float scaleY)
    bool ToXps(this SkiaSharp.SKPicture skPicture, Stream stream,
              SkiaSharp.SKColor background, float scaleX, float scaleY)

Supported export formats:
    Raster: PNG, JPEG and WEBP. SkiaSharp has encoders for those three
            only; any other SKEncodedImageFormat value (Bmp, Gif, Ico, ...)
            makes ToImage/Save return false and write nothing (there is no
            TIFF value at all). Save(string path, ...) encodes in memory
            first, so on failure it creates no file and leaves an existing
            file at that path untouched.
    Vector/document: SVG, PDF, XPS (via the extension methods above)


================================================================================

HIT TESTING
===========
Hit test SVG elements or scene nodes by point or rectangle, optionally
through a canvas transform. All of these take the ShimSkiaSharp SKPoint /
SKRect / SKMatrix types (using CodeBrix.SkiaSvg.ShimSkiaSharp;).

Element hit testing:
    IEnumerable<SvgElement> HitTestElements(SKPoint point)
    IEnumerable<SvgElement> HitTestElements(SKRect rect)
    IEnumerable<SvgElement> HitTestElements(SKPoint point,
                                            SKMatrix canvasMatrix)
    IEnumerable<SvgElement> HitTestElements(SKRect rect,
                                            SKMatrix canvasMatrix)
    SvgElement HitTestTopmostElement(SKPoint point)
    SvgElement HitTestTopmostElement(SKPoint point, SKMatrix canvasMatrix)

Scene-node hit testing (retained mode):
    IEnumerable<SvgSceneNode> HitTestSceneNodes(SKPoint point)
    IEnumerable<SvgSceneNode> HitTestSceneNodes(SKRect rect)
    IEnumerable<SvgSceneNode> HitTestSceneNodes(SKPoint point,
                                                SKMatrix canvasMatrix)
    IEnumerable<SvgSceneNode> HitTestSceneNodes(SKRect rect,
                                                SKMatrix canvasMatrix)
    SvgSceneNode HitTestTopmostSceneNode(SKPoint point)
    SvgSceneNode HitTestTopmostSceneNode(SKPoint point,
                                         SKMatrix canvasMatrix)

Coordinate conversion:
    bool TryGetPicturePoint(SKPoint point, SKMatrix canvasMatrix,
                            out SKPoint picturePoint)
    bool TryGetPictureRect(SKRect rect, SKMatrix canvasMatrix,
                           out SKRect pictureRect)

The canvasMatrix overloads map a point that is in CANVAS space back into
PICTURE space before testing. Build that matrix from the same transform you
applied to the canvas, using the ShimSkiaSharp factory methods:

    using CodeBrix.SkiaSvg.ShimSkiaSharp;

    // the app drew the SVG at 2x, offset by (30, 15)
    var canvasMatrix = SKMatrix.CreateScale(2f, 2f)
                               .PostConcat(SKMatrix.CreateTranslation(30f, 15f));
    var hit = svg.HitTestTopmostElement(new SKPoint(mouseX, mouseY),
                                        canvasMatrix);

ShimSkiaSharp.SKMatrix also offers CreateIdentity(), CreateTranslation(x, y),
CreateScale(x, y), CreateScale(x, y, pivotX, pivotY), CreateRotation(radians)
(+ pivot overload), CreateRotationDegrees(degrees) (+ pivot overload),
CreateSkew(x, y), a nine-float constructor
SKMatrix(scaleX, skewX, transX, skewY, scaleY, transY, persp0, persp1,
persp2), PreConcat / PostConcat, MapPoint, MapRect and
bool TryInvert(out SKMatrix inverse).

Example:
    using CodeBrix.SkiaSvg;
    using CodeBrix.SkiaSvg.ShimSkiaSharp;
    using CodeBrix.SvgParse;

    using var svg = SKSvg.CreateFromFile("interactive.svg");

    var element = svg.HitTestTopmostElement(new SKPoint(100, 50));
    if (element != null)
    {
        // SvgElement.ElementName is protected internal in CodeBrix.SvgParse
        // and is NOT reachable from a consumer assembly; identify the element
        // by its CLR type instead.
        Console.WriteLine($"Hit: {element.GetType().Name} (ID: {element.ID})");
    }

Hit results come back in rendering order; HitTestTopmostElement returns the
frontmost (last-rendered) element, honoring pointer-events, clip paths and
masks.


================================================================================

RETAINED SCENE GRAPH
====================
The retained scene graph is a compiled, queryable representation of the
rendered SVG. It enables efficient partial updates (mutations) without
re-rendering the entire document.

Access on SKSvg:
    SvgSceneDocument RetainedSceneGraph { get; }  // compiles on first read
    bool HasRetainedSceneGraph { get; }
    bool TryEnsureRetainedSceneGraph(out SvgSceneDocument sceneDocument)

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

Rendering from the scene graph (Model = ShimSkiaSharp.SKPicture,
Picture = SkiaSharp.SKPicture):
    ShimSkiaSharp.SKPicture CreateRetainedSceneGraphModel()
    SkiaSharp.SKPicture     CreateRetainedSceneGraphPicture()
    ShimSkiaSharp.SKPicture CreateRetainedSceneNodeModel(SvgSceneNode node,
                                                         SKRect? clip = null)
    SkiaSharp.SKPicture     CreateRetainedSceneNodePicture(SvgSceneNode node,
                                                         SKRect? clip = null)
    ShimSkiaSharp.SKPicture CreateRetainedSceneModel(SvgElement element,
                                                         SKRect? clip = null)
    SkiaSharp.SKPicture     CreateRetainedScenePicture(SvgElement element,
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

SvgSceneMutationResult:
    bool Succeeded { get; }
    int CompilationRootCount { get; }     // subtrees that were recompiled
    int ResourceCount { get; }            // resources that were recompiled

SvgSceneDocument (namespace CodeBrix.SkiaSvg):
    SvgDocument SourceDocument { get; }
    SvgSceneNode Root { get; }
    SKRect CullRect { get; }
    long Revision { get; }
    IReadOnlyDictionary<string, SvgSceneNode> NodesById { get; }
    IReadOnlyDictionary<string, SvgSceneResource> ResourcesById { get; }
    IEnumerable<SvgSceneNode> Traverse()
    bool TryGetNode(string addressKey, out SvgSceneNode node)
    bool TryGetNode(SvgElement element, out SvgSceneNode node)
    bool TryGetNodes(string addressKey, out IReadOnlyList<SvgSceneNode> nodes)
    bool TryGetNodeById(string id, out SvgSceneNode node)
    bool TryGetResource(string addressKey, out SvgSceneResource resource)
    bool TryGetResourceById(string id, out SvgSceneResource resource)
    bool TryGetElement(string addressKey, out SvgElement element)
    bool TryGetElementById(string id, out SvgElement element)
    int MarkDirty(string addressKey, bool includeDescendants = false)
    SvgSceneMutationResult ApplyMutation(SvgElement element,
        IReadOnlyCollection<string> changedAttributes = null)
    SvgSceneMutationResult ApplyMutation(string addressKey,
        IReadOnlyCollection<string> changedAttributes = null)
    SvgSceneMutationResult ApplyMutationById(string id,
        IReadOnlyCollection<string> changedAttributes = null)
    void ClearDirty()
    IEnumerable<SvgSceneNode> HitTest(SKPoint point)
    IEnumerable<SvgSceneNode> HitTest(SKRect rect)
    SvgSceneNode HitTestTopmostNode(SKPoint point)
    ShimSkiaSharp.SKPicture CreateModel()
    ShimSkiaSharp.SKPicture CreateNodeModel(SvgSceneNode node,
                                            SKRect? clip = null)

SvgSceneNode (all setters are internal - read-only to consumers):
    SvgSceneNodeKind Kind
    SvgElement Element
    SvgElement HitTestTargetElement
    string ElementAddressKey / ElementId / ElementTypeName
    SvgPointerEvents PointerEvents
    bool IsVisible / bool IsDisplayNone
    string Cursor
    bool CreatesBackgroundLayer
    SKRect? BackgroundClip
    string ClipResourceKey / MaskResourceKey / FilterResourceKey
    string CompilationRootKey
    bool IsCompilationRootBoundary
    SvgSceneCompilationStrategy CompilationStrategy
    SvgSceneNode Parent / MaskNode
    IReadOnlyList<SvgSceneNode> Children
    ShimSkiaSharp.SKPicture LocalModel
    ShimSkiaSharp.SKPath HitTestPath
    SKRect GeometryBounds / TransformedBounds
    SKMatrix Transform / TotalTransform
    SKRect? Overflow / SKRect? Clip

SvgSceneNodeKind enum:
    Unknown, Fragment, Group, Anchor, Use, Switch, Image, Text, Marker,
    Path, Shape, Mask, Container

SvgSceneResource:
    string Key { get; }
    SvgSceneResourceKind Kind { get; }
    SvgElement SourceElement { get; }
    string AddressKey { get; }
    string Id { get; }
    IReadOnlyCollection<string> SubtreeAddresses { get; }
    IReadOnlyCollection<string> DependencyKeys { get; }
    IReadOnlyCollection<string> ReverseDependencyKeys { get; }
    IReadOnlyCollection<string> DependentCompilationRoots { get; }

SvgSceneResourceKind enum:
    Unknown, ClipPath, Mask, Filter, Gradient, Pattern, Marker, Symbol,
    PaintServer


SCENE COMPILATION AND RENDERING WITHOUT AN SKSvg
------------------------------------------------
SKSvg drives these for you, but they are public so a host can compile and
render a scene on its own schedule. All three are static classes in the
CodeBrix.SkiaSvg namespace.

SvgSceneCompiler - compiles an SVG document into a scene document against an
explicit cull rectangle:
    static bool TryCompile(SvgDocument sourceDocument,
                           SKRect cullRect,
                           ISvgAssetLoader assetLoader,
                           DrawAttributes ignoreAttributes,
                           out SvgSceneDocument sceneDocument)

SvgSceneRuntime - the same, deriving the viewport from the fragment, plus
one-call model creation:
    static bool TryCompile(SvgFragment sourceFragment,
                           ISvgAssetLoader assetLoader,
                           DrawAttributes ignoreAttributes,
                           out SvgSceneDocument sceneDocument)
    static bool TryCompile(SvgFragment sourceFragment,
                           ISvgAssetLoader assetLoader,
                           DrawAttributes ignoreAttributes,
                           SKRect standaloneDocumentViewport,
                           out SvgSceneDocument sceneDocument)
    static ShimSkiaSharp.SKPicture CreateModel(SvgFragment sourceFragment,
                           ISvgAssetLoader assetLoader,
                           DrawAttributes ignoreAttributes
                               = DrawAttributes.None)
    static ShimSkiaSharp.SKPicture CreateModel(SvgFragment sourceFragment,
                           ISvgAssetLoader assetLoader,
                           DrawAttributes ignoreAttributes,
                           SKRect standaloneDocumentViewport)

SvgSceneRenderer - flattens a compiled scene into a drawing model:
    static ShimSkiaSharp.SKPicture Render(SvgSceneDocument sceneDocument)

SvgSceneCompilationStrategy enum - the strategy recorded on each scene node;
currently one value:
    DirectRetained


================================================================================

ANIMATION
=========
SVG SMIL animation with an explicit, caller-driven clock.

SKSvg properties:
    SvgAnimationController AnimationController { get; }
    bool HasAnimations { get; }
    TimeSpan AnimationTime { get; }
    TimeSpan AnimationMinimumRenderInterval { get; set; }  // negative values
                                                           //   clamp to zero
    bool HasPendingAnimationFrame { get; }
    int LastAnimationDirtyTargetCount { get; }
    bool UsesAnimationLayerCaching { get; }

SKSvg control methods:
    void SetAnimationTime(TimeSpan time)
    void AdvanceAnimation(TimeSpan delta)
    void ResetAnimation()
    bool FlushPendingAnimationFrame()
    bool NotifyPointerEvent(SvgElement element, SvgPointerEventType eventType)

SvgAnimationController (namespace CodeBrix.SkiaSvg; IDisposable):
    SvgAnimationController(SvgDocument sourceDocument)
    SvgDocument SourceDocument { get; }
    SvgAnimationClock Clock { get; }
    bool HasAnimations { get; }
    event EventHandler<SvgAnimationFrameChangedEventArgs> FrameChanged
    SvgDocument CreateAnimatedDocument()
    SvgDocument CreateAnimatedDocument(TimeSpan time)
    bool RecordPointerEvent(SvgElement element, SvgPointerEventType eventType)
    void Reset()
    void Dispose()

SvgAnimationClock (namespace CodeBrix.SkiaSvg) - the timeline itself:
    TimeSpan CurrentTime { get; }
    event EventHandler<SvgAnimationClockChangedEventArgs> TimeChanged
    void Reset()
    void Seek(TimeSpan time)
    void AdvanceBy(TimeSpan delta)

SvgAnimationClockChangedEventArgs:
    TimeSpan Time { get; }

SvgPointerEventType enum - the event kinds SMIL begin/end conditions react
to, and the argument to NotifyPointerEvent / RecordPointerEvent:
    Move, Press, Release, Enter, Leave, Wheel, Click

SvgAnimationInvalidation (static, namespace CodeBrix.SkiaSvg) - the helper
that decides how far an attribute change propagates:
    static bool AffectsDescendantSubtree(string attributeName)
        True for inheritable presentation attributes (fill, stroke,
        font-family, visibility, opacity-family, text-anchor and the rest of
        the inherited set), meaning a change invalidates the whole subtree
        rather than just the element.


ANIMATION HOST BACKENDS
-----------------------
These types let a UI host declare what it can do and be told which playback
strategy to use. They make no decisions on their own - the host still calls
SetAnimationTime / AdvanceAnimation.

SvgAnimationHostBackend enum:
    Default, Manual, DispatcherTimer, RenderLoop, NativeComposition

SvgAnimationHostBackendCapabilities:
    SvgAnimationHostBackendCapabilities(bool isHostReady,
                                        bool supportsDispatcherTimer,
                                        bool supportsRenderLoop,
                                        bool supportsNativeComposition)
    bool IsHostReady { get; }
    bool SupportsDispatcherTimer { get; }
    bool SupportsRenderLoop { get; }
    bool SupportsNativeComposition { get; }

SvgAnimationHostBackendResolution:
    SvgAnimationHostBackendResolution(
        SvgAnimationHostBackend requestedBackend,
        SvgAnimationHostBackend actualBackend,
        string fallbackReason)
    SvgAnimationHostBackend RequestedBackend { get; }
    SvgAnimationHostBackend ActualBackend { get; }
    string FallbackReason { get; }        // null when nothing fell back
    bool IsFallback { get; }

SvgAnimationHostBackendResolver (static):
    static SvgAnimationHostBackendResolution Resolve(
        SvgAnimationHostBackend requestedBackend,
        SvgAnimationHostBackendCapabilities capabilities,
        bool hasAnimations)

Example:
    var caps = new SvgAnimationHostBackendCapabilities(
        isHostReady: true,
        supportsDispatcherTimer: true,
        supportsRenderLoop: false,
        supportsNativeComposition: false);

    var resolution = SvgAnimationHostBackendResolver.Resolve(
        SvgAnimationHostBackend.Default, caps, svg.HasAnimations);

    if (resolution.IsFallback)
    {
        Console.WriteLine($"Falling back: {resolution.FallbackReason}");
    }
    // resolution.ActualBackend now says how to drive the clock


================================================================================

NATIVE COMPOSITION
==================
Layer-based decomposition for optimized animation rendering. Instead of
re-rendering the entire SVG each frame, a host composites the layers and
re-renders only the animated ones.

    bool SupportsNativeComposition { get; }
    bool TryCreateNativeCompositionScene(out SvgNativeCompositionScene scene)
    bool TryCreateNativeCompositionFrame(out SvgNativeCompositionFrame frame)

SvgNativeCompositionScene / SvgNativeCompositionFrame:
    SKRect SourceBounds { get; }
    IReadOnlyList<SvgNativeCompositionLayer> Layers { get; }

SvgNativeCompositionLayer:
    int DocumentChildIndex { get; }
    bool IsAnimated { get; }
    SkiaSharp.SKPicture Picture { get; }
    SkiaSharp.SKPoint Offset { get; }
    SkiaSharp.SKSize Size { get; }
    float Opacity { get; }
    bool IsVisible { get; }


================================================================================

INTERACTION
===========
Pointer/mouse event dispatch for interactive SVGs, with tunneling, targeting
and bubbling phases, hover/press/capture tracking and CSS cursor resolution.
Everything here lives in the CodeBrix.SkiaSvg namespace.

SvgInteractionDispatcher - THE ENTRY POINT for pointer input. Create one per
interactive view and keep it alive across events (it holds hover, press and
capture state):
    SvgInteractionDispatcher()            // implicit parameterless ctor
    bool RaiseSvgElementEvents { get; set; }   // default true
    SvgElement HoveredElement { get; }
    SvgElement PressedElement { get; }
    SvgElement CapturedElement { get; }
    string CurrentCursor { get; }
    event EventHandler<SvgPointerEventArgs> Dispatched

    SvgElement HitTestTopmostElement(SKSvg svg, SKPoint picturePoint)

    // fire-and-forget forms
    void HandlePointerMoved(SKSvg svg, SvgPointerInput input)
    void HandlePointerPressed(SKSvg svg, SvgPointerInput input)
    void HandlePointerReleased(SKSvg svg, SvgPointerInput input)
    void HandlePointerWheelChanged(SKSvg svg, SvgPointerInput input)
    void HandlePointerExited(SvgPointerInput input)

    // result-returning forms
    SvgInteractionDispatchResult DispatchPointerMoved(SKSvg svg,
                                                      SvgPointerInput input)
    SvgInteractionDispatchResult DispatchPointerPressed(SKSvg svg,
                                                      SvgPointerInput input)
    SvgInteractionDispatchResult DispatchPointerReleased(SKSvg svg,
                                                      SvgPointerInput input)
    SvgInteractionDispatchResult DispatchPointerWheelChanged(SKSvg svg,
                                                      SvgPointerInput input)
    SvgInteractionDispatchResult DispatchPointerExited(SvgPointerInput input)
    SvgInteractionDispatchResult DispatchPointerExited(SKSvg svg,
                                                      SvgPointerInput input)

    void Reset()      // clears hover/press/capture and event registrations

SvgInteractionDispatchResult:
    SvgElement TargetElement { get; }
    string Cursor { get; }
    bool Handled { get; }

SvgPointerInput - the input state you construct per event. The point is in
PICTURE coordinates (convert first with TryGetPicturePoint if your canvas is
transformed), and it is a ShimSkiaSharp.SKPoint:
    SvgPointerInput(SKPoint picturePoint,
                    SvgPointerDeviceType pointerDeviceType,
                    SvgMouseButton button,
                    int clickCount,
                    int wheelDelta,
                    bool altKey,
                    bool shiftKey,
                    bool ctrlKey,
                    string sessionId)
    SKPoint PicturePoint { get; }
    SvgPointerDeviceType PointerDeviceType { get; }
    SvgMouseButton Button { get; }
    int ClickCount { get; }
    int WheelDelta { get; }
    bool AltKey / ShiftKey / CtrlKey { get; }
    string SessionId { get; }      // null becomes string.Empty

SvgPointerEventArgs (the payload of the Dispatched event):
    SvgPointerEventType EventType { get; }
    SvgElement Element { get; }            // element for the current phase
    SvgElement TargetElement { get; }      // original target
    SvgElement RelatedElement { get; }     // element entered/left
    SvgPointerEventRoutePhase RoutePhase { get; }
    SvgPointerInput Input { get; }
    string Cursor { get; }
    bool Handled { get; set; }             // set true to stop routing
    SKPoint PicturePoint { get; }          // shortcut for Input.PicturePoint

Enums:
    SvgPointerDeviceType    Unknown, Mouse, Touch, Pen
    SvgMouseButton          None, Left, Middle, Right, XButton1, XButton2
    SvgPointerEventRoutePhase   Tunnel, Target, Bubble
    SvgPointerEventType     Move, Press, Release, Enter, Leave, Wheel, Click

Example:
    using CodeBrix.SkiaSvg;
    using CodeBrix.SkiaSvg.ShimSkiaSharp;

    var dispatcher = new SvgInteractionDispatcher();
    dispatcher.Dispatched += (_, e) =>
    {
        if (e.RoutePhase == SvgPointerEventRoutePhase.Target &&
            e.EventType == SvgPointerEventType.Press)
        {
            Console.WriteLine($"pressed {e.TargetElement?.ID}");
            e.Handled = true;
        }
    };

    var input = new SvgPointerInput(
        new SKPoint(30, 30),
        SvgPointerDeviceType.Mouse,
        SvgMouseButton.Left,
        clickCount: 1,
        wheelDelta: 0,
        altKey: false,
        shiftKey: false,
        ctrlKey: false,
        sessionId: "mouse");

    var result = dispatcher.DispatchPointerPressed(svg, input);
    // result.TargetElement / result.Cursor / result.Handled

SKSvg.NotifyPointerEvent(element, eventType) is the lower-level hook that
feeds a pointer event to the SMIL animation controller only; the dispatcher
is what routes events to elements.


================================================================================

CONFIGURATION: SKSvgSettings
============================
    SkiaSharp.SKAlphaType AlphaType { get; set; }   // default Unpremul
    SkiaSharp.SKColorType ColorType { get; set; }   // default
                                                    //   SKImageInfo
                                                    //   .PlatformColorType
    SkiaSharp.SKColorSpace SrgbLinear { get; set; }
    SkiaSharp.SKColorSpace Srgb { get; set; }
    IList<ITypefaceProvider> TypefaceProviders { get; set; }
    SkiaSharp.SKRect? StandaloneViewport { get; set; }  // default null
    bool EnableSvgFonts { get; set; }               // default true
    bool EnableTextReferences { get; set; }         // default true (<tref>)

Default typeface provider chain, in order:
    1. FontManagerTypefaceProvider()   // system fonts via SKFontManager
    2. DefaultTypefaceProvider()       // SkiaSharp default typeface

Example:
    var svg = new SKSvg();
    svg.Settings.TypefaceProviders.Insert(0,
        new CustomTypefaceProvider("/opt/fonts/Brand-Regular.ttf"));
    svg.Settings.EnableSvgFonts = true;
    svg.Load("fonts.svg");


TYPEFACE PROVIDERS
------------------
Namespace CodeBrix.SkiaSvg.TypefaceProviders.

    public interface ITypefaceProvider
    {
        SkiaSharp.SKTypeface FromFamilyName(
            string fontFamily,
            SkiaSharp.SKFontStyleWeight fontWeight,
            SkiaSharp.SKFontStyleWidth fontWidth,
            SkiaSharp.SKFontStyleSlant fontStyle);
    }

FontManagerTypefaceProvider : ITypefaceProvider
    FontManagerTypefaceProvider()
    SkiaSharp.SKFontManager FontManager { get; set; }
    SkiaSharp.SKTypeface CreateTypeface(Stream stream, int index = 0)
    SkiaSharp.SKTypeface CreateTypeface(SkiaSharp.SKStreamAsset stream,
                                        int index = 0)
    SkiaSharp.SKTypeface CreateTypeface(string path, int index = 0)
    SkiaSharp.SKTypeface CreateTypeface(SkiaSharp.SKData data, int index = 0)

DefaultTypefaceProvider : ITypefaceProvider
    Parameterless; resolves through the SkiaSharp default typeface.

CustomTypefaceProvider : ITypefaceProvider, IDisposable
    Serves exactly ONE font face, loaded up front. THERE IS NO
    PARAMETERLESS CONSTRUCTOR:
        CustomTypefaceProvider(Stream stream, int index = 0)
        CustomTypefaceProvider(SkiaSharp.SKStreamAsset stream, int index = 0)
        CustomTypefaceProvider(string path, int index = 0)
        CustomTypefaceProvider(SkiaSharp.SKData data, int index = 0)
        SkiaSharp.SKTypeface Typeface { get; set; }
        string FamilyName { get; set; }   // initialized from the loaded face
        void Dispose()
    It answers FromFamilyName only when one of the comma-separated requested
    family names equals FamilyName EXACTLY (after trimming quotes) AND the
    requested weight, width and slant all match the loaded face. Register one
    provider per face you want available, or override FamilyName to make a
    face answer to the name the SVG asks for.


TEXT SHAPING AND ASSET LOADING SEAM
-----------------------------------
Namespace CodeBrix.SkiaSvg.Model. These interfaces are the seam a host can
implement to take over image loading, font metrics and glyph shaping. The
built-in implementation, SkiaSvgAssetLoader (namespace CodeBrix.SkiaSvg),
implements all of them on top of SkiaSharp and HarfBuzz.

    public interface ISvgAssetLoader
    {
        SKImage LoadImage(Stream stream);
        List<TypefaceSpan> FindTypefaces(string text,
                                         SKPaint paintPreferredTypeface);
        SKFontMetrics GetFontMetrics(SKPaint paint);
        float MeasureText(string text, SKPaint paint, ref SKRect bounds);
        SKPath GetTextPath(string text, SKPaint paint, float x, float y);
    }

    public interface ISvgTextReferenceRenderingOptions
    {
        bool EnableTextReferences { get; }
    }

    public interface ISvgTextRunTypefaceResolver
    {
        SKTypeface FindRunTypeface(string text,
                                   SKPaint paintPreferredTypeface);
    }

    public interface ISvgTextGlyphRunResolver
    {
        bool TryShapeGlyphRun(string text, SKPaint paint,
                              out ShapedGlyphRun shapedRun);
    }

    public interface ISvgTextDirectedGlyphRunResolver
    {
        bool TryShapeGlyphRun(string text, SKPaint paint, bool rightToLeft,
                              out ShapedGlyphRun shapedRun);
    }

(The SKImage / SKPaint / SKPath / SKRect / SKFontMetrics / SKTypeface in
those signatures are the ShimSkiaSharp ones.)

Value types carried across the seam:
    public record struct TypefaceSpan(string Text, float Advance,
                                      SKTypeface Typeface);
    public readonly record struct ShapedGlyphRun(ushort[] Glyphs,
                                                 SKPoint[] Points,
                                                 int[] Clusters,
                                                 float Advance);

SkiaSvgAssetLoader (namespace CodeBrix.SkiaSvg) implements ISvgAssetLoader,
ISvgTextReferenceRenderingOptions, ISvgTextRunTypefaceResolver,
ISvgTextGlyphRunResolver and ISvgTextDirectedGlyphRunResolver:
    SkiaSvgAssetLoader(SkiaModel skiaModel)
    bool EnableSvgFonts { get; }
    bool EnableTextReferences { get; }
It caches resolved typefaces, paints and shaped runs, so reuse one instance
rather than creating one per render.

SkiaModel (namespace CodeBrix.SkiaSvg) converts the intermediate drawing
model into real SkiaSharp objects. SKSvg owns one (svg.SkiaModel), and you
only need it directly when calling the static SKSvg.ToPicture / SKSvg.Draw
helpers or converting model values by hand:
    SkiaModel(SKSvgSettings settings)
    SKSvgSettings Settings { get; }
    SkiaSharp.SKPoint ToSKPoint(SKPoint point)
    SkiaSharp.SKPoint[] ToSKPoints(IList<SKPoint> points)
    SkiaSharp.SKPoint3 ToSKPoint3(SKPoint3 point3)
    SkiaSharp.SKPointI ToSKPointI(SKPointI pointI)
    SkiaSharp.SKSize ToSKSize(SKSize size)
    SkiaSharp.SKSizeI ToSKSizeI(SKSizeI sizeI)
    SkiaSharp.SKRect ToSKRect(SKRect rect)
    SkiaSharp.SKMatrix ToSKMatrix(SKMatrix matrix)
    SkiaSharp.SKImage ToSKImage(SKImage image)
    SkiaSharp.SKTypeface ToSKTypeface(SKTypeface typeface)
    SkiaSharp.SKColor ToSKColor(SKColor color)
    SkiaSharp.SKColorF ToSKColor(SKColorF color)
    SkiaSharp.SKShader ToSKShader(SKShader shader)
    SkiaSharp.SKColorFilter ToSKColorFilter(SKColorFilter colorFilter)
    ... plus the matching enum converters (ToSKPaintStyle, ToSKStrokeCap,
    ToSKStrokeJoin, ToSKTextAlign, ToSKTextEncoding, ToSKFontStyleWeight,
    ToSKFontStyleWidth, ToSKFontStyleSlant, ToSKShaderTileMode,
    ToSKColorChannel).
The conversion is one-way: model -> SkiaSharp. There is no SkiaSharp ->
model converter.


DOCUMENT SERVICES
-----------------
SvgService (static, namespace CodeBrix.SkiaSvg.Model.Services) opens SVG
sources into an SvgDocument without constructing an SKSvg, and measures a
fragment:
    static SvgDocument Open(string path, SvgParameters? parameters = null)
    static SvgDocument Open(Stream stream, SvgParameters? parameters = null)
    static SvgDocument Open(XmlReader reader)
    static SvgDocument OpenSvg(string path, SvgParameters? parameters = null)
    static SvgDocument OpenSvgz(string path, SvgParameters? parameters = null)
    static SvgDocument FromSvg(string svg)
    static SvgDocument OpenVectorDrawable(string path,
                                          SvgParameters? parameters = null)
    static SvgDocument OpenVectorDrawable(Stream stream,
                                          SvgParameters? parameters = null)
    static SvgDocument OpenVectorDrawable(XmlReader reader)
    static SvgDocument FromVectorDrawable(string xml)
    static SKSize GetDimensions(SvgFragment svgFragment,
                                SKRect skViewport = default)
Open(path) dispatches on the file extension, so it reads both .svg and
gzip-compressed .svgz.


================================================================================

EDITING THE DRAWING MODEL
=========================
This library renders SVGs; it is not an SVG editor. What it does offer is
programmatic inspection and mutation of two things: the parsed SVG DOM
(through CodeBrix.SvgParse types) and the intermediate ShimSkiaSharp drawing
model. After editing the drawing model, call svg.RebuildFromModel() to
re-render.

SvgDocumentEditingExtensions (static, namespace
CodeBrix.SkiaSvg.Model.Editing) - extension methods on SvgDocument:
    static IEnumerable<SvgElement> TraverseElements(this SvgDocument document)
    static int UpdateStyleAttributes(this SvgDocument document,
                                     Func<SvgVisualElement, bool> predicate,
                                     Action<SvgVisualElement> update)
        Returns the number of elements updated.

EditMode enum (namespace CodeBrix.SkiaSvg.ShimSkiaSharp.Editing):
    InPlace         // modify the object in place (default)
    CloneOnWrite    // clone the object before modifying it

SKPictureEditingExtensions (static, namespace
CodeBrix.SkiaSvg.ShimSkiaSharp.Editing) - extension methods on the
ShimSkiaSharp SKPicture, recursing into nested pictures:
    static IEnumerable<TCommand> FindCommands<TCommand>(this SKPicture picture)
        where TCommand : CanvasCommand
    static int ReplaceCommands(this SKPicture picture,
                               Func<CanvasCommand, CanvasCommand> replace)
    static int UpdatePaints(this SKPicture picture,
                            Func<SKPaint, bool> predicate,
                            Action<SKPaint> update,
                            EditMode mode = EditMode.InPlace)
    static int UpdatePaths(this SKPicture picture,
                           Func<SKPath, bool> predicate,
                           Action<SKPath> update,
                           EditMode mode = EditMode.InPlace)

SKPathEditingExtensions (same namespace):
    static int UpdateCommands(this SKPath path,
                              Func<PathCommand, bool> predicate,
                              Func<PathCommand, PathCommand> replace)
    static void Transform(this SKPath path, SKMatrix matrix)

SKPaintEditingExtensions (same namespace):
    static void ApplyColorTransform(this SKPaint paint,
                                    Func<SKColor, SKColor> transform)
    static void ApplyShaderTransform(this SKPaint paint,
                                     Func<SKShader, SKShader> transform)

CanvasCommandVisitorExtensions (same namespace):
    static void Accept(this CanvasCommand command,
                       ICanvasCommandVisitor visitor)

All of these throw ArgumentNullException on a null receiver, predicate or
callback.

Example - recolor every red fill in the drawing model and re-render:
    using CodeBrix.SkiaSvg;
    using CodeBrix.SkiaSvg.ShimSkiaSharp;
    using CodeBrix.SkiaSvg.ShimSkiaSharp.Editing;

    using var svg = SKSvg.CreateFromFile("chart.svg");

    int changed = svg.Model.UpdatePaints(
        p => p.Color.HasValue && p.Color.Value.Red == 255,
        p => p.Color = new SKColor(0, 128, 0, 255));

    if (changed > 0)
    {
        svg.RebuildFromModel();
    }


GRADIENT MESH
-------------
A mesh of colored points that can be turned into a shader (namespace
CodeBrix.SkiaSvg.Model / .Services):
    public sealed class GradientMesh
    {
        List<GradientMeshPoint> Points { get; }
    }
    public sealed record GradientMeshPoint(SKPoint Position, SKColor Color);

    public static class GradientMeshService
    {
        static SKShader ToShader(GradientMesh mesh);
    }
(SKPoint / SKColor / SKShader here are the ShimSkiaSharp types.)


================================================================================

SHIMSKIASHARP: THE INTERMEDIATE DRAWING MODEL
=============================================
Namespace CodeBrix.SkiaSvg.ShimSkiaSharp. This is a self-contained,
inspectable description of what to draw - a picture is a list of canvas
commands, a path is a list of path commands - which SkiaModel later replays
onto a real SkiaSharp canvas. Consumers touch it for hit testing (SKPoint,
SKRect, SKMatrix), for the scene graph, and for model editing.

Value types:
    SKPoint, SKPointI, SKPoint3, SKSize, SKSizeI, SKRect, SKMatrix,
    SKColor, SKColorF, SKFontMetrics

Drawing objects:
    SKPicture (a record: SKPicture(SKRect CullRect,
                                   IList<CanvasCommand> Commands))
    SKCanvas, SKPictureRecorder, SKPaint, SKPath, SKDrawable, SKImage,
    SKTypeface, SKTextBlob, ClipPath, PathClip

Canvas commands (abstract record CanvasCommand, all records):
    ClipPathCanvasCommand(ClipPath ClipPath, SKClipOperation Operation,
                          bool Antialias)
    ClipRectCanvasCommand(SKRect Rect, SKClipOperation Operation,
                          bool Antialias)
    DrawImageCanvasCommand(SKImage Image, SKRect Source, SKRect Dest,
                           SKPaint Paint = null)
    DrawPathCanvasCommand(SKPath Path, SKPaint Paint)
    DrawPictureCanvasCommand(SKPicture Picture)
    DrawTextCanvasCommand(string Text, float X, float Y, SKPaint Paint)
    DrawTextBlobCanvasCommand(SKTextBlob TextBlob, float X, float Y,
                              SKPaint Paint)
    DrawTextOnPathCanvasCommand(string Text, SKPath Path, float HOffset,
                                float VOffset, SKPaint Paint)
    SaveCanvasCommand(int Count), RestoreCanvasCommand(int Count),
    SaveLayerCanvasCommand(int Count, SKPaint Paint = null)
    SetMatrixCanvasCommand(SKMatrix DeltaMatrix, SKMatrix TotalMatrix)

Path commands (abstract record PathCommand, all records):
    MoveToPathCommand(float X, float Y)
    LineToPathCommand(float X, float Y)
    QuadToPathCommand(float X0, float Y0, float X1, float Y1)
    CubicToPathCommand(float X0, float Y0, float X1, float Y1,
                       float X2, float Y2)
    ArcToPathCommand(float Rx, float Ry, float XAxisRotate,
                     SKPathArcSize LargeArc, SKPathDirection Sweep, ...)
    ClosePathCommand
    AddRectPathCommand(SKRect Rect)
    AddRoundRectPathCommand(SKRect Rect, float Rx, float Ry)
    AddOvalPathCommand(SKRect Rect)
    AddCirclePathCommand(float X, float Y, float Radius)
    AddPolyPathCommand(IList<SKPoint> Points, bool Close)

Shaders (abstract record SKShader):
    ColorShader, LinearGradientShader, RadialGradientShader,
    TwoPointConicalGradientShader, PictureShader,
    PerlinNoiseFractalNoiseShader, PerlinNoiseTurbulenceShader

Color filters (abstract record SKColorFilter):
    BlendModeColorFilter(SKColor Color, SKBlendMode Mode),
    ColorMatrixColorFilter(float[] Matrix), LumaColorColorFilter,
    TableColorFilter(byte[] TableA, byte[] TableR, byte[] TableG,
                     byte[] TableB)

Image filters (abstract record SKImageFilter) - the SVG filter-primitive
model:
    ArithmeticImageFilter, BlendModeImageFilter, BlurImageFilter,
    ColorFilterImageFilter, DilateImageFilter, ErodeImageFilter,
    DisplacementMapEffectImageFilter, DistantLitDiffuseImageFilter,
    DistantLitSpecularImageFilter, PointLitDiffuseImageFilter,
    PointLitSpecularImageFilter, SpotLitDiffuseImageFilter,
    SpotLitSpecularImageFilter, ImageImageFilter,
    MatrixConvolutionImageFilter, MergeImageFilter, OffsetImageFilter,
    PaintImageFilter, PictureImageFilter, ShaderImageFilter,
    TileImageFilter

Path effects (abstract record SKPathEffect):
    DashPathEffect(float[] Intervals, float Phase)

Enumerations:
    SKBlendMode, SKClipOperation, SKColorChannel, SKColorSpace,
    SKFilterQuality, SKFontStyleSlant, SKFontStyleWeight, SKFontStyleWidth,
    SKPaintStyle, SKPathArcSize, SKPathDirection, SKPathFillType, SKPathOp,
    SKShaderTileMode, SKStrokeCap, SKStrokeJoin, SKTextAlign, SKTextEncoding

Interfaces:
    IDeepCloneable<out T>       // T Clone() deep-copy contract implemented
                                //   by the commands, paints, paths,
                                //   pictures, shaders and filters
    ICanvasCommandVisitor       // visitor over the canvas-command records;
                                //   pair it with
                                //   CanvasCommandVisitorExtensions.Accept

Helper:
    SKPathBoundsHelper          // path bounds computation over the command
                                //   list


================================================================================

COMPLETE EXAMPLES
=================

Example 1: Load and render an SVG to a canvas
---------------------------------------------
    using CodeBrix.SkiaSvg;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromFile("logo.svg");

    canvas.Clear(SKColors.White);
    canvas.DrawPicture(svg.Picture);


Example 2: Convert an SVG to PNG
--------------------------------
    using CodeBrix.SkiaSvg;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromFile("icon.svg");

    // save as PNG at 2x scale
    svg.Save("icon.png", SKColors.Transparent,
             SKEncodedImageFormat.Png, 100, 2f, 2f);


Example 3: Convert an SVG to PDF
--------------------------------
    using CodeBrix.SkiaSvg;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromFile("document.svg");
    svg.Picture.ToPdf("document.pdf", SKColors.White, 1f, 1f);


Example 4: Hit testing
----------------------
    using CodeBrix.SkiaSvg;
    using CodeBrix.SkiaSvg.ShimSkiaSharp;   // for SKPoint
    using CodeBrix.SvgParse;                // for SvgVisualElement

    using var svg = SKSvg.CreateFromFile("map.svg");

    foreach (var el in svg.HitTestElements(new SKPoint(150, 75)))
    {
        Console.WriteLine($"Element: {el.GetType().Name}");
        if (el is SvgVisualElement visual)
        {
            Console.WriteLine($"  Fill: {visual.Fill}");
        }
    }

    var top = svg.HitTestTopmostElement(new SKPoint(150, 75));
    Console.WriteLine($"Topmost: {top?.GetType().Name}");


Example 5: Animation playback
-----------------------------
    using CodeBrix.SkiaSvg;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromFile("animation.svg");

    if (svg.HasAnimations)
    {
        // render frames at 30fps for 5 seconds
        var frameDuration = TimeSpan.FromMilliseconds(1000.0 / 30);

        for (int frame = 0; frame < 150; frame++)
        {
            svg.SetAnimationTime(frameDuration * frame);
            canvas.Clear(SKColors.White);
            canvas.DrawPicture(svg.Picture);
            // ... present the frame
        }
    }


Example 6: Retained scene graph with mutation
---------------------------------------------
    using CodeBrix.SkiaSvg;
    using CodeBrix.SvgParse;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromFile("dashboard.svg");

    if (svg.TryEnsureRetainedSceneGraph(out var scene))
    {
        // modify an element in the DOM
        var bar = svg.SourceDocument.GetElementById<SvgRectangle>("bar1");
        bar.Height = new SvgUnit(SvgUnitType.Pixel, 150);
        bar.Fill = new SvgColorServer(SvgColor.Green);

        // recompile only the affected subtree
        var mutation = svg.ApplyRetainedSceneMutationById("bar1");
        Console.WriteLine($"recompiled {mutation.CompilationRootCount} roots");

        canvas.DrawPicture(svg.Picture);
    }


Example 7: Load an Android VectorDrawable
-----------------------------------------
    using CodeBrix.SkiaSvg;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromVectorDrawable("ic_launcher.xml");
    canvas.DrawPicture(svg.Picture);


Example 8: Register an application font (headless / container safe)
-------------------------------------------------------------------
    using CodeBrix.SkiaSvg;
    using CodeBrix.SkiaSvg.TypefaceProviders;
    using SkiaSharp;

    var svg = new SKSvg();

    // one provider per face; highest priority goes first
    var regular = new CustomTypefaceProvider("/app/fonts/Brand-Regular.ttf");
    var bold = new CustomTypefaceProvider("/app/fonts/Brand-Bold.ttf");
    svg.Settings.TypefaceProviders.Insert(0, bold);
    svg.Settings.TypefaceProviders.Insert(0, regular);

    svg.Load("text-heavy.svg");
    canvas.DrawPicture(svg.Picture);

    // CustomTypefaceProvider is IDisposable - dispose with the SKSvg


Example 9: Export to multiple formats
-------------------------------------
    using CodeBrix.SkiaSvg;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromFile("diagram.svg");
    var bg = SKColors.White;

    // raster
    svg.Save("output.png", bg, SKEncodedImageFormat.Png, 100, 1f, 1f);
    svg.Save("output.jpg", bg, SKEncodedImageFormat.Jpeg, 90, 1f, 1f);

    // vector / document
    svg.Picture.ToPdf("output.pdf", bg, 1f, 1f);
    svg.Picture.ToSvg("output.svg", bg, 1f, 1f);
    svg.Picture.ToXps("output.xps", bg, 1f, 1f);


Example 10: Interactive view - hover cursor and click
-----------------------------------------------------
    using CodeBrix.SkiaSvg;
    using CodeBrix.SkiaSvg.ShimSkiaSharp;

    using var svg = SKSvg.CreateFromFile("floorplan.svg");
    var dispatcher = new SvgInteractionDispatcher();

    static SvgPointerInput Input(float x, float y, SvgMouseButton button,
                                 int clicks) =>
        new SvgPointerInput(new SKPoint(x, y), SvgPointerDeviceType.Mouse,
                            button, clicks, 0, false, false, false, "mouse");

    // pointer moved -> update the cursor
    var moved = dispatcher.DispatchPointerMoved(
        svg, Input(120, 80, SvgMouseButton.None, 0));
    string cursor = moved.Cursor;              // e.g. "pointer"

    // pointer pressed -> which room was clicked
    var pressed = dispatcher.DispatchPointerPressed(
        svg, Input(120, 80, SvgMouseButton.Left, 1));
    Console.WriteLine($"clicked: {pressed.TargetElement?.ID}");


Example 11: Rasterize an SVG whose content does not start at (0,0)
------------------------------------------------------------------
    using CodeBrix.SkiaSvg;
    using SkiaSharp;

    using var svg = SKSvg.CreateFromFile("offset-art.svg");
    var cull = svg.Picture.CullRect;
    const float scale = 2f;

    var info = new SKImageInfo(
        (int)Math.Ceiling(cull.Width * scale),
        (int)Math.Ceiling(cull.Height * scale));

    using var bitmap = new SKBitmap(info);
    using (var canvas = new SKCanvas(bitmap))
    {
        canvas.Clear(SKColors.Transparent);
        canvas.Scale(scale);
        canvas.Translate(-cull.Left, -cull.Top);
        canvas.DrawPicture(svg.Picture);
    }

    using var image = SKImage.FromBitmap(bitmap);
    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
    using var file = File.OpenWrite("offset-art.png");
    data.SaveTo(file);


================================================================================

MINIMUM VIABLE PROJECT
======================
A console application that turns an SVG file into a PNG.

MySvgTool.csproj:
    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net10.0</TargetFramework>
        <Nullable>disable</Nullable>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include="CodeBrix.SkiaSvg.MitLicenseForever" />
        <!-- required on Linux; use the macOS/Win32 variant per platform -->
        <PackageReference Include="SkiaSharp.NativeAssets.Linux" />
      </ItemGroup>
    </Project>

(Add the packages with `dotnet add package <id>` so the current versions are
written into the csproj.)

Program.cs:
    using System;
    using CodeBrix.SkiaSvg;
    using SkiaSharp;

    if (args.Length < 2)
    {
        Console.Error.WriteLine("usage: MySvgTool <in.svg> <out.png>");
        return 1;
    }

    try
    {
        using var svg = SKSvg.CreateFromFile(args[0]);
        bool ok = svg.Save(args[1], SKColors.Transparent,
                           SKEncodedImageFormat.Png, 100, 1f, 1f);
        Console.WriteLine(ok ? "written" : "nothing to write");
        return ok ? 0 : 2;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"failed: {ex.Message}");
        return 3;
    }


================================================================================

PERFORMANCE TIPS
================

1.  Use the static factory methods (SKSvg.CreateFromFile and friends) for
    the simplest case - they load and render in one call.

2.  Reuse SKSvg instances when rendering the same SVG repeatedly. The parsed
    DOM, the intermediate model and the rendered picture are all cached on
    the instance.

3.  Use the retained scene graph for interactive or frequently updated SVGs.
    ApplyRetainedSceneMutation() recompiles only the affected compilation
    roots and resources (the returned SvgSceneMutationResult tells you how
    many) instead of re-rendering the whole document.

4.  For animation, prefer native composition
    (TryCreateNativeCompositionScene) when SupportsNativeComposition is
    true: the SVG is decomposed into layers and only the animated ones need
    re-rendering.

5.  Set AnimationMinimumRenderInterval to throttle animation frame rates
    (negative values clamp to zero).

6.  Dispose SKSvg instances when done - they hold unmanaged SkiaSharp
    resources. CustomTypefaceProvider is IDisposable too.

7.  Reuse a single SkiaSvgAssetLoader / SkiaModel pair when you call the
    static SKSvg.ToPicture / SKSvg.Draw helpers in a loop; both cache
    typefaces, paints and shaped text runs.

8.  Use ToBitmap() with the SKColorType your platform actually uses to avoid
    an extra color-format conversion.
    SHARP EDGE (verified 2026-07-29): ToBitmap()/Draw() scale by
    CullRect.Width/Height but do NOT translate by CullRect.Left/Top - an SVG
    whose content bounds do not start at (0,0) comes out clipped or offset.
    For such files rasterize by hand as in Example 11. Also note that
    ToBitmap truncates (int)(Width * scale) rather than rounding.
    SHARP EDGE: CreateFromStream()/Load() on non-SVG bytes throws (for
    example System.Xml.XmlException) rather than returning null - wrap the
    load in try/catch when the input is untrusted (user files, zip entries).

9.  For batch export, load the SVG once and call Save() or the export
    extension methods several times.

10. Insert custom ITypefaceProviders at index 0 for highest priority; the
    chain is consulted in order and the first non-null answer wins.

11. For headless, container or CI environments with no system fonts,
    register the fonts explicitly with CustomTypefaceProvider instead of
    relying on FontManagerTypefaceProvider.

12. Turn off SVG features you do not need with IgnoreAttributes (for
    example DrawAttributes.Filter | DrawAttributes.Mask) when rendering
    thumbnails at speed.


================================================================================

COMMON PITFALLS TO AVOID
========================

1.  DO NOT confuse the NuGet package id with the namespace.
      Package:   CodeBrix.SkiaSvg.MitLicenseForever
      Namespace: CodeBrix.SkiaSvg

2.  DO NOT use the upstream Svg.Skia, Svg.Model or ShimSkiaSharp namespaces.
    Everything is rooted at CodeBrix.SkiaSvg (see OVERVIEW for the mapping).

3.  DO NOT write `using CodeBrix.SkiaSvg.Interaction;` - that namespace does
    not exist. The pointer types are in CodeBrix.SkiaSvg. The same goes for
    scene-graph and animation types: folder names are not namespaces here.

4.  DO NOT mix up the two SKPoint / SKRect / SKMatrix / SKPicture / SKPaint
    / SKPath families. Hit testing, SvgPointerInput, the scene graph and the
    editing extensions take CodeBrix.SkiaSvg.ShimSkiaSharp types; svg.Picture,
    svg.Draw(canvas) and the export extension methods take SkiaSharp types.
    There is NO implicit conversion - `new SKPoint(x, y)` under `using
    SkiaSharp;` will not bind to HitTestElements. Add
    `using CodeBrix.SkiaSvg.ShimSkiaSharp;` for hit testing, and if both
    usings are in scope, disambiguate with an alias:
        using ShimPoint = CodeBrix.SkiaSvg.ShimSkiaSharp.SKPoint;
    SkiaModel converts model -> SkiaSharp only; going the other way means
    building the model value yourself (see the SKMatrix factories under HIT
    TESTING).

5.  DO NOT target .NET versions below 10.

6.  DO NOT forget to dispose SKSvg instances - they implement IDisposable
    and hold unmanaged SkiaSharp resources.

7.  DO NOT confuse svg.Model (ShimSkiaSharp.SKPicture, the intermediate
    drawing model) with svg.Picture (SkiaSharp.SKPicture, the rendered
    result). Edit Model, then call RebuildFromModel() to refresh Picture.

8.  DO NOT assume thread safety. SkiaSharp objects are not thread-safe, and
    neither is an SKSvg instance. Every SKSvg method that touches the
    picture takes the instance's own lock, which is exposed as the public
    `object Sync { get; }` property - take `lock (svg.Sync)` around any
    multi-step sequence you perform from another thread.

9.  DO NOT call ApplyRetainedSceneMutation without a retained scene graph.
    Reading svg.RetainedSceneGraph or calling TryEnsureRetainedSceneGraph
    compiles it; if compilation fails, both report null/false rather than
    throwing.

10. DO NOT expect animation to run by itself. There is no internal timer:
    you must drive the clock with SetAnimationTime or AdvanceAnimation, and
    call FlushPendingAnimationFrame when HasPendingAnimationFrame is true.

11. DO NOT expect the SvgPointerEventType members to be named
    PointerDown/PointerUp. They are Move, Press, Release, Enter, Leave,
    Wheel and Click.

12. DO NOT call `new CustomTypefaceProvider()` - there is no parameterless
    constructor, and one instance serves exactly one face, matching only on
    an exact family name plus exact weight/width/slant. Register one
    provider per face (or set FamilyName to the name the SVG asks for).

13. DO NOT assume system fonts exist in Docker/CI images. Register fonts
    with CustomTypefaceProvider.

14. DO NOT forget that ReLoad(parameters) has no default argument - pass
    null explicitly.

15. DO NOT forget that VectorDrawable support is for Android vector
    drawable XML, not compiled Android binary resources.

16. DO NOT forget the background-color argument on Save() and the export
    extension methods. Use SKColors.Transparent for transparency (PNG and
    other alpha-capable formats only) or an opaque color otherwise; JPEG
    with a transparent background produces a black background.

17. DO NOT ship only the package on Linux/macOS/Windows without the matching
    SkiaSharp.NativeAssets.* package in the application project - it
    compiles and then fails at run time.


================================================================================

WHAT THIS PACKAGE DOES NOT DO
=============================
Do NOT reach for this library for:

  - Authoring SVG markup from scratch or serializing a DOM back to an .svg
    file. It renders and inspects; for building or editing the SVG DOM,
    the document object model comes from the CodeBrix.SvgParse package
    (PackageId CodeBrix.SvgParse.MsplLicenseForever) - that is where
    SvgDocument, SvgElement and the element types are defined and where
    DOM-level manipulation belongs. This package adds only the
    render-oriented editing helpers described under EDITING THE DRAWING
    MODEL. (The upstream "Svg.Skia.Custom" companion does not exist here.)
  - SVG optimization or minification.
  - Converting HTML/CSS documents to SVG.
  - 3D rendering or WebGL-style effects.
  - Video or animated-GIF export - it renders individual frames; encoding
    them into a movie is your job.
  - Browser-identical SVG rendering. It rasterizes with SkiaSharp, not with
    a browser engine, and it implements SVG 1.1 plus SMIL, not the whole of
    SVG 2.
  - PDF/XPS text extraction or reflow. ToPdf/ToXps write the rendering, not
    a structured document.
  - Validating an SVG against the specification, or reporting parse
    diagnostics.
  - Font shaping configuration beyond typeface resolution (HarfBuzz is used
    internally; there is no public feature/variation-axis API).

CodeBrix.SkiaSvg IS for: loading SVG documents and Android VectorDrawables,
rendering them to SkiaSharp surfaces, exporting to raster and vector
formats, hit testing elements, driving SMIL animations, and building
interactive SVG views with the retained scene graph and pointer dispatch.


================================================================================

WORKING EXAMPLES ON GITHUB
==========================
The test project is the executable documentation for this package. Browse it
at:

    https://github.com/ellisnet/CodeBrix.SkiaSvg/tree/main/tests/CodeBrix.SkiaSvg.Tests

Feature-to-test-file map:

  Core SVG loading and rendering:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/SKSvgTests.cs

  SKSvg settings and configuration:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/SKSvgSettingsTests.cs

  Hit testing (point, rect, pointer-events, clips and masks):
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/HitTestTests.cs

  Retained scene graph and mutation:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/SvgRetainedSceneGraphTests.cs

  Rebuild from the intermediate model:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/SKSvgRebuildFromModelTests.cs

  Animation clock:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/SvgAnimationClockTests.cs

  Animation controller (SMIL bindings, begin/end, pointer conditions):
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/SvgAnimationControllerTests.cs

  Animation smoke tests:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/AnimationSmokeTests.cs

  Animation host backend resolution:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/SvgAnimationHostBackendResolverTests.cs

  Native composition:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/SKSvgNativeCompositionTests.cs

  Pointer dispatch (SvgInteractionDispatcher, routing phases, cursors):
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/SvgInteractionDispatcherTests.cs

  pointer-events attribute handling:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/Model/PointerEventsTests.cs

  Editing extensions (pictures, paths, paints, EditMode):
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/ShimSkiaSharp/EditingHelpersTests.cs

  Deep cloning of the drawing model:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/ShimSkiaSharp/CloneCoreTests.cs

  ShimSkiaSharp value types (SKPoint, SKRect, SKMatrix, SKPath, SKCanvas):
    https://github.com/ellisnet/CodeBrix.SkiaSvg/tree/main/tests/CodeBrix.SkiaSvg.Tests/ShimSkiaSharp

  VectorDrawable loading (and the Android spec cases):
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/VectorDrawableTests.cs
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/Model/VectorDrawableAndroidSpecTests.cs

  SVG document compatibility loading:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/SvgDocumentCompatibilityLoaderTests.cs

  Marker parsing:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/SvgMarkerParsingTests.cs

  Opacity and fragment rendering:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/OpacityRenderingTests.cs
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/StructFragmentRenderingTests.cs

  Painting service and pattern paint resolution:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/Model/PaintingServiceTests.cs
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/Model/SvgPatternPaintStateResolverTests.cs

  Conditional processing (requiredFeatures / systemLanguage):
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/Model/SvgConditionalProcessingTests.cs

  SkiaModel text/font API and canvas-command handling:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/SkiaModelTextApiTests.cs
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/SkiaModelDrawTests.cs

  Typeface resolution and fake-bold adjustment:
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/Issue405Tests.cs

  Rendering-conformance suites (W3C SVG 1.1 and resvg):
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/W3CTestSuiteTests.cs
    https://github.com/ellisnet/CodeBrix.SkiaSvg/blob/main/tests/CodeBrix.SkiaSvg.Tests/resvgTests.cs

To read a file's source directly, fetch the raw URL:
    https://raw.githubusercontent.com/ellisnet/CodeBrix.SkiaSvg/main/tests/CodeBrix.SkiaSvg.Tests/SKSvgTests.cs


================================================================================

QUICK REFERENCE CARD
====================

--- Install ---
dotnet add package CodeBrix.SkiaSvg.MitLicenseForever
dotnet add package SkiaSharp.NativeAssets.Linux    (per-platform natives)

--- Namespaces ---
using CodeBrix.SkiaSvg;                       // SKSvg + everything core
using CodeBrix.SkiaSvg.ShimSkiaSharp;         // SKPoint/SKRect/SKMatrix/...
using CodeBrix.SkiaSvg.ShimSkiaSharp.Editing; // editing extensions
using CodeBrix.SkiaSvg.Model;                 // ISvgAssetLoader, SvgParameters
using CodeBrix.SkiaSvg.Model.Services;        // SvgService
using CodeBrix.SkiaSvg.Model.Editing;         // SvgDocumentEditingExtensions
using CodeBrix.SkiaSvg.TypefaceProviders;     // font resolution
using CodeBrix.SvgParse;                      // SvgDocument, SvgElement
using SkiaSharp;                              // real Skia types

--- Load ---
From file:          SKSvg.CreateFromFile("path.svg")
From stream:        SKSvg.CreateFromStream(stream)
From string:        SKSvg.CreateFromSvg(svgText)
From XmlReader:     SKSvg.CreateFromXmlReader(reader)
From document:      SKSvg.CreateFromSvgDocument(doc)
VectorDrawable:     SKSvg.CreateFromVectorDrawable("path.xml")
Reload:             svg.ReLoad(null)

--- Render ---
Draw to canvas:     canvas.DrawPicture(svg.Picture)
Draw via SKSvg:     svg.Draw(canvas)
Get picture:        svg.Picture           (SkiaSharp.SKPicture)
Get model:          svg.Model             (ShimSkiaSharp.SKPicture)
Rebuild:            svg.RebuildFromModel()
Wireframe:          svg.Wireframe = true
Skip features:      svg.IgnoreAttributes = DrawAttributes.Filter

--- Export ---
Save raster:        svg.Save("out.png", bg, SKEncodedImageFormat.Png, 100, 1f, 1f)
To bitmap:          svg.Picture.ToBitmap(bg, sx, sy, colorType, alphaType, cs)
To PDF:             svg.Picture.ToPdf("out.pdf", bg, 1f, 1f)
To SVG:             svg.Picture.ToSvg("out.svg", bg, 1f, 1f)
To XPS:             svg.Picture.ToXps("out.xps", bg, 1f, 1f)

--- Hit Testing (ShimSkiaSharp SKPoint/SKRect/SKMatrix) ---
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
Compile by hand:    SvgSceneRuntime.TryCompile(fragment, loader, attrs, out doc)
Flatten:            SvgSceneRenderer.Render(sceneDocument)

--- Animation ---
Has animations:     svg.HasAnimations
Set time:           svg.SetAnimationTime(TimeSpan.FromSeconds(2))
Advance:            svg.AdvanceAnimation(TimeSpan.FromMilliseconds(100))
Reset:              svg.ResetAnimation()
Flush frame:        svg.FlushPendingAnimationFrame()
Pending:            svg.HasPendingAnimationFrame
Clock:              svg.AnimationController.Clock.Seek(time)
Backend choice:     SvgAnimationHostBackendResolver.Resolve(req, caps, hasAnim)

--- Native Composition ---
Supported:          svg.SupportsNativeComposition
Full scene:         svg.TryCreateNativeCompositionScene(out scene)
Frame only:         svg.TryCreateNativeCompositionFrame(out frame)

--- Interaction ---
Create once:        var d = new SvgInteractionDispatcher();
Move/press/release: d.DispatchPointerMoved/Pressed/Released(svg, input)
Wheel/exit:         d.DispatchPointerWheelChanged/Exited(svg, input)
Result:             result.TargetElement / result.Cursor / result.Handled
Observe routing:    d.Dispatched += (s, e) => { e.Handled = true; }
State:              d.HoveredElement / d.PressedElement / d.CapturedElement

--- Editing the model ---
Find commands:      svg.Model.FindCommands<DrawPathCanvasCommand>()
Update paints:      svg.Model.UpdatePaints(pred, upd, EditMode.InPlace)
Update paths:       svg.Model.UpdatePaths(pred, upd)
Walk the DOM:       svg.SourceDocument.TraverseElements()
Restyle the DOM:    svg.SourceDocument.UpdateStyleAttributes(pred, upd)
Then:               svg.RebuildFromModel()

--- Settings ---
Custom fonts:       svg.Settings.TypefaceProviders.Insert(0, provider)
SVG fonts:          svg.Settings.EnableSvgFonts = true
Text references:    svg.Settings.EnableTextReferences = true
Viewport:           svg.Settings.StandaloneViewport = rect

--- Cleanup ---
Dispose:            svg.Dispose()   // or a 'using' declaration
Clone:              var copy = svg.Clone()
Lock:               lock (svg.Sync) { ... }

Target: .NET 10 or later
License: MIT


================================================================================
END OF AGENT-README
