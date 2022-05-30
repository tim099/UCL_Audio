using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib
{
    /// <summary>
    /// reference https://answers.unity.com/questions/737002/wav-byte-to-audioclip.html
    /// </summary>
    public class UCL_Wav
    {
        // convert two bytes to one float in the range -1 to 1
        static float BytesToFloat(byte iFirstByte, byte iSecondByte)
        {
            return ((short)((iSecondByte << 8) | iFirstByte) / 32768.0F);// convert to range from -1 to (just below) 1
        }

        static int BytesToInt(byte[] bytes, int offset = 0)
        {
            int aVal = 0;
            for (int i = 0; i < 4; i++)
            {
                aVal |= ((int)bytes[offset + i]) << (i * 8);
            }
            return aVal;
        }
        public float[] ChannelDatas { get; internal set; }
        //public float[] RightChannel { get; internal set; }
        public int ChannelCount { get; internal set; }
        public int SampleCount { get; internal set; }
        public int Frequency { get; internal set; }
        public string ClipName { get; internal set; } = "UCL_Wav";
        public AudioClip Clip
        {
            get
            {
                if(m_Clip == null)
                {
                    m_Clip = AudioClip.Create(ClipName, SampleCount, ChannelCount, Frequency, false);
                    
                    m_Clip.SetData(ChannelDatas, 0);
                    //if(ChannelCount>1) m_Clip.SetData(RightChannel, 1);
                }

                return m_Clip;
            }
        }
        AudioClip m_Clip = null;
        public void Init(byte[] iWav, string iName = "New Wav")
        {
            ClipName = iName;
            // Determine if mono or stereo
            ChannelCount = iWav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels

            // Get the frequency
            Frequency = BytesToInt(iWav, 24);

            // Get past all the other sub chunks to get to the data subchunk:
            int aPos = 12;   // First Subchunk ID from 12 to 16

            // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
            while (!(iWav[aPos] == 100 && iWav[aPos + 1] == 97 && iWav[aPos + 2] == 116 && iWav[aPos + 3] == 97))
            {
                aPos += 4;
                int aChunkSize = iWav[aPos] + iWav[aPos + 1] * 256 + iWav[aPos + 2] * 65536 + iWav[aPos + 3] * 16777216;
                aPos += 4 + aChunkSize;
            }
            aPos += 8;// Pos is now positioned to start of actual sound data.

            int aDataLen = (iWav.Length - aPos) / 2;
            
            SampleCount = aDataLen / (ChannelCount * 2);    // 2 bytes per sample (16 bit sound mono)

            ChannelDatas = new float[aDataLen];

            for (int i = 0; i < aDataLen; i++)
            {
                ChannelDatas[i] = BytesToFloat(iWav[aPos + 2 * i], iWav[aPos + 2 * i + 1]);
            }
            Debug.LogError(string.Format("[WAV: ChannelCount={0}, SampleCount={1}, Frequency={2}]", ChannelCount, SampleCount, Frequency));
        }
        public UCL_Wav(byte[] iWav, string iName = "New Wav")
        {
            Init(iWav, iName);
        }
    }

}
