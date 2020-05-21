using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_AudioListenerDebugGUI : UCL_AudioDebugGUI {
        public override void Init() {
            base.Init();
            m_Texture.m_WaveCol = Color.green;
        }
        public override void UpdateData() {
            base.UpdateData();
            AudioListener.GetOutputData(m_Data, 0);
        }
    }
}