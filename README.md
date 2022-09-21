# IMPORTANT NOTE
This project is currently unifinished. Development will be finished before 2023. That I can promise.

# What's The WAV All About?
WAVY Music is a music tool for Unity which takes advantage of the fact that WAV files store audio markers. Using this technology, WAVY Music can read directly from your WAV file and pinpoint exactly where you placed you marker so you can execute events at the markers position and even create seamless loops for you songs.
Please note that WAVY Music also works for other audio formats such as MP3, FLAC and OGG. WAV is just recommended for it's usage of markers (which are also called audio cues).

# How To Install
1. Download the repository.
2. Place the unzipped folder into your Unity Assets folder.
3. You're now done and ready to use WAVY Music. Yep it's that simple.

NOTE: WAVY Music can actually be placed in any folder you want and doesn't have to be at the root of your project, just make sure that the folder you put it in is a subfolder of the Assets folder so it's considered an asset by Unity.

# How To Use The WAV
WAVY Music uses scriptable objects to store it's song information. So to get started, you create a new WAVY Song scriptable object by right clicking in your projects tab and go to "Create/WAVY Song", you can then name your song to anything that fits.

## The Inspector
The WAVY Music inspector is quite simple to work with. Everything is divided into 4 different foldable categories: Song Data, Song Markers, Song List & WAVY Music Settings.

**Song Data:** This section contains fields for... Well, Song Data! This can be things such as which audio files to use and what name the song should have.

**Song Markers:** This section contains fields for viewing the markers in your song, as well as using the markers to set loop points, start points and even set song events (more on those later).

**Song List:** This section contains a toggle field for whether your song should be in the WAVY Song List and it also contains a global array for the WAVY Song List.

**WAVY Music Settings:** This entire section is simply the same inspector as a WAVY Settings object. In here you can change some settings like which audio mixer your WAVY Songs get sent to and many editor settings.
