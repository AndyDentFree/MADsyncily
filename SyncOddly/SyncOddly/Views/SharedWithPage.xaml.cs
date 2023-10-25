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

public partial class SharedWithPage : BasePage
{
    SharedWithViewModel _viewModel;

    public SharedWithPage()
    {
        InitializeComponent();
        BindingContext = _viewModel = new SharedWithViewModel();
    }
}
