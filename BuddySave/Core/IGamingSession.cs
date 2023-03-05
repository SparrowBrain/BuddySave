using BuddySave.Core.Models;

namespace BuddySave.Core;

public interface IGamingSession
{
    Task RunServerWithAutoSave(GameSave gameSave, Session session, ServerParameters serverParameters);
}