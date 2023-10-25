using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SyncOddly.ViewModels;
using static SyncOddly.Settings;

namespace SyncOddly.Views;

public partial class SettingsPage : ContentPage
{
    private bool _toggleDuringInit = true;

    public SettingsPage()
    {
        InitializeComponent();
        ModeSwitch.IsToggled = Settings.Current.Mode == AppMode.App;
        _toggleDuringInit = false;  // suppress OnModeSwitchToggled
        BindingContext = new SettingsViewModel();
    }

    private void OnModeSwitchToggled(object sender, ToggledEventArgs e)
    {
        if (_toggleDuringInit)
            return;
        Settings.Current.Mode = ModeSwitch.IsToggled ? AppMode.App : AppMode.Debug;
        // Refresh the Shell
        Application.Current.MainPage = new AppShell();
    }
}
