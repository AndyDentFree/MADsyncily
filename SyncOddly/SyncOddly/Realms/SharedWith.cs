using System;
using System.Collections.Generic;
using MongoDB.Bson;
using Realms;

namespace SyncOddly.Models;

public class SharedWith : RealmObject
{
    
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    [MapTo("ownerId")]
    public ObjectId OwnerId { get; set; }

    [MapTo("recipientId")]
    public ObjectId RecipientId { get; set; }  // could be a single user or group (exercise for later)
    
    [MapTo("created")]
    public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
    
    #region Duplicated content, repeated per sharing relationship
    /// content copied from owned versions in Person, for each unique OwnerId+RecipientID pairing 
    [MapTo("appointments")]
    public IList<Appointment> Appointments { get; }

    [MapTo("notes")]
    public IList<Note> Notes { get; }
    #endregion

    public String DisplayTitle { get {
            return $"{OwnerId.ToString()} -> {RecipientId.ToString()}"; 
    }}

    public String DisplaySubTitle
    {
        get
        {
            var numAppt = Appointments.Count;
            var numNotes = Notes.Count;
            return (numAppt == 0 ? "No" : $"{numAppt}") +
                " Appointments, " +
                (numNotes == 0 ? "No" : $"{numNotes}") +
                " Notes";

        }
    }

    public (String, String) ParticipantNames(Realm realm)
    {
        var lhs = realm.Find<Person>(OwnerId)?.Name ?? "N/A";
        var rhs = realm.Find<Person>(RecipientId)?.Name ?? "N/A";
        return (lhs, rhs);
    }

    public (bool, String) IsValidForOwner(Person owner, Realm realm)
    {
        if (RecipientId == OwnerId)
        {
            return (false, "SharedWith must have different recipient from owner");
        }
        if (OwnerId != owner.Id)
        {
            return (false, $"SharedWith (O:{OwnerId}, R:{RecipientId}) is not owned by {owner.Id}");
        }
        if (realm.Find<Person>(RecipientId) is null)
        {
            // this may not be an error in future? Represents a space leak though.
            return (false, $"SharedWith (O:{OwnerId}, R:{RecipientId}) recipient doesn't exist");
        }
        // need neat way to check a list is all within another list
        foreach (var sharedNote in Notes)
        {
            if (!sharedNote.MatchesOneOf(owner.Notes))
            {
                return (false, $"SharedWith (O:{OwnerId}, R:{RecipientId}) has a note '{sharedNote.Title}' missing from the Owner!");
            }
            if (!sharedNote.IsShared)
            {
                return (false, $"SharedWith (O:{OwnerId}, R:{RecipientId}) has a note '{sharedNote.Title}' not marked as Shared!");
            }
        }
        foreach (var sharedAppt in Appointments)
        {
            if (!sharedAppt.MatchesOneOf(owner.Appointments))
            {
                return (false, $"SharedWith (O:{OwnerId}, R:{RecipientId}) has an appointment '{sharedAppt.DisplayTitle}' missing from the Owner!");
            }
            if (!sharedAppt.IsShared)
            {
                return (false, $"SharedWith (O:{OwnerId}, R:{RecipientId}) has an appointment '{sharedAppt.DisplayTitle}' not marked as Shared!");
            }
        }
        return (true, "");
    }

    // Helper called inside a write context.
    public void AddShared(IEnumerable<Appointment> appointments, IEnumerable<Note> notes)
    {
        foreach (var appt in appointments)
        {
            Appointments.Add(appt.Clone());
        }
        foreach (var note in notes)
        {
            Notes.Add(note.Clone());
        }
    }

    // Add shared items if they are not already present
    public void AddSharedIfNew(IEnumerable<Appointment> appointments, IEnumerable<Note> notes)
    {
        if (Appointments.Count == 0)
        {
            foreach (var appt in appointments)
            {
                Appointments.Add(appt.Clone());
            }
        }
        else
        {
            foreach (var appt in appointments)
            {
                if (!appt.MatchesOneOf(Appointments))
                {
                    Appointments.Add(appt.Clone());
                }
            }

        }
        if (Notes.Count == 0)
        {
            foreach (var note in notes)
            {
                Notes.Add(note.Clone());
            }
        }
        else
        {
            var toScan = Notes.AsRealmQueryable();
            foreach (var note in notes)
            {                    
                if (!note.MatchesOneOf(toScan))
                {
                    Notes.Add(note.Clone());
                }
            }

        }
    } // AddSharedIfNew
}

