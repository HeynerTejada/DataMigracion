using AppDesktop.ViewModel;
using System.Windows;

namespace AppDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IMainViewModel _ViewModel;

        public MainWindow(IMainViewModel ViewModel)
        {
            _ViewModel=ViewModel;
            DataContext = _ViewModel;
            InitializeComponent();
        }
    }
}