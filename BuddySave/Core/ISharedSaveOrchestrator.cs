using BuddySave.Core.Models;

namespace BuddySave.Core;

public interface ISharedSaveOrchestrator
{
    Task Load(GameSave gameSave, Session session);

    Task Save(GameSave gameSave, Session session);
}