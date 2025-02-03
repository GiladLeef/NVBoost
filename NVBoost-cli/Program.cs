using System.ComponentModel.Design;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using nvboost_cli.NVML;
using nvboost_cli.NVML.NvmlTypes;
using nvboost_cli.Types;

namespace nvboost_cli;

public class Program
{
    [Option(CommandOptionType.SingleValue, Description = "select gpu id", LongName = "gpu", ShortName = "g")]
    public static uint GPUId { get; set; }
    
    [Option(CommandOptionType.NoValue, Description = "list available gpus", LongName = "listGPU")]
    public static bool DoListGPUs { get; set; }
    
    [Option(CommandOptionType.SingleValue, Description = "set core offset mHz", LongName = "coreOffset", ShortName = "c")]
    public static int CoreOffset { get; set; } = -1;
        
    [Option(CommandOptionType.SingleValue, Description = "set mem offset mHz", LongName = "memoryOffset",ShortName = "m")]
    public static int MemoryOffset { get; set; }= -1;
    
    [Option(CommandOptionType.SingleValue, Description = "set power limit in Watts", LongName = "powerLimit",ShortName = "p")]
    public static uint PowerLimit { get; set; }= 0;
    
    [Option(CommandOptionType.SingleValue, Description = "set fan speed", LongName = "fanSpeed",ShortName = "fs")]
    public static int FanSpeed { get; set; }= -1;
    
    [Option(CommandOptionType.NoValue, Description = "enable auto fan speed", LongName = "autoFanSpeed",ShortName = "afs")]
    public static bool AutoFanSpeed { get; set; }= false;
    
    [Option(CommandOptionType.SingleValue, Description = "load a fan speed curve json from the specified path.", LongName = "fanProfile",ShortName = "fp")]
    public static string FanSpeedCurveJson { get; set; }= "";
static NvmlService? _nvmlService;
    NvmlGPU? _SelectedGPU = null;
    
    public static void Main(string[] args)
        => CommandLineApplication.Execute<Program>(args);




    private void OnExecute()
    {
        var cancelTokenSource = new CancellationTokenSource();
        
        _nvmlService = new NvmlService();

        if (DoListGPUs)
        {
            foreach (var g in _nvmlService.GPUList)
            {
                Console.WriteLine("Name: " + g.Name + "\tID: " + g.DeviceIndex);
            }

            return;
        }
        
        
        

        
        foreach (var gpu in _nvmlService.GPUList)
        {
            if (gpu.DeviceIndex == GPUId)
                _SelectedGPU = gpu;
        }

        if (_SelectedGPU == null)
        {
            Console.WriteLine("GPU index not found");
            return;
        }
        
        
        if (CoreOffset >= 0)
            Console.WriteLine(_SelectedGPU.SetClockOffset(NvmlClockType.NVML_CLOCK_GRAPHICS, NvmlPStates.NVML_PSTATE_0, CoreOffset));
        if (MemoryOffset >= 0)
            Console.WriteLine(_SelectedGPU.SetClockOffset(NvmlClockType.NVML_CLOCK_MEM, NvmlPStates.NVML_PSTATE_0, MemoryOffset));
        if (PowerLimit > 0)
            Console.WriteLine(_SelectedGPU.SetPowerLimit(PowerLimit));

        if (FanSpeed >= 0)
        {
            _SelectedGPU.ApplySpeedToAllFans((uint)FanSpeed);
        }

        if (AutoFanSpeed)
            _SelectedGPU.ApplyPolicyToAllFans(NvmlFanControlPolicy.NVML_FAN_POLICY_TEMPERATURE_CONTINOUS_SW);

        if (FanSpeedCurveJson != "")
        {
            var curve = JsonConvert.DeserializeObject<FanCurve>(File.ReadAllText(FanSpeedCurveJson));
            if (curve is null)
            {
                Console.WriteLine("Fan curve not valid.");
                return;
            }
            Thread t = new Thread(() => FanSpeedProfileThread(500,curve,cancelTokenSource.Token));
            t.Start();
        }

    }

    private uint LastFanTemp = 0;
    
    private void FanSpeedProfileThread(int updateDelayMilliseconds, FanCurve fanCurve,CancellationToken cancelToken)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            Thread.Sleep(updateDelayMilliseconds);
            
            if (_SelectedGPU is null || _SelectedGPU.GPUTemperature == LastFanTemp)
            {
                Console.WriteLine("No temp change since last update. skipping");
                continue;
            }
                
            LastFanTemp = _SelectedGPU.GPUTemperature;
            Console.WriteLine($"GPU temp: {_SelectedGPU.GPUTemperature}, Fan Speed: {fanCurve.GPUTempToFanSpeedMap[_SelectedGPU.GPUTemperature]}");
            _SelectedGPU.ApplySpeedToAllFans(fanCurve.GPUTempToFanSpeedMap[_SelectedGPU.GPUTemperature]);
        }
    }

   
}