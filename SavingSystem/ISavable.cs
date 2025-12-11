using System;

namespace SavingSystem
{
    public interface ISavable
    {
        void Save(ref GameData gameData);
        void Load(ref GameData gameData);
    }
}