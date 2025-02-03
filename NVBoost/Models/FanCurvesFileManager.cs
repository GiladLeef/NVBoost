using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using nvboost.ViewModels;

namespace nvboost.Models;

public static class FanCurvesFileManager
{
    public static List<FanCurve> GetFanCurves(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine("File not found: " + path + ", it will be created when you save a new curve.");
            return [];
        }
        
        try
        {
            
            var deserialized = JsonConvert.DeserializeObject<List<FanCurve>>(File.ReadAllText(path));
            if (deserialized == null)
            {
                Console.WriteLine("Error loading file " + path);
                return [];
            }
            
            Console.WriteLine("Successfully loaded "+path);
            return deserialized;
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine("Error loading file " + path + ":\n" + ex);
        }
        catch (JsonException ex)
        {
            Console.WriteLine("Invalid "+ path + " file:\n" + ex);
            
        }

        return [];
    }

    public static void SaveFanCurves(string path,IEnumerable<FanCurve> fanCurves)
    {
        File.WriteAllText(path, JsonConvert.SerializeObject(fanCurves, Formatting.Indented));
    }
    
    public static void SaveFanCurves(string path,IEnumerable<FanCurveViewModel> fanCurves)
    {
        File.WriteAllText(path, JsonConvert.SerializeObject(fanCurves, Formatting.Indented));
    }
    
    public static async Task SaveFanCurvesAsync(string path,IEnumerable<FanCurve> fanCurves)
    {
        await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(fanCurves, Formatting.Indented));
    }
    
    public static async Task SaveFanCurvesAsync(string path,IEnumerable<FanCurveViewModel> fanCurves)
    {
        await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(fanCurves, Formatting.Indented));
    }
}