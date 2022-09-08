// Copyright 2021 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using JetBrains.Annotations;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Nuke.Common.CI.AzurePipelines.Configuration
{
    [PublicAPI]
    public class AzurePipelinesPool : ConfigurationEntity
    {
        [CanBeNull]
        public string Name { get; set; }
        public string[] Demands { get; set; } = new string[0];
        public AzurePipelinesImage? VmImage { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            if (Name != null)
            {
                writer.WriteLine($"name: '{Name}'");
            }

            if (Demands.Length != 0)
            {
                using (writer.WriteBlock("demands"))
                {
                    Demands.ForEach(d => writer.WriteLine($"- {d}"));
                }
            }

            if (VmImage != null)
            {
                writer.WriteLine($"vmImage: {VmImage}");
            }
        }
    }
}
