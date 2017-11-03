using System;
using System.ServiceModel;

namespace Ymf825
{
    [ServiceContract]
    public interface IYmf825Client
    {
        #region -- Methods --

        [OperationContract]
        bool CheckAvailable();

        [OperationContract]
        void ResetHardware();

        [OperationContract]
        void ResetSoftware();

        [OperationContract]
        void Write(byte address, byte data);

        [OperationContract]
        void WriteBuffer(byte[] buffer, int offset, int count);

        [OperationContract]
        void BurstWriteBytes(byte address, byte[] data, int offset, int count);

        [OperationContract]
        byte Read(TargetDevice device, byte address);

        [OperationContract]
        void SetTarget(TargetDevice device);

        #endregion

        #region -- Events --

        event EventHandler<DataTransferedEventArgs> DataWrote;

        event EventHandler<DataBurstWriteEventArgs> DataBurstWrote;

        event EventHandler<DataTransferedEventArgs> DataRead;

        #endregion
    }
}
