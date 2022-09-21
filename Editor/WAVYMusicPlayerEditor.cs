using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using WAVYMusic;

namespace WAVYMusicEditor
{
    /// <summary>
    /// The <see cref="WAVYMusicPlayer"/> editor in the inspector
    /// </summary>
    [CustomEditor(typeof(WAVYMusicPlayer))]
    public class WAVYMusicPlayerEditor : Editor
    {
        private WAVYMusicPlayer _musicPlayer;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        private void OnEnable()
        {
            _musicPlayer = (WAVYMusicPlayer)target;
        }
    }
}