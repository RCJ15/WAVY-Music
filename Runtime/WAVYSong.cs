using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WAVYMusic
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> that contains data about a single song stored in the <see cref="WAVYSongList"/>.
    /// </summary>
    public class WAVYSong : ScriptableObject
    {
        public string DisplayName = "New WAVY Song";

        /*
        public float BPM;
        public float BPMStartPoint = 0;
        */

        public AudioClip SongClip;

        public bool HaveTracks;
        public AudioClip[] Tracks;

        public WavMetadata Metadata;
        public bool HaveLoop = true;
        public double LoopPoint;

        public bool HaveLoopStartPoint = false;
        public float LoopStartPoint;

        public bool InSongList = true;

        public bool HaveSongEvents = false;
        public List<Event> SongEvents;

        /// <summary>
        /// How many <see cref="WAVYMusicTrack"/> this <see cref="WAVYSong"/> has.
        /// </summary>
        public int TrackCount => 1 + (HaveTracks ? Tracks.Length : 0);

        /// <summary>
        /// An event on a single <see cref="WAVYSong"/>. Using this you can hook up to different events that happen on specific song positions.
        /// </summary>
        [Serializable] 
        public class Event
        {
            public string EventName;
            public float Time;

            public Event(string name, float time)
            {
                EventName = name;
                Time = time;
            }
        }
    }
}
