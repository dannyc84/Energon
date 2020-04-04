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
