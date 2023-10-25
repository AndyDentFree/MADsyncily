using System;
using Realms;
using Faker;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SyncOddly.Models;

/**
 * Generate a range of sample data both for unit tests and to exercise sync in main app.
 */
public class SampleDataGenerator
{
    public int NumPeople { get; set; }
    public int MinNotesEach { get; set; } = 0;
    public int MaxNotesEach { get; set; } = 40;
    public int MinAppointmentsEach { get; set; } = 0;
    public int MaxAppointmentsEach { get; set; } = 200;
    public int MinShareWithEach { get; set; } = 0;
    public int MaxShareWithEach { get; set; } = 16;
    public int NotesSharePercent { get; set; } = 50;
    public int NotesAreTasksPercent { get; set; } = 50;
    public int AppointmentsSharePercent { get; set; } = 50;
    public int PeopleSharePercent { get; set; } = 20;

    // copied and parameterised from Faker.data method User.Username
    private string EmailFrom(string first, string last)
    {
        var username = Number.RandomNumber(5) switch
        {
            0 => new Regex("\\W").Replace(first, string.Empty).ToLower(),
            1 => new Regex("\\W").Replace(first + last, string.Empty).ToLower(),
            2 => new Regex("\\W").Replace(last + first, string.Empty).ToLower(),
            3 => $"{last}.{first}",
            4 => $"{first}.{last}",
            _ => throw new ApplicationException(),
        };
        return username + "@" + Internet.Host();
    }
    
    public async Task MakeSyncSafeSampleData(Realm realm, bool onServer)
    {
        var createdAt = DateTimeOffset.Now.RealmAccurateMS();
        if (onServer)
            RealmService.AddSampleSubs(realm, createdAt);
        MakePeopleSamples(realm, createdAt);
        MakeSharing(realm, createdAt);
        await realm.Subscriptions.WaitForSynchronizationAsync();
        if (onServer)
            RealmService.RemoveSampleSubs(realm);
        // don't await these cleaning up as we don't care for flushing
    }

    // without sharing
    public void MakePeopleSamples(Realm realm, DateTimeOffset createdAt)
    {
        // make people using the open source random data generator
        var rando = new Random();
        realm.Write(() =>
        {
            for (int i = 0; i < NumPeople; i++) {
                var firstName = Name.FirstName();
                var lastName = Name.LastName();
                var p = new Person
                {
                    Name = firstName + " " + lastName, 
                    Email = EmailFrom(firstName, lastName), 
                    Phone = Phone.GetShortPhoneNumber(),
                    Created = createdAt
                };
                AddNotesAndAppts(realm, p, rando, createdAt);
                Doc.WritePerson(p, true, realm);
            }
            // add sample content to the current user if they don't have any
            if (Doc.Current.CurrentServerUser is not null and Person u) {
                if (u.Notes.Count == 0 && u.Appointments.Count == 0) {
                    AddNotesAndAppts(realm, u, rando, createdAt);
                    Doc.WritePerson(u, false, realm);
                }
            }
        });
    }

    private void AddNotesAndAppts(Realm realm, Person p, Random rando, DateTimeOffset createdAt)
    {
        var numNotesTotal = rando.Next(MinNotesEach, MaxNotesEach);
        var numTasks = numNotesTotal * NotesAreTasksPercent / 100;
        var numNotes = numNotesTotal - numTasks;
        for (int n = 0; n < numNotes; n++) {
            p.Notes.Add(new Note { Title = Lorem.Sentence(), Body = Lorem.Paragraph(rando.Next(1, 7)) });
        }
        for (int n = 0; n < numTasks; n++) {
            p.Notes.Add(new Note { Title = Lorem.Sentence(3), IsTask = true, IsDone = (rando.Next(0, 1) == 1) });
        }
        var numAppts = rando.Next(MinAppointmentsEach, MaxAppointmentsEach);
        for (int a = 0; a < numAppts; a++) {
            var baseDate = DateTime.Now.AddDays(rando.Next(0, 365));
            var apptDate = new DateTimeOffset(baseDate.Year, baseDate.Month, baseDate.Day, rando.Next(6, 23), rando.Next(0, 59), 0, TimeSpan.Zero);
            p.Appointments.Add(new Appointment { When = apptDate, Duration = rando.Next(5, 240), Why = Lorem.Sentence() });
        }
    }

    public void MakeSharing(Realm realm, DateTimeOffset createdAt)
    {
        var sharers = RealmHelpers.RandomSubset<Person>(PeopleSharePercent, realm);
        MakeSharing(sharers, realm, createdAt);
    }

    public void MakeSharing(IEnumerable<Person> sharingFrom, Realm realm, DateTimeOffset createdAt)
    {
        var rando = new Random();
        foreach (var sharer in sharingFrom) {
            ShareTo(realm, sharer, rando, createdAt);
        }
        if (Doc.Current.CurrentServerUser is not null and Person u) {
            ShareTo(realm, u, rando, createdAt);
        }
    }

    private void ShareTo(Realm realm, Person sharer, Random rando, DateTimeOffset createdAt)
    {
        int numShareWith = rando.Next(MinShareWithEach, MaxShareWithEach);
        if (numShareWith == 0)
            return;
        var notesToShare = RealmHelpers.RandomSubsetEmbedded<Note>(sharer.Notes, NotesSharePercent, realm);
        var apptsToShare = RealmHelpers.RandomSubsetEmbedded<Appointment>(sharer.Appointments, AppointmentsSharePercent, realm);
        var shareTo = RealmHelpers.RandomSubset<Person>(numShareWith, realm);
        realm.Write(() =>
        {
            foreach (var sw in shareTo) {
                if (sw.Id != sharer.Id)
                    Doc.ShareJust(sharer, apptsToShare, notesToShare, sw, realm, createdAt);
            }
        });
    }
}
