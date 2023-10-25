using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson;
using Realms;

namespace SyncOddly.Models;

public static class RealmFieldExtensions
{
    /// <summary>
    /// Return a copy truncated to ms accuracy as stored in Realm
    /// </summary>
    /// <remarks>Required for MongoDB Subscription equality tests</remarks>
    /// <param name="date"></param>
    /// <returns></returns>
    public static DateTimeOffset RealmAccurateMS(this DateTimeOffset date)
    {
        return new DateTimeOffset(date.Year, date.Month, date.Day,
            date.Hour, date.Minute, date.Second,
            date.Millisecond, date.Offset);
    }
}