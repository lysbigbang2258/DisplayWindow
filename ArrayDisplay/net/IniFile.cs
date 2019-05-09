// 2018122010:20 AM

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ArrayDisplay.DataFile;
using ArrayDisplay.DiscFile;

namespace ArrayDisplay.Net {
  public class Config
  {
    IniFile iniFile;
    void OpenConfigsFile()
    {
      
      RelativeDirectory rd = new RelativeDirectory();
      string file = rd.Path + "\\configs\\Config.ini";
      if (File.Exists(file))
      {
        iniFile = new IniFile(file);
      }
      else
      {
        MessageBox.Show("注意：Config.ini 文件不存在！！");
      }

    }
    /// <summary>
    /// 写入配置文件
    /// </summary>
    /// <param name="section">部分名</param>
    /// <param name="key">键名</param>
    /// <param name="val">对应值</param>
    public void ConfigsWrite(string section, string key, string val)
    {
      iniFile.IniWriteValue(section, key, val);
    }

    public static void ReadBvalueFile(string path) {
      try {
        using(StreamReader sr = new StreamReader(path)) {
          String line = sr.ReadToEnd();
          Console.WriteLine(line);
        }
      }
      catch(Exception e) {
        Console.WriteLine(e);
        throw;
      }
      
    }

     class IniFile
    {
      string path;
      [DllImport("kernel32")]
      static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
      [DllImport("kernel32")]
      static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

      public IniFile(string iniPath)
      {
        path = iniPath;
      }
      public void IniWriteValue(string section, string key, string val)
      {
        WritePrivateProfileString(section, key, val, this.path);
      }
      public string IniReadValue(string section, string key)
      {
        StringBuilder temp = new StringBuilder(255);
        int i = GetPrivateProfileString(section, key, null, temp, 255, this.path);
        return temp.ToString();
      }
    }  
  }

  
}
