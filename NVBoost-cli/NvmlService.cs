using nvboost_cli.NVML;

namespace nvboost_cli;

public class NvmlService
{
    List<NvmlGPU> _gpuList = new();

    
    public IReadOnlyList<NvmlGPU> GPUList => _gpuList;

    public NvmlService()
    {
        Initialize();   
    }

    public void Shutdown()
    {
        NvmlWrapper.nvmlShutdown();
        _gpuList.Clear();
        
        Console.WriteLine("NvmlService destroyed");
    }

    public void Initialize()
    {
        Console.WriteLine("NvmlInit: " + NvmlWrapper.nvmlInit());

        Console.WriteLine("NvmlDeviceGetCount: "+NvmlWrapper.nvmlDeviceGetCount(out uint deviceCount));

        for (uint i = 0; i < deviceCount; i++)
        {
            var g = new NvmlGPU(i);
            _gpuList.Add(g);
        }

        
        Console.WriteLine("NvmlService initialized");
    }

 
    

    
    ~NvmlService()
    {
        Shutdown();
    }
}