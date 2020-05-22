using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_AudioStreamDebugGUI : UCL_AudioDebugGUI {
        [SerializeField] protected UCL_AudioStream m_AudioStream;
        public override void Init() {
            base.Init();
            m_Texture.m_WaveCol = Color.blue;
            m_AudioStream = GetComponentInChildren<UCL_AudioStream>();
        }
        public override void UpdateData() {
            base.UpdateData();
            //AudioListener.GetOutputData(m_Data, 0);
        }
    }
}