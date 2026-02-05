using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace JobLog {
    [Serializable]
    public class CompanyInfo {
        public string name;
        public string description;

        public CompanyInfo(string name, string description) {
            this.name = name;
            this.description = description;
        }
    }

    public class JobTracker {
        public string data_dir;
        protected string jobs_dir;
        protected string blacklist_path;
        public Dictionary<Guid, JobPosting> jobs;
        public List<CompanyInfo> blacklist;

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

        void loadBlacklist() {
            if (!File.Exists(this.blacklist_path)) {
                // nothing to load
                this.blacklist = new List<CompanyInfo>();
                return;
            }
            DataContractSerializer serializer = new DataContractSerializer(typeof(List<CompanyInfo>));
            using (FileStream f = new FileStream(this.blacklist_path, FileMode.OpenOrCreate)) {
                XmlDictionaryReader xmlReader = XmlDictionaryReader.CreateTextReader(f, new XmlDictionaryReaderQuotas());
                this.blacklist = (List<CompanyInfo>)(serializer.ReadObject(xmlReader, true));
            }
        }

        void saveJob(Guid guid) {
            if (!this.jobs.ContainsKey(guid)) {
                return;
            }
            Directory.CreateDirectory(this.jobs_dir);
            string path = Path.Join(this.jobs_dir, guid.ToString() + ".job");
            using (FileStream f = new FileStream(path, FileMode.Create)) {
                DataContractSerializer serializer = new DataContractSerializer(typeof(JobPosting));
                serializer.WriteObject(f, this.jobs[guid]);
            }
        }

        void saveBlacklist() {
            Directory.CreateDirectory(this.data_dir);
            using (FileStream f = new FileStream(this.blacklist_path, FileMode.Create)) {
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<CompanyInfo>));
                serializer.WriteObject(f, this.blacklist);
            }
        }
    }
}
