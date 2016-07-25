using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
  
namespace AffdexMe
{
    class FilePath
    {
        static public String GetClassifierDataFolder()
        {
            return "C:\\Program Files\\Affectiva\\Affdex SDK\\data";
        }

        static public String GetAffdexLicense()
        {
            string fileName = "affdex.license";
            return File.ReadAllText(fileName);
        }
    }
}
