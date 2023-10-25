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

public class EditPersonViewModel : BaseViewModel, IQueryAttributable
{
    private Realm _realm;
    private bool _isNew;
    private Person _person;
    private ObjectId _personId;  // backing for QueryProperty and used to load _sharedWith
    private Appointment _selectedAppointment;
    private Note _selectedNote;

    // Edit fields to bind to
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    // normally true but in Debug mode with local data can edit other people
    public bool IsCurrentUser => _person.UserId != ObjectId.Empty && _person.UserId == Settings.Current.LoggedInUserId;
    public bool NeedsToCompleteSelf => Settings.Current.UsingServer && IsCurrentUser && Name.Length == 0;
    public bool CanMakeCurrentLocal => !Settings.Current.UsingServer;

    // note that lists for Notes and Appointments come directly from the Person
    public IList<Appointment> Appointments => _person?.Appointments ?? new List<Appointment>();

    public IList<Note> Notes => _person?.Notes ?? new List<Note>();

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

    public Command<Appointment> EditAppointmentCommand { get; }
    public Command<Appointment> DeleteAppointmentCommand { get; }
    public Command<Note> EditNoteCommand { get; }
    public Command MakeCurrentLocalCommand { get;  }


    ///@see ApplyQueryAttributes for population with Shell nav
    public EditPersonViewModel(Person toEdit = null, bool isNew = false, Func<Realm> realmSource = null)
    {
        _realm = (realmSource ?? Doc.MakeDefaultRealm)();
        _isNew = isNew;
        if (toEdit is not null) {
            _person = toEdit;  // probably testing
            _personId = toEdit.Id;
            Name = _person.Name;
            Email = _person.Email;
            Phone = _person.Phone;
        }
        EditAppointmentCommand = new Command<Appointment>(OnEditAppointment);
        DeleteAppointmentCommand = new Command<Appointment>(OnEditAppointment);
        EditNoteCommand = new Command<Note>(OnEditNote);
        MakeCurrentLocalCommand = new Command(OnMakeCurrentLocal);
    }

    public void NewPerson(ObjectId? userId = null)
    {
        _person = new Person() { UserId = userId };
        _personId = _person.Id;
        _isNew = true;
        Name = "";
        Email = "";
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
        OnPropertyChanged(nameof(Appointments));
        OnPropertyChanged(nameof(Notes));
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
        _isNew = false;  // in case we retain this VM, eg: do another save if UI allows it
    }

    async void OnEditAppointment(Appointment appt)
    {
        if (appt == null)
            return;

        _selectedAppointment = appt;
        // This will push the EditAppointmentPage onto the navigation stack
        await Shell.Current.GoToAsync($"{nameof(EditAppointmentPage)}?{nameof(EditAppointmentViewModel.PersonId)}={_person.Id.ToString()}&{nameof(EditAppointmentViewModel.AppointmentId)}={_selectedAppointment.Id.ToString()}");
    }

    async void OnEditNote(Note note)
    {
        if (note == null)
            return;

        _selectedNote = note;
        // This will push the EditNotePage onto the navigation stack
        await Shell.Current.GoToAsync($"{nameof(EditNotePage)}?{nameof(EditNoteViewModel.PersonId)}={_person.Id.ToString()}&{nameof(EditNoteViewModel.NoteId)}={_selectedNote.Id.ToString()}");
    }

    void OnMakeCurrentLocal()
    {
        Doc.Current.CurrentLocalPerson = _person;
    }

    #region IQueryAttributable
    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        string flag = query.DecodedParam(nameof(IsNew));
        if (String.IsNullOrEmpty(flag)) {
            string editId = query.DecodedParam(nameof(PersonId));
            if (String.IsNullOrEmpty(editId)) {
                Debug.Fail("Should have a current selected Person");
            } else {
                LoadPerson(ObjectId.Parse(editId));  // typically from list
            }
        } else {
            NewPerson();
        }
    }
    #endregion
}

