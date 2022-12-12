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
        [Tooltip("The default mixer group that every WAVYSong will have. This can be overriden if you set a MixerGroup in the WAVYSong itself.")]
        [SerializeField] private AudioMixerGroup mixerGroup;
        public AudioMixerGroup MixerGroup => mixerGroup;

        [Tooltip(
@"Determines how much time beforehand a song should schedule a loop.
For example: if you have a WAVYSong with a loop point set to 100 seconds and the Loop Schedule Offset is at 3, then the song will schedule it's loop 3 seconds before 100 seconds have passed.
The song will still loop at 100 seconds, it'll just schedule the loop a bit beforehand to ensure a PERFECT LOOP.
Leaving this option at it's default value will most likely suffice.")]
        [SerializeField] private float loopScheduleOffset = 1;
        public float LoopScheduleOffset => loopScheduleOffset;
        #endregion

#if UNITY_EDITOR
        #region Editor Settings
        [SerializeField] private bool editorExpanded;
        public bool EditorExpanded => editorExpanded;

        [Tooltip("Toggle if a song should be automatically named to match it's audio file when the file is swapped or changed.")]
        [SerializeField] private bool autoNameSong = true;
        public bool AutoNameSong => autoNameSong;

        /*
        public bool ShowBPMLines = true;
        public Color BPMLinesColor = Color.green;
        */

        [Tooltip("If the WAV markers (cues) should be displayed or not. WAVYMusic loses most of it's functionality if this is turned off so please don't.")]
        [SerializeField] private bool showMarkers = true;
        public bool ShowMarkers => showMarkers;

        [Tooltip("The color of the WAV markers (cues).")]
        [SerializeField] private Color markersColor = Color.yellow;
        public Color MarkersColor => markersColor;

        [Tooltip("Some sections of the WAVYSong inspector has InfoBoxes. This will simply disable those from appearing and cluttering your view if you know what you're doing.")]
        [SerializeField] private bool hideInfoBoxes = false;
        public bool HideInfoBoxes => hideInfoBoxes;
        #endregion
#endif
    }
}
