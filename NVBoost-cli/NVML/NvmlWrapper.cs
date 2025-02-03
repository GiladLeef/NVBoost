using System.Runtime.InteropServices;
using System.Text;
using nvboost_cli.NVML.NvmlTypes;

namespace nvboost_cli.NVML;



public static class NvmlWrapper
{
    
#if LINUX
    private const string NVML_DLL = "libnvidia-ml.so";
#elif WINDOWS
    private const string NVML_DLL = "nvml.dll";
#endif
[DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlInit();
    
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlShutdown();
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetCount(out uint deviceCount);
[DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetName(IntPtr device, [MarshalAs(UnmanagedType.LPStr)] StringBuilder name, uint length);

    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetHandleByIndex(uint index, out IntPtr device);
[DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetTemperature(IntPtr device, NvmlTemperatureSensors sensorType, out uint temp);
    
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetUtilizationRates(IntPtr device, out NvmlUtilization utilization);[DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetClockOffsets(IntPtr device, ref NvmlClockOffset_v1 info);
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceSetClockOffsets(IntPtr device, ref NvmlClockOffset_v1 info);
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetPerformanceState(IntPtr device, out NvmlPStates pState);
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetClockInfo(IntPtr device, NvmlClockType type, out uint clock);[DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetPowerManagementLimit(IntPtr device, out uint limit);[DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetPowerUsage(IntPtr device, out uint power);
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetPowerManagementLimitConstraints(IntPtr device, out uint minLimit, out uint maxLimit);
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetPowerManagementDefaultLimit(IntPtr device, out uint defaultLimit);
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceSetFanControlPolicy(IntPtr device,uint fan, NvmlFanControlPolicy policy);[DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetNumFans(IntPtr device, out uint numFans);        
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetTargetFanSpeed(IntPtr device, uint fan, out uint targetSpeed);        [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetFanSpeed_v2(IntPtr device, uint fan, out uint speed);        [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceSetFanSpeed_v2(IntPtr device, uint fan, uint speed);
    
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceSetPowerManagementLimit_v2(IntPtr device, NvmlPowerValue_v2 powerValue);
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceSetPowerManagementLimit(IntPtr device, uint limit);
    [DllImport(NVML_DLL)]
    public static extern NvmlReturnCode nvmlDeviceGetTemperatureThreshold(IntPtr device, NvlmTemperatureThreshold thresholdType, out uint temp);
    
}