using System.Windows;
using System.Windows.Controls;

namespace JobLog {
    /// <summary>
    /// Interaction logic for JobEventWindow.xaml
    /// </summary>
    public partial class JobEventWindow : Window {
        public const int IDX_POSTED = 0;
        public const int IDX_SAVED = 1;
        public const int IDX_APPLIED = 2;
        public const int IDX_CLOSED = 3;
        public const int IDX_OTHER = 4;

        public bool valid = false;

        public JobEventWindow() {
            this.InitializeComponent();
        }

        protected void descriptionLstChanged(object sender, SelectionChangedEventArgs e) {
            int idx = this.description_lst.SelectedIndex;
            this.description_box.Visibility = ((idx == IDX_CLOSED) || (idx == IDX_OTHER) ? Visibility.Visible : Visibility.Hidden);
        }

        protected void doOk(object sender, RoutedEventArgs e) {
            this.valid = true;
            this.Close();
        }

        protected void doCancel(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
