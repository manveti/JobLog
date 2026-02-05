using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
