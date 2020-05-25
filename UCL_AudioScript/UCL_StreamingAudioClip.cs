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
        public UCL_StreamingAudioClip SetBufferCount(int val) {
            m_BufferCount = val;
            return this;
        }
        public int GetDataCount() {
            return m_AudioDatas.Count;
        }
        public UCL_StreamingAudioClip AddData(float[] data, System.Action<float[]> dispose_act = null) {
            if(data.Length != m_LengthSamples * m_Channels) {
                Debug.LogError("data.Length != m_LengthSamples * m_Channels,data.Length:" + data.Length +
                    ",m_LengthSamples * m_Channels:" + (m_LengthSamples * m_Channels));
                dispose_act?.Invoke(data);
                return this;
            }
            if(m_AudioDatas.Count >= (m_MaxBufferCount + m_BufferCount)) {
                Debug.LogWarning("m_AudioDatas.Count:" + m_AudioDatas.Count+" >= (m_MaxBufferCount+m_BufferCount)");
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
        internal protected int m_LoadAt = 0;
        protected int m_PrevPlayAt = 0;
        public void InitData() {
            m_LoadAt = 0;
            m_PrevPlayAt = 0;
        }

        public bool LoadData(ref int play_at,out bool stop) {
            stop = false;

            int play_seg = (play_at / m_LengthSamples);
            if(play_seg < 0) play_seg = 0;
            if(play_seg >= m_BufferCount) play_seg = m_BufferCount - 1;

            int load_pos = m_LoadAt * m_LengthSamples;

            if(m_AudioDatas.Count == 0) {// || play_at < m_LengthSamples
                if(play_at >= load_pos && m_PrevPlayAt <= load_pos) {
                    //Debug.LogWarning("A. stop,m_PrevPlayAt:" + m_PrevPlayAt + ",load_pos:" + load_pos + ",play_at:" + play_at);
                    stop = true;
                } else if(play_at >= load_pos && (m_PrevPlayAt >= load_pos && m_PrevPlayAt > play_at)) {
                    //Debug.LogWarning("B. stop,m_PrevPlayAt:" + m_PrevPlayAt + ",load_pos:" + load_pos + ",play_at:" + play_at);
                    stop = true;
                } else if(play_at <= load_pos && (m_PrevPlayAt <= load_pos && m_PrevPlayAt > play_at)) {
                    //Debug.LogWarning("C. stop,m_PrevPlayAt:" + m_PrevPlayAt + ",load_pos:" + load_pos + ",play_at:" + play_at);
                    stop = true;
                }
                //if(!stop) Debug.LogWarning("X. !stop ,m_PrevPlayAt:" + m_PrevPlayAt + ",load_pos:" + load_pos + ",play_at:" + play_at);
                m_PrevPlayAt = play_at;
                return false;
            }
            m_PrevPlayAt = play_at;


            stop = false;
            int seg_len = m_LengthSamples;

            if(m_LoadAt > play_seg) {
                while(m_AudioDatas.Count > 0 && m_LoadAt < m_BufferCount) {

                    var data = m_AudioDatas.Dequeue();
                    m_Clip.SetData(data.m_Data, m_LoadAt * seg_len);
                    data.Dispose();
                    //++m_LoadCount;
                    ++m_LoadAt;
                }
                if(m_LoadAt >= m_BufferCount) m_LoadAt = 0;
            }
            if(m_LoadAt < play_seg) {
                while(m_AudioDatas.Count > 0 && m_LoadAt < play_seg) {
                    //Debug.LogWarning("m_LoadAt:" + m_LoadAt + ",play_seg:" + play_seg);
                    var data = m_AudioDatas.Dequeue();
                    m_Clip.SetData(data.m_Data, m_LoadAt * seg_len);
                    data.Dispose();
                    ++m_LoadAt;
                }
                if(m_LoadAt >= m_BufferCount) m_LoadAt = 0;
            }
            return true;
        }
        /*
        public bool LoadData(ref int sample_at) {
            if(m_AudioDatas.Count == 0 || sample_at < m_LengthSamples) {
                return false;
            }
            int len = m_BufferCount * m_LengthSamples;

            m_Clip.GetData(m_Buffer, 0);
            while(m_AudioDatas.Count > 0 && sample_at > m_LengthSamples) {
                if(len - sample_at > 0) {
                    int PosAt = sample_at - sample_at % m_LengthSamples;
                    System.Array.Copy(m_Buffer, m_Channels * PosAt, m_Buffer, m_Channels * (PosAt - m_LengthSamples),
                        m_Channels * (len - PosAt));
                }
                sample_at -= m_LengthSamples;
                
                var data = m_AudioDatas.Dequeue();
                System.Array.Copy(data.m_Data, 0, m_Buffer, m_Channels * (len - m_LengthSamples), m_Channels * m_LengthSamples);
                data.Dispose();
            }
            m_Clip.SetData(m_Buffer, 0);

            return true;
        }
        */
    }
}

