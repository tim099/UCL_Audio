﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_AudioSourceDebugGUI : MonoBehaviour {
#if UNITY_EDITOR
        public int m_DataLen = 2048;
        public Core.TextureLib.UCL_AudioTexture2D m_PlayTexture;
        public Vector2Int m_TextureSize = new Vector2Int(64, 64);
        [SerializeField] AudioSource m_Source;

        float[] m_Data;
        private void Awake() {
            if(m_Source == null) {
                m_Source = GetComponent<AudioSource>();
            }
            m_Data = new float[m_DataLen];
            m_PlayTexture = new UCL.Core.TextureLib.UCL_AudioTexture2D(m_TextureSize);
        }

        private void Update() {
            if(m_Source == null) {
                m_Source = transform.GetComponentInChildren<AudioSource>();
                return;
            }

            m_Source.GetOutputData(m_Data, 0);
            m_PlayTexture.SetAudioData(m_Data);
            Core.DebugLib.UCL_DebugOnGUI.Instance.CreateData().SetOnGUIAct(() => {
                GUILayout.Box(m_PlayTexture.texture);
            });
        }

        private void OnGUI() {
            /*
            GUILayout.BeginVertical();
            GUILayout.Space(128);
            

            GUILayout.Box(m_PlayTexture.texture);//GUILayout.Box(m_IsPlaying);//, GUILayout.Width(64), GUILayout.Height(64)
            GUILayout.EndVertical();
            */
        }
#endif
    }
}

