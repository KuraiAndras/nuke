// Copyright 2021 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Nuke.GlobalTool.Rewriting.Cake;

namespace Nuke.GlobalTool
{
    partial class Program
    {
        [UsedImplicitly]
        public static int CakeConvert(string[] args, [CanBeNull] AbsolutePath rootDirectory, [CanBeNull] AbsolutePath buildScript)
        {
            Logger.Warn(
                new[]
                {
                    "Converting .cake files is a best effort approach using syntax rewriting.",
                    "Compile errors are to be expected, however, the following elements are currently covered:",
                    "  - Target definitions",
                    "  - Default targets",
                    "  - Parameter declarations",
                    "  - Absolute paths",
                    "  - Globbing patterns",
                    "  - Tool invocations (dotnet CLI, SignTool)",
                }.JoinNewLine());

            Logger.Normal();
            if (!UserConfirms("Continue?"))
                return 0;
            Logger.Normal();

            if (buildScript == null &&
                UserConfirms("Should a NUKE project be created for better results?"))
            {
                Setup(args, rootDirectory: null, buildScript: null);
            }

            static string GetOutputDirectory(string file)
                => Path.GetDirectoryName(File.Exists(EnvironmentInfo.WorkingDirectory / CurrentBuildScriptName)
                    ? GetConfiguration(EnvironmentInfo.WorkingDirectory / CurrentBuildScriptName, evaluate: true)[BUILD_PROJECT_FILE]
                    : file);

            static string GetOutputFile(string file)
                => (AbsolutePath) GetOutputDirectory(file) / Path.GetFileNameWithoutExtension(file).Capitalize() + ".cs";

            GetCakeFiles().ForEach(x => File.WriteAllText(path: GetOutputFile(x), contents: GetConvertedContent(File.ReadAllText(x))));

            return 0;
        }

        [UsedImplicitly]
        public static int CakeClean(string[] args, [CanBeNull] AbsolutePath rootDirectory, [CanBeNull] AbsolutePath buildScript)
        {
            var cakeFiles = GetCakeFiles().ToList();
            Logger.Info("Found .cake files:");
            cakeFiles.ForEach(x => Logger.Normal($"  - {x}"));

            if (UserConfirms("Delete?"))
                cakeFiles.ForEach(FileSystemTasks.DeleteFile);

            return 0;
        }

        private static IEnumerable<AbsolutePath> GetCakeFiles()
        {
            return Constants.TryGetRootDirectoryFrom(EnvironmentInfo.WorkingDirectory).NotNull().GlobFiles("**/*.cake");
        }

        internal static string GetConvertedContent(string content)
        {
            var options = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None, SourceCodeKind.Script);
            var syntaxTree = CSharpSyntaxTree.ParseText(content, options);
            return new CSharpSyntaxRewriter[]
                   {
                       new RemoveUsingDirectivesRewriter(),
                       new RenameFieldIdentifierRewriter(),
                       new ParameterRewriter(),
                       new AbsolutePathRewriter(),
                       new RegularFieldRewriter(),
                       new TargetDefinitionRewriter(),
                       new InvocationRewriter(),
                       new MemberAccessRewriter(),
                       new IdentifierNameRewriter(),
                       new ToolInvocationRewriter(),
                       new ClassRewriter(),
                       new FormattingRewriter()
                   }.Aggregate(syntaxTree.GetRoot(), (root, rewriter) => rewriter.Visit(root.NormalizeWhitespace(elasticTrivia: true)))
                .ToFullString();
        }
    }
}
