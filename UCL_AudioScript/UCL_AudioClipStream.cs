using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    /// <summary>
    /// UCL_AudioClipStream convert AudioClip to float[] stream!!
    /// </summary>
    public class UCL_AudioClipStream : UCL_AudioStream {
        public AudioClip m_Clip;//Target Clip
        public int m_LoadSampleAt = 0;

        public override void Init(int Length) {
            base.Init(Length * m_Clip.channels);
            m_LoadSampleAt = 0;
            m_Frequency = m_Clip.frequency;
            m_Channels = m_Clip.channels;
        }
        /// <summary>
        /// require Init first to set length of array
        /// please call Dispose(float[] arr) after using the array!!
        /// </summary>
        /// <returns></returns>
        override public float[] Load() {
            if(m_Pool == null) return null;

            int len = m_Length/m_Clip.channels;
            float[] data = Rent();
            m_Clip.GetData(data, m_LoadSampleAt);
            m_LoadSampleAt += len;
            if(m_LoadSampleAt >= m_Clip.samples) {
                m_LoadSampleAt = 0;
            }
            return data;
        }

        public float[] Load(int Length) {
            float[] data = new float[Length * m_Clip.channels];
            m_Clip.GetData(data, m_LoadSampleAt);
            m_LoadSampleAt += Length;
            if(m_LoadSampleAt >= m_Clip.samples) {
                m_LoadSampleAt = 0;
            }
            return data;
        }
        
    }
}