using System;
using System.IO;
using CommonLibraries;

namespace SwControl
{
    /// <summary>
    /// 
    /// Main SwControl.
    /// 
    /// File: Program.cs
    /// 
    /// Authors: Daniele Crivello, Valerio Di Bernardo
    /// </summary>
    class Program
    {
        static void Main()
        {
            try
            {
                string LogDirPath = Directory.GetCurrentDirectory() + @"\LOG";
                if (!Directory.Exists(LogDirPath))
                {
                    Directory.CreateDirectory(LogDirPath);
                }
                LOG.removeFile(LogDirPath + @"\serverLog.txt");
                LOG.createFile(LogDirPath + @"\serverLog.txt");
                ExecutionManager manager = new ExecutionManager();
                manager.start();
            }
            catch (Exception ex)
            {
                LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                              : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                CommunicationLibrary.ChiudiListener();
            }
        }
    }
}
