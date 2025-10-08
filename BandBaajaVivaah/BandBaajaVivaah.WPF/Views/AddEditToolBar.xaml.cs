using System.Windows;
using System.Windows.Controls;

namespace BandBaajaVivaah.WPF.Views
{
    /// <summary>
    /// Interaction logic for AddEditToolBar.xaml
    /// </summary>
    public partial class AddEditToolBar : UserControl
    {
        public event EventHandler BackButtonClicked;
        public event EventHandler SaveButtonClicked;

        public static readonly DependencyProperty IsSaveEnabledProperty =
            DependencyProperty.Register(nameof(IsSaveEnabled), typeof(bool), typeof(AddEditToolBar),
                new PropertyMetadata(true));

        public static readonly DependencyProperty IsBackEnabledProperty =
            DependencyProperty.Register(nameof(IsBackEnabled), typeof(bool), typeof(AddEditToolBar),
                new PropertyMetadata(true));

        public bool IsSaveEnabled
        {
            get { return (bool)GetValue(IsSaveEnabledProperty); }
            set { SetValue(IsSaveEnabledProperty, value); }
        }

        public bool IsBackEnabled
        {
            get { return (bool)GetValue(IsBackEnabledProperty); }
            set { SetValue(IsBackEnabledProperty, value); }
        }
        public AddEditToolBar()
        {
            InitializeComponent();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackButtonClicked?.Invoke(this, EventArgs.Empty);
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
