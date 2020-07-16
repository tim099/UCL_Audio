using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UCL.AudioLib {
    public class UCL_AudioDebugGUI : MonoBehaviour {
        public int m_DataLen = 2048;
        public Core.TextureLib.UCL_AudioTexture2D m_Texture;
        public Vector2Int m_TextureSize = new Vector2Int(64, 64);

        protected float[] m_Data;
        virtual protected void Start() {
            m_Data = new float[m_DataLen];
            m_Texture = new UCL.Core.TextureLib.UCL_AudioTexture2D(m_TextureSize);
            Init();
        }
        virtual public void Init() { }
        virtual public void UpdateData() { }
        virtual public void DrawDebugGUI() {
            UpdateData();
            m_Texture.SetAudioData(m_Data);
            Core.DebugLib.UCL_DebugOnGUI.Instance.CreateData().AddOnGUIAct(() => {
                GUILayout.BeginVertical();
                string target_name = this.GetType().UnderlyingSystemType.Name.Replace("UCL_", "");
                GUILayout.Box(target_name.Replace("DebugGUI",""),GUILayout.Width(m_TextureSize.x+4));
                GUILayout.Box(name, GUILayout.Width(m_TextureSize.x+4));
                GUILayout.Box(m_Texture.texture);
                GUILayout.EndVertical();
            });
        }
        virtual protected void Update() {
            DrawDebugGUI();
        }
    }
}