using System;
using SyncOddly.Models;
using Xamarin.Forms;

namespace SyncOddly.Views;

public class NoteTemplateSelector : DataTemplateSelector
{
    public DataTemplate TaskTemplate { get; set; }
    public DataTemplate NoteTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        var note = item as Note;
        if (note == null)
            return null;

        return note.IsTask ? TaskTemplate : NoteTemplate;
    }
}
