using System;
using Realms;
using Realms.Sync;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Diagnostics;
using MongoDB.Bson;
using System.Threading.Tasks;
using static SyncOddly.Settings;
using SyncOddly.Views;
using Xamarin.Forms;
using SyncOddly.ViewModels;

namespace SyncOddly.Models;

/**
* Singleton that manages common Realm info, factories and provides multi-Realm operations.
* Not part of BaseViewModel because may have non-GUI responsibilities.
* 
* @note RealmObject and EmbeddedObject classes are kept relatively simple - any operation
* which requires knowledge of both Person and SharedWith is in here.
*/
public class Doc
{
    public enum LocalOrServerState
    {
        UseLocal,
        ServerYetToLogin,
        ServerLoggedIn
    }

    public static Doc Current { get; private set; } = new Doc();

    public Realm ActiveRealm { get; private set; }

    public const int SchemaVersionNumber = 5;

    // use a common state for Appointments and Notes as that represents user mode of thinking
    public ShareableListState SharedListFilterState = ShareableListState.ShowAllMine;  // TODO remember these states in settings.

    public Person CurrentServerUser
    {
        get
        {
            if (Settings.Current.UsingServer) {
                if (RealmService.CurrentUserBinaryId is not null and ObjectId userId) {
                    EnsureHaveOpenRealm(true);
                    var serverPersons = RealmService.CurrentUserPersonQuery(ActiveRealm);
                    return serverPersons.FirstOrDefault();
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Current cached object or try to retrieve using last persisted ID
    /// </summary>
    public Person CurrentLocalPerson
    {
        get
        {
            if (Settings.Current.UsingServer)
                return null;
            if (_currentLocalPerson is null) {
                if (Settings.Current.LocalRealmCurrentPersonId is not null and ObjectId locId) {
                    EnsureHaveOpenRealm(false);
                    _currentLocalPerson = ActiveRealm.Find<Person>(locId);
                    if (_currentLocalPerson is null)
                        _currentLocalPerson = ActiveRealm.All<Person>().FirstOrDefault();  // useful demo default
                }
            }
            return _currentLocalPerson;
        }
        set
        {
            if (Settings.Current.UsingServer)
                return;
            _currentLocalPerson = value;
            Settings.Current.LocalRealmCurrentPersonId = _currentLocalPerson?.Id;
            Debug.WriteLine($"Just set {value.Name} ({value.Id}) current local person");
        }
    }
    private Person _currentLocalPerson;

    public Person CurrentPerson => Settings.Current.UsingServer ? CurrentServerUser : CurrentLocalPerson;

    #region Routing
    /// Nav to different tab groups in AppShell.xaml
    public string TabbedMainRoute => Settings.Current.Mode == AppMode.Debug ? $"//mainDebugMode" : $"//mainAppMode";

    /// <summary>
    /// Checks have non-empty Person record matching current login details.
    /// </summary>
    /// <returns>true if caller can continue, false means we handle nav</returns>
    public async Task<bool> EnsureHaveCurrentUserComplete()
    {
        var sc = Settings.Current;
        Debug.Assert(sc.UsingServer, "login flow shouldn't invoke without wanting server");
        EnsureHaveOpenRealm(true);
        // can only have one user in Person at a time but may be empty if not yet created after registration & login
        var currUser = ActiveRealm.All<Person>().FirstOrDefault();
        var isNew = currUser is null;
        if (!isNew) {  // check conformance & has needed fields
            if (sc.LoggedInUserId is not null and ObjectId loggedIn) {
                if (currUser.UserId == loggedIn && RealmService.CurrentUserLoginMatches(loggedIn, currUser.Email)) {
                    if (!String.IsNullOrEmpty(currUser.Name))
                        return true;  // otherwise fall through to edit
                } else {
                    await BaseViewModel.ShowAlertAsync("Sync problem", "Currently logged in but not synchronised People", "Login again");
                    await Doc.Current.NavToLogin();
                    return false;
                }
            } else {
                // unexpected failure condition as should only invoke when logged in
                await BaseViewModel.ShowAlertAsync("Login problem", "Not Currently logged", "Login");
                await Doc.Current.NavToLogin();  
                return false;
            }
        }
        var needsSubs = isNew;
        if (String.IsNullOrEmpty(currUser?.Name)) {
            await Shell.Current.Navigation.PushModalAsync(
                new EditCurrentUserPage(new EditCurrentUserViewModel(currUser, isNew))
            );
            // won't be allowed out of the modal until finish entering non-blank name
            // EditPersonViewModel.DoSave will UpdateCurrentPersonSharingSubs if new
            // heavy-handed but can nav to main when get out of here
        }
        return false;  // nav from EditCurrentUserPage when it dismisses as modal
    }

    public Task NavToMain()
    {
        Debug.WriteLine("NavToMain invoked");
        return Shell.Current.GoToAsync(TabbedMainRoute);
    }

    public Task NavToLogin()
    {
        Debug.WriteLine("NavToLogin invoked");
        return Shell.Current.Navigation.PushModalAsync(new LoginPage());
    }
    #endregion

    #region ServerState
    public LocalOrServerState IsLocalOrServerLoggedIn()
    {
        if (Settings.Current.UsingServer) {
            if (RealmService.CurrentUser is not null && Settings.Current.LoggedInUserId is not null) {
                EnsureHaveOpenRealm(true);
                return LocalOrServerState.ServerLoggedIn;
            }
            return LocalOrServerState.ServerYetToLogin;
        }
        return LocalOrServerState.UseLocal;
    }
    #endregion

    #region DocManagement
    public void EnsureHaveOpenRealm(bool wantedOnServer)
    {
        if (ActiveRealm != null) {
            bool isOnServer = ActiveRealm.Config is FlexibleSyncConfiguration;
            if (wantedOnServer == isOnServer)
                return; // is good
            ActiveRealm.Dispose();  // close local
            ActiveRealm = null;
        }
        if (wantedOnServer) {
            ActiveRealm = RealmService.GetMainThreadRealm();
            Debug.WriteLine("Server Realm opened, may not be logged in");
        } else {
            ActiveRealm = Doc.MakeDefaultRealm();
            Debug.WriteLine("Local database file: " + ActiveRealm.Config.DatabasePath);
        }
    }

    public void WipeLocalData()
    {
        EnsureHaveOpenRealm(false);
        ActiveRealm.Write(() =>
        {
            ActiveRealm.RemoveAll();
        });
    }

    /// <summary>
    /// Very broad tests
    /// </summary>
    /// <param name="realm">Passed in realm which may be an instance just from a test or may be main document</param>
    /// <returns></returns>
    /// <remarks>Also checks PeopleAreUnique</remarks>
    static public (bool, String) OverallDocIsValid(Realm realm)
    {
        var people = realm.All<Person>();
        var accumFails = new List<String>();
        if (people.Count() == 0) {
            if (realm.All<SharedWith>().Count() > 0) {
                return (false, "Cannot have any SharedWith if no People");
            }
            return (true, "");
        }
        // this is very clunky, should probably use relationships for it but unsure of their sync implications
        // so we want to ensure any manual creation doesn't introduce invalid relationships
        var randNames = new HashSet<string>();
        foreach (var p in people) {
            var matchKey = p.Name + p.Email + p.Phone;
            if (randNames.Contains(matchKey)) {
                accumFails.Add($"Non-unique person encountered {matchKey}");
            } else {
                randNames.Add(matchKey);
            }

            var ownedShared = realm.All<SharedWith>().Where(sw => sw.OwnerId == p.Id).OrderBy(sw => sw.RecipientId);  // OK to be empty set
            var prevRecipient = p.Id;  // good starter sentinel
            foreach (var sw in ownedShared) {
                if (sw.RecipientId == prevRecipient) {
                    accumFails.Add($"Multiple sharingWith records for {matchKey} to recipient Id {prevRecipient}");
                    continue;
                }
                var (rOK, msg) = sw.IsValidForOwner(p, realm);
                if (!rOK) {
                    accumFails.Add(msg);
                }
                prevRecipient = sw.RecipientId;
            }
        }
        // for diagnostic purposes want to know each thing that was invalid
        if (accumFails.Count > 0) {
            return (false, String.Join(", ", accumFails));
        }
        return (true, "");

    }
    #endregion


    #region RealmFactory

    /// injectable override so unit tests can easily invoke default, can use shared data
    public static Func<Realm> defaultRealmFactory;

    /// fallback used by all viewmodels if don't pass an explicit factory to them
    public static Realm MakeDefaultRealm()
    {
        // bit a hack that sets ActiveRealm
        if (defaultRealmFactory is null) {
            if (Settings.Current.UsingServer) {
                return RealmService.GetMainThreadRealm();
            } else {
                var docConf = RealmConfiguration.DefaultConfiguration;  // use default so opens same filename as before
                docConf.SchemaVersion = SchemaVersionNumber;

                // second level of default behaviour
                return Realm.GetInstance(docConf);
            }
        }
        return defaultRealmFactory();
    }
    #endregion


    #region PeopleUpdates
    // helper methods that ensure Person and PersonLookup updated in sync, to call inside a write transaction
    public static void WritePerson(Person p, bool isNew, Realm realm)
    {
        if (isNew) {
            realm.Add(p);
            realm.Add(new PersonLookup { Id = p.Id, Name = p.Name, Email = p.Email });
        } else {
            // only search for PersonLookup to update if we've changed something
            var lookupToFix = realm.Find<PersonLookup>(p.Id);
            Debug.Assert(lookupToFix != null, "Must always have paired Lookup");
            lookupToFix.Name = p.Name;
            lookupToFix.Email = p.Email;
        }

    }


    public static void UpdateAppointment(Person fromPerson, Appointment refAppt, Realm realm)
    {
        var apptId = refAppt.Id;
        foreach (var sw in realm.SharedByAppointment(fromPerson.Id, apptId)) {
            foreach (var sharedAppt in sw.Appointments.Where(a => a.Id == apptId)) {
                //TODO have a better way of propagating tagged updates so don't copy all fields
                sharedAppt.Why = refAppt.Why;
                sharedAppt.When = refAppt.When;
                sharedAppt.Duration = refAppt.Duration;
            }
        }
    }


    public static void UpdateNote(Person fromPerson, Note refNote, Realm realm)
    {
        var noteId = refNote.Id;
        foreach (var sw in realm.SharedByNote(fromPerson.Id, noteId)) {
            foreach (var sharedNote in sw.Notes.Where(note => note.Id == noteId)) {
                //TODO have a better way of propagating tagged updates so don't copy all fields
                sharedNote.Title = refNote.Title;
                sharedNote.Body = refNote.Body;
                sharedNote.IsTask = refNote.IsTask;
            }
        }
    }


    // called within a Write, stopping sharing may be because of deletion or cancelling sharing relationship
    public static void DeleteAppointment(Person fromPerson, Appointment embeddedAppointment, Realm realm)
    {
        StopSharingAppointment(fromPerson.Id, embeddedAppointment.Id, realm);
        fromPerson.Appointments.Remove(embeddedAppointment);
    }


    public static void DeleteNote(Person fromPerson, Note embeddedNote, Realm realm)
    {
        StopSharingNote(fromPerson.Id, embeddedNote.Id, realm);
        fromPerson.Notes.Remove(embeddedNote);
    }
    #endregion


    #region Sharing
    // helper methods, to call inside a write transaction

    public static SharedWith ShareAll(Person fromPerson, Person sharedWith, Realm realm)
    {
        var ownedShared = realm.SharedBetween(fromPerson.Id, sharedWith.Id);  // OK to be empty set
        SharedWith sw = null;
        Doc.SetSharedFlag(fromPerson.Appointments, fromPerson.Notes, true);  // prior cloning!
        var numShared = ownedShared.Count();
        Debug.Assert(numShared < 2, "Should only have unique combinations of IDs");
        if (numShared == 0) {
            sw = new SharedWith
            {
                OwnerId = fromPerson.Id,
                RecipientId = sharedWith.Id
            };
            sw.AddShared(fromPerson.Appointments, fromPerson.Notes);
            realm.Add(sw);
        } else {
            Debug.WriteLine($"ShareAll falling through to only share new because recipient already has {numShared} share records");
            sw = ownedShared.First();
            sw.AddSharedIfNew(fromPerson.Appointments, fromPerson.Notes);
        }
        return sw;
    }


    public static SharedWith ShareJust(Person fromPerson, IEnumerable<Appointment> appts, IEnumerable<Note> notes, Person sharedWith, Realm realm, DateTimeOffset createdAt)
    {
        var ownedShared = realm.SharedBetween(fromPerson.Id, sharedWith.Id);
        SharedWith sw = null;
        Doc.SetSharedFlag(appts, notes, true);  // prior cloning
        var numSharedBothParties = ownedShared.Count();
        Debug.Assert(numSharedBothParties < 2, "Should only have unique combinations of IDs");
        if (numSharedBothParties == 0) {
            sw = new SharedWith
            {
                OwnerId = fromPerson.Id,
                RecipientId = sharedWith.Id,
                Created = createdAt
            };
            sw.AddShared(appts, notes);
            realm.Add(sw);
        } else {
            sw = ownedShared.First();
            sw.AddSharedIfNew(appts, notes);
        }
        return sw;
    }

    // only sets flag if will change value, to minimise dirtying and consequent sync
    public static void SetSharedFlag(IEnumerable<Appointment> appts, IEnumerable<Note> notes, bool flag)
    {
        foreach (var a in appts) {
            if (a.IsShared != flag)
                a.IsShared = flag;
        }
        foreach (var n in notes) {
            if (n.IsShared != flag)
                n.IsShared = flag;
        }
    }


    public static void StopSharingAppointment(Person fromPerson, Appointment embeddedAppointment, Realm realm)
    {
        StopSharingAppointment(fromPerson.Id, embeddedAppointment.Id, realm);
        embeddedAppointment.IsShared = false;
    }


    // Need to use Id because the actual embedded object in a Person is not the same as SharedWith
    private static void StopSharingAppointment(ObjectId fromPersonId, ObjectId apptId, Realm realm)
    {
        foreach (var sw in realm.SharedByAppointment(fromPersonId, apptId)) {
            foreach (var sharedAppt in sw.Appointments.Where(appt => appt.Id == apptId)) {
                sw.Appointments.Remove(sharedAppt);
            }
        }
    }


    public static void StopSharingNote(Person fromPerson, Note embeddedNote, Realm realm)
    {
        StopSharingNote(fromPerson.Id, embeddedNote.Id, realm);
        embeddedNote.IsShared = false;
    }


    private static void StopSharingNote(ObjectId fromPersonId, ObjectId noteId, Realm realm)
    {
        foreach (var sw in realm.SharedByNote(fromPersonId, noteId)) {
            foreach (var sharedNote in sw.Notes.Where(note => note.Id == noteId)) {
                sw.Notes.Remove(sharedNote);
            }
        }
    }


    public static void StopSharingAll(Person fromPerson, Realm realm)
    {
        realm.RemoveRange(realm.Shared(fromPerson.Id));
        Doc.SetSharedFlag(fromPerson.Appointments, fromPerson.Notes, false);
    }


    // WARNING at present this doesn't clear IsShared flags even if nobody left sharing the item
    public static void StopSharingBetween(Person fromPerson, Person toPerson, Realm realm)
    {
        realm.RemoveRange(realm.SharedBetween(fromPerson.Id, toPerson.Id));
    }


    // WARNING at present this doesn't clear IsShared flags even if nobody left sharing the item
    public static void StopSharingTo(Person toPerson, Realm realm)
    {
        realm.RemoveRange(realm.SharedTo(toPerson.Id));
    }
    #endregion

    #region DebugDumps

    public static void DumpAll(Realm realm, string heading = "")
    {
        Dump(realm.All<Person>(), realm, heading);
    }

    public static void Dump(IEnumerable<Person> people, Realm realm, string heading = "")
    {
        Debug.WriteLine(heading);
        foreach (var p in people) {
            Dump(p, realm);
        }
    }


    public static void DumpSharedFrom(IEnumerable<Person> people, Realm realm, string heading = "")
    {
        Debug.WriteLine(heading);
        foreach (var p in people) {
            DumpShares(p, realm);
        }
    }

    public static void Dump(Person person, Realm realm)
    {
        Debug.WriteLine($"{person.Name} {person.Email} {person.Phone}:");
        DumpAppointments(person.Appointments);
        DumpNotes(person.Notes);
        DumpShares(person, realm);
    }

    public static void DumpAppointments(IEnumerable<Appointment> appointments, string indent = "    ")
    {
        if (appointments.Count() == 0) {
            Debug.WriteLine($"{indent}Zero appointments");
            return;
        }
        foreach (var appt in appointments) {
            Debug.WriteLine($"{indent}{appt.DisplayTitle}");
        }
    }

    public static void DumpNotes(IEnumerable<Note> notes, string indent = "    ")
    {
        if (notes.Count() == 0) {
            Debug.WriteLine($"{indent}Zero notes");
            return;
        }
        foreach (var note in notes) {
            Debug.WriteLine($"{indent}{note.Title}");
        }
    }

    public static void DumpShares(Person person, Realm realm, bool sayIfNothing = true)
    {
        var shares = realm.All<SharedWith>().Where(sw => sw.OwnerId == person.Id);
        if (shares.Count() == 0) {
            if (sayIfNothing)
                Debug.WriteLine("  Has nothing shared");
            return;
        }
        foreach (var share in shares) {
            var recipient = realm.Find<Person>(share.RecipientId);
            Debug.WriteLine($"  Shares with {recipient.Name}:");
            DumpAppointments(share.Appointments, "  -a-> ");
            DumpNotes(share.Notes, "  -n-> ");
        }
    }
    #endregion
}

