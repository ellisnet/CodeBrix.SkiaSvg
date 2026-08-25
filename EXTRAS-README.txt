================================================================================
EXTRAS-README: CodeBrix.SkiaSvg
Samples, tools and other content in this repository that is not part of a NuGet
package
================================================================================

This repository ships no sample applications, demos or command-line tools.
Exactly one project is packable - src/CodeBrix.SkiaSvg - and everything else
listed below exists only to test or document it. None of it is included in the
CodeBrix.SkiaSvg.MitLicenseForever package.

For runnable, compilable usage of the library, read the test project: the
"WORKING EXAMPLES ON GITHUB" section of AGENT-README.txt maps each feature area
to the test file that exercises it.


TEST PROJECT
============
    tests/CodeBrix.SkiaSvg.Tests/

The only non-package project in the solution. xUnit v3; run it with
`dotnet test CodeBrix.SkiaSvg.slnx` from the repo root. Details on platform
gating and prerequisites are in MAINTAINER-README.txt.


TEST ASSET SETS
===============
These folders are copied to the test output directory by the test project and
are used by the tests; they are not shipped anywhere.

    tests/CodeBrix.SkiaSvg.Tests/Assets/Svg/
        Hand-written SVG files used by the loading, hit-testing, scene-graph
        and animation tests.

    tests/CodeBrix.SkiaSvg.Tests/Assets/Fonts/
        TrueType fonts bundled so that text tests do not depend on whatever
        fonts the host machine happens to have installed.

    tests/CodeBrix.SkiaSvg.Tests/Assets/resources/
        SVG resources referenced from other SVG test files (an SVG-font
        document used by the SVG-font rendering tests).

    tests/CodeBrix.SkiaSvg.Tests/TestAssets/Android/
        Android platform XML used by the VectorDrawable loader and
        Android-spec tests. tests/CodeBrix.SkiaSvg.Tests/TestAssets/
        THIRD-PARTY-NOTICES.txt records where that material came from.

    tests/CodeBrix.SkiaSvg.Tests/ChromeReference/{resvg,W3C}/
        Browser-rendered reference PNGs, kept alongside the corpora they
        correspond to for visual comparison when investigating a rendering
        difference.


VENDORED THIRD-PARTY TEST CORPORA
=================================
    externals/

Two third-party SVG rendering test suites are vendored into this repository so
that a plain clone can run the full test suite with no fetch or setup step:

    externals/resvg/                     an SVG rendering test corpus
                                         (MPL-2.0)
    externals/W3C_SVG_11_TestSuite/      the W3C SVG 1.1 Test Suite
                                         (W3C Document License)

They are pinned to the exact upstream commits that the forked upstream release
referenced, so their reference PNGs match the comparison thresholds in the test
code. externals/README.md records the upstream repositories, the pinned commit
hashes, the licenses and the on-disk layout the tests expect.

This is vendored upstream content, not project source: do not index, refactor,
reformat or reorganize it, and do not treat a file under externals/ as
something this repository authored. If it ever has to be regenerated,
re-download each upstream repository at its pinned commit and lay it out at the
same path.


LOOSE SAMPLE FILES
==================
    tests/Tests/Sign in.svg
    tests/Tests/Sign in.png

An SVG and its rendered PNG kept next to the test project as a visual
reference. They are not referenced by any csproj and are not run by anything.


================================================================================
END OF EXTRAS-README
