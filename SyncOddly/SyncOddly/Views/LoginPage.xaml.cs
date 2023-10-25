using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncOddly.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SyncOddly.ViewModels;

namespace SyncOddly.Views;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class LoginPage : BasePage
{
    public LoginPage()
    {
        InitializeComponent();
        BindingContext = new LoginViewModel();
    }
}
