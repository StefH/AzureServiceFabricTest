using System.Fabric;
using WireMock.Admin.Requests;
using WireMock.Logging;

namespace WireMockService
{
    internal class WireMockServiceFabricLogger : IWireMockLogger
    {
        private readonly StatefulServiceContext _context;

        internal WireMockServiceFabricLogger(StatefulServiceContext context)
        {
            _context = context;
        }

        public void Debug(string formatString, params object[] args)
        {
            // throw new NotImplementedException();
        }

        public void DebugRequestResponse(LogEntryModel logEntryModel, bool isAdminRequest)
        {
            // throw new NotImplementedException();
        }

        public void Error(string formatString, params object[] args)
        {
            string finalMessage = string.Format(formatString, args);
            ServiceEventSource.Current.ServiceMessage(_context, $"[Error] {finalMessage}");
        }

        public void Info(string formatString, params object[] args)
        {
            string finalMessage = string.Format(formatString, args);
            ServiceEventSource.Current.ServiceMessage(_context, $"[Info] {finalMessage}");
        }

        public void Warn(string formatString, params object[] args)
        {
            string finalMessage = string.Format(formatString, args);
            ServiceEventSource.Current.ServiceMessage(_context, $"[Warn] {finalMessage}");
        }
    }
}