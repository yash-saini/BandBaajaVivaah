using System.Windows;
using System.Windows.Controls;

namespace BandBaajaVivaah.WPF.Views
{
    /// <summary>
    /// Interaction logic for PageToolBarView.xaml
    /// </summary>
    public partial class PageToolBarView : UserControl
    {
        public event EventHandler? BackButtonClicked;
        public event EventHandler? AddButtonClicked;
        public event EventHandler? DeleteButtonClicked;
        public event EventHandler? RefreshButtonClicked;

        public event EventHandler? FirstPageClicked;
        public event EventHandler? PreviousPageClicked;
        public event EventHandler? NextPageClicked;
        public event EventHandler? LastPageClicked;

        public Button DeleteButtonControl => DeleteButton;

        public static readonly DependencyProperty CurrentPageProperty =
       DependencyProperty.Register("CurrentPage", typeof(int), typeof(PageToolBarView), new PropertyMetadata(1));

        public int CurrentPage
        {
            get { return (int)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly DependencyProperty TotalPagesProperty =
            DependencyProperty.Register("TotalPages", typeof(int), typeof(PageToolBarView), new PropertyMetadata(1));

        public int TotalPages
        {
            get { return (int)GetValue(TotalPagesProperty); }
            set { SetValue(TotalPagesProperty, value); }
        }

        public static readonly DependencyProperty IsFirstPageProperty =
            DependencyProperty.Register("IsFirstPage", typeof(bool), typeof(PageToolBarView), new PropertyMetadata(true));

        public bool IsFirstPage
        {
            get { return (bool)GetValue(IsFirstPageProperty); }
            set { SetValue(IsFirstPageProperty, value); }
        }

        public static readonly DependencyProperty IsLastPageProperty =
            DependencyProperty.Register("IsLastPage", typeof(bool), typeof(PageToolBarView), new PropertyMetadata(true));

        public bool IsLastPage
        {
            get { return (bool)GetValue(IsLastPageProperty); }
            set { SetValue(IsLastPageProperty, value); }
        }

        public static readonly DependencyProperty IsDeleteEnabledProperty =
        DependencyProperty.Register("IsDeleteEnabled", typeof(bool), typeof(PageToolBarView), new PropertyMetadata(false));

        public bool IsDeleteEnabled
        {
            get { return (bool)GetValue(IsDeleteEnabledProperty); }
            set { SetValue(IsDeleteEnabledProperty, value); }
        }

        public PageToolBarView()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        { 
            RefreshButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void FirstPageButton_Click(object sender, RoutedEventArgs e)
        {
            FirstPageClicked?.Invoke(this, EventArgs.Empty);
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            PreviousPageClicked?.Invoke(this, EventArgs.Empty);
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            NextPageClicked?.Invoke(this, EventArgs.Empty);
        }

        private void LastPageButton_Click(object sender, RoutedEventArgs e)
        {
            LastPageClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
