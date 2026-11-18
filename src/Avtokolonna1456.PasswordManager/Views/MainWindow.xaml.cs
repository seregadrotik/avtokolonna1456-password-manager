using System.Windows;
using System.Windows.Input;
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
            ActionsPopup.IsOpen = true;
        }

        private void ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            ActionsPopup.IsOpen = false;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm?.EditCommand?.CanExecute(null) == true)
            {
                vm.EditCommand.Execute(null);
            }
        }
    }
}
