using System.Windows;

namespace JobLog {
    /// <summary>
    /// Interaction logic for LinkWindow.xaml
    /// </summary>
    public partial class LinkWindow : Window {
        public bool valid = false;

        public LinkWindow() {
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
