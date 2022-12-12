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
            EditorGUILayout.PropertyField(obj.FindProperty("mixerGroup"));
            EditorGUILayout.PropertyField(obj.FindProperty("loopScheduleOffset"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Editor Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(obj.FindProperty("autoNameSong"));
            EditorGUILayout.Space();

            /*
            EditorGUILayout.PropertyField(obj.FindProperty("ShowBPMLines"));
            EditorGUILayout.PropertyField(obj.FindProperty("BPMLinesColor"));

            EditorGUILayout.Space();
            */

            EditorGUILayout.PropertyField(obj.FindProperty("showMarkers"));
            EditorGUILayout.PropertyField(obj.FindProperty("markersColor"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(obj.FindProperty("hideInfoBoxes"));

            return obj.ApplyModifiedProperties();
        }
    }
}
