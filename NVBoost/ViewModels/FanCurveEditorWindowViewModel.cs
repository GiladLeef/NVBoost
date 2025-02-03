using System.Collections.ObjectModel;
using System.Reactive;
using CommunityToolkit.Mvvm.ComponentModel;
using nvboost.Models;
using ReactiveUI;

namespace nvboost.ViewModels;

public partial class FanCurveEditorWindowViewModel : ViewModelBase
{
    [ObservableProperty] private FanCurveViewModel _currentFanCurve;

    public FanCurveEditorWindowViewModel(FanCurveViewModel fanCurve)
    {
        _currentFanCurve = fanCurve;
        SaveCurveCommand = ReactiveCommand.Create(() =>
        {
            CurrentFanCurve.BaseFanCurve.GenerateGPUTempToFanSpeedMap();
            return CurrentFanCurve;
        });
    }
    public FanCurveEditorWindowViewModel() : this(new FanCurveViewModel(FanCurve.DefaultFanCurve()))
    {
        
    }


    public ReactiveCommand<Unit, FanCurveViewModel> SaveCurveCommand { get; }

    public void CancelCommand()
    {
        
    }

    public void AddPointCommand()
    {
        CurrentFanCurve.BaseFanCurve.CurvePoints.Add(new FanCurvePoint());
    }

    public void RemovePointCommand(FanCurvePoint selectedPoint)
    {
        CurrentFanCurve.BaseFanCurve.CurvePoints.Remove(selectedPoint);
    }
}