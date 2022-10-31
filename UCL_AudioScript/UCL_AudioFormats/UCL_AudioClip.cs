using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UCL.AudioLib
{
    public class UCL_AudioClip
    {
        public static UCL_AudioClip Create(byte[] iBytes, string iName = "New Wav.wav")
        {
            try
            {
                if (iName.Contains(".wav"))
                {
                    return new UCL_Wav(iBytes, iName);
                }
            }
            catch(System.Exception iE)
            {
                Debug.LogException(iE);
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
        /// Number of sample frames.
        /// </summary>
        public int LengthSamples => m_ChannelDatas.Length / ChannelsCount;
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
                    m_Clip = AudioClip.Create(ClipName, LengthSamples, ChannelsCount, Frequency, false);
                    m_Clip.SetData(m_ChannelDatas, 0);
                }

                return m_Clip;
            }
        }
        ~UCL_AudioClip()
        {
            //Debug.Log("~UCL_AudioClip()");
            if(m_Clip != null) AudioClip.DestroyImmediate(m_Clip);
        }
        /// <summary>
        /// X range between 0~1
        /// </summary>
        /// <param name="iX"></param>
        /// <returns>-1f~1f</returns>
        virtual public float GetSample(float iX, int iChannel = 0)
        {
            if (m_ChannelDatas.IsNullOrEmpty()) return 0;
            int aLen = SamplesCount / ChannelsCount;
            {
                if (iChannel >= ChannelsCount) iChannel = ChannelsCount - 1;

                int aAt = ChannelsCount * Mathf.FloorToInt(iX * aLen) + iChannel;
                if (aAt >= m_ChannelDatas.Length) aAt = m_ChannelDatas.Length - 1;
                if (aAt < 0) aAt = 0;
                return m_ChannelDatas[aAt];
            }

            //{
            //    if (iChannel >= ChannelsCount) iChannel = ChannelsCount - 1;

            //    int aAt = Mathf.FloorToInt(iX * SamplesCount) + iChannel * SamplesCount;
            //    if (aAt >= m_ChannelDatas.Length) aAt = m_ChannelDatas.Length - 1;
            //    if (aAt < 0) aAt = 0;
            //    return m_ChannelDatas[aAt];

            //}

            
        }
        /// <summary>
        /// Draw channel to Texture
        /// </summary>
        /// <param name="iTexture"></param>
        /// <param name="iLineCol"></param>
        /// <param name="iChannel"></param>
        virtual public void DrawChannel(UCL.Core.TextureLib.UCL_Texture2D iTexture, Color iLineCol, int iChannel = 0)
        {
            iTexture.DrawHorizontalLine(0.5f, iLineCol);
            iTexture.DrawAudioWav((iX) => {
                float aVal = GetSample(iX, iChannel);//-1~1f
                float aSqrt = Mathf.Sqrt(Mathf.Abs(aVal));//0~1f

                if (aVal < 0) return (-0.5f * aSqrt + 0.5f);
                return (0.5f * aSqrt + 0.5f);
            }, iLineCol);// + 0.5f Mathf.Sqrt
        }
    }
}