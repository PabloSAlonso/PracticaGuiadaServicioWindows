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

        public void InitServer()
        {
            Port = GestionarPuerto("puerto.txt");
            IPEndPoint ie = new IPEndPoint(IPAddress.Any, Port);
            Console.WriteLine("Esperando conexiones... (Ctrl+C para salir)");
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
                    Console.WriteLine("Fin de servidor");
                }
            }
        }
        private Socket s;
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
                    string welcome = "Bienvenido al Servicio de Fecha y Hora, Comandos: time | date | all | close *****";
                    sw.WriteLine(welcome);
                    string msg = "";
                    string comando;
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
                                    Console.WriteLine("Comandos disponibles: time | date | all | close *****");
                                    break;
                            }
                            Console.WriteLine($"El cliente usó {msg}");
                        }
                    }
                    catch (IOException)
                    {
                        msg = null;
                    }
                }
            }
        }

        string ProgramData = Environment.GetEnvironmentVariable("ProgramData");
        public int GestionarPuerto(string NombreArchivo)
        {
            try
            {
                int puerto = 0;
                DirectoryInfo d;
                StreamReader sr;
                d = new DirectoryInfo(ProgramData);
                Directory.SetCurrentDirectory(d.Name);
                string archivo = "password.txt";
                if (File.Exists(archivo))
                {
                    using (sr = new StreamReader(ProgramData + "\\" + archivo))
                    {
                        puerto = int.Parse(sr.ReadLine());
                    }
                }
                return puerto;
            }
            catch (Exception e) when (e is IOException || e is FileNotFoundException)
            {
                //Devuelvo un puerto por defecto en caso de error con el archivo
                return 31416;
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
