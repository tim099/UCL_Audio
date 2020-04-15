using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_StreamingAudioClip : MonoBehaviour {
        public struct AudioData {
            public AudioData(float[] _data) {
                m_Data = _data;
            }
            public float[] m_Data;
        }
        public int m_LengthSamples = 1024;
        public int m_Channels = 1;
        public int m_Frequency = 8000;
        //public bool m_3D = false;
        public bool m_Stream = false;

        public AudioClip m_Clip { get; protected set; }

        protected Queue<AudioData> m_AudioDatas;

        bool f_Inited = false;
        virtual public void Init() {
            if(f_Inited) return;
            f_Inited = true;
            m_AudioDatas = new Queue<AudioData>();
            m_Clip = AudioClip.Create(name, m_LengthSamples, m_Channels, m_Frequency, m_Stream);
        }
        private void OnDestroy() {
            if(m_Clip) {
                Destroy(m_Clip);
                m_Clip = null;
            }
        }
        public int GetDataCount() {
            return m_AudioDatas.Count;
        }
        public UCL_StreamingAudioClip AddData(float[] data) {
            m_AudioDatas.Enqueue(new AudioData(data));
            return this;
        }
        public bool LoadData() {
            if(m_AudioDatas.Count == 0) return false;
            var data = m_AudioDatas.Dequeue();
            m_Clip.SetData(data.m_Data, 0);
            return true;
        }
    }
}

