================================================================================
MAINTAINER-README: CodeBrix.SkiaSvg
Notes for people and agents MAINTAINING this repository - not for package
consumers
================================================================================

If you are CONSUMING the NuGet package, stop reading and open AGENT-README.txt
instead. Everything below is about the repository itself: how it is laid out,
how it builds, how it is tested, how it is packaged, and the conventions the
source follows.


PURPOSE AND SCOPE
=================
This repository produces exactly one NuGet package:

    PackageId:  CodeBrix.SkiaSvg.MitLicenseForever
    Assembly:   CodeBrix.SkiaSvg
    Project:    src/CodeBrix.SkiaSvg/CodeBrix.SkiaSvg.csproj
    License:    MIT
    Consumer documentation: AGENT-README.txt (repo root)

The library is an SVG loading/rendering stack over SkiaSharp: a parser-facing
model layer, an intermediate drawing model (ShimSkiaSharp), a retained scene
graph, SMIL animation, native composition, pointer interaction, and export.
The SVG DOM itself comes from the separate CodeBrix.SvgParse package.


REPOSITORY LAYOUT
=================
    src/CodeBrix.SkiaSvg/            the library project (the only packable
                                     project in the repo)
      (root)                         SKSvg (partial across .Model, .HitTest,
                                     .SceneGraph, .NativeComposition,
                                     .Interaction, .AnimationLayers),
                                     SKSvgSettings, SKSvgDrawEventArgs,
                                     SkiaModel (+ .Caching, .TextShaping),
                                     SkiaSvgAssetLoader (+ .Caching),
                                     SKPictureExtensions, InternalsVisibleTo.cs
      Animation/                     SvgAnimationClock, SvgAnimationController,
                                     SvgAnimationParser, SvgAnimationSpline,
                                     SvgAnimationFrameState,
                                     SvgAnimationHostBackend(+Capabilities/
                                     Resolution/Resolver),
                                     SvgAnimationInvalidation,
                                     SvgNativeCompositionScene,
                                     SvgPointerEventType
      Interaction/                   SvgInteractionDispatcher and the pointer
                                     input/event/result types
      Model/                         ISvgAssetLoader and the text-shaping seam,
                                     SvgParameters, DrawAttributes,
                                     GradientMesh, filter contexts/results
      Model/Editing/                 SvgDocumentEditingExtensions
      Model/Services/                SvgService, PaintingService, PathingService,
                                     TransformsService, MaskingService,
                                     FilterEffectsService,
                                     GeometryHitTestService,
                                     GradientMeshService,
                                     SvgPatternPaintStateResolver,
                                     VectorDrawableConverter
      SceneGraph/                    SvgSceneCompiler, SvgSceneRuntime,
                                     SvgSceneRenderer, SvgSceneDocument,
                                     SvgSceneNode(+Kind), SvgSceneResource
                                     (+Kind), SvgSceneMutationResult,
                                     SvgSceneCompilationStrategy, the clip/
                                     text/filter compilers and hit-test and
                                     bounds services
      ShimSkiaSharp/                 the intermediate drawing model: value
                                     types, canvas/path command records,
                                     shaders, color/image filters, path
                                     effects, clone helpers
      ShimSkiaSharp/Editing/         EditMode and the SKPicture/SKPath/SKPaint
                                     editing extensions,
                                     CanvasCommandVisitorExtensions
      TypefaceProviders/             ITypefaceProvider + the three built-in
                                     providers

    tests/CodeBrix.SkiaSvg.Tests/    the xUnit v3 test project
      Common/                        SvgUnitTest base, ImageHelper,
                                     OSXTheory, WindowsTheory, LinuxTestGate
      Assets/, TestAssets/,          test SVGs, fonts and reference images
      ChromeReference/
      Model/, ShimSkiaSharp/         per-area test folders

    tests/Tests/                     two loose sample files (see
                                     EXTRAS-README.txt)

    externals/                       vendored third-party TEST CORPORA (see
                                     PROVENANCE AND VENDORED SOURCES)

    CodeBrix.SkiaSvg.slnx            the solution; its Solution Items folder
                                     carries .gitignore, AGENT-README.txt,
                                     EXTRAS-README.txt, global.json,
                                     icon-codebrix-128.png, LICENSE,
                                     MAINTAINER-README.txt, README-INDEX.txt,
                                     README.md and THIRD-PARTY-NOTICES.txt;
                                     its Tests folder carries the test project

    global.json                      selects the Microsoft.Testing.Platform
                                     test runner. Does NOT pin an SDK version.
                                     See TESTING below.

Source folders map to namespaces EXCEPT for Interaction/, SceneGraph/ and
Animation/, whose types are deliberately declared in the plain
CodeBrix.SkiaSvg namespace so that consumers need one using directive for the
whole entry-point surface. Do not "fix" this by adding sub-namespaces - the
public API depends on it.


