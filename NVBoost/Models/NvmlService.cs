using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using nvboost.NVML;
using nvboost.ViewModels;

namespace nvboost.Models;

public class NvmlService
{
    List<NvmlGPU> _gpuList = new();


    CancellationTokenSource _cts = new();

    public IReadOnlyList<NvmlGPU> GPUList => _gpuList;


    public NvmlService()
    {
        Initialize();
    }

    public void Shutdown()
    {
        _cts.Cancel();
        NvmlWrapper.nvmlShutdown();
        _gpuList.Clear();


        Console.WriteLine("NvmlService destroyed");
    }

    public void Initialize()
    {
        Console.WriteLine("NvmlInit: " + NvmlWrapper.nvmlInit());

        Console.WriteLine("NvmlDeviceGetCount: " + NvmlWrapper.nvmlDeviceGetCount(out uint deviceCount));

        for (uint i = 0; i < deviceCount; i++)
        {
            var g = new NvmlGPU(i);
            _gpuList.Add(g);

        }

        StartFanCurveUpdaterThread();

        Console.WriteLine("NvmlService initialized");
    }

    private void StartFanCurveUpdaterThread()
    {
        var t = new Thread(() =>
        {
            while (_cts.Token.IsCancellationRequested == false)
            {
                foreach (var g in GPUList)
                {
                    if (g.AppliedFanCurve != null)
                        g.ApplySpeedToAllFans(g.AppliedFanCurve.GPUTempToFanSpeedMap[g.GPUTemperature]);
                }
            }

        });
    }

    ~NvmlService()
    {
        Shutdown();
    }
}