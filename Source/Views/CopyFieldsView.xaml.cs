using DuplicateHider.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace DuplicateHider.Views
{
    /// <summary>
    /// Interaktionslogik für CopyFieldsView.xaml
    /// </summary>
    public partial class CopyFieldsView : UserControl
    {
        public CopyFieldsView()
        {
            InitializeComponent();
        }

        public CopyFieldsView(CopyFieldsViewModel model) : this()
        {
            DataContext = model;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Parent is Window window)
            {
                window.Close();
            }
        }
    }
}
