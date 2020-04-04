using System;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CommonLibraries;
using EndPointWCFLibraries;
using WcfService;
using TaskQueue;
using Ammeter;


namespace SwMeasuring
{
    /// <summary>
    /// 
    /// Classe per la gestione di un task.
    /// 
    /// File: TaskManager.cs
    /// 
    /// Authors: Daniele Crivello, Valerio Di Bernardo
    /// </summary>
    class TaskManager
    {
        /// <summary>
        /// Definisce tutti i possibili stati di avanzamento del processo :
        /// notConnected : non è attiva una connessione con AlgorithmStarter
        /// wait_Task : in attesa di un nuovo task da processare
        /// taskControl : in tale stato avviene la gestione del task. Più nello specifico, ci si occuperà di sottomettere, uno per volta, ad AlgorithmStarter tutti i job del task
        /// JobControl : in tale stato avviene il controllo del job. Più nello specifico, ci si preoccuperà di completare il ciclo di esecuzioni previsto per il job
        /// runPhase : in corrispondenza di tale stato è in esecuzione presso C l'algoritmo. Dunque, in tale stato, verranno raccolti i dati provenienti dall'amperometro
        /// handle_Error : in tale stato avverrà o meno il ripristino del task in seguito ad una situazione di errore verificatasi durante la fase di esecuzione presso C
        /// </summary>
        enum states
        {
            notConnected,
            wait_Task,
            taskControl,
            JobControl,
            runPhase,
            handle_Error
        }

        /// <summary>
        /// Indica lo stato attuale di avanzamento del processo 
        /// </summary>
        private states state = states.notConnected;

        /// <summary>
        /// Campi che saranno utilizzati per la gestione del task e del job correnti.
        /// taskID : identificatore del task corrente
        /// num_executions : numero di esecuzioni da richiedere per ogni job
        /// recovery : se impostato a true indica che bisogna intraprendere le azioni di ripristino del task in caso di errore
        /// jobQueue : coda di messaggi newJob. L'insieme dei messaggi di questa coda corrisponde all'insieme dei job, appartenenti al task corrente, ancora da processare
        /// current_Execution : indica a quale esecuzione del job corrente ci si trova
        /// interval: intervallo per il calcolo delle medie sulle misurazioni accumulate
        /// </summary>
        private int taskID;
        private int num_executions;
        private Boolean recovery;
        private Queue<Message> jobQueue = null;
        private int current_Execution;


        /// <summary>
        /// interval: intervallo di scrittura sul DB dei campionamenti
        /// timer: oggetto timer
        /// </summary>
        private const int interval = 110;
        private System.Timers.Timer timer = new System.Timers.Timer(interval);

        /// <summary>
        /// Campi che saranno utilizzati per la costruzione del responso da inviare a sharepoint una volta concluso il task.
        /// response : mantiene il responso da inviare a sharepoint
        /// current_job : indica quale job del task si sta processando
        /// runError : mantiene la descrizione dell'errore che ha causato, in C, la terminazione prematura dell'esecuzione del job.
        ///            Tale campo permetterà di utilizzare la descrizione dell'errore in uno stato diverso da RunPhase, cioè handle_Error
        /// </summary>
        private string response = null;
        private int current_job;
        private string runError = null;

        /// <summary>
        /// Resetta alcuni campi inerenti la gestione del task
        /// </summary>
        private void resetTask()
        {
            this.num_executions = 0;
            this.recovery = false;
            this.jobQueue = null;
            this.response = null;
            this.runError = null;
        }

        /// <summary>
        /// creazione, registrazione degli handler per gli eventi associati (attività e completamento della stessa)
        /// ed avvio dell'esecuzione asincrona del thread worker.
        /// </summary>
        public TaskManager()
        {
            timer.Elapsed += timer_elapsed;
            Ammeter.Ammeter.initAmmeter();
            state = states.notConnected;
        }

