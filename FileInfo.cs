using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashUtil {
    class FileInfo {
        public string Name { get; set; }
        public string MD5 { get; set; }
        public string CRC32 { get; set; }
        public string SHA1 { get; set; }

        public FileInfo() {}

        public FileInfo(string name) {
            this.Name = name;
        }
    }
}
