using System.Windows;

namespace JobLog {
    /// <summary>
    /// Interaction logic for BlacklistEntryWindow.xaml
    /// </summary>
    public partial class BlacklistEntryWindow : Window {
        public bool valid = false;

        public BlacklistEntryWindow() {
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
