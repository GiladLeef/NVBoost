using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using nvboost.Models;
using nvboost.ViewModels;
using ReactiveUI;


namespace nvboost.Views;

public partial class NewOcProfileWindow : ReactiveWindow<NewOcProfileWindowViewModel>
{
    public NewOcProfileWindow()
    {
        InitializeComponent();
        
        // This line is needed to make the previewer happy (the previewer plugin cannot handle the following line).
        if (Design.IsDesignMode) return;
            
        this.WhenActivated(action => action(ViewModel!.CreateProfileCommand.Subscribe(Close)));
    }
}