using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WAVYMusic
{
    /// <summary>
    /// Sorts and caches all <see cref="WAVYSong.Event"/> lists for further use. <para/>
    /// Use <see cref="Get"/> to get either a newly sorted list or an already sorted one.
    /// </summary>
    public static class WAVYSongEventSorter
    {
        private static readonly Dictionary<WAVYSong, List<WAVYSong.Event>> _dictionary = new Dictionary<WAVYSong, List<WAVYSong.Event>>();

        /// <summary>
        /// Returns either a newly sorted list of <see cref="WAVYSong.Event"/> for the <paramref name="song"/> or an already existing sorted one.
        /// </summary>
        public static List<WAVYSong.Event> Get(WAVYSong song)
        {
            // No song events
            if (!song.HaveSongEvents || song.SongEvents == null || song.SongEvents.Count <= 0)
            {
                return null;
            }

            // Return already existing list if it exists
            if (_dictionary.ContainsKey(song))
            {
                return _dictionary[song];
            }

            // Create new sorted list
            List<WAVYSong.Event> list = new List<WAVYSong.Event>();

            // Use linq and sort by the time from lowest to highest
            list = song.SongEvents.OrderBy(e => e.Time).ToList();

            _dictionary[song] = list;

            // Return the newly sorted list
            return list;
        }
    }
}
