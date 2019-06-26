namespace RA
{
    public class FormContent
    {
        public FormContent(string name, string content, string contentDispositionName = "form-data", string contentType = "multipart/form-data")
        {
            Name = name;
            Content = content;
            ContentType = contentType;
            ContentDispositionName = contentDispositionName;
        }

        public string Name { get; private set; }
        public string Content { get; private set; }
        public string ContentType { get; private set; }
        public string ContentDispositionName { get; private set; }
    }
}