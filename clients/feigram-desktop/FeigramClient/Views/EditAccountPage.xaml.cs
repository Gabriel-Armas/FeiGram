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
using System.Windows.Navigation;
using System.Windows.Shapes;
using FeigramClient.Models;

namespace FeigramClient.Views
{
    /// <summary>
    /// Lógica de interacción para EditAccountPage.xaml
    /// </summary>
    public partial class EditAccountPage : Page
    {
        private FullUser _cuenta;
        private readonly Action cerrarModalCallback;


        public EditAccountPage(FullUser cuenta, Action cerrarModal)
        {
            InitializeComponent();
            _cuenta = cuenta;
            DataContext = cuenta;
            cerrarModalCallback = cerrarModal;
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para guardar los cambios...

            // Cierra el modal
            cerrarModalCallback?.Invoke();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            cerrarModalCallback?.Invoke();
        }
    }
}
