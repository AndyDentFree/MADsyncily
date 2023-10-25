using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SyncOddly.ViewModels;
using SyncOddly.Models;
using Realms;

namespace SyncOddly.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class EditCurrentUserPage : ContentPage
{
    private EditCurrentUserViewModel _viewModel;

    public EditCurrentUserPage(EditCurrentUserViewModel vm = null)
    {
        InitializeComponent();

        _viewModel = vm ?? new EditCurrentUserViewModel();
        BindingContext = _viewModel;

        SaveButton.Clicked += async (s, e) =>
        {
            _viewModel.DoSave();

            await Navigation.PopModalAsync();
            await Doc.Current.NavToMain();
        };
    }
}
