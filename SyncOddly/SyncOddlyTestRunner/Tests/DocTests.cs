using MongoDB.Bson;
using NUnit.Framework;
using SyncOddly.Models;
using System.Linq;
using System;
using System.Collections.Generic;

namespace TestSyncOddly;

/*
 * Mostly exercises the utility methods in the Doc class, which are used by different viewmodels.
 * Any data manipulation relating to multiple Realm classes ends up in here.
 * Many of the validation tests are to guard against something being done directly, not via the Doc methods.
 */
public class DocTests : RealmTestBase
{

    [Test]
    public void TestEmptyDocIsValid()
    {
        var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
        Assert.IsTrue(isOK, msg);
    }

    [Test]
    public void TestSharedWithNotRealPersonIsInvalid()
    {
        testRealm.Write(() =>
        {
            testRealm.Add(new SharedWith { OwnerId = ObjectId.GenerateNewId(), RecipientId = ObjectId.GenerateNewId() });
        });
        var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
        Assert.IsFalse(isOK, msg);
    }

    [Test]
    public void TestSharedButEmptyIsOK()
    {
        // this setup will be done by VM later
        testRealm.Write(() =>
        {
            var owner = new Person { Name = "Li", Email = "a@b" };
            var sw = new Person { Name = "Fred", Email = "b@b" };
            testRealm.Add(owner);
            testRealm.Add(sw);
            testRealm.Add(new SharedWith { OwnerId = owner.Id, RecipientId = sw.Id });  // real people but nothing shared in the relationship
        });
        var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
        Assert.IsTrue(isOK, msg);
    }

    [Test]
    public void TestSharedWithExtraNotesIsInvalid()
    {
        // this setup will be done by VM later
        testRealm.Write(() =>
        {
            var owner = new Person { Name = "Boss", Email = "b@b" };
            var sw = new Person { Name = "Harry", Email = "d@b" };
            testRealm.Add(owner);
            testRealm.Add(sw);
            var sharer = new SharedWith { OwnerId = owner.Id, RecipientId = sw.Id };
            sharer.Notes.Add(new Note { Title = "Orphan", Body = "blah blah blahish" });
            testRealm.Add(sharer);
        });
        Assert.AreEqual(1, testRealm.All<SharedWith>().Count(), "Should be a SharedWith record");
        var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
        Assert.IsFalse(isOK, msg);
    }

    [Test]
    public void TestSharedAllSimpleUserIsValid()
    {
        MakeUserSharingAllWithBoss();
        // Doc.DumpAll(testRealm, "small test of shareAll");
        var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
        Assert.IsTrue(isOK, msg);
        Assert.AreEqual(1, testRealm.All<SharedWith>().Count(), "Should be a SharedWith record");
    }

    // expanded TestSharedAllSimpleUserIsValid
    [Test]
    public void TestShareAgainDoesNotDuplicate()
    {
        var owner = MakeUserWithNotesAndAppts();
        var sharee = new Person { Name = "Harry", Email = "d@b" };
        testRealm.Write(() =>
        {
            testRealm.Add(sharee);
            var sw = Doc.ShareAll(owner, sharee, testRealm);
            var notesBefore = sw.Notes.ToArray();
            var apptsBefore = sw.Appointments.ToArray();

            var sw2 = Doc.ShareJust(owner, sw.Appointments.Take(1), sw.Notes.Skip(1).Take(1), sharee, testRealm, createdAt: DateTimeOffset.Now);
            var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
            Assert.IsTrue(isOK, msg);
            var notesAfter = sw2.Notes.ToArray();
            var apptsAfter = sw2.Appointments.ToArray();
            Assert.AreEqual(apptsBefore, apptsAfter);
            Assert.AreEqual(notesBefore, notesAfter);
        });
    }

    [Test]
    public void TestGettingPersonSharedItems()
    {
        var owner = MakeUserWithNotesAndAppts();
        var sharee = new Person { Name = "Harry", Email = "d@b" };
        testRealm.Write(() =>
        {
            testRealm.Add(sharee);
            Assert.AreEqual(owner.SharedNotes.Count(), 0);
            Assert.AreEqual(owner.SharedAppointments.Count(), 0);
            var sw = Doc.ShareJust(owner, owner.Appointments.Take(1), owner.Notes.Skip(1).Take(1), sharee, testRealm, createdAt: DateTimeOffset.Now);
            Assert.AreEqual(owner.SharedNotes.Count(), 1);
            Assert.AreEqual(owner.SharedAppointments.Count(), 1);
            var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
            Assert.IsTrue(isOK, msg);
        });
    }

