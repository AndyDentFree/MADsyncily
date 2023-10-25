using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SyncOddly.ViewModels;

namespace SyncOddly.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditNotePage : ContentPage
    {
        private EditNoteViewModel _viewModel;

        public EditNotePage()
        {
            InitializeComponent();

            _viewModel = new EditNoteViewModel();
            BindingContext = _viewModel;

            SaveButton.Clicked += async (s, e) =>
            {
                _viewModel.DoSave();
                await Navigation.PopAsync();
            };
        }
    }
}
