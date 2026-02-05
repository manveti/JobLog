using System.Windows;

namespace JobLog {
    /// <summary>
    /// Interaction logic for JobCloseWindow.xaml
    /// </summary>
    public partial class JobCloseWindow : Window {
        public bool valid = false;

        public JobCloseWindow() {
            this.InitializeComponent();
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
