using BuddySave.Core.Models;

namespace BuddySave.Core;

public interface IGamingSession
{
    Task Run(GameSave gameSave, Session session, string serverPath);
}