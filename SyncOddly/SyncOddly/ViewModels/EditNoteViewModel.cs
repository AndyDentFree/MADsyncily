using System;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Bson;
using Realms;
using Xamarin.Forms;
using System.Web;
using SyncOddly.Models;
using FormsExtensions;

namespace SyncOddly.ViewModels;

public class EditNoteViewModel : BaseViewModel, IQueryAttributable
{
    protected Realm _realm;
    private Person _person;
    private Note _note;
    private ObjectId _personId;  // backing for QueryProperty and used to load _note
    private ObjectId _noteId;  // backing for QueryProperty 

    // Edit fields to bind to
    public String NoteTitle { get; set; }
    public String Body { get; set; }
    public bool IsTask { get; set; }
    public bool IsDone { get; set; }
    public bool ShowBody => !IsTask;

    public String PersonId
    {
        get
        {
            return _personId.ToString();
        }
        set
        {
            _personId = ObjectId.Parse(value);
        }
    }

    public String NoteId
    {
        get
        {
            return _noteId.ToString();
        }
        set
        {
            LoadNote(ObjectId.Parse(value));
        }
    }


    public string DisplayTitle => _note?.Title ?? "";


    public EditNoteViewModel(Func<Realm> realmSource = null)
    {
        _realm = (realmSource ?? Doc.MakeDefaultRealm)();
    }


    // This would be async if trad cloud database but super-fast Realm local store
    public void LoadNote(ObjectId noteId)
    {
        _noteId = noteId;
        _person = _realm.Find<Person>(_personId);

        _note = _person.Notes.Where(note => note.Id == noteId).FirstOrDefault();
        NoteTitle = _note?.Title ?? "";
        Body = _note?.Body ?? "";
        IsTask = _note?.IsTask ?? false;
        IsDone = _note?.IsDone ?? false;
        RefreshAll();
    }

    private void RefreshAll()
    {
        OnPropertyChanged(nameof(NoteTitle));
        if (IsTask)
        {
            OnPropertyChanged(nameof(IsDone));
        }
        else
        {
            OnPropertyChanged(nameof(Body));
        }
    }

    public void DoSave()
    {
        // check edit fields to determine if need to propagate any change
        if (NoteTitle == _note.Title &&
            IsTask == _note.IsTask &&
            ((IsTask && IsDone == _note.IsDone) || (!IsTask && Body == _note.Body)))
            return;

        _realm.Write(() =>
        {
            _note.Title = NoteTitle;
            _note.IsTask = IsTask;
            if (IsTask)
            {
                _note.IsDone = IsDone;
            }
            else
            {
                _note.Body = Body;
            }
            Doc.UpdateNote(_person, _note, _realm);  // central Doc methods take care of propagation
        });
    }

    #region IQueryAttributable
    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        // The query parameter requires URL decoding.
        PersonId = query.DecodedParam(nameof(PersonId));
        string apptId = query.DecodedParam(nameof(NoteId));
        if (!string.IsNullOrEmpty(apptId))
            LoadNote(ObjectId.Parse(apptId));
    }
    #endregion
}
