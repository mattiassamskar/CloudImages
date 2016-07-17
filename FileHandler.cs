using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CloudImages
{
    public class FileHandler
    {
        public static void DeleteFiles(IEnumerable<string> files)
        {
            files.ToList().ForEach(File.Delete);
        }
    }
}
