using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliente
{
    public class Mapa
    {
        //No es el mismo que el de Paquete, Paquete serializa Comando:Contenido, y este Contenido1,Contenido2,...
        public static string Serializar(List<string> lista)
        {
            if (lista.Count == 0)
            {
                return null;
            }

            //Comprobaremos cual es el primer elemento (por el uso de comas)
            bool primero = true;
            StringBuilder salida = new StringBuilder();

            //Con este foreach recorreremos todos los valores de lista usandolos en linea
            foreach (var linea in lista)
            {
                if (primero)
                {
                    //Añadimos "linea"
                    salida.Append(linea);
                    primero = false;
                }
                else
                {
                    //Añadimos ",linea"
                    salida.Append(string.Format(",{0}", linea));
                }

            }

            return salida.ToString();

        }

        public static List<string> Deserializar(string entrada)
        {
            var lista = new List<string>();

            if (string.IsNullOrEmpty(entrada))
            {
                return lista;
            }

            try
            {
                //Split dividirá en un array todos los strings divididos en ',' sin coger las comas,
                foreach (string linea in entrada.Split(','))
                {
                    lista.Add(linea);
                }
            }
            catch (Exception)
            {
                return null;
            }

            return lista;
        }
    }
}