BUILDING
========
Standard SDK build from the repo root:

    dotnet restore CodeBrix.SkiaSvg.slnx
    dotnet build   CodeBrix.SkiaSvg.slnx

Target framework: net10.0 only. GenerateDocumentationFile is ON, so every
public (and protected-on-unsealed) member must carry an XML doc comment; fix
CS1591 at the source and never add a project-wide <NoWarn>.

THE SKIASHARP / HARFBUZZSHARP LOCK-STEP RULE
--------------------------------------------
The library references SkiaSharp, HarfBuzzSharp and the three
HarfBuzzSharp.NativeAssets.* packages; the test project additionally
references SkiaSharp.NativeAssets.Linux. SkiaSharp and HarfBuzzSharp carry
INDEPENDENT version numbers but ship as a matched release pair from the same
family, and the native-asset packages must match their own parent exactly.

Durable rule: when bumping any one of them, bump ALL of them in the same
commit - SkiaSharp and SkiaSharp.NativeAssets.* to the same SkiaSharp version,
HarfBuzzSharp and every HarfBuzzSharp.NativeAssets.* to the corresponding
HarfBuzzSharp version of that same release - in BOTH the library project and
the test project. A mismatched pair typically builds cleanly and then fails at
run time inside text shaping.

Never write these pinned versions into AGENT-README.txt; the csproj files are
the single source of truth for them.


TESTING
=======
    dotnet test CodeBrix.SkiaSvg.slnx

THE TEST RUNNER IS Microsoft.Testing.Platform (MTP), selected by global.json at
the repo root:

    { "test": { "runner": "Microsoft.Testing.Platform" } }

That file does NOT pin an SDK version, so the newest installed .NET 10 SDK is
still used; it exists solely to select the runner. Because the setting lives in
global.json rather than in the csproj, it applies to every `dotnet test` run
anywhere in the repository, including CI. Keep the file committed - without it,
`dotnet test` silently falls back to the older VSTest bridge. You can tell which
one ran: MTP output ends in a "Test run summary:" block, while the VSTest bridge
invokes MSBuild with `--target:VSTest`.

The test project is xUnit v3 (with xunit.runner.visualstudio and
Microsoft.NET.Test.Sdk) and also references CodeBrix.Imaging for image
comparison. It has an InternalsVisibleTo grant from the library
(src/CodeBrix.SkiaSvg/InternalsVisibleTo.cs).

No network access, environment variables or setup steps are required: the
test assets and the external corpora are all committed.

PLATFORM GATING - the two pixel-comparison suites (resvgTests and
W3CTestSuiteTests) render SVGs and compare them against reference PNGs that
were produced by the macOS Skia rasterizer. The [OSXTheory] attribute plus
Common/LinuxTestGate.cs gate them:
  * macOS   - the canonical baseline; every resvg and W3C case runs.
  * Linux   - the resvg suite runs (geometry, paint, gradients and filters
              match exactly) with a known set of text-only cases skipped,
              because Linux rasterizes glyphs through FreeType while the
              references came from CoreText. The W3C suite is skipped
              entirely: its baselines diverge broadly off macOS.
  * Other   - both suites are skipped.
A pixel-comparison failure on Linux is therefore not automatically a
regression; reproduce it on macOS before treating it as one.


PACKAGING AND PUBLISHING
========================
GeneratePackageOnBuild is true, so every build of the library project emits a
fresh .nupkg.

Versioning is the CodeBrix date-stamped scheme, computed in the csproj from
System.DateTime.UtcNow as 1.<years-since-base>.<day-of-year>.<minute-of-day>.
It is monotonically increasing but is NOT SemVer, so major/minor say nothing
about API compatibility. Two builds inside the same UTC minute produce the
SAME version - never publish two packages from one minute. To re-baseline the
minor number, change _VersionBaseYear in the csproj. Do not replace the
version block with a literal <Version>.

What ships inside the nupkg (declared as <None ... Pack="true"> in the
library csproj):
    icon-codebrix-128.png    (PackageIcon)
    README.md                (PackageReadmeFile)
    AGENT-README.txt         (the consumer guide, taken from the repo root)
    THIRD-PARTY-NOTICES.txt
MAINTAINER-README.txt, EXTRAS-README.txt and README-INDEX.txt are repo-only
and are NOT packed. The externals/ corpora are NOT packed either.

PackageLicenseExpression is MIT and PackageRequireLicenseAcceptance is true.


