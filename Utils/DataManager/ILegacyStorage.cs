using Utils.GPS;

namespace Utils.DataManager
{
    public interface ILegacyStorage
    {
        void Store(double[] dataX, double[] dataY, double[] dataZ, 
            GpsData gps);
        void Close();
        void Discard();
        void Upload();
        bool UploadOnClose { get; set; }
    }
}
