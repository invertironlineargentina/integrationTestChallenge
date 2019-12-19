using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOLEntities
{
    public class Transaccion
    {
        public int ID { get; set; }
        public string IDFix { get; set; }
        public EstadoTransacciones Estado { get; set; }
        public string Simbolo { get; set; }
        public decimal Cantidad { get; set; }
        public decimal CantidadConcertada { get; set; }
        public DateTime FechaOrden { get; set; }
    }
}
