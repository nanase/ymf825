using System;
using System.ServiceModel;

namespace Ymf825
{
    public static class Ymf825Server
    {
        #region -- Private Fields --

        private static readonly object SyncObject = new object();
        private static ServiceHost _serviceHost;

        #endregion

        #region -- Public Fields --

        public const string ServiceUri = "net.pipe://localhost/nanase.cc";
        public const string ServiceName = "YMF825Board_SPI_Server_V2";
        public const string ServiceAddress = ServiceUri + "/" + ServiceName;

        #endregion

        #region -- Public Properties --

        public static IYmf825Client Client { get; private set; }

        #endregion

        #region -- Public Methods --

        public static void Start(Ymf825 spiInterface)
        {
            lock (SyncObject)
            {
                Client = new Ymf825Client(spiInterface);
                _serviceHost = new ServiceHost(Client, new Uri(ServiceUri));
                _serviceHost.AddServiceEndpoint(typeof(IYmf825Client), new NetNamedPipeBinding(), ServiceName);
                _serviceHost.Open();
            }
        }

        public static void Stop()
        {
            lock (SyncObject)
            {
                _serviceHost?.Close();
                _serviceHost = null;
                Client = null;
            }
        }

        #endregion
    }
}
