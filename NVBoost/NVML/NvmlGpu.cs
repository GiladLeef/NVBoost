using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using nvboost.Models;
using nvboost.Models.Exceptions;
using nvboost.NVML.NvmlTypes;
using nvboost.ViewModels;

namespace nvboost.NVML;

public class NvmlGPU : INotifyPropertyChanged
{
    private const uint MAX_NAME_LENGTH = 100;

    private IntPtr _handle;

    private CancellationTokenSource _fanCurveTaskCancellationTokenSource = new();

    public uint DeviceIndex { get; private set; }

    public string Name { get; }


    public NvmlGPU(uint deviceIdx)
    {
        DeviceIndex = deviceIdx;
        var r = NvmlWrapper.nvmlDeviceGetHandleByIndex(deviceIdx, out _handle);
        if (r != NvmlReturnCode.NVML_SUCCESS)
        {
            throw new Exception($"Unable to get device by handle: {r.ToString()}");
        }

        var name = new StringBuilder();
        r = NvmlWrapper.nvmlDeviceGetName(_handle, name, MAX_NAME_LENGTH);
        if (r != NvmlReturnCode.NVML_SUCCESS)
        {
            throw new Exception($"Unable to get device name: {r.ToString()}");
        }

        Name = name.ToString();

        for (uint i = 0; i < GetFanCount().Item2; i++)
        {
            _nvmlGPUFans.Add(new NvmlGPUFan(this, i));
        }

        Task.Run(() =>
        {
            while (true)
            {
                Thread.Sleep(500);
                UpdateProperties();
            }
        });
    }

    private List<NvmlGPUFan> _nvmlGPUFans = new();
    public IReadOnlyList<NvmlGPUFan> FansList => _nvmlGPUFans;

    public FanCurve? AppliedFanCurve { get; private set; }

    public uint GPUTemperature => GetTemperature().Item2;
    public uint GPUPowerUsage => (uint)(GetPowerUsage().Item2 / 1000);

    public NvmlPStates GPUPState => GetPState().Item2;

    public uint GPUClockCurrent => GetCurrentClock(NvmlClockType.NVML_CLOCK_GRAPHICS).Item2;
    public uint MemClockCurrent => GetCurrentClock(NvmlClockType.NVML_CLOCK_MEM).Item2;
    public uint SmClockCurrent => GetCurrentClock(NvmlClockType.NVML_CLOCK_SM).Item2;
    public uint VideoClockCurrent => GetCurrentClock(NvmlClockType.NVML_CLOCK_VIDEO).Item2;

    public uint PowerLimitCurrentMw => GetPowerLimitCurrent().Item2;
    public uint PowerLimitMinMw => GetPowerLimitConstraints().Item2;
    public uint PowerLimitMaxMw => GetPowerLimitConstraints().Item3;
    public uint PowerLimitDefaultMw => GetPowerLimitDefault().Item2;

    public double PowerLimitCurrentW => GetPowerLimitCurrent().Item2 / 1000d;
    public double PowerLimitMinW => GetPowerLimitConstraints().Item2 / 1000d;
    public double PowerLimitMaxW => GetPowerLimitConstraints().Item3 / 1000d;
    public double PowerLimitDefaultW => GetPowerLimitDefault().Item2 / 1000d;

    public ulong MemoryTotal => GetMemoryUsage().Item2.Total;
    public ulong MemoryFree => GetMemoryUsage().Item2.Free;
    public ulong MemoryUsed => GetMemoryUsage().Item2.Used;

    public NvmlUtilization GPUUtilization => GetUtilization().Item2;

    public uint UtilizationCore => GetUtilization().Item2.gpu;


    public string MemoryUsageString => $"{MemoryUsed / 1000000}MB/{MemoryTotal / 1000000}MB (Free: {MemoryFree / 1000000}MB)";
    public uint TemperatureThresholdShutdown => GetTemperatureThreshold(NvlmTemperatureThreshold.NVML_TEMPERATURE_THRESHOLD_SHUTDOWN).Item2;
    public uint TemperatureThresholdSlowdown => GetTemperatureThreshold(NvlmTemperatureThreshold.NVML_TEMPERATURE_THRESHOLD_SLOWDOWN).Item2;
    public uint TemperatureThresholdThrottle => GetTemperatureThreshold(NvlmTemperatureThreshold.NVML_TEMPERATURE_THRESHOLD_GPU_MAX).Item2;

