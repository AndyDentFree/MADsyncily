using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SyncOddly.ViewModels;
using SyncOddly.Models;
using Realms;

namespace SyncOddly.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class EditPersonPage : ContentPage
{
    private EditPersonViewModel _viewModel;

    public EditPersonPage()
    {
        InitializeComponent();

        _viewModel = new EditPersonViewModel();
        BindingContext = _viewModel;

        SaveButton.Clicked += async (s, e) =>
        {
            _viewModel.DoSave();
            await Navigation.PopAsync();
        };
    }
}