        /// <summary>
        /// Metodo per controllare l'avanzamento del processo 
        /// </summary>
        public void start()
        {
            try
            {
                while (true)
                {
                    switch (state)
                    {
                        case states.notConnected:
                            {
                                CommunicationLibrary.Connect();
                                state = states.wait_Task;
                            }
                            break;
                        case states.wait_Task:
                            {
                                resetTask();
                                Task current_task = null;
                                // Estraggo il nuovo task dalla coda e controllo se vi è un task error 
                                if ((current_task = (Task)TaskQueue.TaskQueue.dequeue()) == null)
                                {
                                    this.response = "TASK ERROR : The object Task submitted is a null reference object.";
                                    sendResponse();
                                }
                                else if (current_task.source == null)
                                {
                                    this.response = "TASK ERROR : Source not found in object Task submitted";
                                    sendResponse();
                                }
                                else if (current_task.ItersForJob <= 0)
                                {
                                    this.response = "TASK ERROR : No execution required in object Task submitted";
                                    sendResponse();
                                }
                                else
                                {
                                    /* Nel seguito avviene l'inizializzazione del task. Più nello specifico, l'inizializzazione
                                     * dei campi usati per la gestione del task e del job correnti.
                                     * */
                                    this.taskID = current_task.taskID;
                                    this.recovery = current_task.recovery;
                                    this.jobQueue = new Queue<Message>();
                                    // se è vera la condizione seguente siamo nel caso in cui c'è un solo job da eseguire
                                    if (current_task.parameters == null)
                                    {
                                        this.jobQueue.Enqueue(new newJob(current_task.source, null, true));
                                    }
                                    else
                                    {
                                        Boolean flag = true;
                                        for (int i = 0; i < current_task.parameters.Count; i++)
                                        {
                                            this.jobQueue.Enqueue(new newJob(current_task.source, (current_task.parameters[i].CompareTo("") == 0) ? null : current_task.parameters[i].Split("".ToCharArray()), flag));
                                            flag = false;
                                        }
                                    }
                                    this.num_executions = current_task.ItersForJob;
                                    this.current_job = 0;
                                    this.state = states.taskControl;
                                }
                            }
                            break;
                        case states.taskControl:
                            {
                                Message jobMsg = null;
                                this.current_job++;
                                try
                                {
                                    if ((jobQueue == null) || ((jobMsg = jobQueue.Dequeue()) == null))
                                    {
                                        this.response = "Discard Task is occurred because of an error";
                                        LOG.AddLog("La coda di job è stata trovata non inizializzata oppure ha ritornato un job con riferimento null");
                                        sendResponse();
                                        state = states.wait_Task;
                                        continue;
                                    }
                                }
                                catch (InvalidOperationException ex)
                                {
                                    sendResponse();
                                    state = states.wait_Task;
                                    continue;
                                }
                                if (CommunicationLibrary.InviaMessaggio(jobMsg))
                                {
                                    Message answer = CommunicationLibrary.LeggiMessaggio();
                                    if (answer == null)
                                    {
                                        this.response = "Discard Task is occurred because of an error";
                                        sendResponse();
                                        CommunicationLibrary.ChiudiConnessione();
                                        this.state = states.notConnected;
                                        continue;
                                    }
                                    else if (answer is CompileError)
                                    {
                                        this.response = "Discard Task is occurred because of a compilation error. Error description : " + ((CompileError)answer).errorMessage;
                                        sendResponse();
                                        this.state = states.wait_Task;
                                        continue;
                                    }
                                    else if (answer is InitializationCompleted)
                                    {
                                        this.current_Execution = 0;
                                        this.state = states.JobControl;
                                        continue;
                                    }
                                    else
                                    {
                                        this.response = "Discard Task is occurred because of an error";
                                        LOG.AddLog("Non è stato rispettato il protocollo di comunicazione M-C. L'effetto è il discard task");
                                        sendResponse();
                                        this.state = states.wait_Task;
                                        continue;
                                    }
                                }
                                else
                                {
                                    this.response = "Discard Task is occurred because of an error";
                                    sendResponse();
                                    CommunicationLibrary.ChiudiConnessione();
                                    state = states.notConnected;
                                    continue;
                                }

                            }
                            break;
                        case states.JobControl:
                            {
                                if (this.current_Execution++ < this.num_executions)
                                {
                                    this.state = states.runPhase;
                                }
                                else
                                {
                                    this.response = this.response + "\nJob " + this.current_job + " : completed";
                                    this.state = states.taskControl;
                                }
                            }
                            break;
                        case states.runPhase:
                            {
                                if (!CommunicationLibrary.InviaMessaggio(new RunPhase()))
                                {
                                    this.response = "Discard Task is occurred because of an error";
                                    sendResponse();
                                    CommunicationLibrary.ChiudiConnessione();
                                    this.state = states.notConnected;
                                    continue;
                                }
                                Message ack = CommunicationLibrary.LeggiMessaggio();
                                if (ack == null)
                                {
                                    this.response = "Discard Task is occurred because of an error";
                                    sendResponse();
                                    CommunicationLibrary.ChiudiConnessione();
                                    this.state = states.notConnected;
                                }
                                else if (ack is Ack)
                                {
                                    Ammeter.Ammeter.Start();
                                    this.timer.Enabled = true;
                                    Message msg = CommunicationLibrary.LeggiMessaggio();
                                    this.timer.Enabled = false;
                                    Ammeter.Ammeter.Stop();
                                    if (msg == null)
                                    {
                                        this.response = "Discard Task is occurred because of an error";
                                        sendResponse();
                                        CommunicationLibrary.ChiudiConnessione();
                                        this.state = states.notConnected;
                                    }
                                    else if (msg is RunPhaseError)
                                    {
                                        this.runError = ((RunPhaseError)msg).errorMessage;
                                        this.state = states.handle_Error;
                                    }
                                    else if (msg is RunPhaseCompleted)
                                    {
                                        this.state = states.JobControl;
                                    }
                                    else
                                    {
                                        this.response = "Discard Task is occurred because of an error";
                                        LOG.AddLog("Non è stato rispettato il protocollo di comunicazione M-C. L'effetto è il discard task");
                                        sendResponse();
                                        this.state = states.wait_Task;
                                    }
                                    continue;
                                }
                                else
                                {
                                    this.response = "Discard Task is occurred because of an error";
                                    LOG.AddLog("Non è stato rispettato il protocollo di comunicazione M-C. L'effetto è il discard task");
                                    sendResponse();
                                    this.state = states.wait_Task;
                                }
                            }
                            break;
                        case states.handle_Error:
                            {
                                if (this.recovery)
                                {
                                    this.response = this.response + "\nJob " + this.current_job + " : " + "Discard Job is occurred because of an error during the execution number " + this.current_Execution + " of the algorithm. Error description : " + this.runError;
                                    this.runError = null;
                                    this.state = states.taskControl;
                                }
                                else
                                {
                                    this.response = "Discard Task is occurred because of an error during the execution number " + this.current_Execution + " of the algorithm within the Job " + this.current_job + ". Error description : " + this.runError;
                                    sendResponse();
                                    this.runError = null;
                                    this.state = states.wait_Task;
                                }
                            }
                            break;
                        default:
                            {
                                LOG.AddLog("Il processo è entrato in uno stato non previsto. L'effetto è l'immediata uccisione del processo");
                                MISService.StopService();
                                Ammeter.Ammeter.Shutdown();
                                CommunicationLibrary.ChiudiConnessione();
                                Process.GetCurrentProcess().Kill();
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                       : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                MISService.StopService();
                Ammeter.Ammeter.Shutdown();
                CommunicationLibrary.ChiudiConnessione();
                Process.GetCurrentProcess().Kill();
            }
        }

        /// <summary>
        /// Invio del responso.
        /// </summary>
        private void sendResponse()
        {
            #region DEBUG tenuto solo per vedere il comportamento delle esecuzioni. Verrà tolto al momento dell'inserimento di wcfMethod(response)
            Console.WriteLine(response);
            #endregion
        }

        /// <summary>
        /// Handler per la scrittura sul DB allo scadere del timer.
        /// </summary>
        /// <param name="source"> chiamante </param>
        /// <param name="e"> evento </param>
        private void timer_elapsed(object source, System.Timers.ElapsedEventArgs e)
        {
            DataAccess.DB_Write(taskID, this.current_job, this.current_Execution, recovery, Ammeter.Ammeter.getConsumption(), 'A', DateTime.Now);
        }

    }
}
