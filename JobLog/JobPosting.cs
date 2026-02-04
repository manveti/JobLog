using System;
using System.Collections.Generic;

namespace JobLog {
    [Serializable]
    public class JobEvent {
        public DateTime date;
        public string description;

        public JobEvent(DateTime date, string description) {
            this.date = date;
            this.description = description;
        }
    }

    [Serializable]
    public class JobPosting {
        public string employer;
        public string recruiter;
        public string title;
        public string salary;
        public string description;
        public string notes;
        public List<string> urls;
        public DateTime? posted_date;
        public DateTime saved_date;
        public DateTime? applied_date;
        public List<JobEvent> timeline;
        public DateTime? closed_date;
        public string closed_reason;

        public DateTime opened_date { get => this.posted_date ?? this.saved_date; }
        public bool applied { get => this.applied_date.HasValue; }
        public bool closed { get => this.closed_date.HasValue; }
        public bool pending { get => !this.applied && !this.closed; }
        public bool in_flight { get => this.applied && !this.closed; }

        public JobPosting() {
            this.urls = new List<string>();
            this.saved_date = DateTime.Today;
            this.timeline = new List<JobEvent>();
        }

        public JobPosting(
            string employer,
            string recruiter,
            string title,
            string salary,
            string description,
            string notes,
            IEnumerable<string> urls = null,
            DateTime? posted_date = null,
            DateTime? saved_date = null
        ) {
            this.employer = employer;
            this.recruiter = recruiter;
            this.title = title;
            this.salary = salary;
            this.description = description;
            this.notes = notes;
            if (urls == null) {
                this.urls = new List<string>();
            }
            else {
                this.urls = new List<string>(urls);
            }
            this.posted_date = posted_date;
            this.saved_date = saved_date ?? DateTime.Today;
            this.timeline = new List<JobEvent>();
        }

        public void addUrl(string url) {
            this.urls.Add(url);
        }

        public void updateUrl(int idx, string url) {
            this.urls[idx] = url;
        }

        public void removeUrl(int idx) {
            this.urls.RemoveAt(idx);
        }

        public void addEvent(string description, DateTime? date = null) {
            this.addEvent(new JobEvent(date ?? DateTime.Today, description));
        }

        public void addEvent(JobEvent evt) {
            int idx = this.timeline.FindIndex((e) => e.date > evt.date);
            if (idx < 0) {
                this.timeline.Add(evt);
            }
            else {
                this.timeline.Insert(idx, evt);
            }
        }

        public void updateEvent(int idx, string description, DateTime? date = null) {
            JobEvent evt = this.timeline[idx];
            if (description != null) {
                evt.description = description;
            }
            if ((date.HasValue) && (date.Value != evt.date)) {
                evt.date = date.Value;
                // remove and re-add so that we maintain the right order after date change
                this.timeline.RemoveAt(idx);
                this.addEvent(evt);
            }
        }

        public void removeEvent(int idx) {
            this.timeline.RemoveAt(idx);
        }

        public void close(string reason, DateTime? date = null) {
            this.closed_date = date ?? DateTime.Today;
            this.closed_reason = reason;
        }
    }
}
