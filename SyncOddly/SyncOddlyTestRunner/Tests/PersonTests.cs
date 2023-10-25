using MongoDB.Bson;
using NUnit.Framework;
using System.Linq;

using SyncOddly.Models;
using SyncOddly.ViewModels;

namespace TestSyncOddly;

public class PersonTests : RealmTestBase
{

    [Test]
    public void TestViewModelUpdatesPersonLookup()
    {
        var p = new Person { Name = "Li", Email = "a@b" };
        var pvm = new EditPersonViewModel(p, isNew: true);
        pvm.DoSave();
        var (isOK, msg) = Doc.OverallDocIsValid(testRealm);
        Assert.IsTrue(isOK, msg);

        var p2 = new Person();
        var badSW = new SharedWith { OwnerId = p2.Id, RecipientId = p2.Id };

        var (isOK2, msg2) = badSW.IsValidForOwner(p2, testRealm);
        Assert.IsFalse(isOK2, msg2);
    }
}