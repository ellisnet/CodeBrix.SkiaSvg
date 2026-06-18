using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xunit;

namespace CodeBrix.SkiaSvg.Tests.Common;

// Helpers for running the [OSXTheory] pixel-comparison suites on Linux.
//
// The reference PNGs those suites compare against were rendered on macOS, whose
// Skia text path uses CoreText. On Linux, Skia rasterizes glyphs through
// FreeType, so text edges differ by enough to push some cases past the RMS
// comparison threshold even though the glyph outlines are identical (the test
// fonts are bundled). Non-text rendering (paths, paint, gradients, filters)
// matches, so the resvg suite passes on Linux apart from a known set of
// text-layout cases. The full W3C suite, by contrast, diverges broadly on Linux
// (its baselines are not portable off macOS) and stays macOS-only for now.
//
// Every method here no-ops on non-Linux, so the macOS run is completely
// unaffected: macOS continues to execute every case exactly as before.
internal static class LinuxTestGate
{
    private static bool OnLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    // Skip a single named case when running on Linux because it is a known,
    // text-rasterization-only divergence from the macOS reference render.
    public static void SkipIfKnownLinuxDivergence(string name, ISet<string> knownDivergent)
    {
        if (OnLinux && knownDivergent.Contains(name))
        {
            Assert.Skip(
                $"Known Linux text-rasterization divergence (FreeType vs the macOS CoreText reference) for '{name}'. " +
                "Runs and is asserted on macOS.");
        }
    }

    // Skip an entire suite on Linux because its reference images are not portable
    // off macOS (broad cross-platform divergence, not confined to text).
    public static void SkipSuiteOnLinux(string reason)
    {
        if (OnLinux)
        {
            Assert.Skip(reason);
        }
    }
}
