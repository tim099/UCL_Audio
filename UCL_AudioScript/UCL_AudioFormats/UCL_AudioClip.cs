using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UCL.AudioLib
{
    public class UCL_AudioClip
    {
        public static UCL_AudioClip Create(byte[] iBytes, string iName = "New Wav.wav")
        {
            if (iName.Contains(".wav"))
            {
                return new UCL_Wav(iBytes, iName);
            }

            return null;
        }

        AudioClip m_Clip = null;
        protected float[] m_ChannelDatas;
        public string ClipName { get; internal set; } = "New AudioClip";
        /// <summary>
        /// NumChannels
        /// Mono = 1, Stereo = 2, etc.
        /// </summary>
        public int ChannelsCount { get; internal set; }

        /// <summary>
        /// SampleRate 8000, 44100, etc.
        /// </summary>
        public int Frequency { get; internal set; }
        
        /// <summary>
        /// SamplesCount = DataSize / ChannelsCount
        /// </summary>
        public int SamplesCount { get; internal set; }

        
        /// <summary>
        /// Total play time in seconds
        /// </summary>
        virtual public float ClipTime => 0;

        virtual public AudioClip Clip
        {
            get
            {
                if (m_Clip == null)
                {
                    //Debug.LogError("ClipName:"+ ClipName + "ChannelDatas.Length:" + ChannelDatas.Length+ ",ChannelsCount:"+ ChannelsCount+ ",Frequency:"+ Frequency);
                    m_Clip = AudioClip.Create(ClipName, m_ChannelDatas.Length, ChannelsCount, Frequency, false);
                    m_Clip.SetData(m_ChannelDatas, 0);
                }

                return m_Clip;
            }
        }
        /// <summary>
        /// X range between 0~1
        /// </summary>
        /// <param name="iX"></param>
        /// <returns></returns>
        virtual public float GetSample(float iX, int iChannel = 0)
        {
            if (m_ChannelDatas.IsNullOrEmpty()) return 0;

            if (iChannel >= ChannelsCount) iChannel = ChannelsCount - 1;

            int aAt = ChannelsCount * Mathf.FloorToInt(iX * SamplesCount) + iChannel;
            if (aAt >= m_ChannelDatas.Length) aAt = m_ChannelDatas.Length - 1;
            if (aAt < 0) aAt = 0;
            return m_ChannelDatas[aAt];
        }
    }
}