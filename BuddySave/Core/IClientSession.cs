using BuddySave.Core.Models;

namespace BuddySave.Core;

public interface IClientSession
{
	void RunClient(Session session, ClientParameters clientParameters);
}