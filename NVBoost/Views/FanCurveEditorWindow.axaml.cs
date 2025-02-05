using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using nvboost.Models;
using nvboost.ViewModels;
using ReactiveUI;

namespace nvboost.Views;

public partial class FanCurveEditorWindow : ReactiveWindow<FanCurveEditorWindowViewModel>
{
    public FanCurveEditorWindow()
    {
        InitializeComponent();


        if (Design.IsDesignMode) return;

        this.WhenActivated(action => action(ViewModel!.SaveCurveCommand.Subscribe(Close)));
    }


}