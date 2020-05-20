using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_StreamingAudioPlayer : MonoBehaviour {
        public int m_BufferLen = 4096;
        
        public UCL_AudioStream m_UCL_AudioStream;
        public int m_MaxDataCount = 2;
        public bool m_InitOnStart = true;
        /// <summary>
        /// Stop playing when m_Pause == true
        /// </summary>
        public bool m_Pause = false;
        public UCL_StreamingAudioSource m_UCL_StreamingAudioSource;
        protected int m_DataSize;
        protected bool f_Inited = false;
        virtual public void Init() {
            if(f_Inited) return;
            f_Inited = true;

            m_UCL_AudioStream.Init(m_BufferLen);
            m_DataSize = m_UCL_AudioStream.m_Channels * m_BufferLen;
            if(m_UCL_StreamingAudioSource == null) {
                m_UCL_StreamingAudioSource = UCL_StreamingAudioSource.Create(transform);
            }


            m_UCL_StreamingAudioSource.GetClip()
                .SetFrequency(m_UCL_AudioStream.m_Frequency)
                .SetLengthSamples(m_BufferLen)
                .SetChannels(m_UCL_AudioStream.m_Channels);

            m_UCL_StreamingAudioSource.Init();
            m_UCL_StreamingAudioSource.Play();
        }
        // Start is called before the first frame update
        void Start() {
            if(m_InitOnStart) Init();
        }


        void FixedUpdate() {
            if(m_Pause) return;
            while(m_UCL_StreamingAudioSource.GetDataCount() < m_MaxDataCount) {
                var data = m_UCL_AudioStream.Load();
                if(data != null) {
                    m_UCL_StreamingAudioSource.AddData(data, m_UCL_AudioStream.Return);
                } else {
                    break;
                }

            }
        }
    }
}