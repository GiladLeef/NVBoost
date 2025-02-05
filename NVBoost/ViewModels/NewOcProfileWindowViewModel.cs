using System.Collections.ObjectModel;
using System.Reactive;
using CommunityToolkit.Mvvm.ComponentModel;
using nvboost.Models;
using nvboost.NVML;
using ReactiveUI;

namespace nvboost.ViewModels;

public partial class NewOcProfileWindowViewModel : ViewModelBase
{
    MainWindowViewModel _mainWindowViewModel;

    [ObservableProperty] private uint _powerLimitSliderValue;
    [ObservableProperty] private uint _GpuClockOffsetSliderValue;
    [ObservableProperty] private uint _memClockOffsetSliderValue;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private FanCurveViewModel? _selectedFanCurve;
    public NvmlGPU? SelectedGPU => _mainWindowViewModel.SelectedGPU;
    public ObservableCollection<FanCurveViewModel>? FanCurvesList => MainWindowViewModel.FanCurvesList;


    public NewOcProfileWindowViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
        CreateProfileCommand = ReactiveCommand.Create(() => new OcProfile(Name ?? "New Profile", GpuClockOffsetSliderValue, MemClockOffsetSliderValue, PowerLimitSliderValue, SelectedFanCurve?.BaseFanCurve));

    }

    public ReactiveCommand<Unit, OcProfile> CreateProfileCommand { get; }
    public void CancelButtonCommand()
    {

    }
}