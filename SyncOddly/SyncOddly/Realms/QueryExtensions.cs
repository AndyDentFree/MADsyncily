using System;
using Realms;
using MongoDB.Bson;
using System.Linq;

namespace SyncOddly.Models;

public static class QueryExtensions
{
    public static IQueryable<SharedWith> Shared(this Realm realm, ObjectId ownerId)
    {
        return realm.All<SharedWith>().Where(sw => sw.OwnerId == ownerId);
    }


    public static IQueryable<SharedWith> SharedTo(this Realm realm, ObjectId recipientId)
    {
        return realm.All<SharedWith>().Where(sw => sw.RecipientId == recipientId);
    }


    public static IQueryable<SharedWith> SharedBetween(this Realm realm, ObjectId ownerId, ObjectId recipientId)
    {
        return realm.All<SharedWith>().Where(sw => sw.OwnerId == ownerId && sw.RecipientId == recipientId);
    }


    public static IQueryable<SharedWith> SharedByAppointment(this Realm realm, ObjectId ownerId, ObjectId appointmentId)
    {
        return realm.All<SharedWith>().Filter("ownerId == $0 AND ANY appointments._id == $1", ownerId, appointmentId);
    }


    public static IQueryable<SharedWith> SharedByNote(this Realm realm, ObjectId ownerId, ObjectId noteId)
    {
        return realm.All<SharedWith>().Filter("ownerId == $0 AND ANY notes._id == $1", ownerId, noteId);
    }

}

