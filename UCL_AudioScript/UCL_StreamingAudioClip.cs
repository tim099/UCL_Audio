using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_StreamingAudioClip : MonoBehaviour {
        public class AudioData {
            public AudioData(float[] _data, System.Action<float[]> dispose_act) {
                m_Data = _data;
                m_DisposeAct = dispose_act;
            }
            public void Dispose() {
                m_DisposeAct?.Invoke(m_Data);
            }
            public float[] m_Data;
            System.Action<float[]> m_DisposeAct;
        }
        public int m_LengthSamples = 1024;
        public int m_Channels = 1;
        public int m_Frequency = 8000;
        public bool m_Stream = false;

        /// <summary>
        /// Max m_AudioDatas count
        /// </summary>
        public int m_MaxBufferCount = 4;

        public AudioClip m_Clip { get; protected set; }
        protected AudioData m_PrevData = null;
        protected Queue<AudioData> m_AudioDatas;
        float[] m_EmptyArr;
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
            m_AudioDatas.Clear();
            m_EmptyArr = new float[m_LengthSamples * m_Channels];
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
        public UCL_StreamingAudioClip AddData(float[] data, System.Action<float[]> dispose_act = null) {
            if(m_AudioDatas.Count >= m_MaxBufferCount || data.Length != m_LengthSamples * m_Channels) {
                dispose_act?.Invoke(data);
                return this;
            }
            m_AudioDatas.Enqueue(new AudioData(data, dispose_act));
            return this;
        }
        protected void SetPrevData(AudioData data) {
            if(m_PrevData != null) {
                m_PrevData.Dispose();
            }
            m_PrevData = data;
        }
        public bool LoadData(bool init = false) {
            if(m_AudioDatas.Count == 0) {
                if(m_PrevData != null) {
                    //if(m_Channels>1)
                    m_Clip.SetData(m_PrevData.m_Data, 0);
                    m_Clip.SetData(m_EmptyArr, m_LengthSamples);
                    SetPrevData(null);
                    return true;
                }
                return false;
            }

            var data = m_AudioDatas.Dequeue();

            if(m_PrevData != null) {
                m_Clip.SetData(m_PrevData.m_Data, 0);
            } else {
                m_Clip.SetData(m_EmptyArr, 0);
            }
            m_Clip.SetData(data.m_Data, m_LengthSamples);

            SetPrevData(data);
            return true;
        }
    }
}

