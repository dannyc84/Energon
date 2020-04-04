using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Phidgets;

namespace Ammeter
{
    public class Ammeter
    {
        /// <summary>
        /// 
        /// Classe che definisce le operazioni dell'amperometro.
        /// 
        /// File: Ammeter.cs
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
        /// 
        /// </summary>
        private class AmmeterSensor
        {
            /// <summary>
            /// accumulator: accumulatore delle letture dell'intervallo corrente
            /// count: numero di letture dell'intervallo corrente
            /// t1: tempo corrispondente al momento dell'ultima media calcolata
            /// sensor: oggetto corrispondente al sensore
            /// avgInterval: intervallo di campionamento in millisesondi
            /// startInterval: tempo di inizio del campionamento corrente
            /// averages: coda di campionamenti effettuati
            /// </summary>
            private InterfaceKitAnalogSensor sensor;
            private const int avgInterval = 100;
            private const double k = 0.04204;
            private int accumulator = 0;
            private int samplescount = 0;
            private DateTime startInterval = DateTime.Now;
            private Queue<double> averages = new Queue<double>();

            /// <summary>
            /// Restituisce o imposta l'intervallo iniziale.
            /// </summary>
            public DateTime StartInterval
            {
                get { return startInterval; }
                set { startInterval = value; }
            }

            /// <summary>
            /// Inizializza i sensori.
            /// </summary>
            /// <param name="ikit"> interfaccia </param>
            /// <param name="index"> indice del sensore </param>
            public AmmeterSensor(InterfaceKit ikit, int index)
            {
                this.sensor = ikit.sensors[index];
                this.sensor.Sensitivity = 1;
            }

            /// <summary>
            /// Aggiunge un nuovo valore.
            /// </summary>
            /// <param name="value"> nuovo valore </param>
            public void addValue(int value)
            {
                DateTime t1 = DateTime.Now;
                if (t1.Subtract(startInterval).TotalMilliseconds >= avgInterval)
                {
                    this.averages.Enqueue(((samplescount == 0) ? (double)value : ((double)this.accumulator) / ((double)this.samplescount)) * AmmeterSensor.k);
                    this.startInterval = t1;
                    this.samplescount = 0;
                    this.accumulator = 0;
                }
                this.accumulator += value;
                this.samplescount++;
            }

            /// <summary>
            /// Restituisce l'ultima media registrata.
            /// </summary>
            /// <returns> ultima media registrata </returns>
            public double getLastAvg()
            {
                if (this.averages.Count > 0)
                    return this.averages.Dequeue();
                double avg = ((samplescount == 0) ? (double)this.sensor.Value : (((double)this.accumulator) / ((double)this.samplescount))) * AmmeterSensor.k;
                this.startInterval = DateTime.Now;
                this.samplescount = 0;
                this.accumulator = 0;
                return avg;
            }

            /// <summary>
            /// Svuota l'accumulatore, il numero di sample e la coda dei valori.
            /// </summary>
            public void reset()
            {
                accumulator = 0;
                samplescount = 0;
                averages.Clear();
            }
        }

        /// <summary>
        /// ikit: interfaccia
        /// num_sensors: numero di sensori
        /// sensors: lista di sensori
        /// waitAttached: tempo d'attesa del collegamento
        /// </summary>
        private static InterfaceKit ikit;
        private const int num_sensors = 3;
        private static List<AmmeterSensor> sensors;
        private const int waitAttached = 10000;

        /// <summary>
        /// Inizializza l'amperometro.
        /// </summary>
        public static void initAmmeter()
        {
            ikit = new InterfaceKit();
            ikit.open();
            ikit.waitForAttachment(waitAttached);
            sensors = new List<AmmeterSensor>();
            for (int i = 0; i < num_sensors; i++)
                sensors.Add(new AmmeterSensor(ikit, i));
        }

        /// <summary>
        /// Avvio delle rilevazioni.
        /// </summary>
        public static void Start()
        {
            DateTime t1 = DateTime.Now;
            foreach (AmmeterSensor sensor in sensors)
                sensor.StartInterval = t1;
            ikit.SensorChange += ikit_SensorChange;
        }

        /// <summary>
        /// Rileva i cambiamenti di valore nei sensori.
        /// </summary>
        /// <param name="sender"> chiamante </param>
        /// <param name="e"> evento </param>
        private static void ikit_SensorChange(object sender, Phidgets.Events.SensorChangeEventArgs e)
        {
            if (e.Index < sensors.Count)
            {
                sensors[e.Index].addValue(e.Value);
            }
        }

        /// <summary>
        /// Calcolo delle medie delle rilevazioni accumulate.
        /// </summary>
        /// <returns> media </returns>
        public static float getConsumption()
        {
            double total = 0.0;
            foreach (AmmeterSensor sensor in sensors)
            {
                total += sensor.getLastAvg();
            }
            return (float)total;
        }

        /// <summary>
        /// Interrompe le rilevazioni.
        /// </summary>
        public static void Stop()
        {
            ikit.SensorChange -= ikit_SensorChange;
            foreach (AmmeterSensor sensor in sensors)
            {
                sensor.reset();
            }
        }

        /// <summary>
        /// Chiusura dell'interfaccia.
        /// </summary>
        public static void Shutdown()
        {
            if (ikit != null)
                ikit.close();
        }
    }
}
