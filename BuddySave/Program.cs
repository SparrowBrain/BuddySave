namespace BuddySave
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Hello World!");
        }
    }

    internal class GameSave
    {
        private string Name { get; set; }

        private string LocalPath { get; set; }

        private string CloudPath { get; set; }
    }

    internal interface ISharedSaveManager
    {
        void Start(GameSave gameSave);

        void Stop(GameSave gameSave);
    }

    internal interface ICloudManager
    {
        void UploadSave(GameSave gameSave);

        void DownloadSave(GameSave gameSave);

        bool LockExists(GameSave save);

        void CreateLock(GameSave save);

        void DeleteLock(GameSave save);
    }
}