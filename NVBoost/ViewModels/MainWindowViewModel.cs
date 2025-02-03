using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using nvboost.Models;
using nvboost.NVML;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using nvboost.Models.Exceptions;


namespace nvboost.ViewModels;

public partial class MainWindowViewModel : ViewModelBase  

{
    [ObservableProperty] private NvmlGPU? _selectedGPU;
    [ObservableProperty] private NvmlGPUFan? _selectedGPUFan;
    [ObservableProperty] private OcProfile? _selectedOcProfile;
    [ObservableProperty] private FanCurveViewModel? _selectedFanCurve;private bool _autoApplyProfileLoaded = false;

    public void WindowLoadedHandler()
    {
        if (!_autoApplyProfileLoaded && File.Exists(Program.DefaultDataPath + "/AutoApplyProfile.json"))
        {
            var jobj = JObject.Parse(File.ReadAllText(Program.DefaultDataPath + "/AutoApplyProfile.json"));
            var gpuid = jobj["gpu"]?.Value<uint>();
            var profile = (string)jobj["profile"];
                
            
            SelectedGPU = NvmlService.GPUList.FirstOrDefault(x => x.DeviceIndex == gpuid);
            SelectedOcProfile = OcProfilesList.FirstOrDefault(x => x.Name == profile);
            OcProfileApplyCommand();
            
        }  
    }
    
    partial void OnSelectedFanCurveChanged(FanCurveViewModel? value)
    {
        
        SelectedFanCurve!.UpdateSeries();
        
        
    }

    public void SaveAutoApplyProfile(OcProfile profile)
    {
        

        if (SelectedGPU == null)
        {
            Console.WriteLine("No gpu selected.");
            return;
        }
        
        File.WriteAllText(Program.DefaultDataPath + "/AutoApplyProfile.json", $"{{\"profile\":\"{profile.Name}\",\"gpu\":\"{SelectedGPU.DeviceIndex}\"}}");
    }
    private readonly ProfilesFileManager _profilesFileManager=new(Program.DefaultDataPath+"/profiles.json");

    public ObservableCollection<OcProfile> OcProfilesList => _profilesFileManager.LoadedProfiles;


    public static ObservableCollection<FanCurveViewModel> FanCurvesList { get; private set; } = new();


    private void LoadFanCurvesFromFile()
    {
        foreach (var fanCurve in FanCurvesFileManager.GetFanCurves(Program.DefaultDataPath+"/fan_curves.json"))
        {
            FanCurvesList.Add(new FanCurveViewModel(fanCurve));
        }
    }

    public void KillFanCurveProcessCommand( )
    {
        if (Program.FanCurveProcess is null || Program.FanCurveProcess.HasExited)
        {
            Console.WriteLine("Fan curve process not running");
            return;
        }
        Program.FanCurveProcess.Kill();
    }

    public Interaction<NewOcProfileWindowViewModel, OcProfile?> ShowOcProfileDialog { get; }
    public Interaction<FanCurveEditorWindowViewModel, FanCurveViewModel?> ShowFanCurveEditorDialog { get; }
    public Interaction<SudoPasswordRequestWindowViewModel, SudoPassword?> ShowSudoPasswordRequestDialog { get; }

    private uint _selectedFanRadioButton = 0;
    

    private bool FanSpeedSliderVisible => _selectedFanRadioButton == 1;
    
    public ICommand OpenNewProfileWindowCommand { get; private set; }
    public ICommand OpenFanCurveEditorCommand { get; private set; }
    public ICommand OpenSudoPasswordPromptCommand { get; private set; }


