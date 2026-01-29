using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MiPrimerServicio
{
    public partial class MiPrimerServicio : ServiceBase
    {
        public MiPrimerServicio() 
        {
            InitializeComponent();
        }

        public void WriteEvent(string mensaje)
        {
            const string nombre = "MiPrimerServicio"; // Nombre de la fuente de eventos.
                                                      // Escribe el mensaje deseado en el visor de eventos
            EventLog.WriteEntry(nombre, mensaje);


        }

        private System.Timers.Timer timer;
        protected override void OnStart(string[] args)
        {
            WriteEvent("Iniciando MiPrimerServicio mediante OnStart");
            timer = new System.Timers.Timer();
            timer.Interval = 10000; // cada 10 segundos
            timer.Elapsed += this.TimerTick;
            timer.Start();
        }

        private int t = 0;
        public void TimerTick(object sender, System.Timers.ElapsedEventArgs args)
        {
            WriteEvent("MiPrimerServicio lleva ejecutándose {t} segundos.");
            t += 10;
        }

        protected override void OnPause()
        {
            WriteEvent("MiPrimerServicio en Pausa");
            timer.Stop();
        }
        protected override void OnContinue()
        {
            WriteEvent("Continuando MiPrimerServicio");
            timer.Start();
        }
        protected override void OnStop()
        {
            WriteEvent("Deteniendo MiPrimerServicio");
            timer.Stop();
            timer.Dispose();
            t = 0;
        }
    }
}
