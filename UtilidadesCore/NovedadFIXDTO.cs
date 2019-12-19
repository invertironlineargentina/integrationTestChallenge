using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilidadesCore
{
    public class NovedadFIXDTO
    {
        public DateTime FechaCancelacion { get; set; }

        public DateTime FechaConcertacion { get; set; }

        public decimal Cantidad { get; set; }

        public decimal Precio { get; set; }

        public string Partida { get; set; }

        public string IdTransaccionContraparte { get; set; }

        public string IdFix { get; set; }

        public int IdTransaccion { get; set; }

        public TipoAccionOrden TipoAccionOrden { get; set; }
    }
}
