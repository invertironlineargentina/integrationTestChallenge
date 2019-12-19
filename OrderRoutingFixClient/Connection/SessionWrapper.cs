using QuickFix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRoutingFixClient.Connection
{
    public class SessionWrapper : ISessionWrapper
    {
        private readonly Session _session;

        public SessionWrapper() { }

        public SessionWrapper(SessionID sessionID)
        {
            _session = Session.LookupSession(sessionID);
        }

        public ISessionWrapper LookupSession(SessionID sessionID)
        {
            return new SessionWrapper(sessionID);
        }

        public void Reset(string loggedReason)
        {
            _session.Reset(loggedReason);
        }

        public bool IsLoggedOn()
        {
            return _session.IsLoggedOn;
        }

        public bool GenerateLogout()
        {
            return _session.GenerateLogout();
        }
        public virtual bool Send(Message message)
        {
            return _session.Send(message);
        }

        public void Disconnect(string reason)
        {
            _session.Disconnect(reason);
        }
    }
    
}
