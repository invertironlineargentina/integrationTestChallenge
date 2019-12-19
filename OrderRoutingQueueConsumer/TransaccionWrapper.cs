using IOLEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilidadesCore;

namespace OrderRoutingQueueConsumer
{
    public class TransaccionWrapper
    {
        public Transaccion Transaccion { get; set; }

        public IList<NovedadFIXDTO> Concertaciones { get; set; } = new List<NovedadFIXDTO>();
    }
}
