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

public class EditAppointmentViewModel : BaseViewModel, IQueryAttributable
{
    protected Realm _realm;
    private Person _person;
    private Appointment _appointment;
    private ObjectId _personId;  // backing for QueryProperty and used to load _appointment
    private ObjectId _appointmentId;  // backing for QueryProperty 

    // Edit fields to bind to
    public DateTimeOffset When { get; set; }
    public int Duration { get; set; }
    public string Why { get; set; }

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

    public String AppointmentId
    {
        get
        {
            return _appointmentId.ToString();
        }
        set
        {
            LoadAppointment(ObjectId.Parse(value));
        }
    }


    public string DisplayTitle => _appointment?.DisplayTitle ?? "";


    public EditAppointmentViewModel(Func<Realm> realmSource = null)
    {
        _realm = (realmSource ?? Doc.MakeDefaultRealm)();
    }

    // This would be async if trad cloud database but super-fast Realm local store
    public void LoadAppointment(ObjectId apptId)
    {
        _appointmentId = apptId;
        _person = _realm.Find<Person>(_personId);

        _appointment = _person.Appointments.Where(appt => appt.Id == apptId).FirstOrDefault();
        When = _appointment?.When ?? DateTimeOffset.Now;
        Duration = _appointment?.Duration ?? 0;
        Why = _appointment?.Why ?? "";
        RefreshAll();
    }

    private void RefreshAll()
    {
        OnPropertyChanged(nameof(When));
        OnPropertyChanged(nameof(Duration));
        OnPropertyChanged(nameof(Why));
    }

    public void DoSave()
    {
        // check edit fields to determine if need to propagate any change
        if (Duration == _appointment.Duration &&
            Why == _appointment.Why &&
            When == _appointment.When)
            return;

        _realm.Write(() =>
        {
            _appointment.When = When;
            _appointment.Duration = Duration;
            _appointment.Why = Why;
            Doc.UpdateAppointment(_person, _appointment, _realm);  // central Doc methods take care of propagation
        });
    }

    #region IQueryAttributable
    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        // The query parameter requires URL decoding.
        PersonId = query.DecodedParam(nameof(PersonId));
        string apptId = query.DecodedParam(nameof(AppointmentId));
        if (!string.IsNullOrEmpty(apptId))
            LoadAppointment(ObjectId.Parse(apptId));
    }
    #endregion
}
