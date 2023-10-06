using StrideSourceGenerator.NexAPI;

namespace StrideSourceGenerator.ModeInfos.Yaml;
internal class AssignModeInfo : IContentModeInfo
{
    public string TempVariable { get; }
    public string GenerationInvocation { get; }
    public bool NeedsFinalAssignment { get; }
}
