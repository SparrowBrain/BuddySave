namespace BuddySave.TestTools
{
    public class TempDir : IDisposable
    {
        public TempDir(string dirName, bool create)
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), dirName);
            if (create)
            {
                Create();
            }
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }

        public void Create()
        {
            Directory.CreateDirectory(Path);
        }
    }
}