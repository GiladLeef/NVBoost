using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DynamicData.Binding;
using nvboost.Models;
using nvboost.ViewModels;
using ReactiveUI;

namespace nvboost.Views;

public partial class SudoPasswordRequestWindow  : ReactiveWindow<SudoPasswordRequestWindowViewModel>
{
    public SudoPasswordRequestWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
        
        if (Design.IsDesignMode) return;
            
        this.WhenActivated(action => action(ViewModel!.SavePasswordCommand.Subscribe(Close)));

    }

}