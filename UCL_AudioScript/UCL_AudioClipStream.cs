using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    /// <summary>
    /// UCL_AudioClipStream can convert AudioClip to float[] stream!!
    /// </summary>
    public class UCL_AudioClipStream : MonoBehaviour {
        public AudioClip m_Clip;//Target Clip
        public int m_LoadSampleAt = 0;

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