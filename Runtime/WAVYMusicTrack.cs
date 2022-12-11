using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WAVYMusic
{
    /// <summary>
    /// The object that plays a single track in a <see cref="WAVYSong"/>.
    /// </summary>
    public class WAVYMusicTrack : MonoBehaviour
    {
        private float _loopScheduleOffset = WAVYSettings.Obj.LoopScheduleOffset;

        public AudioSource Source;
        public WAVYSong Song;
        public bool IsMasterTrack;

        private bool _haveEvents;
        private List<WAVYSong.Event> _events;
        private int _eventCount;
        private int _currentEvent;

        private bool _playing;
        private bool _haveScheduledLoop;
        private double _loopTime;

        /// <summary>
        /// The current volume of this <see cref="WAVYMusicTrack"/>.
        /// </summary>
        public float Volume { get => Source.volume; set => Source.volume = value; }

        /// <summary>
        /// Playback position in seconds.
        /// </summary>
        public float Time
        {
            get => Source.time;
            set
            {
                Source.Stop();
                Source.time = value;
                Source.Play();

                // Re-setup the loop
                SetupLoop();
            }
        }

        private void Start()
        {
            // Get component if it's null
            if (Source == null)
            {
                Source = GetComponent<AudioSource>();
            }
        }

        private void Update()
        {
            // Add this track automatically back to the Available Tracks queue if it has stopped playing
            if (_playing && !Source.isPlaying)
            {
                _playing = false;

                WAVYMusicPlayer.AvailableTracks.Enqueue(this);
            }
            else if (!_playing && Source.isPlaying)
            {
                _playing = true;
            }

            // Return if this track isn't the master track or if the song is not playing
            if (!IsMasterTrack || !_playing)
            {
                return;
            }

            // Song events is in a seperate method so that guard clauses will work without the rest being ruined
            HandleSongEvents();

            // Return if we have already scheduled a loop
            if (_haveScheduledLoop)
            {
                return;
            }

            // Schedule the loop a bit beforehand in order to give the audio system time to process for the PERFECT SEAMLESS SMOOTH loop
            if (Song.HaveLoop && Time + _loopScheduleOffset > Song.LoopPoint)
            {
                // Set this to true so we don't accidentely schedule 1,749,814,789 loops
                _haveScheduledLoop = true;

                // Play this song scheduled at the loop time
                WAVYMusicPlayer.PlaySongScheduled(Song, _loopTime);
            }
        }

        private void HandleSongEvents()
        {
            if (!_haveEvents)
            {
                return;
            }

            if (_currentEvent >= _eventCount)
            {
                return;
            }

            WAVYSong.Event evt = _events[_currentEvent];

            if (Time < evt.Time)
            {
                return;
            }

            // Event was triggered
            Song.OnEventTrigger?.Invoke(evt.Name);

            _currentEvent++;
        }

        /// <summary>
        /// Sets the audio clip and plays it instantly.
        /// </summary>
        public void Play(AudioClip clip)
        {
            Source.clip = clip;

            Source.Play();

            SetupEvents();

            SetupLoop();
        }

        /// <summary>
        /// Sets the audio clip and schedules a play.
        /// </summary>
        public void PlayScheduled(AudioClip clip, double time)
        {
            Source.clip = clip;

            if (Song.HaveLoopStartPoint)
            {
                Source.time = Song.LoopStartPoint;
            }

            Source.PlayScheduled(time);

            SetupEvents();

            // Setup the loop with offset seeing as this play was scheduled
            // If we don't offset, then the song will play 2 at the same time for a short while which sounds VERY BAD
            SetupLoop(time - AudioSettings.dspTime);
        }

        private void SetupEvents()
        {
            if (!IsMasterTrack)
            {
                return;
            }

            _haveEvents = Song.HaveSongEvents;

            if (_haveEvents)
            {
                _events = WAVYSongEventSorter.Get(Song);

                // No events were gotten :(
                if (_events == null)
                {
                    _haveEvents = false;
                }
                else
                {
                    // Cache event count
                    _eventCount = _events.Count;

                    _currentEvent = 0;
                }
            }
            else
            {
                _events = null;
            }
        }

        /// <summary>
        /// Setups the loop time if this track is the master track and if the song should have looping.
        /// </summary>
        private void SetupLoop(double offset = 0)
        {
            if (!IsMasterTrack || !Song.HaveLoop)
            {
                return;
            }

            _loopTime = AudioSettings.dspTime + Song.LoopPoint + offset - Time;
            _haveScheduledLoop = false;
        }

        /// <summary>
        /// Stops this track instantly and makes it stop playing.
        /// </summary>
        public void Stop()
        {
            Source.Stop();
        }
    }
}
