using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace WAVYMusic
{
    /// <summary>
    /// The song list for WAVYMusic. Is placed in the "Assets/<see cref="Resources"/>" folder for ease of access.<para/>
    /// This <see cref="ScriptableObject"/> is used both in during runtime to have a global song list. Not having this in your project will break WAVYMusic completely.
    /// </summary>
    public class WAVYSongList : ScriptableObject
    {
        private const string FILE_NAME = "WAVY Song List";

        private static WAVYSongList _cachedObj;
        public static WAVYSongList Obj
        {
            get
            {
                if (_cachedObj == null)
                {
                    _cachedObj = Resources.Load<WAVYSongList>(FILE_NAME);

#if UNITY_EDITOR
                    if (_cachedObj == null)
                    {
                        WAVYSongList obj = CreateInstance<WAVYSongList>();

                        string folder = Path.Combine(Application.dataPath, "Resources");
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }

                        AssetDatabase.CreateAsset(obj, $"Assets/Resources/{FILE_NAME}.asset");

                        Debug.LogWarning("There is no \"WAVYSettings\" in the project! A new one has been created in \"Assets/Resources\". You can move this file to another resources folder if you so wish.", obj);

                        _cachedObj = obj;
                    }
#endif
                }

                return _cachedObj;
            }
        }

#if UNITY_EDITOR
        private SerializedObject _cachedSerializedObject;
        public SerializedObject SerializedObject
        {
            get
            {
                if (_cachedSerializedObject == null)
                {
                    _cachedSerializedObject = new SerializedObject(this);
                }

                return _cachedSerializedObject;
            }
        }
#endif

        [SerializeReference] public List<WAVYSong> Songs = new List<WAVYSong>();

        private static Dictionary<string, WAVYSong> _cachedSongDictionary = null;
        public static Dictionary<string, WAVYSong> SongDictionary
        {
            get
            {
                if (_cachedSongDictionary == null)
                {
                    _cachedSongDictionary = new Dictionary<string, WAVYSong>();

                    foreach (WAVYSong song in Obj.Songs)
                    {
                        if (_cachedSongDictionary.ContainsKey(song.name) || song == null)
                        {
                            continue;
                        }

                        _cachedSongDictionary.Add(song.name, song);
                    }
                }

                return _cachedSongDictionary;
            }
        }

        /// <summary>
        /// Returns the <see cref="WAVYSong"/> with the given <paramref name="songName"/>. <para/>
        /// Keep in mind that this will refer to the name of the <see cref="WAVYSong"/> <see cref="ScriptableObject"/>. Not the <see cref="WAVYSong.DisplayName"/>.
        /// </summary>
        public static WAVYSong GetSong(string songName)
        {
#if UNITY_EDITOR
            // Throw error if the song is not in the dictionary
            if (!SongDictionary.ContainsKey(songName))
            {
                throw new KeyNotFoundException($"A song with the name \"{songName}\" doesn't exist in the WAVYSongList!");
            }
#endif

            return SongDictionary[songName];
        }
    }
}
