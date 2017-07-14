using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servidor
{
    public class Paquete
    {
        public string Comando { get; set; }
        public string Contenido { get; set; }

        public Paquete()
        {

        }

        //Constructor con comando y contenido
        public Paquete(string comando, string contenido)
        {
            Comando = comando;
            Contenido = contenido;
        }
        
        //Constructor que recibirá por ejemplo... "login:usuario,contraseña"
        public Paquete(string datos)
        {
            //Buscamos la posición de los dos puntos para separar los datos
            //StringComparison.Ordinal Buscara 1 por 1 hasta encontrar ":" devolviendo a IndexOf su posicion.
            int posicion = datos.IndexOf(":",StringComparison.Ordinal);

            //Esta linea se encargara de dividir datos para obtener desde el comienzo del string hasta la posicion de ":"
            Comando = datos.Substring(0,posicion);

            //Cogeremos desde el tamaño de Comando+1 hasta el final con el siguiente comando
            Contenido = datos.Substring(Comando.Length+1);
        }

        //Metodo encargado de devolver un string con el contenido de Comando:Contenido,Contenido...
        public string Serializar()
        {
            //Crear un string de acuerdo a los contenidos de Contenido y Comando
            return string.Format("{0}:{1}",Comando,Contenido);
        }
        
        //Es un metodo estatico (accesible sin instanciar el objeto) y es implicito (de ¿forma usuario?) y operador conversor a string
        public static implicit operator string(Paquete paquete)
        {
            return paquete.Serializar();
        }
    }
}
