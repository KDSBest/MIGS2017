using ReliableUdp.Logging;

namespace UdpDispatcher
{
    public class EventSourceLogger : IUdpLogger
    {
        public void Log(string str)
        {
            ServiceEventSource.Current.Message(str);
        }
    }
}