﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UCL.AudioLib {
    public class UCL_StreamingAudioSource : MonoBehaviour {
        [System.Serializable]
        public class IsPlayingEvent : UnityEngine.Events.UnityEvent<bool> {}

        public AudioSource m_Source { get; protected set; }
        public bool m_Playing { get; protected set; } = false;
        public UnityEngine.Events.UnityEvent m_OnPlayEvent;
        public UnityEngine.Events.UnityEvent m_OnStopEvent;
        public IsPlayingEvent m_IsPlayingEvent;
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
            m_Source.loop = false;
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
            m_StreamingAudioClip.AddData(data, dispose_act);
            return this;
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

        void FixedUpdate() {
            if(!f_Inited) return;
            if(m_StreamingAudioClip == null) return;
            if(m_Playing) {
                int length_samples = m_StreamingAudioClip.m_LengthSamples;
                //int channels = m_StreamingAudioClip.m_Channels;
                //Debug.Log("m_Source.timeSamples:" + m_Source.timeSamples);
                int sample_at = m_Source.timeSamples;
                if(!m_Source.isPlaying) {
                    sample_at = m_StreamingAudioClip.m_LengthSamples * m_StreamingAudioClip.m_BufferCount;
                    if(m_StreamingAudioClip.LoadData(ref sample_at)) {
                        //Debug.LogWarning("StartPlay!!");
                        m_Source.Play();
                        m_OnPlayEvent?.Invoke();
                        m_Source.timeSamples = sample_at;// 0;//length_samples;/2
                    }
                } else {//Playing!!
                    if(m_StreamingAudioClip.LoadData(ref sample_at)) {
                        //Debug.LogWarning("ContinuePlay!!:" + m_Source.timeSamples);
                        m_Source.timeSamples = sample_at;//sample_at - length_samples;
                    }

                }

                m_IsPlayingEvent?.Invoke(isPlaying);
            }
        }
    }
}