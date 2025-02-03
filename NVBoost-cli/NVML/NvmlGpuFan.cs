using System.ComponentModel;
using System.Runtime.CompilerServices;
using nvboost_cli.NVML.NvmlTypes;

namespace nvboost_cli.NVML;

public class NvmlGPUFan : INotifyPropertyChanged
{
    public NvmlGPUFan(NvmlGPU parentGPU, uint fanId)
    {
        Task.Run(() =>
        {
            while (true)
            {
                Thread.Sleep(500);
                Updater();
            }
        });
        
        ParentGPU = parentGPU;
        FanId = fanId;
    }
    
    public NvmlGPU ParentGPU { get; private set; }
    public uint FanId { get; private set; }
    public uint TargetSpeed => ParentGPU.GetFanTargetSpeed(FanId).Item2;
    public uint CurrentSpeed => ParentGPU.GetFanCurrentSpeed(FanId).Item2;
    public string Name => "Fan"+FanId;

    private void Updater()
    {
        
        
        foreach (var p in GetType().GetProperties())
        {
            OnPropertyChanged(p.Name);
        }
        
        
    }
    
    public bool SetSpeed(uint speed)
    {
        var r= ParentGPU.SetFanSpeed(FanId, speed);
        Console.WriteLine(r);
        return r == NvmlReturnCode.NVML_SUCCESS;
    }

    public bool SetPolicy(NvmlFanControlPolicy policy)
    {
        var r= ParentGPU.SetFanControlPolicy(FanId, policy);
        Console.WriteLine(r);
        return r == NvmlReturnCode.NVML_SUCCESS;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}