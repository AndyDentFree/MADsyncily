using Xamarin.Essentials;
using System.Diagnostics;
using MongoDB.Bson;

namespace SyncOddly;

// WARNING if add any persisted settings remember to read them in the ctor 
public class Settings
{
    public enum AppMode
    {
        Debug,
        App
    }

    public AppMode Mode
    {
        get
        {
            return _mode;
        }
        set
        {
            _mode = value;
            Preferences.Set(nameof(AppMode), value == AppMode.App);
        }
    }
    private AppMode _mode;

    public bool UsingServer
    {
        get
        {
            return _usingServer;
        }
        set
        {
            _usingServer = value;
            Preferences.Set(nameof(UsingServer), value);
        }
    }
    private bool _usingServer;

    public ObjectId? LoggedInUserId  // see Person.UserId, only valid if UsingServer
    {
        get
        {
            return _usingServer ? _loggedInUserId : null;
        }
        set
        {
            if (_usingServer) {
                _loggedInUserId = value;
                Preferences.Set(nameof(LoggedInUserId), value?.ToString() ?? "");
            }
        }
    }
    private ObjectId? _loggedInUserId;
    
    public string LastLoginEmail  // only valid if UsingServer
    {
        get
        {
            return _usingServer ? _lastLoginEmail : null;
        }
        set
        {
            if (_usingServer) {
                _lastLoginEmail = value;
                Preferences.Set(nameof(LastLoginEmail), value ?? "");
            } else {
                Debug.WriteLine($"Tried to set LastLoginEmail={value} but not in server mode");
            }
        }
    }
    private string _lastLoginEmail;

    public ObjectId? LocalRealmCurrentPersonId
    {
        get
        {
            return _usingServer ? null : _localRealmCurrentPersonId;
        }
        set
        {
            _localRealmCurrentPersonId = value;
            Preferences.Set(nameof(LocalRealmCurrentPersonId), value?.ToString() ?? "");
        }
    }
    private ObjectId? _localRealmCurrentPersonId;

    public bool HasSavedPreferences { get; private set; }

    public static Settings Current { get; private set; } = new Settings();

    public Settings()
    {
        HasSavedPreferences = Preferences.ContainsKey(nameof(UsingServer));
        _mode = Preferences.Get(nameof(AppMode), true) ? AppMode.App : AppMode.Debug;
        _usingServer = Preferences.Get(nameof(UsingServer), false);
        var possId = Preferences.Get(nameof(LoggedInUserId), "");
        ObjectId loadedId;
        _loggedInUserId = ObjectId.TryParse(possId, out loadedId) ? loadedId : null;
        _lastLoginEmail = Preferences.Get(nameof(LastLoginEmail), "");
        var localId = Preferences.Get(nameof(LocalRealmCurrentPersonId), "");
        ObjectId loadedLocalId;
        _localRealmCurrentPersonId = ObjectId.TryParse(localId, out loadedLocalId) ? loadedLocalId : null;
    }
}

