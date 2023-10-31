
using System;
using System.Windows.Input;
using SyncOddly.Models;
using SyncOddly.Views;
using Xamarin.Forms;

namespace SyncOddly.ViewModels
{
    public class FirstRunViewModel : BaseViewModel
    {
        private bool _useServer = false;

        public bool UseServer
        {
            get { return _useServer; }
            set
            {
                SetProperty(ref _useServer, value,
                    onChanged: () => OnPropertyChanged(nameof(ContinueButtonTitle)));
            }
        }
        public string ContinueButtonTitle => UseServer ? "Login/Register" : "Continue";
        public ICommand ExecuteBack { get; set; }
        public ICommand ExecuteContinue { get; set; }

        public FirstRunViewModel()
        {
            ExecuteBack = new Command(OnBack);
            ExecuteContinue = new Command(OnRegisterOrContinue);
        }


        private async void OnBack()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        private async void OnRegisterOrContinue()
        {
            Settings.Current.UsingServer = UseServer;
            if (UseServer)
                await Doc.Current.NavToLogin();
            else 
                await Doc.Current.NavToMain();  //TODO with Settings tab selected https://github.com/AndyDentFree/MADsyncily/issues/7
        }
    }
}

