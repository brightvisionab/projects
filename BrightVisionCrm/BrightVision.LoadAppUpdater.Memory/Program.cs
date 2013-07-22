using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace BrightVision.LoadAppUpdater.Memory
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                string file = Directory.GetCurrentDirectory() + @"\BrightVision.AppUpdater.exe";
                FileStream fs = new FileStream(file, FileMode.Open);
                BinaryReader br = new BinaryReader(fs);
                byte[] bin = br.ReadBytes(Convert.ToInt32(fs.Length));
                fs.Close();
                br.Close();

                // load the bytes into Assembly
                Assembly a = Assembly.Load(bin);
                // search for the Entry Point
                MethodInfo method = a.EntryPoint;
                if (method != null)
                {
                    // create an istance of the Startup form Main method
                    object o = a.CreateInstance(method.Name);
                    method.Invoke(o, null);
                }
                else
                {
                    MessageBox.Show("An error has been encountered while trying to update your version.");
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
