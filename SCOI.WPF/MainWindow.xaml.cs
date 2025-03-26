using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using SCOI.WPF.ViewModels;

namespace SCOI.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point? _lastMousePosition;
        private Thread updateThread;
        public MainWindow()
        {
            DataContext = new MainWindowVM();
            InitializeComponent();
            updateThread = new Thread(() => { isUpdateStarted = true; ((MainWindowVM)DataContext).Function.UpdateAction(); isUpdateStarted = false; })
            { IsBackground = true };
        }
        public void Add_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Файлы рисунков (*.png, *.jpg)|*.png;*.jpg";
            bool? success = ofd.ShowDialog();
            if (success == true)
            {
                string path = ofd.FileName;

            }

        }
        private bool isMousePressed = false;
        private bool isUpdateStarted = false;
        public void MouseMove(object sender, MouseEventArgs e)
        {
            _lastMousePosition = e.GetPosition(func);
            if (isMousePressed == true)
            {
                ((MainWindowVM)DataContext).Function.ChangePoint(((Vector)_lastMousePosition).X, ((Vector)_lastMousePosition).Y);



                //((MainWindowVM)DataContext).Function.UpdateAction();
            }
        }

        public void MouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                ((MainWindowVM)DataContext).Function.AddPoint(((Vector)_lastMousePosition).X, ((Vector)_lastMousePosition).Y);
                isMousePressed = true;

            }
            else if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                ((MainWindowVM)DataContext).Function.DeletePoint(((Vector)_lastMousePosition).X, ((Vector)_lastMousePosition).Y);
            }
        }
        public void MouseUp(object sender, MouseEventArgs args)
        {
            isMousePressed = false;
            ((MainWindowVM)DataContext).Function.UpdateAction();
        }
        private void addButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}