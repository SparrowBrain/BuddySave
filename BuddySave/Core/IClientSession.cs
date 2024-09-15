using BuddySave.Core.Models;

namespace BuddySave.Core;

public interface IClientSession
{
	void StartClient(Session session, ClientParameters clientParameters);
}