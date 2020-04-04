using System;
using System.IO;
using CommonLibraries;
using WcfService;

namespace SwMeasuring
{
    /// <summary>
    /// 
    /// Main dell'EndPointWCF.
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
                if (!Directory.Exists(Directory.GetCurrentDirectory() + @"\LOG"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\LOG");
                }
                LOG.removeFile(Directory.GetCurrentDirectory() + @"\LOG\endpointWcfLog.txt");
                LOG.createFile(Directory.GetCurrentDirectory() + @"\LOG\endpointWcfLog.txt");
                MISService.StartService();
                TaskManager manager = new TaskManager();
                manager.start();
            }
            catch (Exception ex)
            {
                LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                       : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                MISService.StopService();
            }
        }
    }
}
