using MongoDB.Bson;
using NUnit.Framework;
using SyncOddly.Models;
using System.Linq;

namespace TestSyncOddly;


public class SharedWithTests : RealmTestBase
{

    [Test]
    public void SharedWithSelfIsInvalid()
    {
        var p = new Person();
        var badSW = new SharedWith { OwnerId = p.Id, RecipientId = p.Id };

        var (isOK, msg) = badSW.IsValidForOwner(p, testRealm);
        Assert.IsFalse(isOK, msg);
    }
}