using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Audio;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace WAVYMusic
{
    /// <summary>
    /// The settings for WAVYMusic. Is placed in the "Assets/<see cref="Resources"/>" folder for ease of access.<para/>
    /// This <see cref="ScriptableObject"/> is at both runtime and in the editor.
    /// </summary>
    public class WAVYSettings : ScriptableObject
    {
        private const string FILE_NAME = "WAVY Settings";
        
        private static WAVYSettings _cachedObj;
        public static WAVYSettings Obj
        {
            get
            {
                if (_cachedObj == null)
                {
                    _cachedObj = Resources.Load<WAVYSettings>(FILE_NAME);

#if UNITY_EDITOR
                    if (_cachedObj == null)
                    {
                        WAVYSettings obj = CreateInstance<WAVYSettings>();

                        string folder = Path.Combine(Application.dataPath, "Resources");
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }

                        AssetDatabase.CreateAsset(obj, $"Assets/Resources/{FILE_NAME}.asset");

                        Debug.LogWarning("There is no \"WAVYSettings\" in the project! A new one has been created in \"Assets/Resources\". You can move this file to another resources folder if you so wish.", obj);
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

        #region Track Settings
        public AudioMixerGroup MixerGroup;
        #endregion

#if UNITY_EDITOR
        #region Editor Settings
        public bool EditorExpanded;

        public bool AutoNameSong = true;

        /*
        public bool ShowBPMLines = true;
        public Color BPMLinesColor = Color.green;
        */

        public bool ShowMarkers = true;
        public Color MarkersColor = Color.yellow;

        public bool HideInfoBoxes = false;
        #endregion
#endif
    }
}
