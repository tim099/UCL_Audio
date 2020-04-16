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
        public bool m_Stream = false;
        
        public AudioClip m_Clip { get; protected set; }
        protected float[] m_PrevData = null;
        protected Queue<AudioData> m_AudioDatas;

        bool f_Inited = false;
        virtual public void Init() {
            if(f_Inited) return;
            f_Inited = true;
            m_AudioDatas = new Queue<AudioData>();
            UpdateClipSetting();
        }
        private void OnDestroy() {
            ClearClip();
        }
        public void ClearClip() {
            if(m_Clip) {
                Destroy(m_Clip);
                m_Clip = null;
            }
        }
        public void UpdateClipSetting() {
            ClearClip();
            m_Clip = AudioClip.Create(name, 2 * m_LengthSamples, m_Channels, m_Frequency, m_Stream);
        }
        public UCL_StreamingAudioClip SetLengthSamples(int val) {
            m_LengthSamples = val;
            return this;
        }
        public UCL_StreamingAudioClip SetFrequency(int val) {
            m_Frequency = val;
            return this;
        }
        public UCL_StreamingAudioClip SetChannels(int val) {
            m_Channels = val;
            return this;
        }
        public int GetDataCount() {
            return m_AudioDatas.Count;
        }
        public UCL_StreamingAudioClip AddData(float[] data) {
            if(data.Length != m_LengthSamples * m_Channels) return this;
            m_AudioDatas.Enqueue(new AudioData(data));
            return this;
        }
        public bool LoadData() {
            if(m_AudioDatas.Count == 0) {
                if(m_PrevData != null) {
                    //if(m_Channels>1)
                    m_Clip.SetData(m_PrevData, 0);
                    m_PrevData = null;
                    return true;
                }
                return false;
            }

            var data = m_AudioDatas.Dequeue();

            if(m_PrevData != null) {
                m_Clip.SetData(m_PrevData, 0);
            }
            m_Clip.SetData(data.m_Data, m_LengthSamples);

            m_PrevData = data.m_Data;
            return true;
        }
    }
}

