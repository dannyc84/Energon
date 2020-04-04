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
