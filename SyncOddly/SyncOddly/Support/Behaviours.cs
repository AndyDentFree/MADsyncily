using Xamarin.Forms;

namespace SyncOddly.Views;
public class RequiredValidatorBehavior : Behavior<Entry>
{
    private string _defaultBorderColor;  //TODO generalise

    protected override void OnAttachedTo(Entry bindable)
    {
        bindable.TextChanged += OnEntryTextChanged;
        base.OnAttachedTo(bindable);
    }

    protected override void OnDetachingFrom(Entry bindable)
    {
        bindable.TextChanged -= OnEntryTextChanged;
        base.OnDetachingFrom(bindable);
    }

    private void OnEntryTextChanged(object sender, TextChangedEventArgs args)
    {
        var entry = (Entry)sender;

        if (string.IsNullOrEmpty(entry.Text)) {
            entry.BackgroundColor = Color.Red; // or any other visual feedback
        } else {
            entry.BackgroundColor = string.IsNullOrEmpty(_defaultBorderColor) ? Color.Default : Color.FromHex(_defaultBorderColor);
        }
    }
}
