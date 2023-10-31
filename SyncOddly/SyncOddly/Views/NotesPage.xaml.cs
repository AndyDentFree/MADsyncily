using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using SyncOddly.Models;
using SyncOddly.Views;
using SyncOddly.ViewModels;

namespace SyncOddly.Views;

public partial class NotesPage : BasePage
{
    NotesViewModel _viewModel;

    public NotesPage()
    {
        InitializeComponent();
        BindingContext = _viewModel = new NotesViewModel(buttonVisualsUpdate: UpdateLatchedButtonStates);
    }

    private void UpdateLatchedButtonStates()
    {
        //TODO hide Add button in ShowSharedToMe state https://github.com/AndyDentFree/MADsyncily/issues/25
        //TODO hide hoist common logic from Notes and Appointments https://github.com/AndyDentFree/MADsyncily/issues/28
        switch (Doc.Current.SharedListFilterState) {
            case ShareableListState.ShowAllMine:
                SetButtonVisuals("Latched", "Normal", "Normal");
                break;
            case ShareableListState.ShowSharing:
                SetButtonVisuals("Normal", "Latched", "Normal");
                break;
            case ShareableListState.ShowSharedToMe:
                SetButtonVisuals("Normal", "Normal", "Latched");
                break;
        }
    }

    private void SetButtonVisuals(string b1, string b2, string b3)
    {
        VisualStateManager.GoToState(ShowMineButton, b1);
        VisualStateManager.GoToState(ShowSharingButton, b2);
        VisualStateManager.GoToState(ShowSharedToMeButton, b3);

    }
}
