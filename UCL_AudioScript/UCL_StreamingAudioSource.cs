using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_StreamingAudioSource : MonoBehaviour {
        public AudioSource m_Source { get; protected set; }
        public bool m_Playing { get; protected set; } = false;
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
        }
        public UCL_StreamingAudioSource AddData(float[] data) {
            if(m_StreamingAudioClip == null) return this;
            m_StreamingAudioClip.AddData(data);
            return this;
        }
        public int GetDataCount() {
            if(m_StreamingAudioClip == null) return 0;
            return m_StreamingAudioClip.GetDataCount();
        }
        // Update is called once per frame
        void Update() {
            if(!f_Inited) return;
            if(m_StreamingAudioClip == null) return;
            if(m_Playing) {
                int length_samples = m_StreamingAudioClip.m_LengthSamples;
                //int channels = m_StreamingAudioClip.m_Channels;
                int sample_at = m_Source.timeSamples;
                if(!m_Source.isPlaying || sample_at >= length_samples) {
                    //Debug.LogWarning("m_Source.timeSamples:" + m_Source.timeSamples + ",m_Source.clip.samples:" + m_Source.clip.samples);
                    if(m_StreamingAudioClip.LoadData()) {
                        
                        if(!m_Source.isPlaying) {
                            //Debug.LogWarning("StartPlay!!");
                            m_Source.Play();
                            m_Source.timeSamples = length_samples;
                        } else {//sample_at >= m_LengthSamples
                            //Debug.LogWarning("ContinuePlay!!");
                            m_Source.timeSamples = sample_at - length_samples;
                        }
                    }
                }
            }
        }
    }
}