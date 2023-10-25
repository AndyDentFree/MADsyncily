using System;
using System.Collections.Generic;
using Realms;
using Xamarin.Forms;
using SyncOddly.Models;
using System.Linq;

namespace SyncOddly.ViewModels;


/*
	VM for Appointments.
	If there is a lot to abstract, add a further layer as RealmBaseViewModel
    So using slightly more generic names like _selected
*/
public class AppointmentsViewModel : BaseViewModel, AppearanceDetectingVM
{
    protected Realm _realm;
    private Appointment _selectedAppointment;
    private Action _buttonVisualsUpdate;
    public IEnumerable<Appointment> Appointments { get; private set; }
    public Command LoadAppointmentsCommand { get; }
    public Command AddAppointmentCommand { get; }
    public Command<Appointment> AppointmentTapped { get; }
    public Command ShowAllMineCommand { get; }
    public Command ShowSharingCommand { get; }
    public Command ShowSharedToMeCommand { get; }
    //TODO hide hoist common logic from Notes and Appointments https://github.com/AndyDentFree/xamarealms/issues/28

    public AppointmentsViewModel(Func<Realm> realmSource = null, Action buttonVisualsUpdate = null)
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
        IsBusy = false;
        SelectedAppointment = null;
    }

    public void OnDisappearing()
    {
        IsBusy = false;
    }

    public Appointment SelectedAppointment
    {
        get => _selectedAppointment;
        set
        {
            SetProperty(ref _selectedAppointment, value);
            OnAppointmentSelected(value);
        }
    }

    private void MatchFilterState()
    {
        var p = Doc.Current.CurrentPerson;  // selected user on local or server
        if (p is null) {
            Appointments = new List<Appointment>();
            return;
        }
        switch (Doc.Current.SharedListFilterState) {
            case ShareableListState.ShowAllMine:
                Title = "All My Appointments";
                Appointments = p.Appointments.OrderBy(a => a.When);
                break;

            case ShareableListState.ShowSharing:
                Title = "My Shared Appointments";
                Appointments = p.Appointments.Where(a => a.IsShared).OrderBy(a => a.When);
                break;

            case ShareableListState.ShowSharedToMe:
                Title = "Others' Appointments";
                // have set of objects with nested content, horrible instantiation hack for now!
                // TODO cached local in-memory list with way to flush if server update
                var myShared = _realm.All<SharedWith>().Where(sw => sw.RecipientId == p.Id);
                var instAppts = new List<Appointment>();
                foreach (var sw in myShared) {
                    instAppts.AddRange(sw.Appointments);
                }
                Appointments = instAppts;
                break;
        }
    }

    private async void OnAddAppointment(object obj)
    {
        //ASD await Shell.Current.GoToAsync(nameof(NewAppointmentPage));
    }

    async void OnAppointmentSelected(Appointment Appointment)
    {
        if (Appointment == null)
            return;

        // This will push the AppointmentDetailPage onto the navigation stack
        //ASD await Shell.Current.GoToAsync($"{nameof(AppointmentDetailPage)}?{nameof(AppointmentDetailViewModel.AppointmentId)}={Appointment.Id}");
    }

    private void OnSetFilter(ShareableListState newState)
    {
        if (newState == Doc.Current.SharedListFilterState)
            return;
        Doc.Current.SharedListFilterState = newState;
        _buttonVisualsUpdate?.Invoke();
        MatchFilterState();
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Appointments));  // refresh list
    }
}
