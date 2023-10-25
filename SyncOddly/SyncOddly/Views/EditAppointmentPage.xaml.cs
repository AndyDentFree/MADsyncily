using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SyncOddly.ViewModels;

namespace SyncOddly.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditAppointmentPage : ContentPage
    {
        private EditAppointmentViewModel _viewModel;

        public EditAppointmentPage()
        {
            InitializeComponent();

            _viewModel = new EditAppointmentViewModel();
            BindingContext = _viewModel;

            SaveButton.Clicked += async (s, e) =>
            {
                _viewModel.DoSave();
                await Navigation.PopAsync();
            };
        }
    }
}
