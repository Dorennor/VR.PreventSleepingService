namespace VR.PowerUtilities
{
    [Flags]
    public enum ExecutionState : uint
    {
        EsAwayModeRequired = 0x00000040,
        EsContinuous = 0x80000000,
        EsDisplayRequired = 0x00000002,
        EsSystemRequired = 0x00000001,

        [Obsolete("Legacy flag, should not be used.")]
        EsUserPresent = 0x00000004
    }
}