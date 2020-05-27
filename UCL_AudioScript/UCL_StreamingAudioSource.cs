using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UCL.AudioLib {
    public class UCL_StreamingAudioSource : MonoBehaviour {
        [System.Serializable]
        public class IsPlayingEvent : UnityEngine.Events.UnityEvent<bool> {}

        public AudioSource m_Source { get; protected set; }
        public bool m_Playing { get; protected set; } = false;

        /// <summary>
        /// Clear datas cant be loaded in this frame, avoid delay but may lost some data
        /// </summary>
        public bool m_ClearDataAfterLoad = false;
        public UnityEngine.Events.UnityEvent m_OnPlayEvent;
        public UnityEngine.Events.UnityEvent m_OnStopEvent;
        public IsPlayingEvent m_IsPlayingEvent;
        [Range(0f,4f)]public float m_VolumeAlter = 1f;
        [Core.PA.UCL_ReadOnly] [SerializeField] int m_TimeSamples = 0;
        protected UCL_StreamingAudioClip m_StreamingAudioClip;
        

        static public UCL_StreamingAudioSource Create(Transform parent) {
            GameObject Obj = new GameObject("StreamingAudioSource");
            Obj.transform.SetParent(parent);
            var src = Obj.AddComponent<UCL_StreamingAudioSource>();

            return src;
        }

        bool f_Inited = false;
        virtual public void Init() {
            if(f_Inited) return;
            f_Inited = true;
            GetClip();

            m_StreamingAudioClip.Init();

            m_Source = gameObject.AddComponent<AudioSource>();
            m_Source.clip = m_StreamingAudioClip.m_Clip;
            m_Source.loop = true;
        }

        virtual public UCL_StreamingAudioClip GetClip() {
            if(m_StreamingAudioClip == null) {
                m_StreamingAudioClip = gameObject.AddComponent<UCL_StreamingAudioClip>();
            }
            return m_StreamingAudioClip;
        }
        public void Play() {
            m_Playing = true;
            if(f_Inited) return;

        }
        public void Pause() {
            m_Playing = false;
            if(f_Inited) return;
            m_Source.Pause();
            m_OnStopEvent?.Invoke();
        }
        /// <summary>
        /// dispose_act invoke when data no longer in use!!
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dispose_act"></param>
        /// <returns></returns>
        public UCL_StreamingAudioSource AddData(float[] data, System.Action<float[]> dispose_act = null) {
            if(m_StreamingAudioClip == null) return this;
            if(m_VolumeAlter > 1.01f || m_VolumeAlter < 0.99f) {
                //Debug.LogWarning("AlterVolume:" + m_VolumeAlter);
                Lib.PCM_VolumeAlter(data, m_VolumeAlter);
            }
            m_StreamingAudioClip.AddData(data, dispose_act);
            return this;
        }
        /// <summary>
        /// return true if still have space to add data
        /// </summary>
        /// <returns></returns>
        public bool CanAddData() {
            if(m_StreamingAudioClip == null) return false;
            return m_StreamingAudioClip.CanAddData();
        }
        public int GetDataCount() {
            if(m_StreamingAudioClip == null) return 0;
            return m_StreamingAudioClip.GetDataCount();
        }
        public bool isPlaying { get {
                if(!f_Inited || !m_Playing || m_Source == null) return false;
                return m_Source.isPlaying;
            }
        }
        //int p_times = 0;
        virtual protected void AudioUpdate() {
            if(!f_Inited) return;
            if(m_StreamingAudioClip == null) return;
            if(m_Playing) {
                int length_samples = m_StreamingAudioClip.m_LengthSamples;
                //int channels = m_StreamingAudioClip.m_Channels;
                //Debug.Log("m_Source.timeSamples:" + m_Source.timeSamples);
                //Debug.LogWarning("AudioUpdate() m_Source.timeSamples:"+ m_Source.timeSamples);

                if(!m_Source.isPlaying) {//Start play

                    if(m_StreamingAudioClip.GetDataCount() >= (m_StreamingAudioClip.m_BufferCount/4)) {
                        bool stop = false;
                        m_TimeSamples = 0;// m_StreamingAudioClip.m_LengthSamples * m_StreamingAudioClip.m_BufferCount;
                        m_StreamingAudioClip.InitData();
                        if(m_StreamingAudioClip.LoadData(ref m_TimeSamples, out stop)) {
                            //Debug.LogWarning("StartPlay!!:" + ++p_times + ",m_LoadAt:" + m_StreamingAudioClip.m_LoadAt);
                            m_Source.Play();
                            m_Source.timeSamples = 0;
                            m_OnPlayEvent?.Invoke();
                            //m_Source.timeSamples = m_TimeSamples;// 0;//length_samples;/2
                            if(m_ClearDataAfterLoad) {
                                m_StreamingAudioClip.ClearDatas();
                            }
                        }
                    }


                } else {//Playing!!
                    bool stop = false;
                    m_TimeSamples = m_Source.timeSamples;
                    if(m_StreamingAudioClip.LoadData(ref m_TimeSamples, out stop)) {
                        //Debug.LogWarning("ContinuePlay!!:" + m_Source.timeSamples + ",m_LoadAt:" + m_StreamingAudioClip.m_LoadAt);
                        //m_Source.timeSamples = m_TimeSamples;//sample_at - length_samples;
                        if(m_ClearDataAfterLoad) {
                            m_StreamingAudioClip.ClearDatas();
                        }
                    }
                    if(stop) {
                        //Debug.LogWarning("Stop!!:"+ p_times);
                        m_Source.Stop();
                        m_Source.timeSamples = 0;
                    }
                }
                if(m_TimeSamples < 0) Debug.LogError("m_TimeSamples:" + m_TimeSamples);
                m_IsPlayingEvent?.Invoke(isPlaying);
            }
        }
        /*
        virtual protected void AudioUpdate() {
            if(!f_Inited) return;
            if(m_StreamingAudioClip == null) return;
            if(m_Playing) {
                int length_samples = m_StreamingAudioClip.m_LengthSamples;
                //int channels = m_StreamingAudioClip.m_Channels;
                //Debug.Log("m_Source.timeSamples:" + m_Source.timeSamples);
                m_TimeSamples = m_Source.timeSamples;
                if(!m_Source.isPlaying) {
                    m_TimeSamples = m_StreamingAudioClip.m_LengthSamples * m_StreamingAudioClip.m_BufferCount;
                    if(m_StreamingAudioClip.LoadData(ref m_TimeSamples)) {
                        //Debug.LogWarning("StartPlay!!");
                        m_Source.Play();
                        m_OnPlayEvent?.Invoke();
                        m_Source.timeSamples = m_TimeSamples;// 0;//length_samples;/2
                        if(m_ClearDataAfterLoad) {
                            m_StreamingAudioClip.ClearDatas();
                        }
                    }
                } else {//Playing!!
                    if(m_StreamingAudioClip.LoadData(ref m_TimeSamples)) {
                        //Debug.LogWarning("ContinuePlay!!:" + m_Source.timeSamples);
                        m_Source.timeSamples = m_TimeSamples;//sample_at - length_samples;
                        if(m_ClearDataAfterLoad) {
                            m_StreamingAudioClip.ClearDatas();
                        }
                    }

                }
                if(m_TimeSamples < 0) Debug.LogError("m_TimeSamples:" + m_TimeSamples);
                m_IsPlayingEvent?.Invoke(isPlaying);
            }
        }
        */
        int times = 0, f_times = 0;
        void Update() {//Fixed
            //Debug.LogWarning("Update:"+times++);
            //AudioUpdate();
        }
        private void FixedUpdate() {
            //Debug.LogWarning("FixedUpdate:" + f_times++);
            AudioUpdate();
        }
    }
}