using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace CodeBrix.SkiaSvg.Tests.Common; //Was previously: namespace Svg.Skia.UnitTests.Common;

public sealed class OSXTheory : TheoryAttribute
{
    public OSXTheory(
        string message = "macOS only theory",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        Skip = message;
    }
}
