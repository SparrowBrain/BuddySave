using BuddySave.Core;
using BuddySave.Core.Models;

namespace BuddySave.Configuration;

public class BuddySaveConfiguration : IBuddySaveConfiguration
{
	public string CloudPath { get; set; }

	public Session Session { get; set; }

	public ServerParameters ServerParameters { get; set; }

	public ClientParameters ClientParameters { get; set; }

	public GameSave GameSave { get; set; }
}