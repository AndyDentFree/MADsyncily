using System;
using System.Collections.Generic;
using Xamarin.Forms;
using SyncOddly.ViewModels;

namespace SyncOddly.Views
{	
	public partial class FirstRunPage : ContentPage
	{
		public FirstRunPage ()
		{
			InitializeComponent ();
            BindingContext = new FirstRunViewModel();
        }
	}
}

