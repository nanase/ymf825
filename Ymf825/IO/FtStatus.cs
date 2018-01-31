using System.Diagnostics.CodeAnalysis;

namespace Ymf825.IO
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum FtStatus : uint
    {
        FT_OK,
        FT_INVALID_HANDLE,
        FT_DEVICE_NOT_FOUND,
        FT_DEVICE_NOT_OPENED,
        FT_IO_ERROR,
        FT_INSUFFICIENT_RESOURCES,
        FT_INVALID_PARAMETER,
        FT_INVALID_BAUD_RATE,

        FT_DEVICE_NOT_OPENED_FOR_ERASE,
        FT_DEVICE_NOT_OPENED_FOR_WRITE,
        FT_FAILED_TO_WRITE_DEVICE,
        FT_EEPROM_READ_FAILED,
        FT_EEPROM_WRITE_FAILED,
        FT_EEPROM_ERASE_FAILED,
        FT_EEPROM_NOT_PRESENT,
        FT_EEPROM_NOT_PROGRAMMED,
        FT_INVALID_ARGS,
        FT_NOT_SUPPORTED,
        FT_OTHER_ERROR,
        FT_DEVICE_LIST_NOT_READY,
    }

    public static class FtStatusEx
    {
        #region -- Public Methods --

        public static string GetErrorMessage(this FtStatus status)
        {
            switch (status)
            {
                case FtStatus.FT_OK:
                    return null;

                case FtStatus.FT_INVALID_HANDLE:
                    return "無効なハンドルが指定されました。";

                case FtStatus.FT_DEVICE_NOT_FOUND:
                    return "指定されたデバイスが見つかりませんでした。";

                case FtStatus.FT_DEVICE_NOT_OPENED:
                    return "指定されたデバイスを開けませんでした。";

                case FtStatus.FT_IO_ERROR:
                    return "IOエラーが発生しました。";

                case FtStatus.FT_INSUFFICIENT_RESOURCES:
                    return "リソースが不足しています。";

                case FtStatus.FT_INVALID_ARGS:
                case FtStatus.FT_INVALID_PARAMETER:
                    return "無効なパラメータが指定されました。";

                case FtStatus.FT_INVALID_BAUD_RATE:
                    return "無効なボーレートが指定されました。";

                case FtStatus.FT_EEPROM_READ_FAILED:
                case FtStatus.FT_EEPROM_ERASE_FAILED:
                case FtStatus.FT_DEVICE_NOT_OPENED_FOR_ERASE:
                    return "読み込まれようとしたデバイスは開かれていませんでした。";

                case FtStatus.FT_EEPROM_WRITE_FAILED:
                case FtStatus.FT_DEVICE_NOT_OPENED_FOR_WRITE:
                    return "書き込まれようとしたデバイスは開かれていませんでした。";

                case FtStatus.FT_FAILED_TO_WRITE_DEVICE:
                    return "デバイスへの書き込みに失敗しました。";

                case FtStatus.FT_EEPROM_NOT_PRESENT:
                case FtStatus.FT_EEPROM_NOT_PROGRAMMED:
                    return "EEPROM がデバイスに存在しません。";

                case FtStatus.FT_NOT_SUPPORTED:
                    return "実行された命令はサポートされていません。";


                case FtStatus.FT_DEVICE_LIST_NOT_READY:
                    return "デバイスリストを取得中です。";

                default:
                    return "不明なエラーです。";
            }
        }

        #endregion
    }
}
