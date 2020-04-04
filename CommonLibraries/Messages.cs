using System;
using System.Text;

namespace CommonLibraries
{
    /// <summary>
    /// 
    /// Qui sono definiti tutti i tipi di messaggi che possono essere scambiati tra i processi RemoteMeter e AlgorithmStarter.
    /// 
    /// File: Messages.cs
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
    public interface Message { }

    /// <summary>
    /// Messaggio inviato dal sistema di misurazione al sistema di controllo per richiedere l'inizializzazione di un nuovo job (il nuovo job è definito dai campi source e algorithmParams del messaggio).
    /// Il campo compile invece esplicita se l'inizializzazione del job presso il sistema di controllo deve includere o meno la compilazione del sorgente (compile settato a true se la compilazione richiesta, settato a false altrimenti). 
    /// </summary>
    [Serializable]
    public class newJob : Message
    {
        /// <summary>
        /// source: sorgente
        /// algorithmParams: insieme dei parametri
        /// compile: bit per la compilazione
        /// </summary>
        public string source;
        public Object[] algorithmParams;
        public Boolean compile;

        /// <summary>
        /// Costruttore di un nuovo job
        /// </summary>
        /// <param name="code"> codice sorgente </param>
        /// <param name="parameters"> insieme dei parametri</param>
        /// <param name="compile"> bit di compilazione </param>
        public newJob(string code, Object[] parameters, Boolean compile)
        {
            this.source = code;
            this.algorithmParams = parameters;
            this.compile = compile;
        }
    }

    /// <summary>
    /// Messaggio inviato in seguito alla ricezione di un messaggio inatteso.
    /// </summary>
    [Serializable]
    public class UnexpectedMsg : Message
    {

    }

    /// <summary>
    /// Messaggio inviato dal sistema di controllo al sistema di misurazione in seguito al riscontro di un errore che preclude l'avvio o la corretta
    /// conclusione del processo di compilazione del sorgente. Tale messaggio riporta la descrizione dell'errore.
    /// </summary>
    [Serializable]
    public class CompileError : Message
    {
        public string errorMessage;

        public CompileError(string error)
        {
            this.errorMessage = error;
        }
    }

    /// <summary>
    /// Messaggio inviato dal sistema di controllo al sistema di misurazione in seguito al corretto completamento della fase di inizializzazione del job
    /// </summary>
    [Serializable]
    public class InitializationCompleted : Message
    {

    }

    /// <summary>
    /// Messaggio inviato dal sistema di misurazione al sistema di controllo per richiedere l'esecuzione dell'algoritmo
    /// </summary>
    [Serializable]
    public class RunPhase : Message
    {

    }

    /// <summary>
    /// Messaggio inviato dal sistema di controllo al sistema di misurazione per notificare l'inizio dell'esecuzione dell'algoritmo
    /// </summary>
    [Serializable]
    public class Ack : Message
    {

    }

    /// <summary>
    /// Messaggio inviato dal sistema di controllo al sistema di misurazione per notificare che l'esecuzione dell'algoritmo è terminata senza errori
    /// </summary>
    [Serializable]
    public class RunPhaseCompleted : Message
    {

    }

    /// <summary>
    /// Messaggio inviato dal sistema di controllo al sistema di misurazione per notificare che l'esecuzione dell'algoritmo è terminata prematuramente a causa di un errore.
    /// La descrizione di tale errore è riportata nel messaggio.
    /// </summary>
    [Serializable]
    public class RunPhaseError : Message
    {
        /// <summary>
        /// erroreMessage: messaggio d'errore
        /// </summary>
        public string errorMessage;

        /// <summary>
        /// Costruttore del messaggio d'errore
        /// </summary>
        /// <param name="error"> errore occorso </param>
        public RunPhaseError(string error)
        {
            this.errorMessage = error;
        }
    }
}
