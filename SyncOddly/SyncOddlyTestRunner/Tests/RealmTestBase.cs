using System;
using System.Collections.Generic;
using NUnit.Framework;
using Realms;
using SyncOddly.Models;
using System.Diagnostics;

namespace TestSyncOddly;

public class RealmTestBase
{
    protected Realm testRealm = null;  // unit tests can refer to this directly but ViewModels will use factory below

    [SetUp]
    public void Setup()
    {
        //var testConfig = new InMemoryConfiguration(Guid.NewGuid().ToString());  // each test class run has unique in-memory instance
        var testConfig = new RealmConfiguration("SyncOddlyTestRealm-" + Guid.NewGuid().ToString());  // each test class run has unique local file instance)
        Debug.WriteLine($"Test realm at {testConfig.DatabasePath}");
        testRealm = Realm.GetInstance(testConfig);
        Doc.defaultRealmFactory = () =>
        {
            return testRealm;
        };
    }

    // Make a simple sample user, also see SampleDataGenerator
    public Person MakeUserWithNotesAndAppts()
    {
        Person ret = new Person { Name = "Boss", Email = "b@b", Phone = "505 123 456" };
        testRealm.Write(() => {
            testRealm.Add(ret);
            ret.Notes.Add(new Note { Title = "Orphan", Body = "blah blah blahish" });
            ret.Notes.Add(new Note { Title = "Lonely", Body = "An epic" });
            ret.Appointments.Add(new Appointment { When = DateTimeOffset.Now, Duration = 90, Why = "Dinner" });
            ret.Appointments.Add(new Appointment { When = DateTimeOffset.Now, Duration = 10, Why = "Entree" });
        });
        return ret;
    }

    public (Person, Person) MakeUserSharingAllWithBoss()
    {
        var owner = MakeUserWithNotesAndAppts();
        var sw = new Person { Name = "Harry", Email = "d@b" };
        testRealm.Write(() =>
        {
            testRealm.Add(sw);
            Doc.ShareAll(owner, sw, testRealm);
        });
        return (owner, sw);
    }

    // checks names and other details are different, failing at first clash
    public bool PeopleAreUnique(IEnumerable<Person> toCheck)
    {
        var randNames = new HashSet<string>();
        foreach(var person in toCheck)
        {
            var matchKey = person.Name + person.Email + person.Phone;
            if (randNames.Contains(matchKey))
            {
                Debug.WriteLine($"PeopleAreUnique failed at {matchKey}");
                return false;
            }
            randNames.Add(matchKey);
        }
        return true;
    }
}

