using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HashUtil {
    class FileInfo {

        private string _name = "";
        private string _hash = "";

        private string _dir;

        // Note that these statuses are in the order that their corresponding
        // icons are in the form's ImageList collection (imgList). The status is
        // cast to an int to get the index of the image.
        public FileStatus status = FileStatus.UNKNOWN;
        public enum FileStatus {
            UNKNOWN,
            WORKING,
            OK,
            NOTOK,
        }

        //private static Regex re_line = new Regex("^([a-fA-F0-9]{40}) ([a-fA-F0-9]{32}) ([a-fA-F0-9]{8}) (.+)$");
        private static Regex re_hash = new Regex(@"^([a-fA-F0-9]+)\s(.+)$");
        private static Regex re_hash_b = new Regex(@"^(.+)\s([a-fA-F0-9]+)$");


        public string Error;
        public Hashes.Type HashType;

        public string Name { get {
                if (_dir != null && _dir.Length > 0)
                    return _dir + _name;
                return _name;
            } set { _name = value; } }
        public string Basename { get {
                return Path.GetFileName(_name);
            } }

        public string Hash   { get { return _hash; }
            set {
                if (value == null)
                    return;
                _hash = value.Replace("-", "");
            } }

        public FileInfo() {}

        public FileInfo(string name) {
            this.Name = name;
        }

        public void SetDirectory(string directory) {
            if (Directory.Exists(directory)) {
                _dir = directory + Path.DirectorySeparatorChar;
            } else
                throw new Exception("Directory \"" + directory + "\" does not exist!");
        }

        public string FileString() {
            return Hash + " " + Basename;
        }

        public static FileInfo ParseInfoLine(string line) {
            FileInfo fi = new FileInfo();
            Match m_line = re_hash.Match(line);
            if (m_line.Success) {
                fi.Hash = m_line.Groups[1].ToString();
                fi.Name = m_line.Groups[2].ToString();
                fi.HashType = Hashes.FindType(fi.Hash);
                return fi;
            }

            Match m_line_b = re_hash_b.Match(line);
            if (m_line_b.Success) {
                fi.Hash = m_line_b.Groups[2].ToString();
                fi.Name = m_line_b.Groups[1].ToString();
                fi.HashType = Hashes.FindType(fi.Hash);
                return fi;
            }
            throw new Exception("No match on line" + Environment.NewLine + line);
        }
    }
}
