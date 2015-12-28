using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HashUtil {
    class FileInfo {

        private string _name;
        private string _md5;
        private string _crc32;
        private string _sha1;

        private string _dir;

        private static Regex re_line = new Regex("^([a-fA-F0-9]{40}) ([a-fA-F0-9]{32}) ([a-fA-F0-9]{8}) (.+)$");

        public string Name { get {
                if (_dir != null && _dir.Length > 0)
                    return _dir + _name;
                return _name;
            } set { _name = value; } }
        public string Basename { get {
                return Path.GetFileName(_name);
            } }

        public string MD5   { get { return _md5; }
            set {
                if (value == null)
                    return;
                _md5 = value.Replace("-", "");
            } }

        public string CRC32 { get { return _crc32; }
            set {
                if (value == null)
                    return;
                _crc32 = value.Replace("-", "");
            } }

        public string SHA1  { get { return _sha1; }
            set {
                if (value == null)
                    return;
                _sha1 = value.Replace("-", "");
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
            return SHA1 + " " + MD5 + " " + CRC32 + " " + Basename;
        }

        public static FileInfo ParseInfoLine(string line) {
            FileInfo fi = new FileInfo();
            Match m_line = re_line.Match(line);
            if (m_line.Success) {
                fi.SHA1 = m_line.Groups[1].ToString();
                fi.MD5 = m_line.Groups[2].ToString();
                fi.CRC32 = m_line.Groups[3].ToString();
                fi.Name = m_line.Groups[4].ToString();
                return fi;
            }
            throw new Exception("No match on line" + Environment.NewLine + line);
        }
    }
}
