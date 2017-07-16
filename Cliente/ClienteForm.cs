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
            conexionTcp.OnDataRecieved += MensajeRecibido;

            if (!conexionTcp.Connectar(IPADDRESS, PORT))
            {
                MessageBox.Show("¡Error conectando con el servidor!");
                return;
            }
        }

        private void MensajeRecibido(string datos)
        {
            var paquete = new Paquete(datos);
            string comando = paquete.Comando;

            //MESSAGEBOX DE COMPROBACION
            MessageBox.Show(string.Format("{0}:{1}",paquete.Comando,paquete.Contenido));
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

        private bool clientConnected()
        {
             if (conexionTcp.TcpClient.Connected)
            {
                return true;
            }
            else
            {
                textBox3.Text = "Servidor desconectado, intentando reconexión...";

                //Closing += ClienteForm_FormClosing;
                conexionTcp.OnDataRecieved += MensajeRecibido;

                if (!conexionTcp.Connectar(IPADDRESS, PORT))
                {
                    MessageBox.Show("¡Error conectando con el servidor!");
                }

                textBox3.Text = "";

                return false;
            }
        }

        //BOTON DE INICIAR SESION (CREAR PARA REGISTRARSE)
        private void logButton_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(textBox1.Text))
                MessageBox.Show("El campo de usuario está vacío, rellénelo por favor.");

            else if (string.IsNullOrEmpty(textBox2.Text))
                MessageBox.Show("El campo de contraseña está vacío, rellénelo por favor.");

            else if (clientConnected())
            {
                var msgLogin = new Paquete("login",string.Format("{0},{1}",textBox1.Text,textBox2.Text));
                conexionTcp.EnviarPaquete(msgLogin);
            }
        }

        private void regButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
                MessageBox.Show("El campo de usuario está vacío, rellénelo por favor.");

            else if (string.IsNullOrEmpty(textBox2.Text))
                MessageBox.Show("El campo de contraseña está vacío, rellénelo por favor.");

            else if (clientConnected())
            {
                var msgLogin = new Paquete("register", string.Format("{0},{1}", textBox1.Text, textBox2.Text));
                conexionTcp.EnviarPaquete(msgLogin);
            }
        }
    }
}