    public (NvmlReturnCode, NvmlUtilization) GetUtilization()
    {
        var r = NvmlWrapper.nvmlDeviceGetUtilizationRates(_handle, out NvmlUtilization u);
        return (r, u);
    }

    public void ApplyFanCurve(FanCurve fanCurve)
    {
        AppliedFanCurve = fanCurve;
        try
        {
            RunFanProcess(fanCurve);
        }
        catch (SudoPasswordExpiredException)
        {
            throw;
        }
    }

    public bool ApplySpeedToAllFans(uint speed)
    {
        try
        {
            var result = RunSudoCliCommand($"-fs {speed}");
            return true;
        }
        catch (SudoPasswordExpiredException)
        {
            throw;
        }

    }

    public bool ApplyPolicyToAllFans(NvmlFanControlPolicy policy)
    {
        bool result = true;
        foreach (var f in FansList)
            result &= f.SetPolicy(policy);
        return result;
    }

    public bool ApplyAutoSpeedToAllFans()
    {
        try
        {
            var result = RunSudoCliCommand($"-afs");
            return true;
        }
        catch (SudoPasswordExpiredException)
        {
            throw;
        }
    }

    public (NvmlReturnCode, uint) GetTemperature()
    {
        var r = NvmlWrapper.nvmlDeviceGetTemperature(_handle, NvmlTemperatureSensors.NVML_TEMPERATURE_GPU, out uint t);
        return (r, t);
    }


    public (NvmlReturnCode, NvmlMemory) GetMemoryUsage()
    {
        var r = NvmlWrapper.nvmlDeviceGetMemoryInfo(_handle, out NvmlMemory m);
        return (r, m);
    }

    public (NvmlReturnCode, NvmlPStates) GetPState()
    {
        var r = NvmlWrapper.nvmlDeviceGetPerformanceState(_handle, out NvmlPStates p);
        return (r, p);
    }

    public (NvmlReturnCode, NvmlClockOffset_v1) GetClockOffset(NvmlClockType clockType, NvmlPStates pState)
    {
        var clockOffset = new NvmlClockOffset_v1()
        {
            Type = clockType,
            PState = pState
        };

        var r = NvmlWrapper.nvmlDeviceGetClockOffsets(_handle, ref clockOffset);
        return (r, clockOffset);
    }

    public (NvmlReturnCode, uint) GetCurrentClock(NvmlClockType type)
    {


        var r = NvmlWrapper.nvmlDeviceGetClockInfo(_handle, type, out uint c);
        return (r, c);
    }

    public NvmlReturnCode SetClockOffset(NvmlClockType clockType, NvmlPStates pState, int clockOffsetMhz)
    {

        try
        {
            switch (clockType)
            {
                case NvmlClockType.NVML_CLOCK_GRAPHICS:
                    RunSudoCliCommand($"-c {clockOffsetMhz}");
                    break;
                case NvmlClockType.NVML_CLOCK_MEM:
                    RunSudoCliCommand($"-m {clockOffsetMhz}");
                    break;
            }


            Console.WriteLine(" set clock offset: ");

            return NvmlReturnCode.NVML_SUCCESS;
        }
        catch (SudoPasswordExpiredException)
        {
            throw;
        }
    }

    public (NvmlReturnCode, uint) GetPowerLimitCurrent()
    {
        return (NvmlWrapper.nvmlDeviceGetPowerManagementLimit(_handle, out uint limit), limit);
    }

    public (NvmlReturnCode, uint, uint) GetPowerLimitConstraints()
    {
        return (NvmlWrapper.nvmlDeviceGetPowerManagementLimitConstraints(_handle, out uint minLimit, out uint maxLimit), minLimit, maxLimit);
    }

    public (NvmlReturnCode, uint) GetPowerLimitDefault()
    {
        return (NvmlWrapper.nvmlDeviceGetPowerManagementDefaultLimit(_handle, out uint limit), limit);
    }

