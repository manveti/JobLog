using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace JobLog {
    public class JobTracker {
        public string data_dir;
        protected string jobs_dir;
        protected string blacklist_path;
        public Dictionary<Guid, JobPosting> jobs;
        public Dictionary<string, string> blacklist;

        public JobTracker(string data_dir) {
            this.data_dir = data_dir;
            this.jobs_dir = Path.Join(data_dir, "jobs");
            this.blacklist_path = Path.Join(data_dir, "blacklist.xml");
            this.loadJobs();
            this.loadBlacklist();
        }

        public void loadJobs() {
            this.jobs = new Dictionary<Guid, JobPosting>();
            if (!Directory.Exists(this.jobs_dir)) {
                // nothing to load
                return;
            }
            foreach (string fn in Directory.EnumerateFiles(this.jobs_dir, "*.job")) {
                Guid guid;
                try {
                    guid = Guid.Parse(Path.GetFileNameWithoutExtension(fn));
                }
                catch (FormatException) {
                    // this isn't a job file; skip it
                    continue;
                }
                DataContractSerializer serializer = new DataContractSerializer(typeof(JobPosting));
                using (FileStream f = new FileStream(fn, FileMode.OpenOrCreate)) {
                    XmlDictionaryReader xmlReader = XmlDictionaryReader.CreateTextReader(f, new XmlDictionaryReaderQuotas());
                    this.jobs[guid] = (JobPosting)(serializer.ReadObject(xmlReader, true));
                }
            }
        }

        public void loadBlacklist() {
            if (!File.Exists(this.blacklist_path)) {
                // nothing to load
                this.blacklist = new Dictionary<string, string>();
                return;
            }
            DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<string, string>));
            using (FileStream f = new FileStream(this.blacklist_path, FileMode.OpenOrCreate)) {
                XmlDictionaryReader xmlReader = XmlDictionaryReader.CreateTextReader(f, new XmlDictionaryReaderQuotas());
                this.blacklist = (Dictionary<string, string>)(serializer.ReadObject(xmlReader, true));
            }
        }

        protected string jobPath(Guid guid) {
            return Path.Join(this.jobs_dir, guid.ToString() + ".job");
        }

        public void saveJob(Guid guid) {
            if (!this.jobs.ContainsKey(guid)) {
                return;
            }
            Directory.CreateDirectory(this.jobs_dir);
            string path = this.jobPath(guid);
            using (FileStream f = new FileStream(path, FileMode.Create)) {
                DataContractSerializer serializer = new DataContractSerializer(typeof(JobPosting));
                serializer.WriteObject(f, this.jobs[guid]);
            }
        }

        public void saveBlacklist() {
            Directory.CreateDirectory(this.data_dir);
            using (FileStream f = new FileStream(this.blacklist_path, FileMode.Create)) {
                DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<string, string>));
                serializer.WriteObject(f, this.blacklist);
            }
        }

        public Guid addJob(JobPosting posting) {
            Guid guid = Guid.NewGuid();
            this.jobs[guid] = posting;
            //TODO: this.saveJob(guid);
            return guid;
        }

        public void updateJob(Guid guid, JobPosting posting) {
            this.jobs[guid].update(posting);
            //TODO: this.saveJob(guid);
        }

        public void removeJob(Guid guid) {
            this.jobs.Remove(guid);
            //TODO: delete file this.jobPath(guid)
        }

        public void addUpdateBlacklistEntry(string company, string reason, bool save = true) {
            this.blacklist[company] = reason;
            if (save) {
                //TODO: this.saveBlacklist();
            }
        }

        public void removeBlacklistEntry(string company, bool save = true) {
            this.blacklist.Remove(company);
            if (save) {
                //TODO: this.saveBlacklist();
            }
        }
    }
}
