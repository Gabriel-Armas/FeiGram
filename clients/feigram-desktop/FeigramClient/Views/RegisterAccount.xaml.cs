using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FeigramClient.Views
{
    /// <summary>
    /// Lógica de interacción para RegisterAccount.xaml
    /// </summary>
    public partial class RegisterAccount : Page
    {
        private Border _overlay;
        public RegisterAccount(Border Overlay)
        {
            InitializeComponent();
            _overlay = Overlay;
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            _overlay.Visibility = Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _overlay.Visibility = Visibility.Collapsed;
        }
    }
}
