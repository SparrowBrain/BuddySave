namespace BuddySave;

internal interface ISharedSaveOrchestrator
{
    Task Load(GameSave gameSave);

    void Save(GameSave gameSave);
}