    public (NvmlReturnCode, uint) GetPowerUsage()
    {
        return (NvmlWrapper.nvmlDeviceGetPowerUsage(_handle, out uint power), power);
    }

    public NvmlReturnCode SetPowerLimit(uint limitMw)
    {
        try
        {
            var result = RunSudoCliCommand($"-p {limitMw}");
            Console.WriteLine(" set power limit: ");

            return NvmlReturnCode.NVML_SUCCESS;
        }
        catch (SudoPasswordExpiredException)
        {
            throw;
        }

    }

    public NvmlReturnCode SetFanControlPolicy(uint fanId, NvmlFanControlPolicy policy)
    {
        return (NvmlWrapper.nvmlDeviceSetFanControlPolicy(_handle, fanId, policy));
    }

    public NvmlReturnCode SetFanSpeed(uint fanId, uint speed)
    {
        return (NvmlWrapper.nvmlDeviceSetFanSpeed_v2(_handle, fanId, speed));
    }

    public (NvmlReturnCode, uint) GetFanCount()
    {
        return (NvmlWrapper.nvmlDeviceGetNumFans(_handle, out uint fanCount), fanCount);
    }

    public (NvmlReturnCode, uint) GetFanTargetSpeed(uint fanId)
    {
        return (NvmlWrapper.nvmlDeviceGetTargetFanSpeed(_handle, fanId, out uint tSpeed), tSpeed);
    }

    public (NvmlReturnCode, uint) GetFanCurrentSpeed(uint fanId)
    {
        return (NvmlWrapper.nvmlDeviceGetFanSpeed_v2(_handle, fanId, out uint speed), speed);
    }

    public (NvmlReturnCode, uint) GetTemperatureThreshold(NvlmTemperatureThreshold temperatureThresholdType)
    {
        return (NvmlWrapper.nvmlDeviceGetTemperatureThreshold(_handle, temperatureThresholdType, out uint temperatureThreshold), temperatureThreshold);
    }

    private (int, string) RunCommandWithBash(string command)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = command,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
            throw new InvalidOperationException("Failed to start process.");

        process.WaitForExit();
        var output = process.StandardOutput.ReadToEnd();

        return (process.ExitCode, output);
    }
    private (int, string) RunCliCommand(string args, string file = "/usr/local/bin/nvboost")
    {
        var psi = new ProcessStartInfo
        {
            FileName = file,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null)
            throw new InvalidOperationException("Failed to start process.");

        process.WaitForExit();
        var output = process.StandardOutput.ReadToEnd();

        return (process.ExitCode, output);
    }

    private Process RunSudoCliCommand(string args, string file = "/usr/local/bin/nvboost", bool waitForExit = true)
    {
        if (SudoPasswordManager.CurrentPassword?.Password == null || SudoPasswordManager.CurrentPassword.IsExpired)
        {
            throw new SudoPasswordExpiredException("Sudo password is expired");
        }

        var psi = new ProcessStartInfo
        {
            FileName = "/usr/bin/bash",
            Arguments = $"-c \"/usr/bin/sudo -S {file} -g {DeviceIndex} {args}\"",
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Console.WriteLine("Executing: " + psi.FileName + " " + psi.Arguments);

        var process = Process.Start(psi);
        if (process == null)
            throw new InvalidOperationException("Failed to start process.");

        process.StandardInput.WriteLine(SudoPasswordManager.CurrentPassword.Password);

        if (waitForExit)
            process.WaitForExit();

        Console.WriteLine(process.Id);
        return process;
    }

    private void RunFanProcess(FanCurve fanCurve)
    {
        if (Program.FanCurveProcess is not null)
            Program.FanCurveProcess.Kill();

        try
        {
            var tempPath = Program.DefaultDataPath + "/temp/fanCurve-" + fanCurve.Name +
                           DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
            File.WriteAllText(tempPath, JsonConvert.SerializeObject(fanCurve, Formatting.None));

            Program.FanCurveProcess = RunSudoCliCommand($"-fp {tempPath}", waitForExit: false);
        }
        catch (SudoPasswordExpiredException)
        {
            throw;
        }
    }

    private void UpdateProperties()
    {

        foreach (var p in GetType().GetProperties())
        {
            OnPropertyChanged(p.Name);
        }


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