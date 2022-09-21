using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using WAVYMusic;

namespace WAVYMusicEditor
{
    /// <summary>
    /// The editor for the <see cref="WAVYSongList"/>.
    /// </summary>
    [CustomEditor(typeof(WAVYSongList))]
    public class WAVYSongListEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawSongArrayProperty();
        }

        public static void DrawSongArrayProperty(string label = "Songs")
        {
            SerializedObject obj = WAVYSongList.Obj.SerializedObject;

            obj.UpdateIfRequiredOrScript();

            SerializedProperty listProp = obj.FindProperty("Songs");

            int oldSize = listProp.arraySize;

            EditorGUILayout.PropertyField(listProp, new GUIContent(label));
            EditorGUILayout.Space();

            if (obj.ApplyModifiedProperties())
            {
                int length = listProp.arraySize;

                // Convert the serialized property into a list
                List<WAVYSong> list = new List<WAVYSong>();

                bool loggedMessage = false;
                bool canSayMessage = oldSize == length;

                for (int i = 0; i < length; i++)
                {
                    SerializedProperty element = listProp.GetArrayElementAtIndex(i);

                    WAVYSong song = element.objectReferenceValue as WAVYSong;

                    if (!list.Contains(song))
                    {
                        list.Add(song);
                    }
                    else
                    {
                        list.Add(null);

                        if (!loggedMessage && canSayMessage && song != null)
                        {
                            loggedMessage = true;

                            Debug.LogWarning("The Song List already contains this song!");
                        }
                    }
                }

                listProp.arraySize = 0;

                for (int i = 0; i < length; i++)
                {
                    listProp.InsertArrayElementAtIndex(i);
                    SerializedProperty element = listProp.GetArrayElementAtIndex(i);

                    element.objectReferenceValue = list[i];
                }

                obj.ApplyModifiedProperties();
            }
        }
    }
}
