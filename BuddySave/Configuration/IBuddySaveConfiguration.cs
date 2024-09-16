using BuddySave.Core;
using BuddySave.Core.Models;

namespace BuddySave.Configuration;

public interface IBuddySaveConfiguration
{
	string CloudPath { get; set; }

	Session Session { get; }

	ServerParameters ServerParameters { get; set; }

	ClientParameters ClientParameters { get; set; }

	GameSave GameSave { get; }
}