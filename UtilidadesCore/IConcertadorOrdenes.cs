using IOLEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilidadesCore
{
    public interface IConcertadorOrdenes
    {
        void CancelarOrden(Transaccion transaccion, NovedadFIXDTO concertacion);
        void Concertar(Transaccion transaccion, NovedadFIXDTO concertacion);
        void ConfirmarRecepcionOrden(Transaccion transaccion, NovedadFIXDTO concertacion);
        void ConfirmarRecepcionYConcertarOrden(Transaccion transaccion, NovedadFIXDTO concertacion);
    }
}
