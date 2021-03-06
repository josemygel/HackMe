﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Servidor
{
    public partial class ServidorForm : Form
    {
        public delegate void ClientCarrier(ConexionTcp conexionTcp);
        public event ClientCarrier OnClientConnected;
        public event ClientCarrier OnClientDisconnected;
        public delegate void DataRecieved(ConexionTcp conexionTcp, string data);
        public event DataRecieved OnDataRecieved;

        private TcpListener _tcpListener;
        private Thread _acceptThread;
        private List<ConexionTcp> connectedClients = new List<ConexionTcp>();
        private string ADDRESS = "127.0.0.1";
        private int PORT = 1982;

        public ServidorForm()
        {
            InitializeComponent();
        }

        private void ServidorForm_Load(object sender, EventArgs e)
        {
            // TODO: esta línea de código carga datos en la tabla 'dataSet11.Conexiones' Puede moverla o quitarla según sea necesario.
            this.conexionesTableAdapter.Fill(dataSet11.Conexiones);
            // TODO: esta línea de código carga datos en la tabla 'dataSet11.Session' Puede moverla o quitarla según sea necesario.
            this.sessionTableAdapter.Fill(dataSet11.Session);
            // TODO: esta línea de código carga datos en la tabla 'dataSet11.Usuarios' Puede moverla o quitarla según sea necesario.
            this.usuariosTableAdapter.Fill(dataSet11.Usuarios);


            // TODO: esta línea de código carga datos en la tabla 'uNED.Usuarios' Puede moverla o quitarla según sea necesario.
            //this.usuariosTableAdapter.Fill(this.uNED.Usuarios);

            //Se activa este metodo cuando se activa ese delegado
            OnDataRecieved += MensajeRecibido;
            OnClientConnected += ConexionRecibida;
            OnClientDisconnected += ConexionCerrada;

            EscucharClientes(ADDRESS, PORT);
        }

        private void MensajeRecibido(ConexionTcp conexionTcp, string datos)
        {

            //GUARDAR EN LA BASE DE DATOS EL INTENTO DE CONEXION!!!

            var paquete = new Paquete(datos);
            string comando = paquete.Comando;
            if (comando == "login")   //AQUI AÑADIMOS MAS TIPOS DE COMANDO
            {  
                string contenido = paquete.Contenido;
                List<string> valores = Mapa.Deserializar(contenido);

                var msgPack = new Paquete();

                try
                {
                    usuariosTableAdapter.Fill(dataSet11.Usuarios);
                    if (string.IsNullOrEmpty(dataSet11.Usuarios.Select(valores[0]).ToString()))
                        msgPack = new Paquete("resultado", "Sesion Iniciada.");
                }
                catch (Exception)
                {
                    msgPack = new Paquete("resultado", "El usuario no existe, registrese.");
                }
                //usuariosTableAdapter.GetData()

                conexionTcp.EnviarPaquete(msgPack);
            }
            
            if (comando == "register")
            {
                string contenido = paquete.Contenido;
                List<string> valores = Mapa.Deserializar(contenido);
                
                var msgPack = new Paquete();

                try
                {
                    usuariosTableAdapter.Insert(valores[0], valores[1]);
                    usuariosTableAdapter.Update(dataSet11.Usuarios);
                    usuariosTableAdapter.Fill(dataSet11.Usuarios);
                    msgPack = new Paquete("resultado", "Registro realizado con éxito.");
                }
                catch (Exception)
                {
                    msgPack = new Paquete("resultado", "El usuario ya existe.");
                }
                conexionTcp.EnviarPaquete(msgPack);
            }
        }

        private void ConexionRecibida(ConexionTcp conexionTcp)
        {
            //LOCK SIRVE PARA BLOQUEAR VARIABLES (¿Semáforos?)
            lock (connectedClients)
                if (!connectedClients.Contains(conexionTcp))
                    connectedClients.Add(conexionTcp);
            Invoke(new Action(() => label1.Text = string.Format("Clientes: {0}", connectedClients.Count)));
        }

        private void ConexionCerrada(ConexionTcp conexionTcp)
        {
            lock (connectedClients)
                if (connectedClients.Contains(conexionTcp))
                {
                    int cliIndex = connectedClients.IndexOf(conexionTcp);
                    connectedClients.RemoveAt(cliIndex);
                }
            Invoke(new Action(() => label1.Text = string.Format("Clientes: {0}", connectedClients.Count)));
        }

        private void EscucharClientes(string ipAddress, int port)
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
                _tcpListener.Start();
                _acceptThread = new Thread(AceptarClientes);
                _acceptThread.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
            }
        }
        private void AceptarClientes()
        {
            do
            {
                try
                {
                    var conexion = _tcpListener.AcceptTcpClient();
                    var srvClient = new ConexionTcp(conexion)
                    {
                        ReadThread = new Thread(LeerDatos)
                    };
                    srvClient.ReadThread.Start(srvClient);

                    if (OnClientConnected != null)
                        OnClientConnected(srvClient);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString());
                }

            } while(true);
        }

        private void LeerDatos(object client)
        {

            //METODO PARA OPTENER LA IP
            string ip = ((IPEndPoint)(client as ConexionTcp).TcpClient.Client.RemoteEndPoint).Address.ToString();
            if (!string.IsNullOrEmpty(ip))
            {
                conexionesTableAdapter.Insert(ip, DateTime.Now);
                conexionesTableAdapter.Fill(dataSet11.Conexiones);
            }



            /*
             * Profundizar en internet sobre la lectura del buffer
             */
            var cli = client as ConexionTcp;
            var charBuffer = new List<int>();

            do
            {
                try
                {
                    if (cli == null)
                        break;
                    if (cli.StreamReader.EndOfStream)
                        break;
                    int charCode = cli.StreamReader.Read();
                    if (charCode == -1)
                        break;
                    if (charCode != 0)
                    {
                        charBuffer.Add(charCode);
                        continue;
                    }
                    if (OnDataRecieved != null)
                    {
                        var chars = new char[charBuffer.Count];
                        //Convert all the character codes to their representable characters
                        for (int i = 0; i < charBuffer.Count; i++)
                        {
                            chars[i] = Convert.ToChar(charBuffer[i]);
                        }
                        //Convert the character array to a string
                        var message = new string(chars);

                        //Invoke our event
                        OnDataRecieved(cli, message);
                    }
                    charBuffer.Clear();
                }
                catch (IOException)
                {
                    break;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message.ToString());

                    break;
                }
            } while (true);

            if (OnClientDisconnected != null)
                OnClientDisconnected(cli);
        }

        private void ServidorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Cierra puertos, conexiones, etc.
            Environment.Exit(0);
        }

        private void usuariosBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.usuariosBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.dataSet11);

        }

        private void bindingNavigatorAddNewItem_Click(object sender, EventArgs e)
        {

        }

        /*
private void usuariosBindingNavigatorSaveItem_Click(object sender, EventArgs e)
{
this.Validate();
//this.usuariosBindingSource.EndEdit();
//this.tableAdapterManager.UpdateAll(this.uNED);

}

private void button1_Click(object sender, EventArgs e)
{
//this.usuariosTableAdapter.Fill(this.uNED.Usuarios);
}
*/
    }
}
