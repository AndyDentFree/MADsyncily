using System;
using System.Collections.Generic;
using MongoDB.Bson;
using Realms;

namespace SyncOddly.Models;

/// Common subset of Person fields, all records sync to all clients
public class PersonLookup : RealmObject
{
    [PrimaryKey]
    [MapTo("_id")]
    public ObjectId Id { get; set; }  // copied from Person.Id

    [MapTo("name")]
    [Required]
    public string Name { get; set; }

    [MapTo("email")]
    [Required]
    public string Email { get; set; }  // used as username & searching for people you know
}

