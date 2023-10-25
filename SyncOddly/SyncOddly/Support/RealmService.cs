using System;
//using System.Text.Json;
using System.Threading.Tasks;
using Realms;
using Realms.Sync;
using System.Linq;
using System.Diagnostics;
using MongoDB.Bson;


namespace SyncOddly.Models;

public static class RealmService
{
    #region subscription names
    // see DataSubscriptions.md
    public const string SubLookups = "Lookups";
    public const string SubMine = "Mine";
    public const string SubSharing = "Sharing";
    public const string SubInShared = "InShared";
    public const string SubPeopleSamples = "PeopleSamples";
    public const string SubSharingSamples = "SharingSamples";
    #endregion

    private static bool serviceInitialised;

    private static Realms.Sync.App app;

    private static Realm mainThreadRealm;

    public static User CurrentUser => app.CurrentUser;

    //private static String _lastLoginEmail;  // until use other auth
   // public static String LastLoginEmail => _lastLoginEmail;

    private static ObjectId? _currentUserBinaryId = null;
    public static ObjectId? CurrentUserBinaryId
    {
        get
        {
            if (_currentUserBinaryId == null && app?.CurrentUser != null) {
                ObjectId currId;
                _currentUserBinaryId = ObjectId.TryParse(app.CurrentUser.Id, out currId) ? currId : null;
            }
            return _currentUserBinaryId;
        }
    }

    //public static async Task Init()
    public static void Init()
    {
        if (serviceInitialised) {
            return;
        }
        /*
        using Stream fileStream = await FileSystem.OpenAppPackageFileAsync("atlasConfig.json");
        using StreamReader reader = new StreamReader(fileStream);
        var fileContent = await reader.ReadToEndAsync();

        var config = JsonSerializer.Deserialize<RealmAppConfig>(fileContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true});
        */
        PUT YOUR OWN MONGODB APPID IN THE NEXT LINE
        var appConfiguration = new AppConfiguration("YOUR APPID HERE");
        /*{
            BaseUri = new Uri(config.BaseUrl)
        };
        */
        Realms.Logging.Logger.LogLevel = Realms.Logging.LogLevel.Debug;
        app = Realms.Sync.App.Create(appConfiguration);

        serviceInitialised = true;
    }

    public static bool CurrentUserLoginMatches(ObjectId loginId, String email)
    {
        // used property so loads CurrentUserBinaryId
        return CurrentUserBinaryId is not null &&
               _currentUserBinaryId == loginId &&
               email == Settings.Current.LastLoginEmail;
        
        //TODO expand to use other login matching depending on method
    }

    public static Realm GetMainThreadRealm()
    {
        return mainThreadRealm ??= GetRealm();
    }

    public static Realm GetRealm()
    {
        if (app?.CurrentUser is null)
            return null;
        var config = new FlexibleSyncConfiguration(app.CurrentUser)
        {
            PopulateInitialSubscriptions = (realm) =>
            {
                realm.Subscriptions.Add(realm.All<PersonLookup>(), new SubscriptionOptions() { Name = SubLookups });
            }
        };
        config.SchemaVersion = Doc.SchemaVersionNumber;
        return Realm.GetInstance(config);
    }

    public static async Task RegisterAsync(string email, string password)
    {
        await app.EmailPasswordAuth.RegisterUserAsync(email, password);
    }

    public static async Task LoginAsync(string email, string password)
    {
        await app.LogInAsync(Credentials.EmailPassword(email, password));
        Settings.Current.LastLoginEmail = email;
        Settings.Current.LoggedInUserId = CurrentUserBinaryId;  // stash for quick reopen

        //This will populate the initial set of subscriptions the first time the realm is opened
        using var realm = GetRealm();
        await UpdateCurrentPersonSubs(realm);
        //TODO call UpdateLookupSubs but needs GUI refresh, then can remove from populateinitial
    }

    public static async Task LogoutAsync()
    {
        //TODO maybe preserve subs on logout so fast to login again
        if (mainThreadRealm is not null) {
            await RemoveCurrentPersonSubs(mainThreadRealm);  // keeps SubLookup to all PersonLookups
        }
        await app.CurrentUser.LogOutAsync();
        Settings.Current.LoggedInUserId = null;
        _currentUserBinaryId = null;
       /*
        *leave the realm open and active so can login again 
        mainThreadRealm?.Dispose();
        mainThreadRealm = null;
       */
    }

    public static IQueryable<Person> CurrentUserPersonQuery(Realm realm)
    {
        return realm.All<Person>().Where(p => p.UserId == _currentUserBinaryId);
    }


