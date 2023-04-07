using System.Text.RegularExpressions;
using UnityEditor;

internal sealed class GeneratedCsprojPostprocessor : AssetPostprocessor
{
    // Unity currently emits Runtime/Plugins DLLs as Roslyn analyzers in generated
    // csproj files. These are normal runtime references, not analyzers, and the
    // mismatch causes netstandard analyzer warnings in IDEs.
    private static readonly Regex RuntimePluginAnalyzerRegex = new(
        @"^\s*<Analyzer Include=""[^""]*Packages[\\/]+com\.mediafrontjapan\.scip\.unity-inputsystem[\\/]+Runtime[\\/]+Plugins[\\/]+[^""]+\.dll""\s*/>\r?\n?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

    public static string OnGeneratedCSProject(string path, string content)
    {
        return RuntimePluginAnalyzerRegex.Replace(content, string.Empty);
    }
}
