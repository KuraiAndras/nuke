// Copyright 2021 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.IO;
using VerifyXunit;
using Xunit;

namespace Nuke.GlobalTool.Tests
{
    [UsesVerify]
    public class CakeConversionTests
    {
        private static AbsolutePath RootDirectory => Constants.TryGetRootDirectoryFrom(EnvironmentInfo.WorkingDirectory);

        [Theory]
        [MemberData(nameof(CakeFileNames))]
        public Task Test(string fileName)
        {
            var converted = Program.GetConvertedContent(File.ReadAllText(CakeScriptsDirectory / fileName));
            return Verifier.Verify(converted)
                .UseDirectory(CakeScriptsDirectory)
                .UseFileName(Path.GetFileNameWithoutExtension(fileName))
                .UseExtension("cs");
        }

        private static AbsolutePath CakeScriptsDirectory => RootDirectory / "source" / "Nuke.GlobalTool.Tests" / "cake-scripts";

        public static IEnumerable<object[]> CakeFileNames
            => CakeScriptsDirectory.GlobFiles("*.cake").Select(x => new object[] { Path.GetFileName(x) });
    }
}
