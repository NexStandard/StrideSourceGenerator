namespace StrideSourceGenerator.NexAPI;
internal interface IContentModeInfo
{
    public string TempVariable { get; }
    public string GenerationInvocation { get; }
    public bool NeedsFinalAssignment { get; }
}
