using System;
using System.Linq;
using nvboost.NVML;
using nvboost.NVML.NvmlTypes;
using nvboost.ViewModels;
using Newtonsoft.Json;
using nvboost.Models.Exceptions;

namespace nvboost.Models;

public class OcProfile
{
    public OcProfile(string name,uint gpuClockOffset, uint memClockOffset, uint powerLimitMw, FanCurve? fanCurve)
    {
        Name = name;
        GPUClockOffset = gpuClockOffset;
        MemClockOffset = memClockOffset;
        PowerLimitMw = powerLimitMw;
        _fanCurveName = fanCurve != null ? fanCurve.Name : "";
    }

    [JsonConstructor]
    public OcProfile(string name,uint gpuClockOffset, uint memClockOffset, uint powerLimitMw, string fanCurveName)
    {
        Name = name;
        GPUClockOffset = gpuClockOffset;
        MemClockOffset = memClockOffset;
        PowerLimitMw = powerLimitMw;
        _fanCurveName = fanCurveName;
    }
    public string Name { get; set; }
    public uint GPUClockOffset { get; set; }
    public uint MemClockOffset { get; set; }
    
    public uint PowerLimitMw { get; set; }
    
    [JsonIgnore]
    public FanCurve? FanCurve => String.IsNullOrEmpty(_fanCurveName) ? null : MainWindowViewModel.FanCurvesList.First(x => x.Name == _fanCurveName).BaseFanCurve;


    [JsonProperty("fanCurveName")]
    private string _fanCurveName;

    public bool Apply(NvmlGPU targetGPU)
    {
        try
        {
            var r1 = targetGPU.SetClockOffset(NvmlClockType.NVML_CLOCK_GRAPHICS, NvmlPStates.NVML_PSTATE_0,
                (int)GPUClockOffset);
            var r2 = targetGPU.SetClockOffset(NvmlClockType.NVML_CLOCK_MEM, NvmlPStates.NVML_PSTATE_0,
                (int)MemClockOffset);
            var r3 = targetGPU.SetPowerLimit(PowerLimitMw);

            if (FanCurve != null)
                targetGPU.ApplyFanCurve(FanCurve);

            Console.WriteLine(r1.ToString() + r2 + r3);
            return r1 == NvmlReturnCode.NVML_SUCCESS && r2 == NvmlReturnCode.NVML_SUCCESS &&
                   r3 == NvmlReturnCode.NVML_SUCCESS;
        }
        catch (SudoPasswordExpiredException)
        {
            throw;
        }
    }
    
    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static OcProfile? FromJson(string json)
    {
        return JsonConvert.DeserializeObject<OcProfile>(json);
    }
}