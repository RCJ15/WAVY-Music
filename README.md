# IMPORTANT NOTE
This project is currently unifinished. Development will be finished before 2023. That I can promise.

# What's The WAV All About?
WAVY Music is a music tool for Unity which takes advantage of the fact that WAV files store audio markers. Using this technology, WAVY Music can read directly from your WAV file and pinpoint exactly where you placed you marker so you can execute events at the markers position and even create seamless loops for your songs.
Please note that WAVY Music also works for other audio formats such as MP3, FLAC and OGG. WAV is just recommended for it's usage of markers (which are also called audio cues).

# How To Install
1. Download the repository.
2. Place the unzipped folder anywhere in your Unity Assets folder.
3. You're now done and ready to use WAVY Music. Yep it's that simple.

# How To Use The WAV
WAVY Music uses scriptable objects to store it's song information. So to get started, you create a new WAVY Song scriptable object by right clicking in your projects tab and go to "Create/WAVY Song", you can then name your song to anything that fits.

![Creating a WAVY Song Asset and renaming it](https://user-images.githubusercontent.com/73841786/206930664-6470aa55-1596-428e-b075-c96f6ff717aa.png)

## The Inspector
The WAVY Songs inspector is quite simple to work with. Everything is divided into 4 different foldable categories:<br>
**Song Data, Song Markers, Song List & WAVY Music Settings.**

![WAVY Song Inspector](https://user-images.githubusercontent.com/73841786/206930830-f805631d-1411-40e8-8a49-b4099f85b188.png)

### Song Data
This section contains fields for... Well, Song Data! This can be things such as which audio files to use and what name the song should have.

![The Song Data section as seen in the inspector](https://user-images.githubusercontent.com/73841786/206931223-ba1c065d-167b-4606-b091-424bfe841df5.png)

### Song Markers
This section contains fields for viewing the markers in your song, as well as using the markers to set loop points, start points and even set song events.

![The Song Markers section as seen in the inspector](https://user-images.githubusercontent.com/73841786/206931467-3090b24b-788a-4991-8665-579fca352f1c.png)

### Song List
This section contains a toggle field for whether your song should be in the WAVY Song List and it also contains a global array for the WAVY Song List.

![The Song List section as seen in the inspector](https://user-images.githubusercontent.com/73841786/206931508-592da309-e29e-4c35-90bd-31f668fd3b4d.png)

### WAVY Music Settings
This entire section is simply the same inspector as a WAVY Settings object. In here you can change some settings like which audio mixer your WAVY Songs get sent to and how much time beforehand a song should schedule their loop.

![The WAVY Music Settings section as seen in the inspector](https://user-images.githubusercontent.com/73841786/206931558-657720b7-9e0d-4b31-bdb1-24464870dbc9.png)

## Usage In Your Games
Now that you've setup your WAVY Songs, it's now time to actually hear your songs in your games. Here is a little step by step process of how to do so:

**Step 1:** Create a new C# script.

![Creating the C# script](https://user-images.githubusercontent.com/73841786/206931790-37bf5810-cbd8-4fe5-bb5e-a6bb86884888.png)

**Step 2:** Use the namespace: "WAVYMusic".

![Using the namespace: "WAVYMusic"](https://user-images.githubusercontent.com/73841786/206931902-bc52e487-22a9-4c10-8d9a-9c8978ace95d.png)

**Step 3:** Get a reference to your WAVYSong in some way. This can be via a public field, serialized private field or even us the WAVYSongList and get it from just the name alone.

![Getting a reference to the WAVYSong in some way](https://user-images.githubusercontent.com/73841786/206932080-d99dab70-beba-4b27-b37c-98f3ca099b96.png)

**Step 4:** Play the song using "WAVYSong.Play()".

![Playing the song using "WAVYSong.Play()"](https://user-images.githubusercontent.com/73841786/206932150-bd3da858-fee9-4f1f-beae-954e9ff7c71c.png)

And that's it! Of course there is way more to WAVY Music than this, so if you want to go further into the WAVYness then go to this repositorys wiki and become the WAVY Master.
