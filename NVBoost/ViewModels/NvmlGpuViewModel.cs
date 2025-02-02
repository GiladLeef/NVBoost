using nvboost.NVML;

namespace nvboost.ViewModels;

public class NvmlGpuViewModel : ViewModelBase
{
    private readonly NvmlGpu _nvmlGpu;

    public NvmlGpuViewModel(NvmlGpu nvmlGpu)
    {
        _nvmlGpu = nvmlGpu;
    }
    
    public uint GpuTemperature { private set; get; }
    public uint GpuPowerUsage { private set; get; }
    
    
}