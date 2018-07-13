using System.Text;
using System.Windows;
using System.Windows.Input;
using RFTestAUX.ViewModel;

namespace RFTestAUX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
        }
        private void Flipper_OnIsFlippedChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            System.Diagnostics.Debug.WriteLine("Card is flipped = " + e.NewValue);
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            vm.ParaOperateCommand.Execute($"Apply&{EditTemp.Text}&{EditSource.Text}&{EditCmpl.Text}&{EditBand.Text}&{EditTime.Text}");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (DataContext as MainViewModel).WindowLoadedCommand.Execute(null);

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (DataContext as MainViewModel).WindowClosingCommand.Execute(null);
        }
    }
}