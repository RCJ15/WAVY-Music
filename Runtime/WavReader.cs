using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization;

using UnityEngine;

namespace WAVYMusic
{
    /// <summary>
    /// Can read cues (markers) from WAV and BWF files.
    /// </summary>
    public static class WavReader
    {
        /// <summary>
        /// Reads the wav file at the given <paramref name="path"/>
        /// </summary>
        /// <remarks>The file at the <paramref name="path"/> must exist and must be a WAV or BWF extension.</remarks>
        /// <exception cref="FileNotFoundException">The file at the <paramref name="path"/> was not found.</exception>
        /// <exception cref="WrongFileFormatException">The file at the <paramref name="path"/> didn't exist.</exception>
        public static WavMetadata GetMetadata(string path)
        {
            // Check if the file even exists
            if (!File.Exists(path))
            {
                // Throw exception
                throw new FileNotFoundException($"The file at the path \"{path}\" doesn't exist.");
            }

            // Check if the file ends with WAV or BWF
            string fileExtension = Path.GetExtension(path).ToLower();

            if (fileExtension != ".wav" && fileExtension != ".bwf")
            {
                // Throw COOL CUSTOM exception
                throw new WrongFileFormatException($"The file at the path \"{path}\" does not have a WAV or BWF extension.");
            }

            // Create metadata which we will populate later
            WavMetadata metadata = new WavMetadata
            {
                Filename = Path.GetFileName(path)
            };

            // Open a file stream
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            int n = 0;
            // TODO: Change into a for-loop
            while (fs.Position < fs.Length)
            {
                ReadNextChunk(fs, metadata);

                if (n > 999)
                {
                    Debug.LogError("Cancelled infinite loop upon reading wave file metadata...");
                    break;
                }
                n++;
            }

            fs.Close();

            // Calculate duration
            metadata.Duration = (float)metadata.SampleCount / metadata.SampleRate;

            return metadata;
        }

        #region Custom WrongFileFormatException, very unnecessary but cool
        /// <summary>
        /// Custom exception made for one very specific problem. This is very unnecessary but also very cool.
        /// </summary>
        [Serializable]
        public class WrongFileFormatException : Exception
        {
            public WrongFileFormatException() { }
            public WrongFileFormatException(string message) : base(message) { }
            public WrongFileFormatException(string message, Exception inner) : base(message, inner) { }
            protected WrongFileFormatException(
              SerializationInfo info,
              StreamingContext context) : base(info, context) { }
        }
        #endregion

        /// <summary>
        /// Reads the next chunk of a wave file. Copied from: https://forum.unity.com/threads/solved-how-to-read-wav-markers-in-unity.860992/
        /// </summary>
        private static void ReadNextChunk(FileStream fs, WavMetadata data)
        {
            long initialPos = fs.Position;
            string chunkID = GetString(fs, 4);
            uint chunkSize = GetUInt(fs, 4);
            long chunkEndPos = initialPos + chunkSize + 8;

            switch (chunkID.ToLower())
            {
                // Set total byte size and riff type if the chunk ID is "riff"
                case "riff":
                    data.FileBytes = chunkSize + 8;
                    data.RiffTypeID = GetString(fs, 4);
                    break;

                case "fmt ":
                    data.CompressionCode = GetUInt(fs, 2);
                    data.ChannelCount = GetUInt(fs, 2);
                    data.SampleRate = GetUInt(fs, 4);
                    data.AvgBytesPerSec = GetUInt(fs, 4);
                    data.BlockAlign = GetUInt(fs, 2);
                    data.BitRate = GetUInt(fs, 2);

                    fs.Position = chunkEndPos; // Go to end of chunk
                    break;

                case "data":
                    data.SampleCount = (chunkSize / (data.BitRate / 8)) / data.ChannelCount;

                    fs.Position = chunkEndPos; // Go to end of chunk
                    break;

                case "cue ":
                    uint cueCount = GetUInt(fs, 4);
                    data.Cues = new WavCue[cueCount];

                    // Loop through cues
                    for (int i = 0; i < cueCount; i++)
                    {
                        long p = fs.Position;
                        data.Cues[i].ID = GetUInt(fs, 4);
                        data.Cues[i].Position = GetUInt(fs, 4);
                        data.Cues[i].DataChunkID = GetUInt(fs, 4);

                        fs.Position = p + 24; // Skip to next cue
                    }

                    fs.Position = chunkEndPos; // Go to end of chunk
                    break;

                case "list":
                    string listID = GetString(fs, 4).ToLower();

                    if (listID == "adtl") // ADTL = Associated Data List
                    {
                        uint remainingBytes = chunkSize - 4;

                        string subChunkID;
                        uint subChunkSize;
                        int cueIndex = 0;

                        while (remainingBytes > 0)
                        {
                            subChunkID = GetString(fs, 4); // labl
                            subChunkSize = GetUInt(fs, 4); // chunk size

                            if (subChunkID.ToLower() == "labl" && data.Cues != null)
                            {
                                data.Cues[cueIndex].ID = GetUInt(fs, 4);
                                data.Cues[cueIndex].Name = GetString(fs, (int)subChunkSize - 4);

                                remainingBytes -= subChunkSize + 8;

                                // Check for uneven number of remaining bytes (which means the next byte is an empty padding)
                                if (remainingBytes % 2 == 1)
                                {
                                    remainingBytes -= 1;
                                    fs.ReadByte(); // Read the padded byte
                                }

                                cueIndex++;
                            }
                            else
                            {
                                remainingBytes -= subChunkSize;
                                fs.Seek(subChunkSize, SeekOrigin.Current); // Go to end of subchunk
                            }
                        }

                    }
                    fs.Position = chunkEndPos; // Go to end of chunk
                    break;

                default:
                    fs.Position = chunkEndPos; // Go to end of chunk
                    break;
            }


        }

