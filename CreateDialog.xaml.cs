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

namespace PlatformyTechnologiczne
{
    /// <summary>
    /// Logika interakcji dla klasy CreateDialog.xaml
    /// </summary>
    public partial class CreateDialog : Window
    {
        public bool Cancel { get; set; }

        public CreateDialog()
        {
            InitializeComponent();
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            Cancel = false;
            Close();
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Cancel = true;
            Close();
        }
    }
}
