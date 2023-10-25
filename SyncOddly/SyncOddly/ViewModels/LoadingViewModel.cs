using SyncOddly.Models;
using SyncOddly.Views;
using Xamarin.Forms;

namespace SyncOddly.ViewModels;

class LoadingViewModel : BaseViewModel
{
    bool _hackyFlagBecauseOnAppearingInvokedTwice = false;

    // Called by the views OnAppearing method
    public async void AuthenticateAndNavToNext()
    {
        if (_hackyFlagBecauseOnAppearingInvokedTwice)
            return;
        _hackyFlagBecauseOnAppearingInvokedTwice = true;

        var isFirstRun = !Settings.Current.HasSavedPreferences;
        RealmService.Init();
        if (isFirstRun) {
            await Shell.Current.Navigation.PushModalAsync(new FirstRunPage());
        } else {
            switch (Doc.Current.IsLocalOrServerLoggedIn()) {
                case Doc.LocalOrServerState.ServerYetToLogin:
                    await Doc.Current.NavToLogin();
                    break;

                case Doc.LocalOrServerState.ServerLoggedIn:
                    var haveUser = await Doc.Current.EnsureHaveCurrentUserComplete();
                    if (haveUser)
                        await Doc.Current.NavToMain();
                    break;
                    
                default:
                    await Doc.Current.NavToMain();
                    break;
            }
        }
    }
}
