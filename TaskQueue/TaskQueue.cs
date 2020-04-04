using System;
using System.Collections;
using System.Threading;

namespace TaskQueue
{
    public static class TaskQueue
    {
        /// <summary>
        /// 
        /// Classe per le operazioni sulla dei task da processare.
        /// 
        /// File: TaskQueue.cs
        /// 
        /// Authors: Daniele Crivello, Valerio Di Bernardo
        /// </summary>
        private static Queue taskQueue = new Queue();

        /// <summary>
        /// Oggetto usato per la sincronizzazione dei thread. 
        /// Più nello specifico viene usato, una volta ricevuto un nuovo task, per segnalare tale evento in modo da risvegliare l'eventuale thread in attesa.
        /// </summary>
        private static EventWaitHandle mutex = new EventWaitHandle(false, EventResetMode.AutoReset);

        /// <summary>
        /// Inserisce in coda un nuovo task e notifica l'evento.
        /// </summary>
        /// <param name="task">Nuovo task</param>
        public static void enqueue(Object task)
        {
            taskQueue.Enqueue(task);
            mutex.Set();
        }

        /// <summary>
        /// Rimuove e restituisce il task in cima alla coda. Pone in attesa il thread chiamante nel caso in cui la coda sia vuota.
        /// </summary>
        public static Object dequeue()
        {
            mutex.WaitOne();
            return taskQueue.Dequeue();
        }
    }
}
