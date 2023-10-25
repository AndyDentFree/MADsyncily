using System;
using SyncOddly.Models;
using SyncOddly.ViewModels;
using System.Collections.Generic;
using MongoDB.Bson;
using Realms;
using Xamarin.Forms;
using System.Web;
using Realms.Sync;
using FormsExtensions;

namespace SyncOddly.ViewModels;

public class SharedWithDetailViewModel : BaseViewModel, IQueryAttributable
{
    protected Realm _realm;
    private SharedWith _sharedWith;
    private ObjectId _sharedWithId;  // backing for QueryProperty and used to load _sharedWith

    public String SharedWithId
    {
        get
        {
            return _sharedWithId.ToString();
        }
        set
        {
            LoadSharedWith(ObjectId.Parse(value));
        }
    }


    public string DisplayTitle => _sharedWith?.DisplayTitle ?? "";

    public string DisplaySubTitle => _sharedWith?.DisplaySubTitle ?? "";

    public string DisplayNames
    {
        get
        {
            var (owner, sw) = _sharedWith?.ParticipantNames(_realm) ?? ("", "");
            return owner + " / " + sw;
        }
    }

    public IList<Appointment> Appointments => _sharedWith?.Appointments ?? new List<Appointment>();

    public IList<Note> Notes => _sharedWith?.Notes ?? new List<Note>();
    public Command<Appointment> StopSharingAppointmentCommand { get; }
    public Command<Note> StopSharingNoteCommand { get; }


    public SharedWithDetailViewModel(Func<Realm> realmSource = null)
    {
        _realm = (realmSource ?? Doc.MakeDefaultRealm)();
        StopSharingAppointmentCommand = new Command<Appointment>(OnStopSharingAppointment);
        StopSharingNoteCommand = new Command<Note>(OnStopSharingNote);
    }

    // This would be async if trad cloud database but super-fast Realm local store
    public void LoadSharedWith(ObjectId sharedWithId)
    {
        _sharedWithId = sharedWithId;

        _sharedWith = _realm.Find<SharedWith>(sharedWithId);

        OnPropertyChanged(nameof(DisplayTitle));
        OnPropertyChanged(nameof(DisplaySubTitle));
        OnPropertyChanged(nameof(DisplayNames));
        OnPropertyChanged(nameof(Appointments));
        OnPropertyChanged(nameof(Notes));
    }

    async void OnStopSharingAppointment(Appointment appt)
    {
        if (appt == null)
            return;

        // just directly remove it from this record
        _realm.Write(() =>
        {
            _sharedWith.Appointments.Remove(appt);
        });
        OnPropertyChanged(nameof(DisplaySubTitle));
    }

    async void OnStopSharingNote(Note note)
    {
        if (note == null)
            return;

        // just directly remove it from this record
        _realm.Write(() =>
        {
            _sharedWith.Notes.Remove(note);
        });
        OnPropertyChanged(nameof(DisplaySubTitle));
    }

    #region IQueryAttributable
    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        // The query parameter requires URL decoding.
        string id = query.DecodedParam(nameof(SharedWithId));
        if (!string.IsNullOrEmpty(id))
            LoadSharedWith(ObjectId.Parse(id));
    }
    #endregion
}
