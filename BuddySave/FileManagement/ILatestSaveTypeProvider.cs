using BuddySave.Core.Models;

namespace BuddySave.FileManagement;

public interface ILatestSaveTypeProvider
{
    public SaveType Get(GameSave gameSave);
}