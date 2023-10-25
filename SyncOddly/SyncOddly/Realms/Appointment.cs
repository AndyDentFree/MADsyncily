using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using MongoDB.Bson;
using Realms;

namespace SyncOddly.Models;

public class Appointment : EmbeddedObject
{
    [MapTo("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    [MapTo("when")]
    public DateTimeOffset When { get; set; }

    [MapTo("duration")]
    public int Duration { get; set; }  // minutes

    [MapTo("why")]
    public String Why { get; set; }

    [MapTo("isShared")]
    public bool IsShared { get; set; }  // indicates is like a copy is within a SharedWith but may lag deletions from there

    public String DisplayTitle
    {
        get => When.ToString("g") + " " + Why;
    }

    public Appointment Clone()
    {
        return new Appointment { Id = this.Id, When = this.When, Duration = this.Duration, Why = this.Why, IsShared = this.IsShared };
    }

    public bool MatchesOneOf(IList<Appointment> appointments)
    {
        return appointments.Any(a => a.Id == this.Id);
    }

    // use on result from AsRealmQueryable, should perform search in DB code rather than iterating instantiating all
    public bool MatchesOneOf(IQueryable<Appointment> appointments)
    {
        return appointments.Any(a => a.Id == this.Id);
    }

    public override bool Equals(Object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            Appointment a = (Appointment)obj;
            return Id == a.Id &&
                When == a.When &&
                Why == a.Why &&
                Duration == a.Duration &&
                IsShared == a.IsShared;
        }
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

}