    [Test]
    public void TestUniqueChecker()
    {
        testRealm.Write(() =>
        {
            testRealm.Add(new Person { Name = "Harry", Email = "d@b", Phone = "999" });
            testRealm.Add(new Person { Name = "Harry", Email = "d@b", Phone = "999" });
        });
        Assert.IsFalse(PeopleAreUnique(testRealm.All<Person>()), "duplicate people should be detected");
        var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
        Assert.IsFalse(isOK, "Should be invalid to have two people with identical details");
    }


    [Test]
    public void TestDuplicateSharedWithIsInvalid()
    {
        testRealm.Write(() =>
        {
            var owner = new Person { Name = "Boss", Email = "b@b", Phone = "123 456 77" };
            var sw = new Person { Name = "Harry", Email = "d@b" };
            testRealm.Add(owner);
            testRealm.Add(sw);
            testRealm.Add(new SharedWith { OwnerId = owner.Id, RecipientId = sw.Id });
            testRealm.Add(new SharedWith { OwnerId = owner.Id, RecipientId = sw.Id });
        });
        var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
        Assert.IsFalse(isOK, "Should be invalid to have two records in SharedWith in same relationship");
    }


    [Test]
    public void TestSmallSampleGeneratorIsValid()
    {
        var sdg = new SampleDataGenerator { NumPeople = 20 };
        sdg.MakePeopleSamples(testRealm, createdAt: DateTimeOffset.Now);
        var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
        Assert.IsTrue(isOK, msg);
        Assert.IsTrue(PeopleAreUnique(testRealm.All<Person>()), "all unique people generated");
    }


    [Test]
    public void TestLargeSampleGeneratorSubsetIsValid()
    {
        var sdg = new SampleDataGenerator { NumPeople = 2000, NotesSharePercent = 33, AppointmentsSharePercent = 10 };
        sdg.MakePeopleSamples(testRealm, createdAt: DateTimeOffset.Now);
        var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
        Assert.IsTrue(isOK, msg);
        var sharers = RealmHelpers.RandomSubset<Person>(20, testRealm);
        Assert.IsTrue(PeopleAreUnique(sharers), "all unique people extracted in subset");
        // does a lot because of overhead of generating many people with embedded records, so now test sharing
        sdg.MakeSharing(sharers, testRealm, createdAt: DateTimeOffset.Now);
        // Doc.DumpSharedFrom(sharers, testRealm, "\n\nShared stuff");
        var (isOK2, msg2) = Doc.OverallDocIsValid(testRealm);
        Assert.IsTrue(isOK2, msg2);
    }


    [Test]
    public void TestSharedWithExtraAppointmentIsInvalid()
    {
        // this setup will be done by VM later
        testRealm.Write(() =>
        {
            var owner = new Person { Name = "Boss", Email = "b@b" };
            var sw = new Person { Name = "Harry", Email = "d@b" };
            testRealm.Add(owner);
            testRealm.Add(sw);
            var sharer = new SharedWith { OwnerId = owner.Id, RecipientId = sw.Id };
            // do an illegal update directly on the SharedWith
            sharer.Appointments.Add(new Appointment { When = DateTimeOffset.Now, Why = "Sample" });
            testRealm.Add(sharer);
        });
        var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
        Assert.IsFalse(isOK, "Expect error because extra appointment invalid");
    }


    [Test]
    public void TestChangeAppointmentPropagatesToSharedWith()
    {
        var owner = MakeUserWithNotesAndAppts();
        var sw = new Person { Name = "Harry", Email = "d@b" };
        testRealm.Write(() =>
        {
            testRealm.Add(sw);
            var shared = Doc.ShareAll(owner, sw, testRealm);
            var (isOK1, msg1) = Doc.OverallDocIsValid(testRealm);
            Assert.IsTrue(isOK1, msg1);
            var originalAppt = owner.Appointments.Last();
            // pretend have been through an edit and VM copied back local fields
            originalAppt.Why = "Testability";
            originalAppt.When = DateTimeOffset.Now;
            originalAppt.Duration = originalAppt.Duration * 2;
            Doc.UpdateAppointment(owner, originalAppt, testRealm);
            // get what should have been updated
            var apptsAfter = shared.Appointments.ToArray();
            var matchingAppt = apptsAfter.Where(a => a.Id == originalAppt.Id).First();
            Assert.AreEqual(matchingAppt, originalAppt, "Shared is now same");
            var (isOK2, msg2) = Doc.OverallDocIsValid(testRealm);
            Assert.IsTrue(isOK2, "Valid after Appointment updated");
        });
    }


