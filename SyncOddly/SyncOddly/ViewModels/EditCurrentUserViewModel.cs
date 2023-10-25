using System;
using System.Collections.Generic;
using System.Diagnostics;
using MongoDB.Bson;
using System.Web;
using Realms;
using SyncOddly.Models;
using Xamarin.Forms;
using SyncOddly.Views;
using FormsExtensions;

namespace SyncOddly.ViewModels;

/// <summary>
/// Simplified version of EditPersonViewModel without nested objects but aware of creation for logged in user
/// </summary>
public class EditCurrentUserViewModel : BaseViewModel, IQueryAttributable
{
    private Realm _realm;
    private bool _isNew;
    private Person _person;
    private ObjectId _personId;  // backing for QueryProperty and used to load _sharedWith

    // Edit fields to bind to
    public string Name
    {
        get => _name;
        set
        {
            SetProperty(ref _name, value,
                onChanged: () => OnPropertyChanged(nameof(IsValidUser)));
        }
    }
    private string _name;

    public string Email { get; set; }
    public string Phone { get; set; }
    public bool IsValidUser => !String.IsNullOrWhiteSpace(Name) && !String.IsNullOrWhiteSpace(Email);

    // public properties
    public String PersonId
    {
        get
        {
            return _personId.ToString();
        }
        set
        {
            LoadPerson(ObjectId.Parse(value));
        }
    }

    public String IsNew => _isNew ? "Y" : "N";



    ///@see ApplyQueryAttributes for population with Shell nav
    public EditCurrentUserViewModel(Person toEdit = null, bool isNew = false, Func<Realm> realmSource = null)
    {
        _realm = (realmSource ?? Doc.MakeDefaultRealm)();
        _isNew = isNew;
        if (toEdit is null) {
            Email = Settings.Current.LastLoginEmail;
        } else {
            _person = toEdit;  // probably testing
            _personId = toEdit.Id;
            Name = _person.Name;
            Email = _person.Email;
            Phone = _person.Phone;
        }
    }

    public void NewPerson(ObjectId userId)
    {
        _person = new Person() { UserId = userId };
        _personId = _person.Id;
        _isNew = true;
        Name = "";
        Email = Settings.Current.LastLoginEmail;
        Phone = "";
        RefreshAll();
    }


    // This would be async if trad cloud database but super-fast Realm local store
    public void LoadPerson(ObjectId personId)
    {
        _personId = personId;
        _isNew = false;
        _person = _realm.Find<Person>(personId);

        Name = _person.Name;
        Email = _person.Email;
        Phone = _person.Phone;
        RefreshAll();
    }

    private void RefreshAll()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Email));
        OnPropertyChanged(nameof(Phone));
    }

    // Update both Person and PersonLookup
    public void DoSave()
    {
        // check edit fields to determine if need to propagate any change
        if (Name == _person.Name &&
            Email == _person.Email &&
            Phone == _person.Phone)
            return;

        _realm.Write(() =>
        {
            _person.Name = Name;
            _person.Email = Email;
            _person.Phone = Phone;
            Doc.WritePerson(_person, _isNew, _realm);  // central Doc methods take care of propagation
        });
        if (_isNew)
            RealmService.UpdateCurrentPersonSharingSubs(_realm, _personId);

        _isNew = false;  // in case we retain this VM, eg: do another save if UI allows it
    }

    #region IQueryAttributable
    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        string editId = query.DecodedParam(nameof(PersonId));
        if (String.IsNullOrEmpty(editId)) {
            if (Settings.Current.LoggedInUserId is not null and ObjectId loggedInId) {
                if (_isNew) {
                    NewPerson(loggedInId);
                } else {
                    Debug.Assert(_person is not null, "Should have passed current user in when created viewmodel for EditCurrentUserPage");
                }
            } else {
                Debug.Fail("Should have a current logged in user");
            }
        } else {
            LoadPerson(ObjectId.Parse(editId));  // typically from list
        }
    }
    #endregion
}

