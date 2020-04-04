using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Timers;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Threading;
using CommonLibraries;

namespace SwControl
{
    /// <summary>
    /// 
    /// Classe per il controllo dell'algoritmo.
    /// 
    /// File: ExecutionManager.cs
    /// 
    /// Authors: Daniele Crivello, Valerio Di Bernardo
    /// </summary>
    class ExecutionManager
    {
        /// <summary>
        /// Stati di avanzamento del processo AlgorithmStarter :
        /// waitConnection : in attesa di stabilira una connessione TCP permanente con sistema di misurazione
        /// waitRequest : in attesa di una richiesta da sistema di misurazione
        /// initJob : in questo stato avviene l'inizializzazione del job
        /// runPhase : in questo stato avviene l'esecuzione dell'algoritmo
        /// </summary>
        enum states
        {
            waitConnection,
            waitRequest,
            initJob,
            runPhase
        }

        /// <summary>
        /// Indica lo stato attuale di avanzamento del processo 
        /// </summary>
        private states state = states.waitConnection;

        /// <summary>
        /// Rappresenta il job corrente. Ricordo che per definizione il job è una coppia (source,params). 
        /// </summary>
        private newJob currentJob = null;

        /// <summary>
        /// Campi corrispondenti ad oggetti creati in fase di inizializzazione del job e poi utilizzati al momento di eseguire l'algoritmo.
        /// algorithm : oggetto MethodInfo corrispondente all'algoritmo da eseguire
        /// parameters : parametri da passare all'algoritmo
        /// </summary>
        MethodInfo algorithm = null;
        Object[] parameters = null;


        public ExecutionManager()
        {
            this.state = states.waitConnection;
        }

        /// <summary>
        /// Metodo che definisce il comportamento del software di controllo
        /// </summary>
        public void start()
        {
            try
            {
                while (true)
                {
                    switch (state)
                    {
                        case states.waitConnection:
                            {
                                CommunicationLibrary.AcceptClient();
                                this.state = states.waitRequest;
                            }
                            break;
                        case states.waitRequest:
                            {
                                Message request = CommunicationLibrary.LeggiMessaggio();
                                if (request == null)
                                {
                                    CommunicationLibrary.ChiudiConnessione();
                                    this.state = states.waitConnection;
                                }
                                else if (request is newJob)
                                {
                                    this.currentJob = (newJob)request;
                                    this.state = states.initJob;
                                }
                                else if (request is RunPhase)
                                {
                                    this.state = states.runPhase;
                                }
                                else
                                {
                                    if (!CommunicationLibrary.InviaMessaggio(new UnexpectedMsg()))
                                    {
                                        CommunicationLibrary.ChiudiConnessione();
                                        this.state = states.waitConnection;
                                    }
                                }
                            }
                            break;
                        case states.initJob:
                            {
                                string Error = null;
                                if (this.currentJob.compile)
                                {
                                    this.algorithm = null;
                                    this.parameters = null;
                                    if (this.currentJob.source == null)
                                    {
                                        Error = "Compilation failed because of source not found";
                                    }
                                    else
                                    {
                                        CompilerResults result = InitLibrary.compile(this.currentJob.source);
                                        if (result == null)
                                        {
                                            Error = "Compilation failed because of unexpected error";
                                        }
                                        else if (result.Errors.HasErrors)
                                        {
                                            try
                                            {
                                                var errorMessage = new StringBuilder();
                                                foreach (CompilerError error in result.Errors)
                                                {
                                                    errorMessage.AppendFormat("{0} {1}", error.Line, error.ErrorText);
                                                }
                                                Error = errorMessage.ToString();
                                            }
                                            catch (Exception ex)
                                            {
                                                LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                                                       : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                                                Error = "Compilation failed because of unexpected error";
                                            }
                                        }
                                        else
                                        {
                                            Assembly compiledSource = result.CompiledAssembly;
                                            Error = null;
                                            this.algorithm = InitLibrary.getMethod(compiledSource, out Error);
                                        }
                                    }
                                }
                                Message InitResponse = null;
                                if ((this.algorithm == null) && !this.currentJob.compile)
                                {
                                    InitResponse = new UnexpectedMsg();
                                    this.currentJob = null;
                                }
                                else if ((this.algorithm == null) && this.currentJob.compile)
                                {
                                    InitResponse = new CompileError(Error);
                                    this.currentJob = null;
                                }
                                else
                                {
                                    this.parameters = InitLibrary.ConvertParameters(this.algorithm, this.currentJob.algorithmParams);
                                    InitResponse = new InitializationCompleted();
                                }
                                if (CommunicationLibrary.InviaMessaggio(InitResponse))
                                {
                                    this.state = states.waitRequest;
                                }
                                else
                                {
                                    CommunicationLibrary.ChiudiConnessione();
                                    this.state = states.waitConnection;
                                }
                            }
                            break;
                        case states.runPhase:
                            {
                                if (this.algorithm == null)
                                {
                                    if (CommunicationLibrary.InviaMessaggio(new UnexpectedMsg()))
                                    {
                                        this.state = states.waitRequest;
                                    }
                                    else
                                    {
                                        CommunicationLibrary.ChiudiConnessione();
                                        this.state = states.waitConnection;
                                    }
                                }
                                else
                                {

                                    if (!CommunicationLibrary.InviaMessaggio(new Ack()))
                                    {
                                        CommunicationLibrary.ChiudiConnessione();
                                        this.state = states.waitConnection;
                                    }
                                    else
                                    {
                                        Message response = null;
                                        try
                                        {
                                            algorithm.Invoke(null, this.parameters);
                                            response = new RunPhaseCompleted();
                                        }
                                        catch (ArgumentException ex)
                                        {
                                            response = new RunPhaseError("Error in parameters conversion.\nError description : " + ex.Message);
                                        }
                                        catch (TargetInvocationException ex)
                                        {
                                            response = new RunPhaseError("Exception thrown during run phase" + ((ex.InnerException != null) ? "\nException message : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace :\n" + ex.InnerException.StackTrace : ""));
                                        }
                                        catch (TargetParameterCountException ex)
                                        {
                                            response = new RunPhaseError(ex.Message);
                                        }
                                        catch (Exception ex)
                                        {
                                            LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                                                   : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                                            response = new RunPhaseError("Unexpected error during run phase");
                                        }
                                        if (CommunicationLibrary.InviaMessaggio(response))
                                        {
                                            this.state = states.waitRequest;
                                        }
                                        else
                                        {
                                            CommunicationLibrary.ChiudiConnessione();
                                            this.state = states.waitConnection;
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            {
                                LOG.AddLog("Il processo è entrato in uno stato non previsto. L'effetto è l'immediata uccisione del processo");
                                CommunicationLibrary.ChiudiConnessione();
                                CommunicationLibrary.ChiudiListener();
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
                CommunicationLibrary.ChiudiConnessione();
                CommunicationLibrary.ChiudiListener();
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
