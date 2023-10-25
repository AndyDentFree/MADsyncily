using System;
using System.Windows.Input;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using SyncOddly.Models;
using System.Diagnostics;

namespace SyncOddly.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    public SampleDataGenerator Generator { get; set; } = new SampleDataGenerator { NumPeople = 20 };
    public bool UsingServer => Settings.Current.UsingServer;  //TODO toggling should force logout https://github.com/AndyDentFree/xamarealms/issues/6
    public bool CanWipeLocal { get { return !UsingServer; } }

    public SettingsViewModel()
    {
        Title = "Settings";
        GenerateSampleData = new Command(OnGenerateSampleData);
        WipeLocalData = new Command(OnWipeLocalData);
        LogoutServer = new Command(OnLogoutServer);
    }

    public ICommand GenerateSampleData { get; }
    public ICommand WipeLocalData { get; }
    public ICommand LogoutServer { get; }

    async void OnGenerateSampleData()
    {
        Debug.WriteLine($"Creating sample data {(UsingServer ? "On Server…" : "Locally…")}");
        Doc.Current.EnsureHaveOpenRealm(UsingServer);
        await Generator.MakeSyncSafeSampleData(Doc.Current.ActiveRealm, UsingServer);
        Debug.WriteLine("Finished creating sample data");
        await Doc.Current.NavToMain();
    }
    
    void OnWipeLocalData()
    {
        if (UsingServer)
        {
            Debug.WriteLine($"Unable to wipe local data because using server");
            return;
        }
        Doc.Current.WipeLocalData();        
    }

    async void OnLogoutServer()
    {
        if (!UsingServer) {
            Debug.WriteLine($"Logout invoked when not using server");
            return;
        }
        await RealmService.LogoutAsync();
        await Doc.Current.NavToLogin();
    }
}
