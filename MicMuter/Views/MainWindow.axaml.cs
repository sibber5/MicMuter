using System;
using Avalonia.Controls;
using MicMuter.ViewModels;

namespace MicMuter.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ComboBox_OnDropDownOpened(object? sender, EventArgs e)
    {
        ((MainWindowViewModel)DataContext!).NotifyPropertyChanged(nameof(MainWindowViewModel.MicFriendlyNames));
    }
}