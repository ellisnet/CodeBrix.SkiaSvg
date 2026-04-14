using System;
using System.IO;
using System.Linq;
using Xunit;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg.Tests.Model; //Was previously: namespace Svg.Model.UnitTests;

public class ArchitectureGuardTests
{
    [Fact]
    public void ProductionCode_DoesNotReferenceDrawableNamespace()
    {
        var repoRoot = FindRepoRoot();
        var srcRoot = Path.Combine(repoRoot, "src");

        var offenders = Directory.EnumerateFiles(srcRoot, "*.cs", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(repoRoot, path).Replace('\\', '/'))
            .Where(static relativePath => !relativePath.StartsWith("src/CodeBrix.SkiaSvg/Model/Drawables/", StringComparison.Ordinal))
            .Where(relativePath =>
            {
                var fullPath = Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
                var content = File.ReadAllText(fullPath);
                return content.Contains("CodeBrix.SkiaSvg.Model.Drawables", StringComparison.Ordinal);
            })
            .OrderBy(static relativePath => relativePath, StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            "Unexpected production references to CodeBrix.SkiaSvg.Model.Drawables were found: "
            + string.Join(", ", offenders));
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "CodeBrix.SkiaSvg.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the CodeBrix.SkiaSvg repository root.");
    }
}
