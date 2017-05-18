using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenDACT.Class_Files
{
    static class Program
    {
        public static mainForm mainFormTest;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            CreateMainForm();
            Application.Run(mainFormTest);
        }

        public static void CreateMainForm()
        {
            mainFormTest = new mainForm();
        }
    }
}
