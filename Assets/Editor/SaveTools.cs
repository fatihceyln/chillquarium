using System.IO;
using Chillquarium.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Chillquarium.EditorTools
{
    /// <summary>
    /// Menu items for the save file: delete it to start a new game, or show it in Finder.
    /// </summary>
    public static class SaveTools
    {
        private const string DeleteSaveMenu = "Tools/Chillquarium/Delete Save";

        [MenuItem(DeleteSaveMenu)]
        private static void DeleteSave()
        {
            File.Delete(GameSave.SavePath);
            Debug.Log($"Save deleted. The next Play starts a new game. ({GameSave.SavePath})");
        }

        // Greyed out in Play Mode: stopping Play would save the game again right away.
        [MenuItem(DeleteSaveMenu, true)]
        private static bool CanDeleteSave()
        {
            return !EditorApplication.isPlaying && File.Exists(GameSave.SavePath);
        }

        [MenuItem("Tools/Chillquarium/Show Save File")]
        private static void ShowSaveFile()
        {
            EditorUtility.RevealInFinder(File.Exists(GameSave.SavePath) ? GameSave.SavePath : Application.persistentDataPath);
        }
    }
}
