using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashUtil {
    class FileInfo {

        private string _name;
        private string _md5;
        private string _crc32;
        private string _sha1;

        public string Name { get { return _name; } set { _name = value; } }
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

        public string FileString() {
            return SHA1 + " " + MD5 + " " + CRC32 + " " + Basename;
        }
    }
}
