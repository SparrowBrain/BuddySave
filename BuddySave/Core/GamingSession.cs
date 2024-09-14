using BuddySave.Core.Models;

namespace BuddySave.Core;

public class GamingSession(ILockManager lockManager, IServerSession serverSession, IClientSession clientSession) : IGamingSession
{
	public async Task Play(GameSave gameSave, Session session, ServerParameters serverParameters, ClientParameters clientParameters)
	{
		var lockExists = lockManager.LockExists(gameSave);
		var sessionToConnectTo = lockExists ? await lockManager.GetLockedSession(gameSave) : GetLocalSession(session);

		var serverTask = Task.CompletedTask;
		if (!lockExists)
		{
			serverTask = serverSession.RunServerWithAutoSave(gameSave, sessionToConnectTo, serverParameters);
		}

		clientSession.RunClient(sessionToConnectTo, clientParameters);

		await serverTask;
	}

	private static Session GetLocalSession(Session session)
	{
		return new Session(session.UserName, "127.0.0.1", session.Port);
	}
}