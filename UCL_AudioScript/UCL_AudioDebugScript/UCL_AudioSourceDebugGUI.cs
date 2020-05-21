using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_AudioSourceDebugGUI : UCL_AudioDebugGUI {
        [SerializeField] AudioSource m_Source;
        public override void Init() {
            base.Init();
            if(m_Source == null) {
                m_Source = GetComponent<AudioSource>();
            }
        }
        public override void UpdateData() {
            base.UpdateData();
            if(m_Source == null) {
                m_Source = transform.GetComponentInChildren<AudioSource>();
                return;
            }
            m_Source.GetOutputData(m_Data, 0);
        }
    }
}

