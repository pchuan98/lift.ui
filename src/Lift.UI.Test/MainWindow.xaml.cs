﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lift.UI.Test;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        var vm = new MainViewModel();

        LiftUiPropertyGrid.SelectedObject = vm;
        PdPropertyGrid.DataContext = vm;
    }
}


public partial class MainViewModel : ObservableObject
{
    [Category("A")]
    [ObservableProperty]
    private int _age = 1;

    [Category("B")]
    [ObservableProperty]
    private string _name = "kitty";

    [ObservableProperty]
    private bool _male = true;
}
