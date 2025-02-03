using nvboost.NVML;

namespace nvboost.ViewModels;

public class NvmlGPUViewModel : ViewModelBase
{
    private readonly NvmlGPU _nvmlGPU;

    public NvmlGPUViewModel(NvmlGPU nvmlGPU)
    {
        _nvmlGPU = nvmlGPU;
    }
    
    public uint GPUTemperature { private set; get; }
    public uint GPUPowerUsage { private set; get; }
}