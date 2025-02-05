using System;
using System.Reactive;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using nvboost.Models;

namespace nvboost.ViewModels;

public partial class SudoPasswordRequestWindowViewModel : ViewModelBase
{

    public ReactiveCommand<Unit, SudoPassword> SavePasswordCommand { get; }

    [ObservableProperty] private string _passwordBoxText = "";


    public SudoPasswordRequestWindowViewModel()
    {

        SavePasswordCommand = ReactiveCommand.Create(() => new SudoPassword(PasswordBoxText));
    }
}