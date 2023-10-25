using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SyncOddly.ViewModels;

public class AboutViewModel : BaseViewModel
{
    public AboutViewModel()
    {
        Title = "About";
        OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
        //TODO different dest URL https://github.com/AndyDentFree/xamarealms/issues/8
    }

    public ICommand OpenWebCommand { get; }
}
