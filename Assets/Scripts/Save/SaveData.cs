using System;
using System.Collections.Generic;

namespace Chillquarium.Gameplay
{
    // Plain data written to the save file as JSON. Public fields, because JsonUtility writes public fields.
    [Serializable]
    public sealed class SaveData
    {
        public double money;
        public List<FishSaveData> fishes = new();
    }

    [Serializable]
    public sealed class FishSaveData
    {
        // The species asset's file name, e.g. "NeonTetra". Renaming the asset loses saved fish of that species.
        public string species;
        public float xp;
    }
}
