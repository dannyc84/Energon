using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Threading;

namespace CommonLibraries
{
    /// <summary>
    /// 
    /// Mediante questa libreria il sistema di misurazione e di controllo possono stabilire una connessione TCP permanente
    /// ed utilizzarla per lo scambio dei messaggi. 
    /// 
    /// File: ComLibrary.cs
    /// 
    /// Authors: Daniele Crivello, Valerio Di Bernardo
    /// </summary>
    public static class CommunicationLibrary
    {

        /// <summary>
        /// listener : campo corrispondente all'oggetto usato per porsi in ascolto di richieste di connessione
        /// portListener : porta su cui attendere i tentativi di connessione in ingresso
        /// </summary>
        private static TcpListener listener = null;
        private static int portListener = 2025;

        /// <summary>
        /// IP del server remoto
        /// IPserver: IP Vale-PC
        /// </summary>
        private static IPAddress IPserver = new IPAddress(new byte[] { 131, 114, 88, 150 });

        // IPserver: indirizzo OPTIMUS-PRIME
        /* private static IPAddress IPserver = new IPAddress(new byte[] { 131, 114, 2, 239 }); */

        /* #region PARTE DA USARE SOLO PER COMUNICAZIONI IN LOCALE
          * 
          * private static IPAddress IPserver = Dns.GetHostAddresses("localhost")[0];
          * 
          * #endregion */

        /// <summary>
        /// IPEndpont del server
        /// </summary>
        private static IPEndPoint serverEndPoint = new IPEndPoint(IPserver, portListener);

        /// <summary>
        /// Campi corrispondenti agli oggetti di cui si avrà bisogno per stabilire la connessione TCP permanente e per lo scambio dei messaggi
        /// </summary>
        private static TcpClient peer = null;
        private static NetworkStream stream = null;


        /// <summary>
        /// Intervallo di tempo, in millisecondi, da attendere prima di ritentare di stabilire una connessione
        /// </summary>
        private static int wait_Time = 3000;

        /// <summary>
        /// Queste due variabili stabiliscono rispettivamente quali debbano essere le dimensioni dei buffer per i dati in entrata ed in uscita 
        /// </summary>
        private static int bufferInputSize = 16 * 1024;
        private static int bufferOutputSize = 16 * 1024;

        /// <summary>
        /// Indica se è in atto una connessione.
        /// </summary>
        private static Boolean Connected
        {
            get
            {
                if (peer == null)
                    return false;
                return peer.Connected;
            }
        }


        /// <summary>
        /// Definisce la struttura che viene passata al metodo di callback nelle operazioni asincrone. Il campo receivedMsg riporterà il messaggio ricevuto.
        /// Il campo mutex manterrà il riferimento all'oggetto EventWaitHandle responsabile della sincronizzazione fra il thread chiamante l'operazione 
        /// asincrona ed il thread che esegue il metodo di callback. Il campo streamIn corrisponde all'oggetto MemoryStream utilizzato al momento della
        /// deserializzazione del messaggio ricevuto.
        /// </summary>
        private class ParametroCallback
        {
            public Message receivedMsg;
            public MemoryStream streamIn;
            public EventWaitHandle mutex;

            public ParametroCallback(ref EventWaitHandle mutex, ref MemoryStream streamIn)
            {
                if (mutex == null)
                    throw new ArgumentNullException("mutex", "riferimento a mutex uguale a null");
                if (streamIn == null)
                    throw new ArgumentNullException("streamIn", "riferimento a streamIn uguale a null");
                this.mutex = mutex;
                this.streamIn = streamIn;
            }
        }

        /// <summary>
        /// Crea e avvia un oggetto TcpListener.
        /// </summary>
        private static void createListener()
        {
            ChiudiListener();
            listener = new TcpListener(IPserver, portListener);
            listener.Start();
        }

        /// <summary>
        /// Accetta la richiesta di connessione TCP permanente da parte del sistema di misurazione. In caso di errore, quando possibile, si ripone in attesa di
        /// un'altra richiesta di connessione.
        /// </summary>
        public static void AcceptClient()
        {
            try
            {
                createListener();
                peer = listener.AcceptTcpClient();
                stream = peer.GetStream();
            }
            catch (SocketException ex)
            {
                LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                       : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                AcceptClient();
            }
            catch (ObjectDisposedException ex)
            {
                LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                       : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                AcceptClient();
            }
        }

