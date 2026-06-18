using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace CodeBrix.SkiaSvg.Tests.Common; //Was previously: namespace Svg.Skia.UnitTests.Common;

// Gates pixel-comparison theories to the platforms where the bundled reference
// PNGs are valid. macOS is the canonical baseline; Linux is additionally enabled
// (see LinuxTestGate -- the resvg suite runs there, skipping a known set of
// text-rasterization divergences). Other platforms (e.g. Windows) are skipped.
// The class name is retained from the upstream Svg.Skia port for minimal churn.
public sealed class OSXTheory : TheoryAttribute
{
    public OSXTheory(
        string message = "macOS/Linux only theory",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return;
        }

        Skip = message;
    }
}
