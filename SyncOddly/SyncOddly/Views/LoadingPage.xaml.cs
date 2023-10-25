using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SyncOddly.ViewModels;

namespace SyncOddly.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingPage : ContentPage
    {
        private LoadingViewModel _viewModel;

        public LoadingPage()
        {
            InitializeComponent();
            _viewModel = new LoadingViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.AuthenticateAndNavToNext();
        }
    }
}