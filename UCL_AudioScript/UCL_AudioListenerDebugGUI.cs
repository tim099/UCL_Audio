using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_AudioListenerDebugGUI : MonoBehaviour {
#if UNITY_EDITOR
        public int m_DataLen = 2048;

        Core.TextureLib.UCL_AudioTexture2D m_PlayTexture;
        float[] m_Data;
        private void Awake() {
            m_Data = new float[m_DataLen];
            m_PlayTexture = new UCL.Core.TextureLib.UCL_AudioTexture2D(new Vector2Int(64, 64));
            m_PlayTexture.m_WaveCol = Color.green;
        }

        private void Update() {
            AudioListener.GetOutputData(m_Data, 0);
            m_PlayTexture.SetAudioData(m_Data);
            Core.DebugLib.UCL_DebugOnGUI.Instance.CreateData().SetOnGUIAct(delegate () {
                GUILayout.Box(m_PlayTexture.texture);
            });
        }

        private void OnGUI() {
            /*
            GUILayout.BeginVertical();
            GUILayout.Space(128);
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