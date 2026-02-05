using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace JobLog {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        protected JobTracker tracker;

        public MainWindow() {
            //TODO: maybe make data_dir configurable
            string data_dir = Path.Join(Directory.GetCurrentDirectory(), "JobLogData");
            this.tracker = new JobTracker(data_dir);
            this.InitializeComponent();
            //TODO: view bindings
        }

        //TODO: handlers for filters/sorting

        protected void addJobPosting(object sender, RoutedEventArgs e) {
            JobPostingWindow dlg = new JobPostingWindow();
            dlg.Owner = this;
            dlg.ShowDialog();
            if (!dlg.valid) {
                return;
            }

            //TODO: ...
        }

        public static void fix_listview_column_widths(ListView listView) {
            GridView grid = listView.View as GridView;
            if (grid is null) {
                return;
            }
            foreach (GridViewColumn col in grid.Columns) {
                col.Width = col.ActualWidth;
                col.Width = double.NaN;
            }
        }
    }
}
