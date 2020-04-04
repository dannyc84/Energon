using System;
using System.IO;

namespace CommonLibraries
{
    /// <summary>
    /// 
    /// Libreria utilizzata per scrivere messaggi in un file di log.
    /// 
    /// File: Log.cs
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
    public static class LOG
    {
        /// <summary>
        /// Mantiene il path del file di log.
        /// </summary>
        private static string filePath = null;

        /// <summary>
        /// Stabilisce il path del file di log.
        /// </summary>
        public static void createFile(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path", "Il path passato come parametro e relativo al file di log è uguale a null");
            }
            filePath = path;
        }

        /// <summary>
        /// Aggiunge un messaggio nel file di log. 
        /// </summary>
        public static void AddLog(string message)
        {
            StreamWriter writer = File.AppendText(filePath);
            writer.WriteLine(message);
            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// Rimuove il file di log.
        /// </summary>
        public static void removeFile(string path)
        {
            File.Delete(path);
        }
    }
}