PROVENANCE AND VENDORED SOURCES
===============================
CodeBrix.SkiaSvg is a fork of Svg.Skia v4.2.0
(https://github.com/wieslawsoltes/Svg.Skia), MIT licensed, Copyright (c) 2020
Wieslaw Soltes. Several companion packages of that ecosystem were consolidated
into this single library. Full notices are in THIRD-PARTY-NOTICES.txt.

Namespace mapping applied to every ported file:
    Svg.Skia                  -> CodeBrix.SkiaSvg
    Svg.Model                 -> CodeBrix.SkiaSvg.Model
    Svg.Model.Services        -> CodeBrix.SkiaSvg.Model.Services
    Svg.Model.Editing         -> CodeBrix.SkiaSvg.Model.Editing
    ShimSkiaSharp             -> CodeBrix.SkiaSvg.ShimSkiaSharp
    ShimSkiaSharp.Editing     -> CodeBrix.SkiaSvg.ShimSkiaSharp.Editing
    Svg.Skia.TypefaceProviders-> CodeBrix.SkiaSvg.TypefaceProviders

Every ported file keeps its upstream copyright header and carries a
"//Was previously: namespace <upstream>;" comment on its namespace line.
Preserve both when editing; never fabricate a header on a genuinely new file.

externals/ - VENDORED UPSTREAM TEST CORPORA, not source code and not part of
any package. Two third-party SVG suites (a resvg corpus under MPL-2.0 and the
W3C SVG 1.1 Test Suite under the W3C Document License) are checked into the
repository at the exact commits the upstream Svg.Skia release referenced as
git submodules, so the bundled reference PNGs match the comparison thresholds
in the test code. Because they are committed, a normal clone is ready to run
the tests with no fetch or setup step. externals/README.md records the
upstream repositories, the pinned commit hashes and the licenses; regenerate
by re-downloading each upstream repository at its pinned commit and laying it
out at the same path. Do not index, refactor, reformat or otherwise treat
externals/ as project source.


CODING CONVENTIONS
==================
These are the repository-specific rules; they apply to the library and the
test project alike.

  - Nullable reference types are OFF. Never use '?' on reference types
    (string?, MyClass?) and never use the null-forgiveness '!' operator.
    Value-type nullables (int?, SKRect?, MyEnum?) are fine and are used
    throughout.
  - No <ImplicitUsings> and no global usings: every file lists its own using
    directives, System.* first.
  - File-scoped namespaces only (namespace X;), never block-scoped.
  - <GenerateDocumentationFile> is ON - XML doc comments are required on
    public and protected members. Fix CS1591 at the source; <NoWarn> is
    forbidden.
  - The two SK* type families (CodeBrix.SkiaSvg.ShimSkiaSharp.* and
    SkiaSharp.*) coexist in many files. Where both are in scope, the source
    qualifies the SkiaSharp side explicitly (SkiaSharp.SKPicture,
    SkiaSharp.SKCanvas). Keep doing that - an unqualified SKPicture in this
    codebase means the shim type.
  - Tests: xUnit v3, test files named <ClassUnderTest>Tests.cs, method names
    in snake_case or MemberName_snake_case_description, multi-statement tests
    carrying //Arrange //Act //Assert comments, and
    TestContext.Current.CancellationToken passed to every cancellable call.
  - The library project carries the canonical date-stamped version block; do
    not replace it with a literal <Version>.


NOTES
=====
  - SKSvg is split across six partial-class files by feature area
    (.Model, .HitTest, .SceneGraph, .NativeComposition, .Interaction,
    .AnimationLayers). Add new entry-point members to the file that owns the
    feature rather than growing SKSvg.Model.cs.
  - SKSvg's public `object Sync { get; }` is the instance lock. Every method
    that reads or replaces the rendered picture takes it, and the draw path
    uses Monitor.Wait/PulseAll to let disposal wait for in-flight draws.
    Preserve that discipline in new members that touch Picture, Model or the
    retained scene graph.
  - The retained scene graph is cached and invalidated by a dirty flag;
    TryEnsureRetainedSceneGraph recompiles through SvgSceneRuntime.TryCompile
    and stores null on failure rather than throwing.
  - tests/CodeBrix.SkiaSvg.Tests/Model/ArchitectureGuardTests.cs is a
    structural guard: it scans every .cs file under src/ and fails if any of
    them references the retired CodeBrix.SkiaSvg.Model.Drawables namespace. If
    a refactor trips it, fix the structure rather than the guard.
  - The AI-agent pointer stubs at the repo root (AGENTS.md, CLAUDE.md,
    .clinerules, .cursorrules, .cursor/rules/agent-readme.mdc, .windsurfrules,
    .github/copilot-instructions.md, .junie/guidelines.md) all point at
    README-INDEX.txt. Keep them in sync with the canonical family versions;
    they are not per-repo content.


================================================================================
END OF MAINTAINER-README
