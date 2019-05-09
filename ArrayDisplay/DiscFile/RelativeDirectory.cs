using System;
using System.IO;

namespace ArrayDisplay.DiscFile {
    class RelativeDirectory {
        // Author D. Bolton see http://cplus.about.com (c) 2010
        DirectoryInfo dirInfo;

        public RelativeDirectory() { dirInfo = new DirectoryInfo(Environment.CurrentDirectory); }

        public RelativeDirectory(string absolute_dir) { dirInfo = new DirectoryInfo(absolute_dir); }

        public string Dir { get { return dirInfo.Name; } }

        public string Path {
            get { return dirInfo.FullName; }
            set {
                try {
                    DirectoryInfo newDir = new DirectoryInfo(value);
                    dirInfo = newDir;
                }
                catch {
                    // silent
                }
            }
        }

        public bool Up(int num_levels) {
            for (int i = 0; i < num_levels; i++) {
                DirectoryInfo tempDir = dirInfo.Parent;
                if (tempDir != null) dirInfo = tempDir;
                else return false;
            }
            return true;
        }

        public bool Up() { return Up(1); }

        public bool Down(string match) {
            var dirs = dirInfo.GetDirectories(match + '*');
            dirInfo = dirs[0];
            return true;
        }
    }
}
