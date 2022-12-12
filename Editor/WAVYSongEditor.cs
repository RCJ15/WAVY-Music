using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

using WAVYMusic;

using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace WAVYMusicEditor
{
    /// <summary>
    /// The editor for <see cref="WAVYSong"/>
    /// </summary>
    [CustomEditor(typeof(WAVYSong))]
    public class WAVYSongEditor : Editor
    {
        private WAVYSong _wavySong;
        private static WAVYSettings _settings => WAVYSettings.Obj;
        private static WAVYSongList _list => WAVYSongList.Obj;

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            #region Song Data

            //_wavySong.BPM = EditorGUILayout.FloatField("BPM", _wavySong.BPM);

            SerializedProperty songProp = serializedObject.FindProperty("songClip");

            songProp.isExpanded = Foldout(songProp.isExpanded, "Song Data", () =>
            {
                SerializedProperty displayNameProp = serializedObject.FindProperty("displayName");

                EditorGUILayout.PropertyField(displayNameProp);

                serializedObject.ApplyModifiedProperties();

                EditorGUILayout.PropertyField(songProp);

                if (serializedObject.ApplyModifiedProperties())
                {
                    AudioClip clip = songProp.objectReferenceValue as AudioClip;
                    GenerateMetadata(clip, false);

                    if (clip != null && _settings.AutoNameSong)
                    {
                        displayNameProp.stringValue = clip.name;
                    }

                    serializedObject.FindProperty("loopPoint").doubleValue = clip == null ? 0 : clip.length;
                }

                EditorGUILayout.Space();

                DrawOptionalProperty("haveTracks", "tracks", 32);

                EditorGUILayout.Space();
            });

            EditorGUILayout.Space();

            #endregion

            #region Song Markers

            // First get the rect for both the audio clip and markers
            SerializedProperty loopPointProp = serializedObject.FindProperty("loopPoint");

            loopPointProp.isExpanded = Foldout(loopPointProp.isExpanded, "Song Markers", () =>
            {
                Rect rect = GUILayoutUtility.GetRect(Screen.width, 50);

                EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.3f));

                //DrawBPMLines(songProp.objectReferenceValue as AudioClip, rect);
                DrawAudioSamples(songProp.objectReferenceValue as AudioClip, rect);
                DrawMarkers(_wavySong.Metadata, rect);

                EditorGUI.BeginDisabledGroup(songProp.objectReferenceValue == null);

                if (GUILayout.Button("ReGenerate Metadata"))
                {
                    GenerateMetadata(songProp.objectReferenceValue as AudioClip);
                }

                EditorGUI.EndDisabledGroup();

                // Draw the loop fields
                EditorGUILayout.Space(10);

                DrawOptionalProperty("haveLoop", "loopPoint");
                DrawOptionalProperty("haveLoopStartPoint", "loopStartPoint");

                // Draw the song events
                EditorGUILayout.Space();

                DrawOptionalProperty("haveSongEvents", "songEvents", 32);

                EditorGUILayout.Space();
            });

            EditorGUILayout.Space();

            #endregion

            #region Song List
            // Add the toggle to add or remove this song from the list
            SerializedProperty inListProp = serializedObject.FindProperty("inSongList");

            inListProp.isExpanded = Foldout(inListProp.isExpanded, "Song List", () =>
            {
                serializedObject.ApplyModifiedProperties();

                inListProp.boolValue = EditorGUILayout.ToggleLeft("In Song List", inListProp.boolValue);

                if (serializedObject.ApplyModifiedProperties())
                {
                    bool inList = _list.Songs.Contains(_wavySong);

                    if (inListProp.boolValue && !inList)
                    {
                        _list.Songs.Add(_wavySong);
                    }
                    else if (!inListProp.boolValue && inList)
                    {
                        _list.Songs.RemoveAll((song) => song == _wavySong);
                    }
                }

                if (!_settings.HideInfoBoxes)
                {
                    EditorGUILayout.HelpBox("The \"In Song List\" toggle will determine if the song is in the WAVY Song List. \nThe array below is the WAVY Song List. Keep in mind that this list is GLOBAL and the same for ALL WAVY Songs.", MessageType.Info);
                }

                EditorGUILayout.Space();

                WAVYSongListEditor.DrawSongArrayProperty("WAVY Song List");

                EditorGUILayout.Space();

                if (GUILayout.Button("Find \"WAVYSongList\" Scriptable Object"))
                {
                    EditorGUIUtility.PingObject(_list);
                }
            });

            // Draw the settings for the WAVYSongEditor Foldout
            EditorGUILayout.Space();
            #endregion

            #region WAVY Music Settings
            SerializedProperty expandedProp = _settings.SerializedObject.FindProperty("editorExpanded");
            expandedProp.boolValue = Foldout(expandedProp.boolValue, "WAVY Music Settings", () =>
            {
                WAVYSettingsEditor.DrawInspector(_settings.SerializedObject);

                EditorGUILayout.Space();

                if (GUILayout.Button("Find \"WAVYSettings\" Scriptable Object"))
                {
                    EditorGUIUtility.PingObject(_settings);
                }
            });

            _settings.SerializedObject.ApplyModifiedPropertiesWithoutUndo();
            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        private bool Foldout(bool expanded, string text, Action action)
        {
            expanded = EditorGUILayout.BeginFoldoutHeaderGroup(expanded, text);
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (expanded)
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUI.indentLevel++;

                action?.Invoke();

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }

            return expanded;
        }

        private void DrawOptionalProperty(string boolPropName, string mainPropName, float offset = 17)
        {
            SerializedProperty boolProp = serializedObject.FindProperty(boolPropName);
            Rect rect = EditorGUILayout.GetControlRect();
            float oldWidth = rect.width;
            rect.width = offset;

            EditorGUI.PropertyField(rect, boolProp, GUIContent.none);
            serializedObject.ApplyModifiedProperties();

            // Draw the loop point float field
            EditorGUI.BeginDisabledGroup(!boolProp.boolValue);

            rect.x += offset;
            rect.width = oldWidth - offset;

            SerializedProperty mainProp = serializedObject.FindProperty(mainPropName);
            EditorGUI.PropertyField(rect, mainProp);

            EditorGUILayout.Space(EditorGUI.GetPropertyHeight(mainProp) - 18);

            /*
            if (mainProp.isArray && mainProp.isExpanded)
            {
                int length = Mathf.Max(mainProp.arraySize + 1, 2);
                for (int i = 0; i < length; i++)
                {
                    EditorGUILayout.Space(20);
                }

                EditorGUILayout.Space(2);
            }
            */

            EditorGUI.EndDisabledGroup();
        }

        // Using reflection because the protection on my own code is too good
        private static FieldInfo _metadataField = typeof(WAVYSong).GetField("metadata", BindingFlags.NonPublic | BindingFlags.Instance);
        private void GenerateMetadata(AudioClip clip, bool displayMessages = true)
        {
            if (clip == null)
            {
                _metadataField.SetValue(_wavySong, null);
                //prop.managedReferenceValue = default(WavMetadata);
                //_wavySong.Metadata = null;

                if (displayMessages)
                {
                    Debug.LogWarning("SongClip is null!");
                }
                return;
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();

            // Get the proper full path to the clip
            string path = Path.Combine(Directory.GetCurrentDirectory(), AssetDatabase.GetAssetPath(clip)).Replace("/", "\\");

            try
            {
                // Set the metadata
                _metadataField.SetValue(_wavySong, WavReader.GetMetadata(path));
                //prop.managedReferenceValue = WavReader.GetMetadata(path);
                //_wavySong.Metadata = WavReader.GetMetadata(path);

                watch.Stop();

                if (displayMessages)
                {
                    Debug.Log($"Success! Time taken: {watch.ElapsedMilliseconds} {(watch.ElapsedMilliseconds == 1 ? "Millisecond" : "Milliseconds")}");
                }
            }
            catch (Exception)
            {
                // Don't throw exception and instead set the metadata to null
                _metadataField.SetValue(_wavySong, null);
                //prop.managedReferenceValue = default(WavMetadata);
                //_wavySong.Metadata = null;

                watch.Stop();

                if (displayMessages)
                {
                    throw;
                }
            }
        }

        private void DrawAudioSamples(AudioClip clip, Rect rect)
        {
            // Can't draw without a clip 
            if (clip == null)
            {
                return;
            }

            // Get the audio clip waveform using the asset previewer
            Texture2D tex = AssetPreview.GetAssetPreview(clip);

            if (tex == null)
            {
                return;
            }

            tex.filterMode = FilterMode.Point;

            // Draw the texture and cutoff the edges
            GUI.BeginClip(rect);

            rect.x = 0;
            rect.y = 0;

            rect.x -= 7;
            rect.width += 14;

            EditorGUI.DrawTextureTransparent(rect, tex);
            GUI.EndClip();
        }

        private const float MARKER_TEXT_OFFSET = 3;
        private const float SMALL_MARKER_TEXT_ZONE = 30;

        private void DrawMarkers(WavMetadata metadata, Rect rect)
        {
            // Can't draw markers without any metadata, cues or if the settings is disabled
            if (metadata == null || metadata.Cues == null || !_settings.ShowMarkers)
            {
                return;
            }

            List<float> xPoses = new List<float>();
            List<string> cueNames = new List<string>();

            // Loop through all the cues and draw them all individually
            foreach (WavCue cue in metadata.Cues)
            {
                // Get the X position of the cue
                float t = Mathf.InverseLerp(0, metadata.SampleCount, cue.Position);
                float xPos = Mathf.Lerp(rect.x, rect.xMax, t);

                // Create the rect
                Rect cueRect = new Rect(xPos, rect.y, 1, rect.height);

                // Draw the rect
                EditorGUI.DrawRect(cueRect, _settings.MarkersColor);

                cueRect.width += 20;
                cueRect.x -= 10;
                if (GUI.Button(cueRect, GUIContent.none, GUIStyle.none))
                {
                    ShowCueContextMenu(cue, metadata);
                }

                // Add the cue X pos to the xPoses list
                xPoses.Add(xPos);
                cueNames.Add(cue.Name);
            }

            for (int i = 0; i < xPoses.Count; i++)
            {
                float xPos = xPoses[i];
                float nextXPos = i + 1 < xPoses.Count ? xPoses[i + 1] : rect.xMax;

                // Create a new rect for the text
                Rect textRect = new Rect(xPos + MARKER_TEXT_OFFSET, rect.y, nextXPos - xPos - MARKER_TEXT_OFFSET, rect.height);

                bool reverse = textRect.width <= SMALL_MARKER_TEXT_ZONE && i - 1 >= 0;

                if (reverse)
                {
                    float prevXPos = xPoses[i - 1];

                    if (xPos - prevXPos <= SMALL_MARKER_TEXT_ZONE)
                    {
                        reverse = false;
                    }
                    else
                    {
                        textRect.width = xPos - prevXPos - MARKER_TEXT_OFFSET;

                        textRect.x -= textRect.width;
                        textRect.x -= MARKER_TEXT_OFFSET * 2;
                    }
                }

                // Draw the text
                EditorGUI.LabelField(textRect, cueNames[i], new GUIStyle()
                {
                    alignment = reverse ? TextAnchor.LowerRight : TextAnchor.UpperLeft,
                    normal = new GUIStyleState() { textColor = Color.white },
                    clipping = TextClipping.Clip,
                });
            }
        }

        /*
        private void DrawBPMLines(AudioClip clip, Rect rect)
        {
            // Can't draw any BPM lines without a clip, BPM or if the settings is disabled
            if (clip == null || _wavySong.BPM <= 0 || !WAVYSongEditorSettings.ShowBPMLines)
            {
                return;
            }

            // The song length in minutes
            float amountOfMinutes = clip.length / 60;

            // The total amount of beats
            float totalBeatAmount = _wavySong.BPM / 60 * amountOfMinutes;

            // 'i' is increased by the total beats amount divided by 60
            for (float i = _wavySong.BPMStartPoint; i < clip.length; i += totalBeatAmount)
            {
                // Get the X position of the BPM marker
                float t = Mathf.InverseLerp(0, clip.length, i);
                float xPos = Mathf.Lerp(rect.x, rect.xMax, t);

                // Create the rect
                Rect bpmMarkerRect = new Rect(xPos, rect.y, 1, rect.height);

                // Draw the rect
                EditorGUI.DrawRect(bpmMarkerRect, WAVYSongEditorSettings.BPMLinesColor);
            }
        }
        */

        private void ShowCueContextMenu(WavCue cue, WavMetadata metadata)
        {
            GenericMenu menu = new GenericMenu();

            float time = Mathf.Lerp(0, _wavySong.SongClip.length, (float)cue.Position / (float)metadata.SampleCount);

            /*
            // Set BPM start point
            menu.AddItem(new GUIContent($"Set \"{cue.Name}\" as BPM start point"), false, () =>
            {
                _wavySong.BPMStartPoint = Mathf.Lerp(0, _wavySong.SongClip.length, t);
            });

            menu.AddSeparator("");
            */

            // Set loop point
            menu.AddItem(new GUIContent($"Set \"{cue.Name}\" as Loop Point"), false, serializedObject.FindProperty("haveLoop").boolValue ? () =>
            {
                // Get the LoopPoint property
                serializedObject.FindProperty("loopPoint").floatValue = time;

                // Apply Modified Properties to allow undo 
                serializedObject.ApplyModifiedProperties();
            }
            : null);

            // Set loop start point
            menu.AddItem(new GUIContent($"Set \"{cue.Name}\" as Loop Start Point"), false, serializedObject.FindProperty("haveLoopStartPoint").boolValue ? () =>
            {
                // Get the LoopStartPoint property
                serializedObject.FindProperty("loopStartPoint").floatValue = time;

                // Apply Modified Properties to allow undo 
                serializedObject.ApplyModifiedProperties();
            }
            : null);

            // Add a seperator to the context menu
            menu.AddSeparator("");

            // Add event
            menu.AddItem(new GUIContent($"Add \"{cue.Name}\" as a new Event Point"), false, serializedObject.FindProperty("haveSongEvents").boolValue  ? () =>
            {
                // Get the SongEvents property
                SerializedProperty prop = serializedObject.FindProperty("songEvents");

                // Add a new event on the property
                int index = prop.arraySize;
                prop.InsertArrayElementAtIndex(index);

                // Get the newly added SongEvent on the SongEvents array
                SerializedProperty newEventProp = prop.GetArrayElementAtIndex(index);

                // Set properties on the SongEvent
                newEventProp.FindPropertyRelative("Name").stringValue = cue.Name;
                newEventProp.FindPropertyRelative("Time").doubleValue = time;

                // Apply Modified Properties to allow undo 
                serializedObject.ApplyModifiedProperties();
            }
            : null);

            menu.ShowAsContext();
        }

        private void OnEnable()
        {
            _wavySong = (WAVYSong)target;

            serializedObject.FindProperty("inSongList").boolValue = _list.Songs.Contains(_wavySong);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            //_wavySong.InSongList = _list.Songs.Contains(_wavySong);
        }
    }
}