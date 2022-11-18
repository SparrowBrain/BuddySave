namespace BuddySave;

internal interface ISharedSaveManager
{
    void Start(GameSave gameSave);

    void Stop(GameSave gameSave);
}