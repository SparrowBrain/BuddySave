using BuddySave.Core.Models;

namespace BuddySave.Core;

public class GamingSession(ILockManager lockManager, IServerSession serverSession, IClientSession clientSession)
{
	public async Task Play(GameSave gameSave, Session session, ServerParameters serverParameters, ClientParameters clientParameters)
	{
		var lockExists = lockManager.LockExists(gameSave);
		var sessionToConnectTo = lockExists ? await lockManager.GetLockedSession(gameSave) : session;

		var serverTask = Task.CompletedTask;
		if (!lockExists)
		{
			serverTask = serverSession.RunServerWithAutoSave(gameSave, sessionToConnectTo, serverParameters);
		}

		clientSession.RunClient(sessionToConnectTo, clientParameters);

		await serverTask;
	}
}