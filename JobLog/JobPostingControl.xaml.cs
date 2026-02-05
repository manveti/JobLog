using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JobLog {
    /// <summary>
    /// Interaction logic for JobPostingControl.xaml
    /// </summary>

    public class JobEventRow {
        public const int IDX_POSTED = -2;
        public const int IDX_SAVED = -3;
        public const int IDX_APPLIED = -4;
        public const int IDX_CLOSED = -5;

        protected int _idx;
        protected DateTime _date;
        protected string _description;

        public int idx => this._idx;
        public string date => this._date.ToString("yyyy-MM-dd");
        public string description => this._description;

        public JobEventRow(int idx, DateTime date, string description) {
            this._idx = idx;
            this._date = date;
            this._description = description;
        }
    }

    public partial class JobPostingControl : UserControl {
        protected JobPosting posting;
        public DateTime? posted_date;
        public DateTime saved_date;
        public DateTime? applied_date;
        public List<JobEventRow> timeline_rows;
        public DateTime? closed_date;
        public string closed_reason;

        public JobPostingControl() {
            this.InitializeComponent();
            this.updatePosting();
        }

        protected static void IsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            JobPostingControl ctrl = (JobPostingControl)d;
            bool isReadOnly = (bool)(e.NewValue);
            ctrl.employer_box.IsReadOnly = isReadOnly;
            ctrl.recruiter_box.IsReadOnly = isReadOnly;
            ctrl.title_box.IsReadOnly = isReadOnly;
            ctrl.salary_box.IsReadOnly = isReadOnly;
            ctrl.description_box.IsReadOnly = isReadOnly;
        }

        public static readonly DependencyProperty IsReadOnlyProp = DependencyProperty.Register(
            "IsReadOnly",
            typeof(bool),
            typeof(JobPostingControl),
            new PropertyMetadata(false, new PropertyChangedCallback(IsReadOnlyChanged))
        );

        public bool IsReadOnly {
            get => (bool)this.GetValue(IsReadOnlyProp);
            set => this.SetValue(IsReadOnlyProp, value);
        }

        public string employer {
            get => this.employer_box.Text;
            set => this.employer_box.Text = value ?? "";
        }

        public string recruiter {
            get => this.recruiter_box.Text;
            set => this.recruiter_box.Text = value ?? "";
        }

        public string title {
            get => this.title_box.Text;
            set => this.title_box.Text = value ?? "";
        }

        public string salary {
            get => this.salary_box.Text;
            set => this.salary_box.Text = value ?? "";
        }

        public string description {
            get => this.description_box.Text;
            set => this.description_box.Text = value ?? "";
        }

        public string notes {
            get => this.notes_box.Text;
            set => this.notes_box.Text = value ?? "";
        }

        public void updatePosting(JobPosting posting = null) {
            this.posting = posting?.copy() ?? new JobPosting();
            this.employer = this.posting.employer;
            this.recruiter = this.posting.recruiter;
            this.title = this.posting.title;
            this.salary = this.posting.salary;
            this.description = this.posting.description;
            this.notes = this.posting.notes;
            this.urls_list.ItemsSource = this.posting.urls;
            this.posted_date = this.posting.posted_date;
            this.saved_date = this.posting.saved_date;
            this.applied_date = this.posting.applied_date;
            this.closed_date = this.posting.closed_date;
            this.closed_reason = this.posting.closed_reason;
            this.timeline_rows = new List<JobEventRow>();
            this.timeline_list.ItemsSource = this.timeline_rows;
            this.updateTimeline();
        }

        public JobPosting getPosting() {
            this.posting.employer = this.employer;
            this.posting.recruiter = this.recruiter;
            this.posting.title = this.title;
            this.posting.salary = this.salary;
            this.posting.description = this.description;
            this.posting.notes = this.notes;
            this.posting.posted_date = this.posted_date;
            this.posting.saved_date = this.saved_date;
            this.posting.applied_date = this.applied_date;
            this.posting.closed_date = this.closed_date;
            this.posting.closed_reason = this.closed_reason;
            return this.posting;
        }

        public void updateTimeline() {
            //TODO: preserve selection if possible
            this.timeline_rows.Clear();
            if (this.posted_date.HasValue) {
                this.timeline_rows.Add(new JobEventRow(JobEventRow.IDX_POSTED, this.posted_date.Value, "Posted"));
            }
            this.timeline_rows.Add(new JobEventRow(JobEventRow.IDX_SAVED, this.saved_date, "Saved"));
            if (this.applied_date.HasValue) {
                this.timeline_rows.Add(new JobEventRow(JobEventRow.IDX_APPLIED, this.applied_date.Value, "Applied"));
            }
            for (int i = 0; i < this.posting.timeline.Count; i++) {
                this.timeline_rows.Add(new JobEventRow(i, this.posting.timeline[i].date, this.posting.timeline[i].description));
            }
            if (this.closed_date.HasValue) {
                this.timeline_rows.Add(new JobEventRow(JobEventRow.IDX_CLOSED, this.closed_date.Value, this.closed_reason ?? ""));
            }
            this.timeline_rows.Reverse();
            this.timeline_list.Items.Refresh();
            MainWindow.fix_listview_column_widths(this.timeline_list);
        }

        public void addTimelineEvent(object sender, RoutedEventArgs e) {
            JobEventWindow dlg = new JobEventWindow();
            dlg.Owner = Window.GetWindow(this);
            dlg.date_box.SelectedDate = DateTime.Today;
            dlg.ShowDialog();
            if ((!dlg.valid) || (!dlg.date_box.SelectedDate.HasValue)) {
                return;
            }

            switch (dlg.description_lst.SelectedIndex) {
            case JobEventWindow.IDX_POSTED:
                this.posted_date = dlg.date_box.SelectedDate;
                break;
            case JobEventWindow.IDX_SAVED:
                this.saved_date = dlg.date_box.SelectedDate.Value;
                break;
            case JobEventWindow.IDX_APPLIED:
                this.applied_date = dlg.date_box.SelectedDate;
                break;
            case JobEventWindow.IDX_CLOSED:
                this.closed_date = dlg.date_box.SelectedDate;
                this.closed_reason = dlg.description_box.Text;
                break;
            default:
                this.posting.addEvent(dlg.description_box.Text, dlg.date_box.SelectedDate);
                break;
            }
            this.updateTimeline();
        }

        public void updateTimelineEvent(object sender, RoutedEventArgs e) {
            int? idx = this.timeline_list.SelectedValue as int?;
            if (!idx.HasValue) {
                return;
            }
            JobEventWindow dlg = new JobEventWindow();
            dlg.Owner = Window.GetWindow(this);
            switch (idx.Value) {
            case JobEventRow.IDX_POSTED:
                dlg.date_box.SelectedDate = this.posted_date;
                dlg.description_lst.SelectedIndex = JobEventWindow.IDX_POSTED;
                break;
            case JobEventRow.IDX_SAVED:
                dlg.date_box.SelectedDate = this.saved_date;
                dlg.description_lst.SelectedIndex = JobEventWindow.IDX_SAVED;
                dlg.description_lst.IsEnabled = false;
                break;
            case JobEventRow.IDX_APPLIED:
                dlg.date_box.SelectedDate = this.applied_date;
                dlg.description_lst.SelectedIndex = JobEventWindow.IDX_APPLIED;
                break;
            case JobEventRow.IDX_CLOSED:
                dlg.date_box.SelectedDate = this.closed_date;
                dlg.description_lst.SelectedIndex = JobEventWindow.IDX_CLOSED;
                dlg.description_box.Text = this.closed_reason;
                break;
            default:
                if ((idx.Value < 0) || (idx.Value >= this.posting.timeline.Count)) {
                    return;
                }
                dlg.date_box.SelectedDate = this.posting.timeline[idx.Value].date;
                dlg.description_lst.SelectedIndex = JobEventWindow.IDX_OTHER;
                dlg.description_box.Text = this.posting.timeline[idx.Value].description;
                break;
            }
            int prev_desc_idx = dlg.description_lst.SelectedIndex;
            dlg.ShowDialog();
            if ((!dlg.valid) || (!dlg.date_box.SelectedDate.HasValue)) {
                return;
            }

            if (dlg.description_lst.SelectedIndex != prev_desc_idx) {
                // event type changed; remove and re-add
                switch (prev_desc_idx) {
                case JobEventWindow.IDX_POSTED:
                    this.posted_date = null;
                    break;
                case JobEventWindow.IDX_SAVED:
                    // IDX_SAVED shouldn't be possible
                    return;
                case JobEventWindow.IDX_APPLIED:
                    this.applied_date = null;
                    break;
                case JobEventWindow.IDX_CLOSED:
                    this.closed_date = null;
                    this.closed_reason = null;
                    break;
                default:
                    if ((idx.Value >= 0) && (idx.Value < this.posting.timeline.Count)) {
                        this.posting.removeEvent(idx.Value);
                    }
                    break;
                }
            }

            switch (dlg.description_lst.SelectedIndex) {
            case JobEventWindow.IDX_POSTED:
                this.posted_date = dlg.date_box.SelectedDate;
                break;
            case JobEventWindow.IDX_SAVED:
                this.saved_date = dlg.date_box.SelectedDate.Value;
                break;
            case JobEventWindow.IDX_APPLIED:
                this.applied_date = dlg.date_box.SelectedDate;
                break;
            case JobEventWindow.IDX_CLOSED:
                this.closed_date = dlg.date_box.SelectedDate;
                this.closed_reason = dlg.description_box.Text;
                break;
            default:
                if (prev_desc_idx == JobEventWindow.IDX_OTHER) {
                    this.posting.updateEvent(idx.Value, dlg.description_box.Text, dlg.date_box.SelectedDate);
                }
                else {
                    this.posting.addEvent(dlg.description_box.Text, dlg.date_box.SelectedDate);
                }
                break;
            }
            this.updateTimeline();
        }

        public void removeTimelineEvent(object sender, RoutedEventArgs e) {
            int? idx = this.timeline_list.SelectedValue as int?;
            if (!idx.HasValue) {
                return;
            }
            switch (idx.Value) {
            case JobEventRow.IDX_POSTED:
                this.posted_date = null;
                break;
            case JobEventRow.IDX_SAVED:
                // can't remove saved date
                return;
            case JobEventRow.IDX_APPLIED:
                this.applied_date = null;
                break;
            case JobEventRow.IDX_CLOSED:
                this.closed_date = null;
                this.closed_reason = null;
                break;
            default:
                if ((idx.Value >= 0) && (idx.Value < this.posting.timeline.Count)) {
                    this.posting.removeEvent(idx.Value);
                }
                break;
            }
            this.updateTimeline();
        }

        public void closeJob(object sender, RoutedEventArgs e) {
            JobCloseWindow dlg = new JobCloseWindow();
            dlg.Owner = Window.GetWindow(this);
            dlg.date_box.SelectedDate = DateTime.Today;
            dlg.ignore_employer_box.Visibility = (String.IsNullOrEmpty(this.employer) ? Visibility.Hidden : Visibility.Visible);
            dlg.ignore_recruiter_box.Visibility = (String.IsNullOrEmpty(this.recruiter) ? Visibility.Hidden : Visibility.Visible);
            dlg.ShowDialog();
            if ((!dlg.valid) || (!dlg.date_box.SelectedDate.HasValue)) {
                return;
            }

            this.closed_date = dlg.date_box.SelectedDate;
            this.closed_reason = dlg.reason_box.Text;
            this.updateTimeline();

            //TODO: if this.employer && dlg.ignore_employer_box.IsChecked: blacklist employer; same for recruiter
        }

        public void addUrl(object sender, RoutedEventArgs e) {
            LinkWindow dlg = new LinkWindow();
            dlg.Owner = Window.GetWindow(this);
            dlg.ShowDialog();
            if ((!dlg.valid) || (String.IsNullOrEmpty(dlg.url_box.Text))) {
                return;
            }

            this.posting.addUrl(dlg.url_box.Text);
            this.urls_list.Items.Refresh();
        }

        public void updateUrl(object sender, RoutedEventArgs e) {
            int idx = this.urls_list.SelectedIndex;
            if ((idx < 0) || (idx >= this.posting.urls.Count)) {
                return;
            }

            LinkWindow dlg = new LinkWindow();
            dlg.Owner = Window.GetWindow(this);
            dlg.url_box.Text = this.posting.urls[idx];
            dlg.ShowDialog();
            if ((!dlg.valid) || (String.IsNullOrEmpty(dlg.url_box.Text))) {
                return;
            }

            this.posting.updateUrl(idx, dlg.url_box.Text);
            this.urls_list.Items.Refresh();
        }

        public void removeUrl(object sender, RoutedEventArgs e) {
            int idx = this.urls_list.SelectedIndex;
            if ((idx < 0) || (idx >= this.posting.urls.Count)) {
                return;
            }

            this.posting.removeUrl(idx);
            this.urls_list.Items.Refresh();
        }

        public void openUrl(object sender, RoutedEventArgs e) {
            int idx = this.urls_list.SelectedIndex;
            if ((idx < 0) || (idx >= this.posting.urls.Count)) {
                return;
            }

            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = this.posting.urls[idx];
            //TODO: maybe allow configuration of browser to use, in which case .FileName = browser path
            //      if so, .FileName = url if no args configured, otherwise string.Format(configured arg format, url)
            proc.Start();
        }

        public void copyUrl(object sender, RoutedEventArgs e) {
            int idx = this.urls_list.SelectedIndex;
            if ((idx < 0) || (idx >= this.posting.urls.Count)) {
                return;
            }

            Clipboard.SetText(this.posting.urls[idx]);
        }
    }
}
