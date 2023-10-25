using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using Realms;

namespace SyncOddly.Models;

public class Note : EmbeddedObject
{
    [MapTo("_id")]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    [MapTo("title")]
    public String Title { get; set; }

    [MapTo("body")]
    public String Body { get; set; }

    [MapTo("isTask")]
    public bool IsTask { get; set; }  // bool to check out the TwoWay binding and having a checkbox in a list view

    [MapTo("isDone")]
    public bool IsDone { get; set; }  // only valid and visible if IsTask, use two bools for easier binding than a bool?

    [MapTo("isShared")]
    public bool IsShared { get; set; }  // indicates is like a copy is within a SharedWith but may lag deletions from there

    public Note Clone()
    {
        return new Note { Id = this.Id, Title = this.Title, Body = this.Body, IsTask = this.IsTask, IsDone = this.IsDone, IsShared = this.IsShared };
    }

    public bool MatchesOneOf(IList<Note> notes)
    {
        return notes.Any(n => n.Id == this.Id);
    }

    // use on result from AsRealmQueryable, should perform search in DB code rather than iterating instantiating all
    public bool MatchesOneOf(IQueryable<Note> notes)
    {
        return notes.Any(n => n.Id == this.Id);
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
            Note n = (Note)obj;
            return Id == n.Id &&
                Title == n.Title &&
                Body == n.Body &&
                IsTask == n.IsTask &&
                IsDone == n.IsDone &&
                IsShared == n.IsShared;
        }
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

}

