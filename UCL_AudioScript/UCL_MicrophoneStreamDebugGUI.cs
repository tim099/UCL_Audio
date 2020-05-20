using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_MicrophoneStreamDebugGUI : MonoBehaviour {
#if UNITY_EDITOR
        public int m_DataLen = 2048;

        Core.TextureLib.UCL_AudioTexture2D m_PlayTexture;
        UCL_MicrophoneStream m_MicrophoneStream;
        float[] m_Data;
        private void Awake() {
            m_Data = new float[m_DataLen];
            m_PlayTexture = new UCL.Core.TextureLib.UCL_AudioTexture2D(new Vector2Int(64, 64));
            m_PlayTexture.m_WaveCol = Color.red;
            m_MicrophoneStream = GetComponent<UCL_MicrophoneStream>();
        }

        private void Update() {
            if(m_MicrophoneStream == null) return;

            m_MicrophoneStream.GetOutputData(m_Data);

            m_PlayTexture.SetAudioData(m_Data);
            Core.DebugLib.UCL_DebugOnGUI.Instance.CreateData().SetOnGUIAct(delegate () {
                //Debug.LogWarning("Draw UCL_MicrophoneStreamDebugGUI");
                GUILayout.Box(m_PlayTexture.texture);
            });
        }

        private void OnGUI() {
            /*
            GUILayout.BeginVertical();
            GUILayout.Space(256);
            GUILayout.BeginHorizontal();
            GUILayout.Space(128);

            GUILayout.Box(m_PlayTexture.texture);//GUILayout.Box(m_IsPlaying);//, GUILayout.Width(64), GUILayout.Height(64)
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            */
        }
#endif
    }
}