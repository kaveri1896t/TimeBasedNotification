using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PushNotification.ViewModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PushNotification.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LocalNotificationPage : ContentPage
    {
        public LocalNotificationPage()
        {
            InitializeComponent();
            BindingContext = new LocalNotificationPageViewModel();

        }
    }
}