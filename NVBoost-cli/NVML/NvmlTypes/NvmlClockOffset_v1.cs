namespace nvboost_cli.NVML.NvmlTypes;

public struct NvmlClockOffset_v1
{
    public NvmlClockOffset_v1()
    {
        
        
        
        
        Version = 16777240;
    }

    public uint Version;
    public NvmlClockType Type;
    public NvmlPStates PState;
    public int ClockOffsetMHz;
    public int MinClockOffsetMHz;
    public int MaxClockOffsetMHz;

    public override string ToString()
    {
        return $"Version: " + Version +
               "\nType: " + Type +
               "\nPState: " + PState +
               "\nClockOffsetMHz: " + ClockOffsetMHz +
               "\nMinClockOffsetMHz: " + MinClockOffsetMHz +
               "\nMaxClockOffsetMHz: " + MaxClockOffsetMHz;
    }
    
}