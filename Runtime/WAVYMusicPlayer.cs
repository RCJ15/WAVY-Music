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

        private static List<WAVYMusicTrack> _tracks = new List<WAVYMusicTrack>();

        /// <summary>
        /// A list of all the <see cref="WAVYMusicTrack"/> which are available for use currently.
        /// </summary>
        public static Queue<WAVYMusicTrack> AvailableTracks = new Queue<WAVYMusicTrack>();

        private static Dictionary<WAVYSong, WAVYMusicTrack[]> _songTracks = new Dictionary<WAVYSong, WAVYMusicTrack[]>();
        private static Dictionary<WAVYSong, Coroutine[]> _songCoroutines = new Dictionary<WAVYSong, Coroutine[]>();
        private static Dictionary<WAVYSong, float[]> _songTracksVolume = new Dictionary<WAVYSong, float[]>();
        private static Dictionary<WAVYSong, int[]> _enabledTracks = new Dictionary<WAVYSong, int[]>();

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
        /// NOTE: No tracks on the song will be played. Use the <paramref name="enabledTracks"/> to enable which tracks should play.
        /// </summary>
        public static void PlaySong(WAVYSong song, params int[] enabledTracks)
        {
            if (_songTracks.ContainsKey(song))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Song \"{song.name}\" is already playing!");
#endif
                return;
            }

            Instance.PlaySongLocal(song, null, enabledTracks);
        }

        /// <summary>
        /// Plays the given song at the scheduled time. <para/>
        /// NOTE: No tracks on the song will be played. Use the <paramref name="enabledTracks"/> to enable which tracks should play.
        /// </summary>
        public static void PlaySongScheduled(string songName, double scheduledTime, params int[] enabledTracks)
        {
            PlaySongScheduled(GetSong(songName), scheduledTime, enabledTracks);
        }

        /// <summary>
        /// Plays the given song at the scheduled time. <para/>
        /// NOTE: No tracks on the song will be played. Use the <paramref name="enabledTracks"/> to enable which tracks should play.
        /// </summary>
        public static void PlaySongScheduled(WAVYSong song, double scheduledTime, params int[] enabledTracks)
        {
            Instance.PlaySongLocal(song, scheduledTime, enabledTracks);
        }
        #endregion

        #region Stop Song
        /// <summary>
        /// Fades out the song with the given <paramref name="songName"/> for <paramref name="fadeDuration"/> seconds. Set <paramref name="fadeDuration"/> to 0 for an instant cut.
        /// </summary>
        public static void StopSong(string songName, float fadeDuration)
        {
            StopSong(GetSong(songName), fadeDuration);
        }

        /// <summary>
        /// Fades out the <paramref name="song"/>  for <paramref name="fadeDuration"/> seconds. Set <paramref name="fadeDuration"/> to 0 for an instant cut.
        /// </summary>
        public static void StopSong(WAVYSong song, float fadeDuration)
        {
            if (!_songTracks.ContainsKey(song))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"No song called \"{song.name}\" is currently playing!");
#endif
                return;
            }

            WAVYMusicTrack[] tracks = _songTracks[song];

            foreach (WAVYMusicTrack track in tracks)
            {

            }
        }
        #endregion

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

            // Calculate the amount of tracks we are going to need to play this song
            int trackCount = song.TrackCount;

            // Create an array of the tracks which we will use to play our song
            WAVYMusicTrack[] tracks = new WAVYMusicTrack[trackCount];

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

            // Loop through all of our chosen tracks
            int length = trackCount;

            for (int i = 0; i < length; i++)
            {
                WAVYMusicTrack track = tracks[i];
                bool mainTrack = i == 0;

                if (mainTrack)
                {
                    track.HandleLooping = true;
                }
                else
                {
                    track.HandleLooping = false;
                }

                track.Song = song;

                AudioClip clip = mainTrack ? song.SongClip : song.Tracks[i - 1];

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

            // Add tracks to dictionary
            _songTracks[song] = tracks;
        }

        #region Set Song Tracks
        /// <summary>
        /// Fades in all the tracks in the <paramref name="enabledTracks"/> array.
        /// </summary>
        public static void SetSongTracks(string songName, float fadeDuration, params int[] enabledTracks)
        {
            SetSongTracks(GetSong(songName), fadeDuration, enabledTracks);
        }

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
        public static void SetTrackVolume(string songName, float volume, params int[] tracks)
        {
            SetTrackVolume(GetSong(songName), volume, tracks);
        }

        public static void SetTrackVolume(WAVYSong song, float volume, params int[] tracks)
        {
            Instance.SetTrackVolumeLocal(song, volume, tracks);
        }

        private void SetTrackVolumeLocal(WAVYSong song, float volume, params int[] tracks)
        {
            if (!_songTracksVolume.ContainsKey(song))
            {
                return;
            }

            bool cancelCoroutines = !_songCoroutines.ContainsKey(song);

            foreach (int track in tracks)
            {
                if (!cancelCoroutines)
                {
                    CancelFadeCoroutine(song, track, false);
                }

                _songTracksVolume[song][track] = volume * VolumeScale;
            }
        }

        public static void SetTrackVolume(string songName, float volume, int track)
        {
            SetTrackVolume(GetSong(songName), volume, track);
        }

        public static void SetTrackVolume(WAVYSong song, float volume, int track)
        {
            Instance.SetTrackVolumeLocal(song, volume, track);
        }

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


        public static void FadeTrack(string songName, int track, float duration, float targetVolume)
        {
            FadeTrack(GetSong(songName), track, duration, targetVolume);
        }

        public static void FadeTrack(WAVYSong song, int track, float duration, float targetVolume)
        {
            Instance.FadeTrackLocal(song, track, duration, targetVolume);
        }

        private void FadeTrackLocal(WAVYSong song, int track, float duration, float targetVolume)
        {
            if (!_songCoroutines.ContainsKey(song))
            {
                return;
            }

            CancelFadeCoroutine(song, track, false);

            _songCoroutines[song][track] = StartCoroutine(StartFade(song, track, duration, targetVolume));
        }

        private void UpdateTrackVolume(WAVYSong song, int track)
        {
            // Return if there are no tracks currently
            if (!_songTracks.ContainsKey(song))
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
        /// Fades from the current volume to the <paramref name="targetVolume"/> over the time of <paramref name="duration"/> seconds.
        /// </summary>
        private IEnumerator StartFade(WAVYSong song, int track, float duration, float targetVolume)
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

            yield break;
        }
        #endregion
    }
}