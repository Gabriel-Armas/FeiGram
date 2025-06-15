using FeigramClient.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FeigramClient.Views
{
    public partial class BanAccountPage : Page
    {
        private FullUser _cuenta;
        private readonly Action cerrarModalCallback;

        public BanAccountPage(FullUser cuenta, Action cerrarModal)
        {
            InitializeComponent();
            _cuenta = cuenta;
            DataContext = cuenta;
            cerrarModalCallback = cerrarModal;
        }

        private void Confirmar_Click(object sender, RoutedEventArgs e)
        {
            // Aquí va la lógica de banear, por ejemplo: llamada a API, etc.
            MessageBox.Show($"Usuario {_cuenta.Name} baneado.");

            cerrarModalCallback?.Invoke();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            cerrarModalCallback?.Invoke();
        }
    }
}
