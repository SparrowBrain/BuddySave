using BuddySave.Core.Models;

namespace BuddySave.Core;

public interface IGamingSession
{
	Task Play(GameSave gameSave, Session session, ServerParameters serverParameters, ClientParameters clientParameters);
}