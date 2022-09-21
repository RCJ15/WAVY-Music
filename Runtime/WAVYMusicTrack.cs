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
        public AudioSource Source;
        public WAVYSong Song;
        public bool HandleLooping;

        private bool _playing;
        private bool _haveScheduledLoop;
        private double _loopTime;

        public float Volume { get => Source.volume; set => Source.volume = value; }

        private void Start()
        {
            if (Source == null)
            {
                Source = GetComponent<AudioSource>();
            }
        }

        private void Update()
        {
            if (_playing && !Source.isPlaying)
            {
                _playing = false;

                WAVYMusicPlayer.AvailableTracks.Enqueue(this);
            }
            else if (!_playing && Source.isPlaying)
            {
                _playing = true;
            }

            // Return if the song is not playing, if we have already scheduled a loop or if this track shouldn't handle looping
            if (!_playing || _haveScheduledLoop || !HandleLooping)
            {
                return;
            }

            // Schedule the loop a bit beforehand in order to give the audio system time to process for the PERFECT SEAMLESS SMOOTH loop
            if (Song.HaveLoop && Source.time + 1 > Song.LoopPoint)
            {
                // Set this to true so we don't accidentely schedule 1,749,814,789 loops
                _haveScheduledLoop = true;

                // Play this song scheduled at the loop time
                WAVYMusicPlayer.PlaySongScheduled(Song, _loopTime);
            }
        }

        /// <summary>
        /// Sets the audio clip and plays it instantly.
        /// </summary>
        public void Play(AudioClip clip)
        {
            Source.clip = clip;

            Source.Play();

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

            // Setup the loop with offset seeing as this play was scheduled
            // If we don't offset, then the song will play 2 at the same time for a short while which sounds VERY BAD
            SetupLoop(time - AudioSettings.dspTime);
        }

        /// <summary>
        /// Setups the loop time if this track handles looping and the song should have looping.
        /// </summary>
        private void SetupLoop(double offset = 0)
        {
            if (HandleLooping && Song.HaveLoop)
            {
                _loopTime = AudioSettings.dspTime + Song.LoopPoint + offset;
                _haveScheduledLoop = false;
            }
        }

        /// <summary>
        /// Fades this track out for <paramref name="fadeDuration"/> seconds and will make this song stop playing completely after the fade.
        /// </summary>
        public void StopSong(float fadeDuration)
        {

        }
    }
}
