using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_MicrophoneStreamDebugGUI : UCL_AudioDebugGUI {
        [SerializeField] protected UCL_MicrophoneStream m_MicrophoneStream;
        public override void Init() {
            base.Init();
            m_Texture.m_WaveCol = Color.red;
            if(m_MicrophoneStream == null) m_MicrophoneStream = GetComponent<UCL_MicrophoneStream>();
        }
        public override void UpdateData() {
            if(m_MicrophoneStream == null) return;

            base.UpdateData();
            m_MicrophoneStream.GetOutputData(m_Data);
        }
    }
}