    public MainWindowViewModel()
    {
        if (!Directory.Exists(Program.DefaultDataPath))
            Directory.CreateDirectory(Program.DefaultDataPath);
        
        if (!Directory.Exists(Program.DefaultDataPath+"/temp"))
            Directory.CreateDirectory(Program.DefaultDataPath+"/temp");
        
        foreach(var f in Directory.GetFiles(Program.DefaultDataPath+"/temp"))
            File.Delete(f);
        
        LoadFanCurvesFromFile();
        
        
            
        
        ShowOcProfileDialog = new Interaction<NewOcProfileWindowViewModel, OcProfile?>();
        OpenNewProfileWindowCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var ocProfileWindowViewModel = new NewOcProfileWindowViewModel(this);

            var result = await ShowOcProfileDialog.Handle(ocProfileWindowViewModel);
            
            if (result !=null)
                OcProfilesList.Add(result);

            await _profilesFileManager.UpdateProfilesFileAsync();
        });
        
        
        ShowFanCurveEditorDialog = new Interaction<FanCurveEditorWindowViewModel, FanCurveViewModel?>();
        OpenFanCurveEditorCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var fanCurveEditorWindowViewModel = new FanCurveEditorWindowViewModel();

            var result = await ShowFanCurveEditorDialog.Handle(fanCurveEditorWindowViewModel);

            if (result == null)
                return;
            
            if (FanCurvesList.Any(x => x.Name == result.Name))
            {
                
                FanCurveViewModel existingCurve = FanCurvesList.First(x => x.Name == result.Name);
                existingCurve.BaseFanCurve.CurvePoints = result.BaseFanCurve.CurvePoints;
            }
            else
            {
                
                FanCurvesList.Add(result);
            } 
            
            
            await FanCurvesFileManager.SaveFanCurvesAsync(Program.DefaultDataPath+"/fan_curves.json", FanCurvesList.Select(x => x.BaseFanCurve));

            
            

            
        });
        
        ShowSudoPasswordRequestDialog = new Interaction<SudoPasswordRequestWindowViewModel, SudoPassword?>();
        OpenSudoPasswordPromptCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var sudoPasswordRequestWindowViewModel = new SudoPasswordRequestWindowViewModel();

            var result = await ShowSudoPasswordRequestDialog.Handle(sudoPasswordRequestWindowViewModel);
            
            if (result !=null)
                SudoPasswordManager.CurrentPassword = result;

        });
        
        
          
    }
public void DeleteOcProfile()
    {
        if (SelectedOcProfile is not null)
            OcProfilesList.Remove(SelectedOcProfile);
        FanCurvesFileManager.SaveFanCurves(Program.DefaultDataPath+"/fan_curves.json", FanCurvesList.Select(x => x.BaseFanCurve));
    }
    
    public void OcProfileApplyCommand()
    {
        if (SelectedGPU is null)
        {
            Console.WriteLine("No gpu selected!");
            return;
        }


        try
        {
            SelectedOcProfile?.Apply(SelectedGPU);
            _autoApplyProfileLoaded = true;
        }catch (SudoPasswordExpiredException)
        {
            
            OpenSudoPasswordPromptCommand.Execute(null);
        }
    }
    
    public void OcProfileApplyCommand(NvmlGPU? gpu, OcProfile? profile)
    {
        if (gpu is null)
        {
            Console.WriteLine("No gpu selected!");
            return;
        }


        try
        {
            profile?.Apply(gpu);
        }catch (SudoPasswordExpiredException)
        {
            
            OpenSudoPasswordPromptCommand.Execute(null);
        }
    }
    

    bool CanOcProfileApplyCommand()
    {
        return SelectedGPU != null;
    }
    
    public bool FanApplyButtonClick(uint speed)
    {
        if (SelectedGPUFan is null || SelectedGPU is null) return false;

        try
        {
            switch (_selectedFanRadioButton)
            {
                case 0:
                    return SelectedGPU.ApplyAutoSpeedToAllFans();
                case 1:
                    return SelectedGPU.ApplySpeedToAllFans(speed);
                default:
                    return false;
            }
        }catch (SudoPasswordExpiredException)
        {
            
            OpenSudoPasswordPromptCommand.Execute(null);
            return false;
        }
    }

    public void FanRadioButtonClicked(uint id)
    {
        

        _selectedFanRadioButton = id;
        
    }

    public static NvmlService NvmlService { get; set; } = new();

}
