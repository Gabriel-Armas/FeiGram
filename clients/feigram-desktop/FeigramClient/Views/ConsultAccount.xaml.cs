using FeigramClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Lógica de interacción para ConsultAccount.xaml
    /// </summary>
    public partial class ConsultAccount : Page
    {
        public ObservableCollection<FullUser> ListaDeCuentas { get; set; }

        public ConsultAccount()
        {
            InitializeComponent();
            ListaDeCuentas = new ObservableCollection<FullUser>();

            // Simulación: agrega un usuario de ejemplo
            ListaDeCuentas.Add(new FullUser
            {
                Name = "Ana López",
                Email = "ana.lopez@ejemplo.com",
                Tuition = "A12345678"
            });

            this.DataContext = this;
        }

        private void AgregarCuenta_Click(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Visible;
            ModalFrame.Navigate(new RegisterAccount(Overlay));
        }

        private void Editar_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;

            var cuenta = boton?.Tag as FullUser;

            if (cuenta != null)
            {
                ModalFrame.Navigate(new EditAccountPage(cuenta, CerrarModal));
                Overlay.Visibility = Visibility.Visible;
            }
        }

        private void Banear_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var cuenta = boton?.Tag as FullUser;

            if (cuenta != null)
            {
                ModalFrame.Navigate(new BanAccountPage(cuenta, CerrarModal));
                Overlay.Visibility = Visibility.Visible;
            }
        }

        private void CerrarModal()
        {
            ModalFrame.Content = null;
            Overlay.Visibility = Visibility.Collapsed;
        }

    }




    public class Cuenta
    {
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Matricula { get; set; }
    }
}
