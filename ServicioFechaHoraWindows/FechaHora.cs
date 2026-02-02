using ServicioFechaHoraWindows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EjercicioFechaHoraWindows
{
    internal class FechaHora
    {
        public bool ServerRunning { set; get; } = true;
        public int Port { set; get; } = 0;
        ServicioWindows serv = new ServicioWindows();
        string programData = Environment.GetEnvironmentVariable("ProgramData");
        int puertoDefecto = 31416;
        private Socket s;
        string archivoPuerto = "puerto.txt";
        string archivoMensajes = "mensajes.txt";
        DateTime dt;

        public void InitServer()
        {
            Port = GestionarPuerto(archivoPuerto);
            IPEndPoint ie = new IPEndPoint(IPAddress.Any, Port);
            Console.WriteLine("Esperando conexiones... (Ctrl+C para salir)");
            serv.WriteEvent("Puerto:" + Port);
            using (s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    s.Bind(ie);
                    Console.WriteLine($"Servidor iniciado. " +
    $"Escuchando en {ie.Address}:{ie.Port}");
                    s.Listen(1);
                    while (ServerRunning)
                    {
                        Socket client = s.Accept();
                        Thread hilo = new Thread(() => ClientDispatcher(client));
                        hilo.IsBackground = true;
                        hilo.Start();
                    }
                }
                catch (SocketException)
                {
                    serv.WriteEvent("Ambos puertos ocupados");
                    Console.WriteLine("Fin de servidor");
                    CerrarServidor();
                }
            }
        }
        private void ClientDispatcher(Socket sClient)
        {
            using (sClient)
            {
                IPEndPoint ieClient = (IPEndPoint)sClient.RemoteEndPoint;
                Console.WriteLine($"Cliente conectado:{ieClient.Address} " +
                $"en puerto {ieClient.Port}");
                Encoding codificacion = Console.OutputEncoding;
                using (NetworkStream ns = new NetworkStream(sClient))
                using (StreamReader sr = new StreamReader(ns, codificacion))
                using (StreamWriter sw = new StreamWriter(ns, codificacion))
                {
                    sw.AutoFlush = true;
                    string welcome = "Bienvenido al Servicio de Fecha y Hora, Comandos: time | date | all";
                    sw.WriteLine(welcome);
                    string msg = "";
                    DateTime ahora;
                    try
                    {
                        msg = sr.ReadLine();
                        if (msg != null)
                        {
                            switch (msg)
                            {
                                case "time":
                                    ahora = DateTime.Now;
                                    sw.WriteLine(ahora.ToString("HH:mm:ss"));
                                    break;
                                case "date":
                                    ahora = DateTime.Now;
                                    sw.WriteLine(ahora.ToString("dd-MM-yyyy"));
                                    break;
                                case "all":
                                    ahora = DateTime.Now;
                                    sw.WriteLine(ahora.ToString("dd-MM-yyyy HH:mm:ss"));
                                    break;

                                default:
                                    Console.WriteLine($"No se ha reconocido el comando {msg} en la lista de comandos disponibles");
                                    serv.WriteEvent("Comando inválido:" + msg);
                                    Console.WriteLine("Comandos disponibles: time | date | all");
                                    break;
                            }
                            Console.WriteLine($"El cliente usó {msg}");
                            dt = DateTime.Now;
                            RegistrarMensajes(archivoMensajes, $"[{dt.ToString("dd/MM/yyyy - HH:mm:ss")}-@{ieClient.Address}:{ieClient.Port}] {msg}");

                        }
                    }
                    catch (IOException)
                    {
                        msg = null;
                    }
                }
            }
        }

        public int GestionarPuerto(string NombreArchivo)
        {
            try
            {
                int puerto = 0;
                DirectoryInfo d;
                StreamReader sr;
                d = new DirectoryInfo(programData);
                Directory.SetCurrentDirectory(d.FullName);
                string ruta = d.FullName + "\\" + NombreArchivo;
                if (File.Exists(ruta))
                {
                    using (sr = new StreamReader(ruta))
                    {
                        puerto = int.Parse(sr.ReadLine());
                    }
                }
                return puerto;
            }
            catch (Exception e) when (e is IOException || e is FileNotFoundException)
            {
                //Devuelvo un puerto por defecto en caso de error con el archivo y notifico error
                serv.WriteEvent("Error de archivo de puerto");
                return puertoDefecto;
            }
        }

        public void RegistrarMensajes(string NombreArchivo, string mensaje)
        {
            try
            {
                DirectoryInfo d;
                StreamWriter sw;
                d = new DirectoryInfo(programData);
                Directory.SetCurrentDirectory(d.FullName);
                string ruta = d.FullName + "\\" + NombreArchivo;
                using (sw = new StreamWriter(ruta, true))
                {
                    sw.WriteLine(mensaje);
                }
            }
            catch (Exception e) when (e is IOException || e is FileNotFoundException)
            {
                // Notifico error con el archivo 
                serv.WriteEvent("Error de archivo de comandos realizados");
            }
        }

        public void CerrarServidor()
        {
            Console.WriteLine("Cerrando Servidor");
            ServerRunning = false;
            s.Close();
        }
    }
}
