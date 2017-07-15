using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cliente
{
    public partial class ClienteForm : Form
    {
        public static ConexionTcp conexionTcp = new ConexionTcp();
        public static string IPADDRESS = "127.0.0.1";
        public static int PORT = 1982;
        public ClienteForm()
        {
            InitializeComponent();
        }

        private void ClienteForm_Load(object sender, EventArgs e)
        {

            //Closing += ClienteForm_FormClosing;
            conexionTcp.OnDataRecieved += MensajeRecibido;

            if (!conexionTcp.Connectar(IPADDRESS, PORT))
            {
                MessageBox.Show("¡Error conectando con el servidor!");
            }
        }

        private void MensajeRecibido(string datos)
        {
            var paquete = new Paquete(datos);
            string comando = paquete.Comando;
            if (comando == "resultado")
            {
                string contenido = paquete.Contenido;

                Invoke(new Action(() => label1.Text = string.Format("Respuesta:{0}", contenido)));
            }
        }

        private void ClienteForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        //BOTON DE INICIAR SESION (CREAR PARA REGISTRARSE)
        private void button1_Click(object sender, EventArgs e)
        {
            if (conexionTcp.TcpClient.Connected)
            {
                var msgLogin = new Paquete("login",string.Format("{0},{1}",textBox1.Text,textBox2.Text));
                conexionTcp.EnviarPaquete(msgLogin);
            }
            else
            {
                textBox3.Text = "Servidor desconectado, intentando reconexión...";

                Closing += ClienteForm_FormClosing;
                conexionTcp.OnDataRecieved += MensajeRecibido;

                if (!conexionTcp.Connectar(IPADDRESS, PORT))
                {
                    MessageBox.Show("¡Error conectando con el servidor!");
                }

                textBox3.Text = "";
            }
        }
    }
}
