using System.ServiceModel;

namespace Ymf825
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Single,
        UseSynchronizationContext = false)]
    public class Ymf825Client : IYmf825Client
    {
        #region -- Private Fields --

        private readonly Ymf825Driver driver;
        private readonly Ymf825 spiInterface;

        #endregion

        #region -- Constructors --

        public Ymf825Client(Ymf825 spiInterface)
        {
            this.spiInterface = spiInterface;
            driver = new Ymf825Driver(spiInterface);
        }

        #endregion

        #region -- Public Methods --

        public bool Ping() => true;

        public Ymf825Driver GetDriver() => driver;

        public Ymf825 GetSpiInterface() => spiInterface;

        #endregion
        
    }
}
