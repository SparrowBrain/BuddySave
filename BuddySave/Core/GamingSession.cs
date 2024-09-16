using BuddySave.Core.Models;

namespace BuddySave.Core;

public class GamingSession(ILockManager lockManager, IServerSession serverSession, IClientSession clientSession) : IGamingSession
{
	private const string LocalhostIp = "127.0.0.1";

	public async Task Play(GameSave gameSave, Session session, ServerParameters serverParameters, ClientParameters clientParameters)
	{
		var lockExists = lockManager.LockExists(gameSave);
		var sessionToConnectTo = lockExists ? await lockManager.GetLockedSession(gameSave) : GetLocalSession(session);

		var runServerWithAutoSave = Task.CompletedTask;
		if (!lockExists)
		{
			runServerWithAutoSave = serverSession.RunServerWithAutoSave(gameSave, session, serverParameters);
		}

		clientSession.StartClient(sessionToConnectTo, clientParameters);

		await runServerWithAutoSave;
	}

	private static Session GetLocalSession(Session session)
	{
		return new Session(session.UserName, LocalhostIp, session.Port);
	}
}