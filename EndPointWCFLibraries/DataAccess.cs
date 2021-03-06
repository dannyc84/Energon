﻿using System;
using System.Data;
using System.Data.SqlClient;


namespace EndPointWCFLibraries
{
    /// <summary>
    /// 
    /// Classe per le operazioni sul database TaskData.
    /// 
    /// File: DataAccess.cs
    /// 
    /// Authors: Daniele Crivello, Valerio Di Bernardo
    /// </summary>
    public class DataAccess
    {
        /// <summary>
        /// cmd: comando query
        /// connectionString: stringa di connessione
        /// </summary>
        private static SqlCommand cmd;
        private static string connectionString = "Server = (local); DataBase = TaskData; Integrated Security = SSPI";

        /// <summary>
        /// Procedura per l'invocazione della store procedure per lo svuotamento del DB
        /// </summary>
        public static void DB_Clear()
        {

            // creazione ed apertura di una connessione SQL (autenticazione Windows) 
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // creazione ed esecuzione del comando per l'eliminazione di un record
                cmd = new SqlCommand("removeRecord", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// Procedura per l'invocazione della store procedure per la scrittura di un record sul DB
        /// </summary>
        /// <param name="TaskID">
        /// task corrente
        /// </param>
        /// <param name="JobID">
        /// job corrente
        /// </param>
        /// <param name="Executions">
        /// esecuzione corrente
        /// </param>
        /// <param name="FlagRestore">
        /// bit di ripristino corrente
        /// </param>
        /// <param name="Value">
        /// media corrente
        /// </param>
        /// <param name="UnitOfMeasure">
        /// unità di misura corrente
        /// </param>
        /// <param name="Timestamp">
        /// timestamp corrente
        /// </param>
        public static void DB_Write(int TaskID, int JobID, int Executions, bool FlagRestore, float Value, char UnitOfMeasure, DateTime Timestamp)
        {
            // creazione ed apertura di una connessione SQL (autenticazione Windows) 
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // creazione del comando per la scrittura di un record
                cmd = new SqlCommand("addRecord", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                // creazione dei parametri
                cmd.Parameters.Add(new SqlParameter("@TaskID", TaskID));
                cmd.Parameters.Add(new SqlParameter("@JobID", JobID));
                cmd.Parameters.Add(new SqlParameter("@Executions", Executions));
                cmd.Parameters.Add(new SqlParameter("@FlagRestore", FlagRestore));
                cmd.Parameters.Add(new SqlParameter("@Value", Value));
                cmd.Parameters.Add(new SqlParameter("@UnitOfMeasure", UnitOfMeasure));
                cmd.Parameters.Add(new SqlParameter("@Timestamp", Timestamp));

                // esecuzione del comando 
                cmd.ExecuteNonQuery();
            }
        }

    }
}