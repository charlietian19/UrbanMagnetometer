namespace Utils.GDrive
{
    public interface IUploader
    {
        void Upload(string fileNameToUpload, string parent);
    }
}