    /// <summary>
    /// Refresh the filtered sync for the current Person record for other searches.
    /// </summary>
    /// <returns>false if logged out</returns>
    public static async Task<bool> UpdateCurrentPersonSubs(Realm realm)
    {
        if (_currentUserBinaryId is null)
            return false;

        realm.Subscriptions.Update(() =>
        {
            realm.Subscriptions.Add(CurrentUserPersonQuery(realm), new SubscriptionOptions() { Name = SubMine, UpdateExisting = true });
            // remove these two so if disconnected, removed data relating to person
            realm.Subscriptions.Remove(SubSharing);
            realm.Subscriptions.Remove(SubInShared);
        });

        //There is no need to wait for synchronization if we are disconnected
        if (realm.SyncSession.ConnectionState == ConnectionState.Disconnected)
            return true;
        await realm.Subscriptions.WaitForSynchronizationAsync();  // pulls in nested objects for this user so could be big chunk

        // now add the dependent subs        
        var numMe = realm.All<Person>().Count();
        if (numMe != 1) {
            if (numMe == 0)
                return true; // copes if no person as yet - see Doc.EnsureHaveCurrentUserComplete
            // bit of ugly hack to throw up user error here
            await SyncOddly.ViewModels.BaseViewModel.ShowAlertAsync("Data sync problem", $"Have {numMe} 'current people'", "Continue");
            return false;
        }
        // has to do two-stage as Person record needed to get key for other two subscriptions
        var currUser = realm.All<Person>().FirstOrDefault();
        UpdateCurrentPersonSharingSubs(realm, currUser?.Id);  //TODO consider not waiting again for this

        if (realm.SyncSession.ConnectionState != ConnectionState.Disconnected)
            await realm.Subscriptions.WaitForSynchronizationAsync();
        return true;
    }

    /// <summary>
    /// Second-stage subs setup only when have completed getting current user person loaded by subs
    /// </summary>
    /// <remarks>
    /// Doesn't wait for sync because also used when setting up stuff that won't have data to download
    /// </remarks>
    public static void UpdateCurrentPersonSharingSubs(Realm realm, ObjectId? personId)
    {
        if (personId is null)
            return;

        realm.Subscriptions.Update(() =>
        {
            realm.Subscriptions.Add(realm.All<SharedWith>().Where(sw => sw.OwnerId == personId), new SubscriptionOptions() { Name = SubSharing, UpdateExisting = true });
            realm.Subscriptions.Add(realm.All<SharedWith>().Where(sw => sw.RecipientId == personId), new SubscriptionOptions() { Name = SubInShared, UpdateExisting = true });
        });
    }

    private static async Task RemoveCurrentPersonSubs(Realm realm)
    {
        realm.Subscriptions.Update(() =>
        {
            realm.Subscriptions.Remove(SubMine);
            realm.Subscriptions.Remove(SubSharing);
            realm.Subscriptions.Remove(SubInShared);
        });
        await realm.Subscriptions.WaitForSynchronizationAsync();
    }

    /// <summary>
    /// Non async update used instead of populating initial as could be big download, can be slower
    /// </summary>
    /// <param name="realm"></param>
    public static void UpdateLookupSubs(Realm realm)
    {
        //TODO https://github.com/AndyDentFree/xamarealms/issues/11 use this from LoginAsync but also need GUI refresh on completion
        realm.Subscriptions.Update(() =>
        {
            realm.Subscriptions.Add(realm.All<PersonLookup>(), new SubscriptionOptions() { Name = SubLookups, UpdateExisting = true });
        });
    }
    
    public static void AddSampleSubs(Realm realm, DateTimeOffset exactCreationTimestamp)
    {
        realm.Subscriptions.Update(() =>
        {
            realm.Subscriptions.Add(realm.All<SharedWith>().Where(sw => sw.Created == exactCreationTimestamp), 
                new SubscriptionOptions() { Name = SubSharingSamples, UpdateExisting = true });
            realm.Subscriptions.Add(realm.All<Person>().Where(p => p.Created == exactCreationTimestamp), 
                new SubscriptionOptions() { Name = SubPeopleSamples, UpdateExisting = true });
        });
    }
    
    public static void RemoveSampleSubs(Realm realm)
    {
        realm.Subscriptions.Update(() =>
        {
            realm.Subscriptions.Remove(SubSharingSamples);
            realm.Subscriptions.Remove(SubPeopleSamples);
        });
    }
}

/*
 * hide struct parsed from JSON in template
public class RealmAppConfig
{
    public string AppId { get; set; }

    public string BaseUrl { get; set; }
}
*/
