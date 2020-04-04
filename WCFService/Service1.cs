using System;
using System.ServiceModel;
using System.Text;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Diagnostics;
using CommonLibraries;
using TaskQueue;

namespace WcfService
{
    /// <summary>
    /// 
    /// Implementazione dell'interfaccia per il servizio WCF.
    /// 
    /// File: Service1.cs
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
    /// </summary>
    public class MISService : IMISService
    {

        /// <summary>
        /// service: servizio
        /// </summary>
        private static ServiceHost service = null;

        /// <summary>
        /// serviceAddress: indirizzo dell'endpoint WCF
        /// </summary>
        private static string serviceAddress = "http://localhost:8080/MIS";

        /// <summary>
        /// Avvio del servizio.
        /// </summary>
        public static void StartService()
        {
            service = new ServiceHost(typeof(MISService));
            service.AddServiceEndpoint(typeof(IMISService), new WS2007HttpBinding(), new Uri(MISService.serviceAddress));
            service.Open();
        }

        /// <summary>
        /// Termine del servizio.
        /// </summary>
        public static void StopService()
        {
            try
            {
                if ((service != null) && (service.State != CommunicationState.Closed))
                    service.Close();
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Sottomissione del task.
        /// </summary>
        /// <param name="task"> task da processare </param>
        public void submit(Task task)
        {
            try
            {
                TaskQueue.TaskQueue.enqueue(task);
            }
            catch (Exception ex)
            {
                LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                       : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                MISService.StopService();
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
