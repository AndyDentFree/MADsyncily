using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Realms;
using Xamarin.Forms;

using SyncOddly.Models;
using SyncOddly.Views;

namespace SyncOddly.ViewModels;

public class SharedWithViewModel : BaseViewModel, AppearanceDetectingVM
{
    protected Realm _realm;
    private SharedWith _selectedSharedWith;

    public IQueryable<SharedWith> SharedWith { get; private set; }
    public Command LoadSharedWithCommand { get; }
    public Command AddSharedWithCommand { get; }
    public Command<SharedWith> SharedWithTapped { get; }

    public SharedWithViewModel(Func<Realm> realmSource = null)
    {
        _realm = (realmSource ?? Doc.MakeDefaultRealm)();
        Title = "SharedWith";
        SharedWith = _realm.All<SharedWith>();
        Debug.WriteLine($"SharedWithViewModel just loaded {SharedWith.Count()} items");
        LoadSharedWithCommand = new Command(async () => await ExecuteLoadSharedWithCommand());

        SharedWithTapped = new Command<SharedWith>(OnSharedWithTapped);

        AddSharedWithCommand = new Command(OnAddSharedWith);
    }

    async Task ExecuteLoadSharedWithCommand()
    {
        IsBusy = true;

        try
        {
            // only do something when sync
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void OnAppearing()
    {
        // not appropriate as load Realm directly in ctor,  IsBusy = true;
        // update sync subs if using server
    }

    public void OnDisappearing()
    {
        IsBusy = false;
    }

    public SharedWith SelectedSharedWith
    {
        get => _selectedSharedWith;
        set
        {
            SetProperty(ref _selectedSharedWith, value);
            OnSharedWithTapped(value);
        }
    }

    private async void OnAddSharedWith(object obj)
    {
        //ASD await Shell.Current.GoToAsync(nameof(NewSharedWithPage));
    }

    async void OnSharedWithTapped(SharedWith SharedWith)
    {
        if (SharedWith == null)
            return;

        _selectedSharedWith = SharedWith;
        await Shell.Current.GoToAsync($"{nameof(SharedWithDetailPage)}?{nameof(SharedWithDetailViewModel.SharedWithId)}={_selectedSharedWith.Id.ToString()}");
    }
}
