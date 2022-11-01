using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using WAVYMusic;

namespace WAVYMusicEditor
{
    /// <summary>
    /// The editor for <see cref="WAVYSettings"/>.
    /// </summary>
    [CustomEditor(typeof(WAVYSettings))]
    public class WAVYSettingsEditor : Editor
    {
        private WAVYSettings _settings;

        private void OnEnable()
        {
            _settings = (WAVYSettings)target;
        }

        public override void OnInspectorGUI()
        {
            DrawInspector(_settings.SerializedObject);
        }

        public static bool DrawInspector(SerializedObject obj)
        {
            EditorGUILayout.PropertyField(obj.FindProperty("MixerGroup"));
            EditorGUILayout.PropertyField(obj.FindProperty("LoopScheduleOffset"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Editor Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(obj.FindProperty("AutoNameSong"));
            EditorGUILayout.Space();

            /*
            EditorGUILayout.PropertyField(obj.FindProperty("ShowBPMLines"));
            EditorGUILayout.PropertyField(obj.FindProperty("BPMLinesColor"));

            EditorGUILayout.Space();
            */

            EditorGUILayout.PropertyField(obj.FindProperty("ShowMarkers"));
            EditorGUILayout.PropertyField(obj.FindProperty("MarkersColor"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(obj.FindProperty("HideInfoBoxes"));

            return obj.ApplyModifiedProperties();
        }
    }
}
