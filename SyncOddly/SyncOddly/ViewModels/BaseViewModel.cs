using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Realms;
using Xamarin.Forms;

using SyncOddly.Models;
using System.Threading.Tasks;

namespace SyncOddly.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
{

    bool isBusy = false;
    public bool IsBusy
    {
        get { return isBusy; }
        set { SetProperty(ref isBusy, value); }
    }

    string title = string.Empty;
    public string Title
    {
        get { return title; }
        set { SetProperty(ref title, value); }
    }

    protected bool SetProperty<T>(ref T backingStore, T value,
        [CallerMemberName] string propertyName = "",
        Action onChanged = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        onChanged?.Invoke();
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Show a modal alert which pauses execution
    /// </summary>
    /// <remarks
    /// Unlike PushModalAsync, awaiting this method will not continue until the alert is dismissed.
    /// A common gotcha with PushModalAsync is that the await continues as soon as the modal appears.
    /// </remarks>
    /// <returns></returns>

    public static Task ShowAlertAsync(string title, string message, string acceptButton, string cancelButton=null)
    {
        return Application.Current.MainPage.DisplayAlert(title, message, acceptButton, cancelButton);
    }

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        var changed = PropertyChanged;
        if (changed == null)
            return;

        changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}