        /// <summary>
        /// Reads the bytes of the <see cref="FileStream"/> and returns them in <see cref="uint"/> format. <para/>
        /// See: <see cref="BitConverter.ToUInt32"/>
        /// </summary>
        private static uint GetUInt(FileStream fs, int byteNum)
        {
            // Use the BitConverter to convert the byte array into a uint
            return BitConverter.ToUInt32(ReadBytes(fs, byteNum), 0);
        }

        /// <summary>
        /// Reads the bytes of the <see cref="FileStream"/> and returns them in a UTF8 <see cref="string"/> format. <para/>
        /// See: <see cref="Encoding.UTF8"/>
        /// </summary>
        private static string GetString(FileStream fs, int byteNum)
        {
            // Use the Text Encoder to convert the byte array into a UTF8 format string
            // Also trim away all '\0' characters at the end and start of the string since those are useless characters
            return Encoding.UTF8.GetString(ReadBytes(fs, byteNum)).Trim('\0');
        }

        /// <summary>
        /// Reads the next bytes of the <see cref="FileStream"/> and returns them as a <see cref="byte"/> array.
        /// </summary>
        private static byte[] ReadBytes(FileStream fs, int num)
        {
            // Create an array of bytes but limit the size of the array to only be 4 or more
            byte[] bytes = new byte[Mathf.Max(4, num)];

            // Fill the byte array by reading the bytes from the FileStream
            for (int i = 0; i < num; i++)
            {
                byte b = (byte)fs.ReadByte();

                bytes[i] = b;
            }

            // Return the newly filled byte array
            return bytes;
        }
    }

    /// <summary>
    /// The Metadata inside a WAV or BWF file. <para/>
    /// Use <see cref="WavReader.GetMetadata"/> to get the metadata.
    /// </summary>
    [Serializable]
    public class WavMetadata
    {
        /// <summary>
        /// Name of the file, including extension.
        /// </summary>
        public string Filename;

        /// <summary>
        /// Duration of audio (in seconds).
        /// </summary>
        public float Duration;

        /// <summary>
        /// Total file size in bytes.
        /// </summary>
        public uint FileBytes;

        /// <summary>
        /// RIFF type ID. Usually "WAVE".
        /// </summary>
        public string RiffTypeID;

        /// <summary>
        /// Compression code. Uncompressed PCM audio will have a value of 1.
        /// </summary>
        public uint CompressionCode;

        /// <summary>
        /// Number of audio channels. 1 = Mono, 2 = Stereo.
        /// </summary>
        public uint ChannelCount;

        /// <summary>
        /// Samples per second.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// Average bytes per second. For example, a PCM wave file that has a sampling rate of 44100 Hz, 1 channel, and sampling resolution of 16 bits (2 bytes) per sample, will have an average number of bytes equal to 44100 * 2 * 1 = 88,200.
        /// </summary>
        public uint AvgBytesPerSec;

        /// <summary>
        /// Byte-size of sample blocks. For example, a PCM wave that has a sampling resolution of 16 bits (2 bytes) and has 2 channels will record a block of samples in 2 * 2 = 4 bytes.
        /// </summary>
        public uint BlockAlign;

        /// <summary>
        /// Significant bits per sample. Defines the sampling resolution of the file. A typical sampling resolution is 16 bits per sample, but could be anything greater than 1.
        /// </summary>
        public uint BitRate;

        /// <summary>
        /// Total number of audio samples.
        /// </summary>
        public uint SampleCount;

        /// <summary>
        /// The maximum sample value. Sample values range between -maxSampleValue and +maxSampleValue. The value depends on the bitrate.
        /// </summary>
        public int MaxSampleValue;

        /// <summary>
        /// Cues/markers found in the wave file.
        /// </summary>
        public WavCue[] Cues;

        public override string ToString()
        {
            string str;

            str = Filename + "\n";
            str += "Duration: " + Duration + " s\n";
            str += "Size: " + FileBytes + "\n";
            str += "Riff type ID: " + RiffTypeID + "\n";
            str += "Compression code: " + CompressionCode + "\n";
            str += "Channel count: " + ChannelCount + "\n";
            str += "Sample rate: " + SampleRate + "\n";
            str += "Avg bytes per sec: " + AvgBytesPerSec + "\n";
            str += "Block align: " + BlockAlign + "\n";
            str += "Bitrate: " + BitRate + "\n";
            str += "Sample count: " + SampleCount + "\n";

            if (Cues == null)
                str += "No cues";
            else
                str += "Cues:\n";
            foreach (WavCue c in Cues)
            {
                str += string.Format(" - ID: {0} - Name: {1} - Position: {2} - dataChunkID: {3}\n", c.ID, c.Name, c.Position, c.DataChunkID);
            }
            return str;
        }
    }

    [Serializable]
    public struct WavCue
    {
        /// <summary>
        /// Unique index of this cue.
        /// </summary>
        public uint ID;

        /// <summary>
        /// Identifier-string for the cue/marker.
        /// </summary>
        public string Name;

        /// <summary>
        /// The sample on which this cue appears within the audio.
        /// </summary>
        public uint Position;

        /// <summary>
        /// Either "data" or "slnt" depending on whether the cue occurs in a data chunk or in a silent chunk.
        /// </summary>
        public uint DataChunkID;

        public override string ToString()
        {
            return $"(ID: {ID}, Name: {Name}, Position: {Position}, Data Chunk ID: {DataChunkID})";
        }
    }
}