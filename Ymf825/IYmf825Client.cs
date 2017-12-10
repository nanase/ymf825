using System.ServiceModel;

namespace Ymf825
{
    [ServiceContract]
    public interface IYmf825Client
    {
        #region -- Methods --

        [OperationContract]
        bool Ping();

        [OperationContract]
        Ymf825Driver GetDriver();

        [OperationContract]
        Ymf825 GetSpiInterface();

        #endregion
    }
}