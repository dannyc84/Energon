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
    /// 
    /// 
    /// Si dichiara che il contenuto di questo file e',
    /// in ogni sua parte, opera originale degli autori.
    /// 
    /// Copyright (C) 2011  Daniele Crivello, Valerio Di Bernardo
    /// This program is free software: you can redistribute it and/or modify
    /// it under the terms of the GNU General Public License as published by
    /// the Free Software Foundation, either version 3 of the License, or 
    /// (at your option) any later version.
    /// This program is distributed in the hope that it will be useful,
    /// but WITHOUT ANY WARRANTY; without even the implied warranty of 
    /// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
    /// See the GNU General Public License for more details.
    /// You should have received a copy of the GNU General Public License
    /// along with this program.  If not, see http://www.gnu.org/licenses/
    /// 
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
