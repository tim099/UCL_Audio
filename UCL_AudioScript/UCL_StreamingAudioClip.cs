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
        public int m_BufferCount = 8;
        /// <summary>
        /// Max m_AudioDatas count
        /// </summary>
        public int m_MaxBufferCount = 4;

        public AudioClip m_Clip { get; protected set; }
        protected Queue<AudioData> m_AudioDatas;
        float[] m_EmptyArr;
        float[] m_Buffer;
        bool f_Inited = false;
        virtual public void Init() {
            if(f_Inited) return;
            f_Inited = true;
            m_AudioDatas = new Queue<AudioData>();
            UpdateClipSetting();
        }
        private void OnDestroy() {
            ClearDatas();
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
            m_Buffer = new float[m_LengthSamples * m_Channels * m_BufferCount];
            m_Clip = AudioClip.Create(name, m_BufferCount * m_LengthSamples, m_Channels, m_Frequency, m_Stream);
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
            if(data.Length != m_LengthSamples * m_Channels || m_AudioDatas.Count >= (m_MaxBufferCount+m_BufferCount)) {
                dispose_act?.Invoke(data);
                return this;
            }
            m_AudioDatas.Enqueue(new AudioData(data, dispose_act));
            return this;
        }
        public void ClearDatas() {
            if(m_AudioDatas == null) return;
            while(m_AudioDatas.Count > 0) {
                m_AudioDatas.Dequeue().Dispose();
            }
        }
        public bool LoadData(ref int sample_at) {
            if(m_AudioDatas.Count == 0 || sample_at < m_LengthSamples) {
                return false;
            }
            int len = m_BufferCount * m_LengthSamples;

            m_Clip.GetData(m_Buffer, 0);
            while(m_AudioDatas.Count > 0 && sample_at >= m_LengthSamples) {
                if(len - sample_at > 0) {
                    System.Array.Copy(m_Buffer, m_Channels * sample_at, m_Buffer, m_Channels * (sample_at - m_LengthSamples),
                        m_Channels * (len - sample_at));
                }
                sample_at -= m_LengthSamples;
                
                var data = m_AudioDatas.Dequeue();
                System.Array.Copy(data.m_Data, 0, m_Buffer, m_Channels * (len - m_LengthSamples), m_Channels * m_LengthSamples);
                data.Dispose();
            }
            m_Clip.SetData(m_Buffer, 0);

            return true;
        }
    }
}

