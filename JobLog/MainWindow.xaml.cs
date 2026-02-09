using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace JobLog {
    public class JobPostingRow {
        protected const int SORT_EMPLOYER = 0;
        protected const int SORT_RECRUITER = 1;
        protected const int SORT_OPENED = 2;
        protected const int SORT_RECENT = 3;

        protected Guid _guid;
        public string recruiter;
        protected string _company;
        protected string _title;
        protected string _salary;
        public DateTime opened;
        public DateTime last_activity;

        public Guid guid => this._guid;
        public string company => this._company;
        public string title => this._title;
        public string salary => this._salary;

        public JobPostingRow(
            Guid guid,
            string employer,
            string recruiter,
            string title,
            string salary,
            DateTime opened,
            DateTime last_activity
        ) {
            this._guid = guid;
            this._company = (String.IsNullOrEmpty(employer) ? "Unknown" : employer);
            if (!String.IsNullOrEmpty(recruiter)) {
                this._company += $" (via {recruiter})";
            }
            // recruiter is just for sorting by recruiter; sort postings without a recruiter last
            this.recruiter = (String.IsNullOrEmpty(recruiter) ? "ZZZZZZZZ" : recruiter);
            if (!String.IsNullOrEmpty(employer)) {
                this.recruiter += $" (for {employer})";
            }
            this._title = title;
            this._salary = salary;
            this.opened = opened;
            this.last_activity = last_activity;
        }

        public int CompareTo(JobPostingRow row, int field = SORT_OPENED) {
            int result;
            switch (field) {
            case SORT_EMPLOYER:
                result = this._company.CompareTo(row._company);
                if (result != 0) {
                    return result;
                }
                // fall back to opened date
                return row.opened.CompareTo(this.opened);
            case SORT_RECRUITER:
                result = this.recruiter.CompareTo(row.recruiter);
                if (result != 0) {
                    return result;
                }
                // fall back to opened date
                return row.opened.CompareTo(this.opened);
            case SORT_RECENT:
                result = row.last_activity.CompareTo(this.last_activity);
                if (result != 0) {
                    return result;
                }
                // fall back to employer...
                result = this._company.CompareTo(row._company);
                if (result != 0) {
                    return result;
                }
                // ...then to recruiter
                return this.recruiter.CompareTo(row.recruiter);
            default:
                result = row.opened.CompareTo(this.opened);
                if (result != 0) {
                    return result;
                }
                // fall back to employer...
                result = this._company.CompareTo(row._company);
                if (result != 0) {
                    return result;
                }
                // ...then to recruiter
                return this.recruiter.CompareTo(row.recruiter);
            }
        }
    }

    public class BlacklistRow {
        protected string _company;
        protected string _reason;

        public string company => this._company;
        public string reason => this._reason;

        public BlacklistRow(string company, string reason) {
            this._company = company;
            this._reason = reason;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        protected JobTracker tracker;
        protected List<JobPostingRow> job_rows;
        protected Guid? selected_job;
        protected List<BlacklistRow> blacklist_rows;

        public MainWindow() {
            //TODO: maybe make data_dir configurable
            string data_dir = Path.Join(Directory.GetCurrentDirectory(), "JobLogData");
            this.tracker = new JobTracker(data_dir);
            this.job_rows = new List<JobPostingRow>();
            this.selected_job = null;
            this.blacklist_rows = new List<BlacklistRow>();
            this.InitializeComponent();
            this.updateJobRows();
            this.jobs_list.ItemsSource = this.job_rows;
            this.updateBlacklistRows();
            this.ignored_list.ItemsSource = this.blacklist_rows;
        }

        protected void sortJobRows() {
            this.job_rows.Sort((x, y) => x.CompareTo(y, this.jobs_sort_box.SelectedIndex));
        }

        protected void updateJobRows() {
            this.job_rows.Clear();
            foreach (Guid guid in this.tracker.jobs.Keys) {
                JobPosting posting = this.tracker.jobs[guid];
                if ((posting.pending) && (this.show_pending_box.IsChecked != true)) {
                    continue;
                }
                if ((posting.in_flight) && (this.show_in_flight_box.IsChecked != true)) {
                    continue;
                }
                if ((posting.closed) && (this.show_closed_box.IsChecked != true)) {
                    continue;
                }
                DateTime opened_date = posting.posted_date ?? posting.saved_date;
                DateTime latest_date = posting.saved_date;
                if (posting.closed) {
                    latest_date = posting.closed_date.Value;
                }
                else if (posting.timeline.Count > 0) {
                    latest_date = posting.timeline[posting.timeline.Count - 1].date;
                }
                else if (posting.applied) {
                    latest_date = posting.applied_date.Value;
                }
                this.job_rows.Add(new JobPostingRow(
                    guid, posting.employer, posting.recruiter, posting.title, posting.salary, opened_date, latest_date
                ));
            }
            this.sortJobRows();
            this.jobs_list.Items.Refresh();
            if (this.selected_job.HasValue) {
                this.jobs_list.SelectedValue = this.selected_job;
            }
            MainWindow.fix_listview_column_widths(this.jobs_list);
        }

        protected void jobFiltersChanged(object sender, RoutedEventArgs e) {
            this.updateJobRows();
        }

        protected void jobSortChanged(object sender, SelectionChangedEventArgs e) {
            this.sortJobRows();
            this.jobs_list.Items.Refresh();
            if (this.selected_job.HasValue) {
                this.jobs_list.SelectedValue = this.selected_job;
            }
            // items aren't changing, so no need to fix column widths
        }

        protected void selectJob(object sender, SelectionChangedEventArgs e) {
            Guid? new_sel = this.jobs_list.SelectedValue as Guid?;
            if ((!new_sel.HasValue) || (new_sel == this.selected_job)) {
                return;
            }
            bool need_refresh = this.applyJobPostingChangesIfNeeded();
            this.selected_job = new_sel;
            if (!this.tracker.jobs.ContainsKey(this.selected_job.Value)) {
                this.selected_job = null;
            }
            JobPosting posting = null;
            if (this.selected_job.HasValue) {
                posting = this.tracker.jobs[this.selected_job.Value];
            }
            this.posting_ctrl.updatePosting(posting);
            if (need_refresh) {
                this.updateJobRows();
            }
        }

        protected void updateBlacklistRows() {
            string selected = this.ignored_list.SelectedValue as string;
            this.blacklist_rows.Clear();
            foreach (string company in this.tracker.blacklist.Keys) {
                this.blacklist_rows.Add(new BlacklistRow(company, this.tracker.blacklist[company]));
            }
            this.blacklist_rows.Sort((x, y) => x.company.CompareTo(y.company));
            this.ignored_list.Items.Refresh();
            this.ignored_list.SelectedValue = selected;
            MainWindow.fix_listview_column_widths(this.ignored_list);
        }

        protected void updateBlacklistFromPostingControl(JobPostingControl ctrl) {
            bool changed = false;
            foreach (string company in ctrl.new_blacklist.Keys) {
                this.tracker.addUpdateBlacklistEntry(company, ctrl.new_blacklist[company], save: false);
                changed = true;
            }
            if (changed) {
                ctrl.new_blacklist.Clear();
                this.tracker.saveBlacklist();
                this.updateBlacklistRows();
            }
        }

        protected void addJobPosting(object sender, RoutedEventArgs e) {
            JobPostingWindow dlg = new JobPostingWindow();
            dlg.Owner = this;
            dlg.ShowDialog();
            if (!dlg.valid) {
                return;
            }

            Guid guid = this.tracker.addJob(dlg.posting);
            this.updateBlacklistFromPostingControl(dlg.posting_ctrl);
            this.updateJobRows();
            this.jobs_list.SelectedValue = guid;
        }

        protected void editJobPosting(object sender, RoutedEventArgs e) {
            if ((!this.selected_job.HasValue) || (!this.tracker.jobs.ContainsKey(this.selected_job.Value))) {
                return;
            }

            JobPostingWindow dlg = new JobPostingWindow(this.tracker.jobs[this.selected_job.Value]);
            dlg.Owner = this;
            dlg.ShowDialog();
            if (!dlg.valid) {
                return;
            }

            this.tracker.updateJob(this.selected_job.Value, dlg.posting);
            this.updateBlacklistFromPostingControl(dlg.posting_ctrl);
            this.updateJobRows();
        }

        protected void removeJobPosting(object sender, RoutedEventArgs e) {
            if ((!this.selected_job.HasValue) || (!this.tracker.jobs.ContainsKey(this.selected_job.Value))) {
                return;
            }
            this.tracker.removeJob(this.selected_job.Value);
            this.selected_job = null;
            this.updateJobRows();
        }

        protected bool applyJobPostingChangesIfNeeded() {
            if ((!this.selected_job.HasValue) || (!this.tracker.jobs.ContainsKey(this.selected_job.Value))) {
                return false;
            }
            bool needed = this.posting_ctrl.changed;
            if (!needed) {
                return false;
            }
            this.updateBlacklistFromPostingControl(this.posting_ctrl);
            this.tracker.updateJob(this.selected_job.Value, this.posting_ctrl.getPosting());
            return needed;
        }

        protected void applyJobPostingChanges(object sender, RoutedEventArgs e) {
            if (!this.applyJobPostingChangesIfNeeded()) {
                return;
            }
            // need full refresh because something might have changed w.r.t. sorting or filtering
            this.updateJobRows();
        }

        protected void revertJobPosting(object sender, RoutedEventArgs e) {
            JobPosting posting = null;
            if (this.selected_job.HasValue) {
                posting = this.tracker.jobs[this.selected_job.Value];
            }
            this.posting_ctrl.updatePosting(posting);
        }

        protected void addBlacklistEntry(object sender, RoutedEventArgs e) {
            BlacklistEntryWindow dlg = new BlacklistEntryWindow();
            dlg.Owner = this;
            dlg.ShowDialog();
            if ((!dlg.valid) || (String.IsNullOrEmpty(dlg.company_box.Text))) {
                return;
            }

            this.tracker.addUpdateBlacklistEntry(dlg.company_box.Text, dlg.reason_box.Text);
            this.updateBlacklistRows();
        }

        protected void editBlacklistEntry(object sender, RoutedEventArgs e) {
            string selected = this.ignored_list.SelectedValue as string;
            if ((String.IsNullOrEmpty(selected)) || (!this.tracker.blacklist.ContainsKey(selected))) {
                return;
            }

            BlacklistEntryWindow dlg = new BlacklistEntryWindow();
            dlg.Owner = this;
            dlg.company_box.Text = selected;
            dlg.reason_box.Text = this.tracker.blacklist[selected];
            dlg.ShowDialog();
            if ((!dlg.valid) || (String.IsNullOrEmpty(dlg.company_box.Text))) {
                return;
            }

            if (dlg.company_box.Text != selected) {
                // company changed; remove old blacklist entry
                this.tracker.removeBlacklistEntry(selected, save: false);
            }
            this.tracker.addUpdateBlacklistEntry(dlg.company_box.Text, dlg.reason_box.Text);
            this.updateBlacklistRows();
        }

        protected void removeBlacklistEntry(object sender, RoutedEventArgs e) {
            string selected = this.ignored_list.SelectedValue as string;
            if ((String.IsNullOrEmpty(selected)) || (!this.tracker.blacklist.ContainsKey(selected))) {
                return;
            }

            this.tracker.removeBlacklistEntry(selected);
            this.updateBlacklistRows();
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
