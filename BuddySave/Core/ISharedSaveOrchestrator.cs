using BuddySave.Core.Models;

namespace BuddySave.Core;

public interface ISharedSaveOrchestrator
{
    Task<OrchestratorResult> Load(GameSave gameSave, Session session);

    Task Save(GameSave gameSave, Session session);
}