using UnityEngine;

/// <summary>
/// Parameter object for the rules used to generate 
/// the module's sourcefiles, errorcodes, patch files etc.
/// </summary>
public sealed class RuleParameters : MonoBehaviour
{
    public MonoRandom Random;

    public int SourceFileCount;
    public int ErrorCodeCount;
    public int PatchFileCount;
    public int VersionCount;
    public int ParameterCount;

    public RuleParameters(MonoRandom random, int sourceFileCount, int errorCodeCount,
        int patchFileCount, int versionCount, int parameterCount)
    {
        Random = random;
        SourceFileCount = sourceFileCount;
        ErrorCodeCount = errorCodeCount;
        PatchFileCount = patchFileCount;
        VersionCount = versionCount;
        ParameterCount = parameterCount;
    }
}
