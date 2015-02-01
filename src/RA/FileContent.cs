namespace RA
{
    public class FileContent
    {
        public FileContent(string fileName, string contentDispositionName, string contentType, byte[] content)
        {
            FileName = fileName;
            ContentType = contentType;
            ContentDispositionName = contentDispositionName;
            Content = content;
        }

        public string FileName { get; private set; }
        public string ContentType { get; private set; }
        public string ContentDispositionName { get; private set; }
        public byte[] Content { get; private set; }
    }
}