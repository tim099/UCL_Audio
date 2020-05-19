using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_AudioSourceDebugGUI : MonoBehaviour {
#if UNITY_EDITOR
        public int m_DataLen = 2048;
        [SerializeField] AudioSource m_Source;

        Core.TextureLib.UCL_Texture2D m_PlayTexture;
        float[] m_Data;
        int[] m_DrawData;
        private void Awake() {
            if(m_Source == null) {
                m_Source = GetComponent<AudioSource>();
            }
            m_Data = new float[m_DataLen];
            m_PlayTexture = new UCL.Core.TextureLib.UCL_Texture2D(new Vector2Int(64, 64));
            m_DrawData = new int[m_PlayTexture.width];
        }

        private void Update() {
            if(m_Source == null) {
                m_Source = transform.GetComponentInChildren<AudioSource>();
                return;
            }
            int seg = m_DataLen / m_DrawData.Length;
            m_Source.GetOutputData(m_Data, 0);
            for(int i = 0; i < m_DrawData.Length; i++) {
                float val = 0;
                int end = i * (seg + 1);
                if(end > m_DataLen) end = m_DataLen;
                for(int j = i*seg ; j < end; j++) {
                    val += Mathf.Abs(m_Data[j]);
                }
                m_DrawData[i] = Mathf.RoundToInt((val* m_PlayTexture.height) /seg);
            }
            for(int j = 0; j < m_PlayTexture.width; j++) {
                for(int i = 0; i < m_PlayTexture.height; i++) {
                    m_PlayTexture.SetPixel(j, i, m_DrawData[j] < i ? Color.black : Color.yellow);
                }
            }
        }

        private void OnGUI() {

            GUILayout.BeginVertical();
            GUILayout.Space(128);
            

            GUILayout.Box(m_PlayTexture.texture);//GUILayout.Box(m_IsPlaying);//, GUILayout.Width(64), GUILayout.Height(64)
            GUILayout.EndVertical();
        }
#endif
    }
}

