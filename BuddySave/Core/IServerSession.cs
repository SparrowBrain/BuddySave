using BuddySave.Core.Models;

namespace BuddySave.Core;

public interface IServerSession
{
    Task RunServerWithAutoSave(GameSave gameSave, Session session, ServerParameters serverParameters);
}