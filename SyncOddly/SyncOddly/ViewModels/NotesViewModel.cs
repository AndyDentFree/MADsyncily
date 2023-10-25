using System;
using Realms;
using SyncOddly.Models;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Linq;

namespace SyncOddly.ViewModels;


/*
	VM for Notes.
	If there is a lot to abstract, add a further layer as RealmBaseViewModel
*/
public class NotesViewModel : BaseViewModel, AppearanceDetectingVM
{
    protected Realm _realm;
    private Note _selectedNote;
    private Action _buttonVisualsUpdate;
    public IEnumerable<Note> Notes { get; private set; }
    public Command LoadNotesCommand { get; }
    public Command AddNoteCommand { get; }
    public Command<Note> NoteTapped { get; }
    public Command ShowAllMineCommand { get; }
    public Command ShowSharingCommand { get; }
    public Command ShowSharedToMeCommand { get; }
    //TODO hide hoist common logic from Notes and Appointments https://github.com/AndyDentFree/xamarealms/issues/28

    public NotesViewModel(Func<Realm> realmSource = null, Action buttonVisualsUpdate = null)
    {
        _realm = (realmSource ?? Doc.MakeDefaultRealm)();
        _buttonVisualsUpdate = buttonVisualsUpdate;
        MatchFilterState();
        ShowAllMineCommand = new Command(() => OnSetFilter(ShareableListState.ShowAllMine));
        ShowSharingCommand = new Command(() => OnSetFilter(ShareableListState.ShowSharing));
        ShowSharedToMeCommand = new Command(() => OnSetFilter(ShareableListState.ShowSharedToMe));
    }


    public void OnAppearing()
    {
        // not appropriate as load Realm directly in ctor, IsBusy = true;
        SelectedNote = null;
    }

    public void OnDisappearing()
    {
        IsBusy = false;
    }

    public Note SelectedNote
    {
        get => _selectedNote;
        set
        {
            SetProperty(ref _selectedNote, value);
            OnNoteSelected(value);
        }
    }

    private void MatchFilterState()
    {
        var p = Doc.Current.CurrentPerson;  // selected user on local or server
        if (p is null) {
            Notes = new List<Note>();
            return;
        }
        switch (Doc.Current.SharedListFilterState) {
            case ShareableListState.ShowAllMine:
                Title = "All My Notes";
                Notes = p.Notes.OrderBy(n => n.Title);
                break;

            case ShareableListState.ShowSharing:
                Title = "My Shared Notes";
                Notes = p.Notes.Where(n => n.IsShared).OrderBy(n => n.Title);
                break;

            case ShareableListState.ShowSharedToMe:
                Title = "Others' Notes";
                // have set of objects with nested content, horrible instantiation hack for now!
                // TODO cached local in-memory list with way to flush if server update
                var myShared = _realm.All<SharedWith>().Where(sw => sw.RecipientId == p.Id);
                var instNotes = new List<Note>();
                foreach (var sw in myShared) {
                    instNotes.AddRange(sw.Notes);
                }
                Notes = instNotes;
                break;
        }
    }

    private async void OnAddNote(object obj)
    {
        //ASD await Shell.Current.GoToAsync(nameof(NewNotePage));
    }

    async void OnNoteSelected(Note Note)
    {
        if (Note == null)
            return;

        // This will push the NoteDetailPage onto the navigation stack
        //ASD await Shell.Current.GoToAsync($"{nameof(NoteDetailPage)}?{nameof(NoteDetailViewModel.NoteId)}={Note.Id}");
    }

    private void OnSetFilter(ShareableListState newState)
    {
        if (newState == Doc.Current.SharedListFilterState)
            return;
        Doc.Current.SharedListFilterState = newState;
        _buttonVisualsUpdate?.Invoke();
        MatchFilterState();
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Notes));  // refresh list
    }
}
