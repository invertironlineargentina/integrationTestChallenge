using QuickFix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRoutingFixClient.Connection
{
    public interface ISessionWrapper
    {
        bool GenerateLogout();
        bool IsLoggedOn();
        ISessionWrapper LookupSession(SessionID sessionID);
        void Reset(string loggedReason);
        bool Send(Message message);
        void Disconnect(string reason);
    }
}
