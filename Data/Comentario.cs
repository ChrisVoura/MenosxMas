using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiPrimeraWebApp.Data
{
    public class Comentario
    {
        public int Id {get; set;}        
        public int ClienteId {get; set;}
        public int ProductoId {get; set;}
        public string? Descripcion {get; set;}
        public int valor {get; set;}
        public DateTime FechaCreaciacion {get; set;} = DateTime.Now;

    }
}