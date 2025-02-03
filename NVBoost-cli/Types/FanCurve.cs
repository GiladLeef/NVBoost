using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace nvboost_cli.Types;

public class FanCurve
{
    public string Name { get; set; }
    public ObservableCollection<FanCurvePoint> CurvePoints { get; set; }
    
    [JsonIgnore]
    public uint[] GPUTempToFanSpeedMap { get; private set; } = new uint[101];

    [JsonIgnore]
    public bool NeedsUpdate { get; private set; } = false;
    
    public FanCurve(params FanCurvePoint[] curvePoints) : this("New Curve", curvePoints)
    {
    } 

    [JsonConstructor]
    public FanCurve(string name, FanCurvePoint[] curvePoints)
    {
        Name = name;
        CurvePoints = new(curvePoints);
        GenerateGPUTempToFanSpeedMap();
    }

    public void GenerateGPUTempToFanSpeedMap()
    {
        for (int i = 0; i < CurvePoints.Count-1; i++) 
        {
            for (uint j = CurvePoints[i].Temperature; j <= CurvePoints[i+1].Temperature; j++) 
            {
                GPUTempToFanSpeedMap[j] = MapGPUTempToFanPercent(CurvePoints[i].Temperature, CurvePoints[i + 1].Temperature,CurvePoints[i].FanSpeed,CurvePoints[i+1].FanSpeed,j);
            }
        }
     
    }
    
    public static FanCurve DefaultFanCurve()
    {
        return new FanCurve(
            new FanCurvePoint()
            {
                Temperature = 0,
                FanSpeed = 0
            },
            new FanCurvePoint()
            {
                Temperature = 45,
                FanSpeed = 0
            },
            new FanCurvePoint()
            {
                Temperature = 55,
                FanSpeed = 20
            },
            new FanCurvePoint()
            {
                Temperature = 65,
                FanSpeed = 40
            },
            new FanCurvePoint()
            {
                Temperature = 80,
                FanSpeed = 80
            },
            new FanCurvePoint()
            {
                Temperature = 100,
                FanSpeed = 100
            });
    }

    public static FanCurve FromSetSpeed(uint speed)
    {
        return new FanCurve(
            new FanCurvePoint()
            {
                Temperature = 0,
                FanSpeed = speed
            },
            new FanCurvePoint()
            {
                Temperature = 100,
                FanSpeed = speed
            }
        );
    }    private uint MapGPUTempToFanPercent(uint temp1, uint temp2, uint perc1, uint perc2, uint intemp)
    {
        return perc1 + (intemp-temp1)*(perc2-perc1)/(temp2-temp1);
    }

    public override string ToString()
    {
        var final = "";

        for (int i = 0; i < GPUTempToFanSpeedMap.Length; i++)
        {
            final += $"GPUTemp: {i}, FanSpeed: {GPUTempToFanSpeedMap[i]}\n";
        }

        return final;
    }
    
}