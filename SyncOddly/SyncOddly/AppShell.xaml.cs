using System;
using System.Collections.Generic;
using SyncOddly.ViewModels;
using SyncOddly.Views;
using Xamarin.Forms;
using static SyncOddly.Settings;

namespace SyncOddly;

public partial class AppShell : Xamarin.Forms.Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(SharedWithDetailPage), typeof(SharedWithDetailPage));
        Routing.RegisterRoute(nameof(EditPersonPage), typeof(EditPersonPage));
        Routing.RegisterRoute(nameof(EditAppointmentPage), typeof(EditAppointmentPage));
        Routing.RegisterRoute(nameof(EditNotePage), typeof(EditNotePage));
    }

}
