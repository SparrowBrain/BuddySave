using BuddySave.Core.Models;

namespace BuddySave.Core;

internal interface ISharedSaveOrchestrator
{
    Task Load(GameSave gameSave);

    void Save(GameSave gameSave);
}