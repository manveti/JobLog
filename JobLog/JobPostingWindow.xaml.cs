using System.Windows;

namespace JobLog {
    /// <summary>
    /// Interaction logic for JobPostingWindow.xaml
    /// </summary>
    public partial class JobPostingWindow : Window {
        public bool valid = false;

        public JobPosting posting {
            get => this.posting_ctrl.getPosting();
            set => this.posting_ctrl.updatePosting(value);
        }

        public JobPostingWindow(JobPosting posting = null) {
            this.InitializeComponent();
            if (posting != null) {
                this.posting_ctrl.updatePosting(posting);
            }
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
