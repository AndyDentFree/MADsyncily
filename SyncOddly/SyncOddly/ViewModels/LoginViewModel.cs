using System;
using System.Windows.Input;
using System.Threading.Tasks;
using SyncOddly.Models;
using Xamarin.Forms;

namespace SyncOddly.ViewModels;

public class LoginViewModel : BaseViewModel, AppearanceDetectingVM
{
    public ICommand LoginCommand { get; private set; }
    public ICommand SignUpCommand { get; private set; }

    public LoginViewModel()
    {
        Email = Settings.Current.LastLoginEmail;
        LoginCommand = new Command(async () => await OnLogin()); ;
        SignUpCommand = new Command(async () => await OnSignUp());
    }

    public string Email { get; set; }

    public string Password { get; set; }


    public async void OnAppearing()
    {
        //await RealmService.Init();  - was only async in template because they do async reading and parsing JSON config file
        RealmService.Init();

        if (RealmService.CurrentUser != null) {
            await GoToMainPage();
        }
    }

    public void OnDisappearing()
    {
        IsBusy = false;
    }


    public async Task OnLogin()
    {
        if (!await VeryifyEmailAndPassword()) {
            return;
        }

        await DoLogin();
    }

    public async Task OnSignUp()
    {
        if (!await VeryifyEmailAndPassword()) {
            return;
        }

        await DoSignup();
    }

    private async Task DoLogin()
    {
        try {
            IsBusy = true;
            await RealmService.LoginAsync(Email, Password);
            IsBusy = false;
        }
        catch (Exception ex) {
            IsBusy = false;
            await ShowAlertAsync("Login failed", ex.Message, "Ok");
            return;
        }
        await GoToMainPage();
    }

    private async Task DoSignup()
    {
        try {
            IsBusy = true;
            await RealmService.RegisterAsync(Email, Password);
            IsBusy = false;
        }
        catch (Exception ex) {
            IsBusy = false;
            await ShowAlertAsync("Sign up failed", ex.Message, "Ok");
            return;
        }
        await DoLogin();
    }

    private async Task<bool> VeryifyEmailAndPassword()
    {
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password)) {
            await ShowAlertAsync("Error", "Please specify both the email and the password", "Ok");
            return false;
        }
        return true;
    }

    private async Task GoToMainPage()
    {
        bool canShowMain = await Doc.Current.EnsureHaveCurrentUserComplete();
        // if returns false, it has handled flow
        if (canShowMain)  // flag used so sits at modal entering current user
            await Doc.Current.NavToMain();
    }

}
