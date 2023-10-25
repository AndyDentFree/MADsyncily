using NUnit.Framework;
using SyncOddly.Models;
using System;

namespace TestSyncOddly;


// might not do anything as these are embedded (as are Notes) and so not used outside of Person or SharedWith
public class AppointmentTests : RealmTestBase
{
    [Test]
    public void ConfirmTestsAppearInRunner()
    {
        Assert.Pass();
    }
}