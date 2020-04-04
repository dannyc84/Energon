using System;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Text;
using CommonLibraries;

namespace SwControl
{
    /// <summary>
    /// 
    /// Libreria contenente metodi usati per l'inizializzazione del job presso il sistema di controllo.
    /// 
    /// File: InitLibrary.cs
    /// 
    /// Authors: Daniele Crivello, Valerio Di Bernardo
    /// </summary>
    public static class InitLibrary
    {
        /// <summary>
        /// Compila il sorgente passato come parametro.
        /// </summary>
        /// <param name="source">Sorgente</param>
        /// <returns>Ritorna un oggetto che rappresenta il risultato della compilazione. Ritorna null se si è verificato un errore indipendente dal processo di compilazione</returns>
        public static CompilerResults compile(string source)
        {
            if (source == null)
            {
                LOG.AddLog("Compilation failed because of null reference of source");
                return null;
            }
            using (var provider = new CSharpCodeProvider())
            {
                var options = new CompilerParameters();
                try
                {
                    options.GenerateInMemory = true;
                    return provider.CompileAssemblyFromSource(options, source);
                }
                catch (Exception ex)
                {
                    LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                           : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                    return null;
                }
            }
        }

        /// <summary>
        /// Ritorna, se tutto ok, l'oggetto MethodInfo corrispondente al metodo contenente l'algoritmo da eseguire. 
        /// Riporta l'errore nella stringa error e ritorna null altrimenti.  
        /// </summary>
        public static MethodInfo getMethod(Assembly ass, out string error)
        {
            error = null;
            MethodInfo method = null;
            if (ass == null)
            {
                LOG.AddLog("The object Assembly passed in InitLibrary.getMethod is a null reference object.");
                error = "Failed attempt to process the job because of error";
                return null;
            }
            try
            {
                foreach (Type tipo in ass.GetTypes())
                {
                    if (String.Compare(tipo.Name, "Driver", false, System.Globalization.CultureInfo.InvariantCulture) == 0)
                    {
                        try
                        {
                            if ((method = tipo.GetMethod("Main")) != null)
                                return method;
                        }
                        catch (AmbiguousMatchException ex) // vale per tipo.getMethod()
                        {
                            error = "More than one method is found with the specified name. This choice don't allow to process the job";
                            return null;
                        }
                        catch (Exception ex)
                        {
                            LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                                   : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                            error = "Unexpected error";
                            return null;
                        }
                    }
                }
                error = "Method not found. Likely the source structure doesn't match with the expected structure";
                return null;
            }
            catch (Exception ex)
            {
                LOG.AddLog((ex.InnerException == null) ? "Exception : " + ex.Message + "\nTargetSite : " + ex.TargetSite + "\nStackTrace : " + ex.StackTrace
                                                       : "InnerException : " + ex.InnerException.Message + "\nTargetSite : " + ex.InnerException.TargetSite + "\nStackTrace : " + ex.InnerException.StackTrace);
                error = "Unexpected error";
                return null;
            }
        }

        /// <summary>
        /// Converte l'array passato come parametro nell'array di parametri che dovrà essere passato al momento dell'invocazione del metodo. 
        /// </summary>
        /// <returns>Ritorna l'array risultato della conversione se algorithm diverso da null, ritorna null altrimenti</returns>
        public static Object[] ConvertParameters(MethodInfo algorithm, Object[] jobParameters)
        {
            if ((algorithm == null) || (jobParameters == null))
            {
                return null;
            }
            ParameterInfo[] typeParams = algorithm.GetParameters();
            Object[] parameters = new Object[jobParameters.Length];
            for (int index = 0; index < jobParameters.Length; index++)
            {
                try
                {
                    parameters[index] = Convert.ChangeType(jobParameters[index], typeParams[index].ParameterType, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    parameters[index] = jobParameters[index];
                }
            }
            return parameters;
        }
    }
}
