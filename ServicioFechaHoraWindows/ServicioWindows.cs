using EjercicioFechaHoraWindows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServicioFechaHoraWindows
{
    public partial class ServicioWindows : ServiceBase
    {
        public ServicioWindows()
        {
            InitializeComponent();
            ServiceInstaller serviceInstaller = new ServiceInstaller();
            serviceInstaller.Description = "Descripción de mi proyecto";
        }
        public void WriteEvent(string mensaje)
        {
            const string nombre = "ServicioWindows"; // Nombre de la fuente de eventos. Escribe el mensaje deseado en el visor de eventos
            try
            {
                EventLog.WriteEntry(nombre, mensaje);
            }
            catch (Exception ex) 
            {
                dt = DateTime.Now;
                fh.RegistrarMensajes(archivoMensajes, $"[ERROR] {nombre} {dt}");
            }
        }

        FechaHora fh = new FechaHora();
        DateTime dt;
        string archivoMensajes = "mensajes.txt";
        protected override void OnStart(string[] args)
        {
            Thread hilo = new Thread(() => fh.InitServer());
            hilo.Start();
        }

        protected override void OnStop()
        {
            WriteEvent("Se detuvo el proceso");
            fh.CerrarServidor();
        }
    }
}
