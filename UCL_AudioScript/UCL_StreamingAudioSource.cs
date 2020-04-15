using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_StreamingAudioSource : MonoBehaviour {
        AudioSource m_Source;
        UCL_StreamingAudioClip m_StreamingAudioClip;
        public bool m_Playing = false;

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

            m_StreamingAudioClip = gameObject.AddComponent<UCL_StreamingAudioClip>();
            m_StreamingAudioClip.Init();

            m_Source = gameObject.AddComponent<AudioSource>();
            m_Source.clip = m_StreamingAudioClip.m_Clip;
            m_Source.loop = false;
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
            if(m_Playing) {
                if(!m_Source.isPlaying) {
                    if(m_StreamingAudioClip.LoadData()) {
                        m_Source.Play();
                    }
                }
            }
        }
    }
}