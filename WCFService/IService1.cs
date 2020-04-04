using System;
using System.ServiceModel;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace WcfService
{

    /// <summary>
    /// 
    /// Classe per la definizione di un task.
    ///
    /// File: IService1.cs
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
    [DataContract]
    public class Task
    {
        /// <summary>
        /// /// source : sorgente corrispondente all'algoritmo
        /// parameters : collezione degli insiemi di parametri con cui eseguire l'algoritmo
        /// ItersForJob : numero di esecuzioni richieste per ogni job
        /// recovery : indica se l'utente ha richiesto il ripristino del task(recovery settato a true) oppure
        /// se preferisce il discard task(recovery settato a false) in caso di errore.
        /// </summary>
        [DataMember]
        public int taskID { get; set; }
        [DataMember]
        public string source { get; set; }
        [DataMember]
        public StringCollection parameters { get; set; }
        [DataMember]
        public int ItersForJob { get; set; }
        [DataMember]
        public bool recovery { get; set; }
    }

    /// <summary>
    /// Interfaccia del ontratto di servizio WCF.
    /// </summary>
    [ServiceContract]
    public interface IMISService
    {
        /// <summary>
        /// Firma del metodo per la sottomissione della task.
        /// </summary>
        /// <param name="task"> task da processare </param>
        [OperationContract(IsOneWay = true)]
        void submit(Task task);
    }

}
