using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.SkiaSvg.ShimSkiaSharp;
using Xunit;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg.Tests.ShimSkiaSharp; //Was previously: namespace ShimSkiaSharp.UnitTests;

public class CloneCoverageTests
{
    [Fact]
    public void AllEligibleTypesSupportCloneOrDeepClone()
    {
        var types = typeof(SKPaint).Assembly
            .GetExportedTypes()
            .Where(type => type.Namespace == nameof(ShimSkiaSharp))
            .Where(type => type.IsClass);

        var missing = types
            .Where(type => !SupportsClone(type))
            .Select(type => type.FullName)
            .ToList();

        Assert.True(missing.Count == 0, $"Missing clone support: {string.Join(", ", missing)}");
    }

    private static bool SupportsClone(Type type)
    {
        if (typeof(ICloneable).IsAssignableFrom(type))
        {
            return true;
        }

        return type.GetInterfaces().Any(HasDeepCloneInterface);
    }

    private static bool HasDeepCloneInterface(Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDeepCloneable<>);
}
