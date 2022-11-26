using BuddySave.Core.Models;

namespace BuddySave.Core;

internal interface ISharedSaveOrchestrator
{
    Task Load(GameSave gameSave, Session session);

    Task Save(GameSave gameSave, Session session);
}