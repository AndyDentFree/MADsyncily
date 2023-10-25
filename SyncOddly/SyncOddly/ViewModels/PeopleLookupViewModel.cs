using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Realms;
using Xamarin.Forms;

using SyncOddly.Models;

namespace SyncOddly.ViewModels;

public class PeopleLookupViewModel : BaseViewModel, AppearanceDetectingVM
{
    protected Realm _realm;
    private PersonLookup _selectedPersonLookup;

    public IQueryable<PersonLookup> PeopleLookup { get; private set; }
    public Command LoadPeopleLookupCommand { get; }
    public Command AddPersonCommand { get; }
    public Command<PersonLookup> PersonLookupTapped { get; }

    public PeopleLookupViewModel(Func<Realm> realmSource = null)
    {
        _realm = (realmSource ?? Doc.MakeDefaultRealm)();
        Title = "People";
        PeopleLookup = _realm.All<PersonLookup>().OrderBy(p => p.Name);
        LoadPeopleLookupCommand = new Command(async () => await ExecuteLoadPeopleCommand());
        Debug.WriteLine($"PeopleLookupViewModel just loaded {PeopleLookup.Count()} items");

        PersonLookupTapped = new Command<PersonLookup>(OnPersonLookupSelected);

        AddPersonCommand = new Command(OnAddPerson);
    }

    async Task ExecuteLoadPeopleCommand()
    {
        IsBusy = true;

        try
        {
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
        _selectedPersonLookup = null;
    }

    public void OnDisappearing()
    {
        IsBusy = false;
    }

    public PersonLookup SelectedPersonLookup
    {
        get => _selectedPersonLookup;
        set
        {
            SetProperty(ref _selectedPersonLookup, value);
            OnPersonLookupSelected(value);
        }
    }

    private async void OnAddPerson(object obj)
    {
        //ASD await Shell.Current.GoToAsync(nameof(NewPersonLookupPage));
    }

    async void OnPersonLookupSelected(PersonLookup PersonLookup)
    {
        if (PersonLookup == null)
            return;

        // This will push the PersonLookupDetailPage onto the navigation stack
        //ASD await Shell.Current.GoToAsync($"{nameof(PersonLookupDetailPage)}?{nameof(PersonLookupDetailViewModel.PersonLookupId)}={PersonLookup.Id}");
    }
}
