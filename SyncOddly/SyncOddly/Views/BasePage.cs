using SyncOddly.ViewModels;
using Xamarin.Forms;

namespace SyncOddly.Views;

public class BasePage : ContentPage
{
    protected override void OnAppearing()
    {
        base.OnAppearing();
        (BindingContext as AppearanceDetectingVM)?.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        (BindingContext as AppearanceDetectingVM)?.OnDisappearing();
    }
}
