using MongoDB.Bson;
using SyncOddly.ViewModels;
using Xamarin.Forms;
using SyncOddly.Models;

namespace SyncOddly.Views;


public partial class SharedWithDetailPage : ContentPage
{
    private SharedWithDetailViewModel _viewModel;

    public SharedWithDetailPage()
    {
        InitializeComponent();
        BindingContext = new SharedWithDetailViewModel();
    }
}
