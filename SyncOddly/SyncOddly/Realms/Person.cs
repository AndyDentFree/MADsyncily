using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Realms;
//using Realms.Sync;

namespace SyncOddly.Models;

public class Person : RealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();  // copied to ownerId or recipientId, in SharedWith

    [MapTo("user_id")]
    [Required]
    public ObjectId? UserId { get; set; } = ObjectId.Empty;  // only set if there's an associated login

    [MapTo("name")]
    [Required]
    public string Name { get; set; }

    [MapTo("email")]
    [Required]
    public string Email { get; set; }  // used as username - see LoginViewModel

    [MapTo("phone")]
    public string Phone { get; set; }  // optional but useful for searching
    
    [MapTo("created")]
    public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;

    #region Content duplicated into SharedWith, repeated per sharing relationship
    [MapTo("appointments")]
    public IList<Appointment> Appointments { get; }

    [MapTo("notes")]
    public IList<Note> Notes { get; }
    #endregion

    [Ignored]
    public IQueryable<Appointment> SharedAppointments
    {
        get { return Appointments.Where(a => a.IsShared).AsQueryable(); }
    }

    [Ignored]
    public IQueryable<Note> SharedNotes
    {
        get { return Notes.Where(a => a.IsShared).AsQueryable(); }
    }
}
