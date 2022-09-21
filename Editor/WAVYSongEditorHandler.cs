using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using WAVYMusic;

namespace WAVYMusicEditor
{
    /// <summary>
    /// Handles creating and deleting <see cref="WAVYSong"/>.
    /// </summary>
    public class WAVYSongEditorHandler : AssetModificationProcessor
    {
        [MenuItem("Assets/Create/WAVY Song", false, 150)]
        private static void CreateAsset()
        {
            // Get the currently opened project window path
            Type projectWindowUtilType = typeof(ProjectWindowUtil);
            MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            string path = getActiveFolderPath.Invoke(null, new object[0]).ToString();

            // Create a new WAVY Song
            WAVYSong obj = ScriptableObject.CreateInstance<WAVYSong>();

            ProjectWindowUtil.CreateAsset(obj, path + "/New WAVY Song.asset");

            WAVYSongList.Obj.Songs.Add(obj);
        }

        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            WAVYSong obj = AssetDatabase.LoadAssetAtPath<WAVYSong>(assetPath);

            // Remove song from song list if it was a WAVY Song and is in the WAVY Song List
            if (obj != null && WAVYSongList.Obj.Songs.Contains(obj))
            {
                WAVYSongList.Obj.Songs.Remove(obj);
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}
