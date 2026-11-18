using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Avtokolonna1456.PasswordManager.ViewModels;

namespace Avtokolonna1456.PasswordManager.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            ActionsPopup.IsOpen = !ActionsPopup.IsOpen;
        }

        private void ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            ActionsPopup.IsOpen = false;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var source = e.OriginalSource as DependencyObject;
            if (FindAncestor<Button>(source) != null) return;
            if (FindAncestor<DataGridRow>(source) == null) return;

            var vm = DataContext as MainViewModel;
            if (vm?.EditCommand?.CanExecute(null) == true)
            {
                vm.EditCommand.Execute(null);
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T match) return match;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
