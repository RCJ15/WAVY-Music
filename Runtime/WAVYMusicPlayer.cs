using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WAVYMusic
{
    /// <summary>
    /// The primary script that plays a <see cref="WAVYSong"/> and keeps track of which music tracks are available/in use. <para/>
    /// You'll use this script the most when trying to use WAVY Music.
    /// </summary>
    public class WAVYMusicPlayer : MonoBehaviour
    {
        /// <summary>
        /// The static singleton instance of this object.
        /// </summary>
        public static WAVYMusicPlayer Instance { get; private set; }

        private static float _volumeScale = 1;
        /// <summary>
        /// The amount all volume is scaled by.
        /// </summary>
        public static float VolumeScale
        {
            get => _volumeScale;
            set
            {
                foreach (var pair in _songTracksVolume)
                {
                    int length = pair.Value.Length;

                    for (int i = 0; i < length; i++)
                    {
                        pair.Value[i] /= _volumeScale;
                        pair.Value[i] *= value;
                    }
                }

                _volumeScale = value;
            }
        }

        private readonly static List<WAVYMusicTrack> _tracks = new List<WAVYMusicTrack>();

        /// <summary>
        /// A list of all the <see cref="WAVYMusicTrack"/> which are available for use currently.
        /// </summary>
        public readonly static Queue<WAVYMusicTrack> AvailableTracks = new Queue<WAVYMusicTrack>();

        private readonly static HashSet<WAVYSong> _songsBeingStopped = new HashSet<WAVYSong>();
        private readonly static Dictionary<WAVYSong, WAVYMusicTrack[]> _songTracks = new Dictionary<WAVYSong, WAVYMusicTrack[]>();
        private readonly static Dictionary<WAVYSong, Coroutine[]> _songCoroutines = new Dictionary<WAVYSong, Coroutine[]>();
        private readonly static Dictionary<WAVYSong, float[]> _songTracksVolume = new Dictionary<WAVYSong, float[]>();
        private readonly static Dictionary<WAVYSong, int[]> _enabledTracks = new Dictionary<WAVYSong, int[]>();

        private readonly static Dictionary<WAVYSong, float> _pendingSongTime = new Dictionary<WAVYSong, float>();

        [RuntimeInitializeOnLoadMethod]
        private static void CreateMusicPlayer()
        {
            GameObject newObj = new GameObject("WAVY Music Player");

            DontDestroyOnLoad(newObj);

            newObj.AddComponent<WAVYMusicPlayer>();
        }

        private void Awake()
        {
            // Destroy self if an instance already exists
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            // Set this to the new singleton instance
            Instance = this;

            // Tag this object as don't destroy on load so we can have music playing between scenes
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Creates a new <see cref="WAVYMusicTrack"/> and returns it.
        /// </summary>
        private WAVYMusicTrack CreateTrack()
        {
            // Create a new game object which will be our new WAVY Music Track (Make sure it's named correctly for debug purposes)
            GameObject newObj = new GameObject($"WAVY Music Track ({_tracks.Count})");

            // Make sure the track is a child of this object so it's also affacted by the DontDestroyOnLoad
            newObj.transform.SetParent(transform);

            // Add a audio source to the object
            AudioSource source = newObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;

            // Set the mixer group of the track
            source.outputAudioMixerGroup = WAVYSettings.Obj.MixerGroup;

            // Add the actual WAVY Music Track component to the object
            WAVYMusicTrack track = newObj.AddComponent<WAVYMusicTrack>();
            track.Source = source;

            // Add the track to the list
            _tracks.Add(track);

            // Return the track
            return track;
        }

        /// <summary>
        /// This simply calls <see cref="WAVYSongList"/>.GetSong() and returns the value. <para/>
        /// For more info, see: <see cref="WAVYSongList.GetSong(string)"/>.
        /// </summary>
        public static WAVYSong GetSong(string songName)
        {
            return WAVYSongList.GetSong(songName);
        }

        #region Play Song
        /// <summary>
        /// Will use the <see cref="WAVYSongList.SongDictionary"/> and call <see cref="PlaySong(WAVYSong)"/> using the result from the dictionary. <para/>
        /// NOTE: No tracks on the song will be played. Use the <paramref name="enabledTracks"/> to enable which tracks should play.
        /// </summary>
        public static void PlaySong(string songName, params int[] enabledTracks)
        {
            // Play the song
            PlaySong(GetSong(songName), enabledTracks);
        }

        /// <summary>
        /// Plays the given song. <para/>
        /// NOTE: Only the first track on the song will be played. Use the <paramref name="enabledTracks"/> to enable which tracks should play.
        /// </summary>
        public static void PlaySong(WAVYSong song, params int[] enabledTracks)
        {
            // Interrupt the stopping of this song if it's being stopped currently
            if (_songsBeingStopped.Contains(song))
            {
                InterruptSongStopping(song);
            }

            Instance.PlaySongLocal(song, null, enabledTracks);
        }

        /// <summary>
        /// Plays the given song at the scheduled time. <para/>
        /// NOTE: Only the first track on the song will be played. Use the <paramref name="enabledTracks"/> to enable which tracks should play.
        /// </summary>
        public static void PlaySongScheduled(string songName, double scheduledTime, params int[] enabledTracks)
        {
            PlaySongScheduled(GetSong(songName), scheduledTime, enabledTracks);
        }

        /// <summary>
        /// Plays the given <paramref name="song"/> at the scheduled time. <para/>
        /// NOTE: Only the first track on the song will be played. Use the <paramref name="enabledTracks"/> to enable which tracks should play.
        /// </summary>
        public static void PlaySongScheduled(WAVYSong song, double scheduledTime, params int[] enabledTracks)
        {
            Instance.PlaySongLocal(song, scheduledTime, enabledTracks);
        }

        /// <summary>
        /// Plays the given <paramref name="song"/> at the scheduled time. <para/>
        /// NOTE: Only the first track on the song will be played. Use the <paramref name="enabledTracks"/> to enable which tracks should play.
        /// </summary>
        private void PlaySongLocal(WAVYSong song, double? scheduledTime = null, params int[] enabledTracks)
        {
            if (enabledTracks.Length <= 0)
            {
                if (_enabledTracks.ContainsKey(song))
                {
                    enabledTracks = _enabledTracks[song];
                }
                else
                {
                    enabledTracks = new int[] { 0 };

                    _enabledTracks[song] = enabledTracks;
                }
            }

            // Store the track count
            int trackCount = song.TrackCount;

            WAVYMusicTrack[] tracks;

            // Check if the song is already playing
            if (IsSongPlaying(song))
            {
                tracks = _songTracks[song];
            }
            else
            {
                // If not, then we will create a new array of tracks
                // Create an array of the tracks which we will use to play our song
                tracks = new WAVYMusicTrack[trackCount];

                // Populate the array
                for (int i = 0; i < trackCount; i++)
                {
                    // Check if the queue is empty or not
                    if (AvailableTracks.Count > 0)
                    {
                        // Get the next available track from the queue
                        tracks[i] = AvailableTracks.Dequeue();
                    }
                    else
                    {
                        // Create a completey new track as there are no available tracks in the queue
                        tracks[i] = CreateTrack();
                    }
                }

                // Add tracks to dictionary
                _songTracks[song] = tracks;
            }

            // Add coroutine array to the song coroutines dictionary
            if (!_songCoroutines.ContainsKey(song))
            {
                _songCoroutines[song] = new Coroutine[trackCount];
            }

            _songTracksVolume[song] = new float[trackCount];

            // Set the enabled tracks
            foreach (int track in enabledTracks)
            {
                _songTracksVolume[song][track] = 1;
            }

            // Try get time
            if (_pendingSongTime.TryGetValue(song, out float time))
            {
                // If succeed, then remove the song from the dictionary
                _pendingSongTime.Remove(song);
            }

            // Loop through all of our chosen tracks
            int length = trackCount;

            for (int i = 0; i < length; i++)
            {
                WAVYMusicTrack track = tracks[i];

                bool mainTrack = i == 0;

                track.IsMasterTrack = mainTrack;

                track.Song = song;

                AudioClip clip = mainTrack ? song.SongClip : song.Tracks[i - 1];

                // Set time
                track.Time = time;

                // Set volume
                track.Volume = _songTracksVolume[song][i];

                if (scheduledTime.HasValue)
                {
                    track.PlayScheduled(clip, scheduledTime.Value);
                }
                else
                {
                    track.Play(clip);
                }
            }
        }

        #endregion

        #region Stop Song
        /// <summary>
        /// Fades out the <paramref name="song"/> for <paramref name="fadeDuration"/> seconds. Set <paramref name="fadeDuration"/> to 0 or below for an instant cut.
        /// </summary>
        public static void StopSong(string songName, float fadeDuration = 0)
        {
            StopSong(GetSong(songName), fadeDuration);
        }

        /// <summary>
        /// Fades out the <paramref name="song"/> for <paramref name="fadeDuration"/> seconds. Set <paramref name="fadeDuration"/> to 0 or below for an instant cut.
        /// </summary>
        public static void StopSong(WAVYSong song, float fadeDuration = 0)
        {
            Instance.StopSongLocal(song, fadeDuration);
        }

        /// <summary>
        /// Fades out the <paramref name="song"/> for <paramref name="fadeDuration"/> seconds. Set <paramref name="fadeDuration"/> to 0 or below for an instant cut.
        /// </summary>
        private void StopSongLocal(WAVYSong song, float fadeDuration = 0)
        {
            // Return and say a warning if the song is already playing
            if (!IsSongPlaying(song))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"No song called \"{song.name}\" is currently playing!");
#endif
                return;
            }

            InterruptSongStoppingLocal(song);

            // Get all the tracks for the song
            WAVYMusicTrack[] tracks = _songTracks[song];

            Action onFinish = () =>
            {
                // Remove this song from everything to make it stop playing
                _songsBeingStopped.Remove(song);

                _songTracks.Remove(song);

                foreach (WAVYMusicTrack track in tracks)
                {
                    track.Stop();
                }
            };

            // Make all tracks fade to 0 for the specified duration
            int length = tracks.Length;

            for (int i = 0; i < length; i++)
            {
                FadeTrackLocal(song, i, fadeDuration, 0, i == 0 ? onFinish : null);
            }

            // Add this song as a song that's being currently stopped
            // NOTE: We do this AFTER making all of the tracks fade out
            // This is because FadeTrackLocal is coded to NOT WORK when a song is inside of the _songsBeingStopped HashSet
            _songsBeingStopped.Add(song);
        }

        /// <summary>
        /// Stop a song from being stopped if it's currently being stopped with <see cref="StopSong(string, float)"/>.
        /// </summary>
        public static void InterruptSongStopping(string songName)
        {
            InterruptSongStopping(GetSong(songName));
        }

        /// <summary>
        /// Stop a song from being stopped if it's currently being stopped with <see cref="StopSong(WAVYSong, float)"/>.
        /// </summary>
        public static void InterruptSongStopping(WAVYSong song)
        {
            Instance.InterruptSongStoppingLocal(song);
        }

        /// <summary>
        /// Stop a song from being stopped if it's currently being stopped with <see cref="StopSong(WAVYSong, float)"/>.
        /// </summary>
        public void InterruptSongStoppingLocal(WAVYSong song)
        {
            if (_songsBeingStopped.Contains(song))
            {
                _songsBeingStopped.Remove(song);
            }

            if (!_songCoroutines.ContainsKey(song))
            {
                return;
            }

            for (int i = 0; i < song.TrackCount; i++)
            {
                CancelFadeCoroutine(song, i, false);
            }
        }
        #endregion

        #region Set Song Tracks
        /// <summary>
        /// Fades in all the tracks in the <paramref name="enabledTracks"/> array and fades out all tracks outside not in the array.
        /// </summary>
        public static void SetSongTracks(string songName, float fadeDuration, params int[] enabledTracks)
        {
            SetSongTracks(GetSong(songName), fadeDuration, enabledTracks);
        }

        /// <summary>
        /// Fades in all the tracks in the <paramref name="enabledTracks"/> array and fades out all tracks outside not in the array.
        /// </summary>
        public static void SetSongTracks(WAVYSong song, float fadeDuration, params int[] enabledTracks)
        {
            _enabledTracks[song] = enabledTracks;

            // Create HashSet which we will use to check which tracks to disable
            HashSet<int> trackCheck = new HashSet<int>();

            // Loop through all the enabled trakcs
            foreach (int track in enabledTracks)
            {
                // Continue to the next track if we have aready enabled this track
                if (trackCheck.Contains(track))
                {
                    continue;
                }

                // Fade in the track
                FadeTrack(song, track, fadeDuration, 1);

                // Add the track to the HashSet
                trackCheck.Add(track);
            }

            // Loop through all tracks
            int length = _songTracks[song].Length;
            for (int i = 0; i < length; i++)
            {
                // Continue to the next track if the track is enabled
                if (trackCheck.Contains(i))
                {
                    continue;
                }

                // Fade out the track as it's not supposed to be enabled
                FadeTrack(song, i, fadeDuration, 0);
            }
        }

        #endregion

        #region Set Track Volume
        /// <summary>
        /// Instantly sets the volume of all the given <paramref name="tracks"/> to <paramref name="volume"/> multiplied by <see cref="VolumeScale"/>.
        /// </summary>
        public static void SetTrackVolume(string songName, float volume, params int[] tracks)
        {
            SetTrackVolume(GetSong(songName), volume, tracks);
        }

        /// <summary>
        /// Instantly sets the volume of all the given <paramref name="tracks"/> to <paramref name="volume"/> multiplied by <see cref="VolumeScale"/>.
        /// </summary>
        public static void SetTrackVolume(WAVYSong song, float volume, params int[] tracks)
        {
            Instance.SetTrackVolumeLocal(song, volume, tracks);
        }

        /// <summary>
        /// Instantly sets the volume of all the given <paramref name="tracks"/> to <paramref name="volume"/> multiplied by <see cref="VolumeScale"/>.
        /// </summary>
        private void SetTrackVolumeLocal(WAVYSong song, float volume, params int[] tracks)
        {
            if (!_songTracksVolume.ContainsKey(song))
            {
                return;
            }

            foreach (int track in tracks)
            {
                SetTrackVolumeLocal(song, volume, track);
            }
        }

        /// <summary>
        /// Instantly sets the volume of the track with the index <paramref name="track"/> to <paramref name="volume"/> multiplied by <see cref="VolumeScale"/>.
        /// </summary>
        public static void SetTrackVolume(string songName, float volume, int track)
        {
            SetTrackVolume(GetSong(songName), volume, track);
        }

        /// <summary>
        /// Instantly sets the volume of the track with the index <paramref name="track"/> to <paramref name="volume"/> multiplied by <see cref="VolumeScale"/>.
        /// </summary>
        public static void SetTrackVolume(WAVYSong song, float volume, int track)
        {
            Instance.SetTrackVolumeLocal(song, volume, track);
        }

        /// <summary>
        /// Instantly sets the volume of the track with the index <paramref name="track"/> to <paramref name="volume"/> multiplied by <see cref="VolumeScale"/>.
        /// </summary>
        private void SetTrackVolumeLocal(WAVYSong song, float volume, int track)
        {
            if (!_songTracksVolume.ContainsKey(song))
            {
                return;
            }

            CancelFadeCoroutine(song, track);

            _songTracksVolume[song][track] = volume * VolumeScale;
        }
        #endregion

        #region Fade Track
        /// <summary>
        /// Fades from the current volume to the <paramref name="targetVolume"/> over the time of <paramref name="duration"/> seconds. <para/>
        /// Performs <paramref name="onFinish"/> when the track is finished fading out.
        /// </summary>
        public static void FadeTrack(string songName, int track, float duration, float targetVolume, Action onFinish = null)
        {
            FadeTrack(GetSong(songName), track, duration, targetVolume, onFinish);
        }

        /// <summary>
        /// Fades from the current volume to the <paramref name="targetVolume"/> over the time of <paramref name="duration"/> seconds. <para/>
        /// Performs <paramref name="onFinish"/> when the track is finished fading out.
        /// </summary>
        public static void FadeTrack(WAVYSong song, int track, float duration, float targetVolume, Action onFinish = null)
        {
            Instance.FadeTrackLocal(song, track, duration, targetVolume, onFinish);
        }

        /// <summary>
        /// Fades from the current volume to the <paramref name="targetVolume"/> over the time of <paramref name="duration"/> seconds. <para/>
        /// Performs <paramref name="onFinish"/> when the track is finished fading out.
        /// </summary>
        private void FadeTrackLocal(WAVYSong song, int track, float duration, float targetVolume, Action onFinish = null)
        {
            if (!_songCoroutines.ContainsKey(song))
            {
                return;
            }

            if (_songsBeingStopped.Contains(song))
            {
                return;
            }

            CancelFadeCoroutine(song, track, false);

            // Account for instant fade out (as in cutting it off instantly)
            if (duration <= 0)
            {
                _songCoroutines[song][track] = null;

                _songTracksVolume[song][track] = 0;

                UpdateTrackVolume(song, track);

                onFinish?.Invoke();
            }
            else
            {
                _songCoroutines[song][track] = StartCoroutine(StartFade(song, track, duration, targetVolume, onFinish));
            }
        }

        private void UpdateTrackVolume(WAVYSong song, int track)
        {
            // Return if the song isn't playing currently
            if (!IsSongPlaying(song))
            {
                return;
            }

            // Set the track volume
            _songTracks[song][track].Volume = _songTracksVolume[song][track];
        }

        private void CancelFadeCoroutine(WAVYSong song, int track, bool doCheck = true)
        {
            if (!doCheck && !_songCoroutines.ContainsKey(song))
            {
                return;
            }

            Coroutine coroutine = _songCoroutines[song][track];

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        /// <summary>
        /// Fades from the current volume to the <paramref name="targetVolume"/> over the time of <paramref name="duration"/> seconds. <para/>
        /// Performs <paramref name="onFinish"/> when the track is finished fading out.
        /// </summary>
        private IEnumerator StartFade(WAVYSong song, int track, float duration, float targetVolume, Action onFinish = null)
        {
            void Update() => UpdateTrackVolume(song, track);
            void SetVolume(float vol) => _songTracksVolume[song][track] = vol * VolumeScale;

            // Set the time and current volume
            float currentTime = 0;
            float start = _songTracksVolume[song][track];
            Update();

            // Loop whilst the time is under the duration
            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;

                SetVolume(Mathf.Lerp(start, targetVolume, currentTime / duration));
                Update();

                yield return null;
            }

            SetVolume(targetVolume);
            Update();

            onFinish?.Invoke();

            yield break;
        }
        #endregion

        #region Song Data
        //-- Is Song Playing
        /// <summary>
        /// Returns whether the song is playing or not.
        /// </summary>
        public static bool IsSongPlaying(string songName)
        {
            return IsSongPlaying(GetSong(songName));
        }

        /// <summary>
        /// Returns whether the <paramref name="song"/> is playing or not.
        /// </summary>
        public static bool IsSongPlaying(WAVYSong song)
        {
            return _songTracks.ContainsKey(song);
        }

        //-- Get Song Time
        /// <summary>
        /// Returns the playback position of the song master track in seconds. <para/>
        /// NOTE: Will return the Pending Song Time instead if the song ISN'T PLAYING.
        /// </summary>
        public static float GetSongTime(string songName)
        {
            return GetSongTime(songName);
        }

        /// <summary>
        /// Returns the playback position of the <paramref name="song"/> master track in seconds. <para/>
        /// NOTE: Will return the Pending Song Time instead if the <paramref name="song"/> ISN'T PLAYING.
        /// </summary>
        public static float GetSongTime(WAVYSong song)
        {
            // Return 0 by default if the song isn't playing
            if (!IsSongPlaying(song))
            {
                _pendingSongTime.TryGetValue(song, out float time);

                return time;
            }

            // 0 is always the Master Track.
            return _songTracks[song][0].Time;
        }

        //-- Set Song Time
        /// <summary>
        /// Sets the song time for the song to <paramref name="time"/>. Values below 0 will be ignored. <para/>
        /// NOTE: Will set the Pending Song Time if the song ISN'T PLAYING. The Pending Song Time will be used when the song is going to be played. <para/>
        /// EXAMPLE: If you set the song time to 2 and the song ISN'T PLAYING, then when you call <see cref="PlaySong(string, int[])"/> the song time will be instantly set to 2. Use <see cref="GetSongTime(string)"/> to get the Pending Song Time if the song ISN'T PLAYING.
        /// </summary>
        public static void SetSongTime(string songName, float time)
        {
            SetSongTime(GetSong(songName), time);
        }

        /// <summary>
        /// Sets the song time for the <paramref name="song"/> to <paramref name="time"/>. Values below 0 will be ignored. <para/>
        /// NOTE: Will set the Pending Song Time if the <paramref name="song"/> ISN'T PLAYING. The Pending Song Time will be used when the <paramref name="song"/> is going to be played. <para/>
        /// EXAMPLE: If you set the song time to 2 and the <paramref name="song"/> ISN'T PLAYING, then when you call <see cref="PlaySong(string, int[])"/> the song time will be instantly set to 2. Use <see cref="GetSongTime(string)"/> to get the Pending Song Time if the <paramref name="song"/> ISN'T PLAYING.
        /// </summary>
        public static void SetSongTime(WAVYSong song, float time)
        {
            // Ignore values below 0
            if (time < 0)
            {
                return;
            }

            // Return and set the pending song time if the song isn't playing currently
            if (!IsSongPlaying(song))
            {
                _pendingSongTime[song] = time;
                return;
            }

            // Set the time for all music tracks
            foreach (WAVYMusicTrack track in _songTracks[song])
            {
                track.Time = time;
            }
        }

        //-- Remove Pending Song Time
        /// <summary>
        /// Will remove the Pending Song Time on the song if there is a Pending Song Time set for it. This is honestly kind of useless... <para/>
        /// TIP: Use this before calling <see cref="PlaySong(string, int[])"/> to ensure the song always begins at the start without using <see cref="SetSongTime(string, float)"/>.
        /// </summary>
        /// <returns>True if the Pending Song Time was successfully removed, false if there was no Pending Song Time set in the first place.</returns>
        public static bool RemovePendingSongTime(string songName)
        {
            return RemovePendingSongTime(GetSong(songName));
        }

        /// <summary>
        /// Will remove the Pending Song Time on the <paramref name="song"/> if there is a Pending Song Time set for it. This is honestly kind of useless... <para/>
        /// TIP: Use this before calling <see cref="PlaySong(WAVYSong, int[])"/> to ensure the song always begins at the start without using <see cref="SetSongTime(WAVYSong, float)"/>.
        /// </summary>
        /// <returns>True if the Pending Song Time was successfully removed, false if there was no Pending Song Time set in the first place.</returns>
        public static bool RemovePendingSongTime(WAVYSong song)
        {
            if (!_pendingSongTime.ContainsKey(song))
            {
                return false;
            }

            _pendingSongTime.Remove(song);

            return true;
        }
        #endregion
    }
}