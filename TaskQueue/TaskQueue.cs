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