    [Test]
    public void TestChangeNotePropagatesToSharedWith()
    {
        var owner = MakeUserWithNotesAndAppts();
        var sw = new Person { Name = "Harry", Email = "d@b" };
        testRealm.Write(() =>
        {
            testRealm.Add(sw);
            var shared = Doc.ShareAll(owner, sw, testRealm);
            var notesAfterSharing = shared.Notes.ToArray();
            Assert.AreNotEqual(shared.Notes.Count, 0);
            Assert.AreEqual(notesAfterSharing.Count(), owner.Notes.Count);

            var (isOK1, msg1) = Doc.OverallDocIsValid(testRealm);
            Assert.IsTrue(isOK1, msg1);
            var originalNote = owner.Notes.First();
            // pretend have been through an edit and VM copied back local fields
            originalNote.Title = "Note Prior to Meeting";
            originalNote.Body = "blah blah blahish";
            Doc.UpdateNote(owner, originalNote, testRealm);
            // get what should have been updated
            var notesAfter = shared.Notes.ToArray();
            var matchingNote = notesAfter.Where(n => n.Id == originalNote.Id).First();
            Assert.AreEqual(matchingNote, originalNote, "Shared is now same");                
            var (isOK2, msg2) = Doc.OverallDocIsValid(testRealm);
            Assert.IsTrue(isOK2, "Valid after Note updated");
        });
    }


    [Test]
    public void TestDeleteSharedAppointment()
    {
        var (owner, sw) = MakeUserSharingAllWithBoss();
        //Doc.DumpAll(testRealm, "DelSharedAppt BEFORE");

        var countBefore = owner.Appointments.Count;
        var appt = owner.Appointments.First();
        Assert.IsNotNull(appt);
        testRealm.Write(() =>
        {
            Doc.DeleteAppointment(owner, appt, testRealm);
        });
        //Doc.DumpAll(testRealm, "DelSharedAppt AFTER");
        Assert.AreEqual(owner.Appointments.Count, countBefore - 1, "One appointment gone");
        var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
        Assert.IsTrue(isOK, msg);
    }


    [Test]
    public void TestDeleteSharedNote()
    {
        var (owner, sw) = MakeUserSharingAllWithBoss();
        var countBefore = owner.Notes.Count;
        var note1 = owner.Notes.First();
        Assert.IsNotNull(note1);
        testRealm.Write(() =>
        {
            Doc.DeleteNote(owner, note1, testRealm);
        });
        Assert.AreEqual(owner.Notes.Count, countBefore - 1, "One note gone");
        var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
        Assert.IsTrue(isOK, msg);
    }


    [Test]
    public void TestCancelingAllSharingFromMe()
    {
        var (owner, sw) = MakeUserSharingAllWithBoss();
        Assert.AreEqual(owner.SharedNotes.Count(), 2);
        Assert.AreEqual(owner.SharedAppointments.Count(), 2);
        testRealm.Write(() =>
        {
            Doc.StopSharingAll(owner, testRealm);
        });
        var sharedCount = testRealm.All<SharedWith>().Count();
        Assert.AreEqual(sharedCount, 0, "Nothing shared");
        var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
        Assert.IsTrue(isOK, msg);
        Assert.AreEqual(owner.SharedNotes.Count(), 0);
        Assert.AreEqual(owner.SharedAppointments.Count(), 0);
    }


    [Test]
    public void TestCancelingSharingToMe()
    {
        var (owner, sw) = MakeUserSharingAllWithBoss();
        testRealm.Write(() =>
        {
            Doc.StopSharingTo(sw, testRealm);
        });
        var sharedCount = testRealm.All<SharedWith>().Count();
        Assert.AreEqual(sharedCount, 0, "Nothing shared");
        var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
        Assert.IsTrue(isOK, msg);
    }


    #region Test Scenarios to implement
    /*

    [Test]
    public void TestCanHaveNoteSameAsShared()
    {

    }


    [Test]
    public void TestCanHaveAppointmentSameAsShared()
    {

    }*/
    #endregion

}