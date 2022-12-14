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
        [SerializeField] private string displayName = "New WAVY Song";
        public string DisplayName => displayName;

        /*
        public float BPM;
        public float BPMStartPoint = 0;
        */

        [SerializeField] private AudioClip songClip;
        public AudioClip SongClip => songClip;

        [SerializeField] private bool haveTracks;
        public bool HaveTracks => haveTracks;
        [SerializeField] private AudioClip[] tracks;
        public AudioClip[] Tracks => tracks;

        [SerializeField] private WavMetadata metadata;
        public WavMetadata Metadata => metadata;
        [SerializeField] private bool haveLoop = true;
        public bool HaveLoop => haveLoop;
        [SerializeField] private double loopPoint;
        public double LoopPoint => loopPoint;

        [SerializeField] private bool haveLoopStartPoint = false;
        public bool HaveLoopStartPoint => haveLoopStartPoint;
        [SerializeField] private float loopStartPoint;
        public float LoopStartPoint => loopStartPoint;

        [SerializeField] private bool inSongList = true;
        public bool InSongList => inSongList;

        [SerializeField] private bool haveSongEvents = false;
        public bool HaveSongEvents => haveSongEvents;
        [SerializeField] private List<Event> songEvents;
        public List<Event> SongEvents => songEvents;

        public delegate void WAVYSongEventDelegate(string eventName);
        [SerializeField] private WAVYSongEventDelegate onEventTrigger;
        public WAVYSongEventDelegate OnEventTrigger => onEventTrigger;

        /// <summary>
        /// How many <see cref="WAVYMusicTrack"/> this <see cref="WAVYSong"/> has.
        /// </summary>
        public int TrackCount => 1 + (HaveTracks ? Tracks.Length : 0);

        /// <summary>
        /// Playback position in seconds.
        /// </summary>
        public float Time
        {
            get => WAVYMusicPlayer.GetSongTime(this);
            set => WAVYMusicPlayer.SetSongTime(this, value);
        }

        /// <summary>
        /// An event on a single <see cref="WAVYSong"/>. Using this you can hook up to different events that happen on specific song positions.
        /// </summary>
        [Serializable] 
        public class Event
        {
            public string Name;
            public double Time;

            public Event(string name, double time)
            {
                Name = name;
                Time = time;
            }
        }

        #region Methods
        /// <summary>
        /// Plays this song. <para/>
        /// NOTE: Only the first track on the song will be played. Use the <paramref name="enabledTracks"/> to enable which tracks should play.
        /// </summary>
        public void Play(params int[] enabledTracks)
        {
            WAVYMusicPlayer.PlaySong(this, enabledTracks);
        }

        /// <summary>
        /// Plays this song at the scheduled time. <para/>
        /// NOTE: Only the first track on the song will be played. Use the <paramref name="enabledTracks"/> to enable which tracks should play.
        /// </summary>
        public void PlayScheduled(double scheduledTime, params int[] enabledTracks)
        {
            WAVYMusicPlayer.PlaySongScheduled(this, scheduledTime, enabledTracks);
        }

        /// <summary>
        /// Fades out this song for <paramref name="fadeDuration"/> seconds. Set <paramref name="fadeDuration"/> to 0 or below for an instant cut.
        /// </summary>
        public void Stop(float fadeDuration = 0)
        {
            WAVYMusicPlayer.StopSong(this, fadeDuration);
        }

        /// <summary>
        /// Stops this song from being stopped if it's currently being stopped with <see cref="Stop(float)"/>.
        /// </summary>
        public void InterruptStopping()
        {
            WAVYMusicPlayer.InterruptSongStopping(this);
        }

        /// <summary>
        /// Fades in all the tracks in the <paramref name="enabledTracks"/> array and fades out all tracks outside not in the array.
        /// </summary>
        public void SetTracks(float fadeDuration, params int[] enabledTracks)
        {
            WAVYMusicPlayer.SetSongTracks(this, fadeDuration, enabledTracks);
        }

        /// <summary>
        /// Instantly sets the volume of all the given <paramref name="tracks"/> to <paramref name="volume"/> multiplied by <see cref="VolumeScale"/>.
        /// </summary>
        public void SetTrackVolume(float volume, params int[] tracks)
        {
            WAVYMusicPlayer.SetTrackVolume(this, volume, tracks);
        }

        /// <summary>
        /// Fades from the current volume to the <paramref name="targetVolume"/> over the time of <paramref name="duration"/> seconds. <para/>
        /// Performs <paramref name="onFinish"/> when the track is finished fading out.
        /// </summary>
        public void FadeTrack(int track, float duration, float targetVolume, Action onFinish = null)
        {
            WAVYMusicPlayer.FadeTrack(this, track, duration, targetVolume, onFinish);
        }
        #endregion
    }
}
