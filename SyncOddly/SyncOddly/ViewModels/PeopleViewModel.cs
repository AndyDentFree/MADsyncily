using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Realms;
using Xamarin.Forms;

using SyncOddly.Models;
using SyncOddly.Views;

namespace SyncOddly.ViewModels;

public class PeopleViewModel : BaseViewModel, AppearanceDetectingVM
{
    protected Realm _realm;
    private Person _selectedPerson;

    public IQueryable<Person> People { get; private set; }
    public Command LoadPeopleCommand { get; }
    public Command AddPersonCommand { get; }
    public Command<Person> EditPersonCommand { get; }

    public PeopleViewModel(Func<Realm> realmSource = null)
    {
        var usingServer = Settings.Current.UsingServer;
        _realm = (realmSource ?? Doc.MakeDefaultRealm)();
        Title =  usingServer ? "Me" : "People";
        People = _realm.All<Person>().OrderBy(p => p.Name);
        LoadPeopleCommand = new Command(async () => await OnLoadPeopleCommand());
        EditPersonCommand = new Command<Person>(OnEditPerson);
        if (!usingServer) {
            AddPersonCommand = new Command(OnAddPerson);
        }
    }

    async Task OnLoadPeopleCommand()
    {
        IsBusy = true;

        try
        {
            // update sync subs if using server
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
        _selectedPerson = null;
    }

    public void OnDisappearing()
    {
        IsBusy = false;
    }

    public Person SelectedPerson
    {
        get => _selectedPerson;
        set
        {
            SetProperty(ref _selectedPerson, value);
            OnEditPerson(value);
        }
    }

    private async void OnAddPerson(object obj)
    {
        await Shell.Current.GoToAsync($"{nameof(EditPersonPage)}?{nameof(EditPersonViewModel.IsNew)}=T");
    }

    async void OnEditPerson(Person Person)
    {
        if (Person == null)
            return;

        _selectedPerson = Person;
        // This will push the EditPersonPage onto the navigation stack
        await Shell.Current.GoToAsync($"{nameof(EditPersonPage)}?{nameof(EditPersonViewModel.PersonId)}={_selectedPerson.Id.ToString()}");
    }
}