        /// <summary>
        /// Tenta di aprire una connessione TCP permanente con il sistema di controllo. In caso di errore, quando possibile, ritenta di stabilire
        /// la connessione.
        /// </summary>
        public static void Connect()
        {
            try
            {
                #region PARTE DA USARE SOLO PER COMUNICAZIONI IN REMOTO
             
                peer = new TcpClient();
                peer.Connect(serverEndPoint);
                
                #endregion
                /* #region PARTE DA USARE SOLO PER COMUNICAZIONI IN LOCALE

                peer = new TcpClient("localhost", portListener);

                #endregion
                 * */
                stream = peer.GetStream();
            }
            catch (SocketException ex)
            {
                LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                       : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                ChiudiConnessione();
                Thread.Sleep(wait_Time);
                Connect();
            }
            catch (ObjectDisposedException ex)
            {
                LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                       : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                ChiudiConnessione();
                Thread.Sleep(wait_Time);
                Connect();
            }
            catch (InvalidOperationException ex)
            {
                LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                       : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                ChiudiConnessione();
                Thread.Sleep(wait_Time);
                Connect();
            }
        }

        /// <summary>
        /// Ritorna il messaggio ricevuto.
        /// </summary>
        /// <returns>Ritorna il messaggio ricevuto nel caso l'intera operazione di ricezione di un nuovo messaggio sia andata a buon fine. Ritorna null altrimenti.</returns>
        public static Message LeggiMessaggio()
        {
            if (Connected && stream != null && stream.CanRead)
            {
                byte[] bufferIn = new byte[bufferInputSize];
                MemoryStream streamIn = new MemoryStream(bufferIn, false);
                EventWaitHandle mutex = new EventWaitHandle(false, EventResetMode.AutoReset);
                IAsyncResult result = stream.BeginRead(bufferIn, 0, bufferInputSize, new AsyncCallback(LeggiCallback), new ParametroCallback(ref mutex, ref streamIn));
                mutex.WaitOne();
                return ((ParametroCallback)result.AsyncState).receivedMsg;
            }
            else
            {
                LOG.AddLog("La connessione è stata trovata chiusa al momento di ricevere un messaggio dalla rete");
                return null;
            }
        }

        /// <summary>
        /// Metodo di callback eseguito in corrispondenza di una operazione di lettura asincrona
        /// </summary>
        private static void LeggiCallback(IAsyncResult ares)
        {
            try
            {
                if (stream.EndRead(ares) == 0)
                {
                    LOG.AddLog("La connessione è stata chiusa prematuramente dall'host remoto");
                    ((ParametroCallback)ares.AsyncState).receivedMsg = null;
                }
                else
                {
                    IFormatter formatter = new BinaryFormatter();
                    ((ParametroCallback)ares.AsyncState).receivedMsg = (Message)formatter.Deserialize(((ParametroCallback)ares.AsyncState).streamIn);
                }
            }
            catch (Exception ex)
            {
                LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                       : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                ((ParametroCallback)ares.AsyncState).receivedMsg = null;
            }
            finally
            {
                ((ParametroCallback)ares.AsyncState).mutex.Set();
            }
        }

        /// <summary>
        /// Invia il messaggio passato per parametro e ritorna l'esito dell'invio(true se ok, false se si è verificato un errore a livello TCP per problemi di network).
        /// Tale metodo è bloccante finchè il messaggio non è spedito in rete.
        /// </summary>
        public static Boolean InviaMessaggio(Message messaggio)
        {
            if (Connected && stream != null && stream.CanWrite)
            {
                try
                {
                    byte[] bufferOut = new byte[bufferOutputSize];
                    IFormatter formatter = new BinaryFormatter();
                    MemoryStream streamOut = new MemoryStream(bufferOut, true);
                    streamOut.Seek(0, SeekOrigin.Begin);
                    formatter.Serialize(streamOut, messaggio);
                    stream.Write(bufferOut, 0, bufferOut.Length);
                    return true;
                }
                catch (Exception ex)
                {
                    LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                              : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                    return false;
                }
            }
            else
            {
                LOG.AddLog("La connessione è stata trovata chiusa al momento di inviare un messaggio sulla rete");
                return false;
            }
        }

        /// <summary>
        /// Chiude la connessione TCP permanente e resetta le variabili associate ad essa
        /// </summary>
        public static void ChiudiConnessione()
        {
            if (peer != null)
            {
                peer.Close();
                peer = null;
            }
            stream = null;
        }


        /// <summary>
        /// Chiude l'oggetto TcpListener e resetta la variabile che mantiene il suo riferimento.
        /// </summary>
        public static void ChiudiListener()
        {
            if (listener != null)
            {
                listener.Stop();
                listener = null;
            }
        }

    }
}
