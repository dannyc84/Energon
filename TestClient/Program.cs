using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TestClient
{
    static class Program
    {
        /// <summary>
        /// 
        /// Main TestClient.
        /// 
        /// File: Program.cs
        /// 
        /// Authors: Daniele Crivello, Valerio Di Bernardo
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
