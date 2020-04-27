using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_MicrophoneStream : UCL_AudioStream {

        #region RequestUserAuthorization
        protected bool f_Authorization = false;
        void RequestUserAuthorization() {
#if UNITY_ANDROID
            ANDROID_Authorization();
#endif
#if UNITY_IPHONE
            IOS_Authorization();
#endif
#if UNITY_STANDALONE_WIN
            f_Authorization = true;
#endif
        }
#if UNITY_ANDROID
        void ANDROID_Authorization() {

            if(!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone)) {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
            }
            if(!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone)) {
                Debug.LogError("RequestUserAuthorization Fail!!");
                return;
            }
            f_Authorization = true;
    }
#endif
#if UNITY_IPHONE
        void IOS_Authorization() {
            StartCoroutine(IOS_RequestUserAuthorization());
        }
        IEnumerator IOS_RequestUserAuthorization() {
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if(!Application.HasUserAuthorization(UserAuthorization.Microphone)) {
                Debug.LogError("RequestUserAuthorization Fail!!");
            } else {
                f_Authorization = true;
            }
            
        }
#endif
        #endregion
        public uint m_DeviceID = 0;
        public bool m_Loop = true;
        public bool m_Recording = true;
        public int m_LengthSec = 4;
        /// <summary>
        /// Clip the audio which abs val smaller than m_ClippingThreshold
        /// </summary>
        public float m_ClippingThreshold = 0;
        /// <summary>
        /// Max m_AudioDatas count
        /// </summary>
        public int m_MaxBufferCount = 4;

        public string m_DeviceName { get; protected set; }
        public int m_ReadPosition =0;

        protected int m_ClippingTimer = 0;
        protected Queue<float[]> m_RecordQue;
        protected AudioClip m_Clip;

        private void OnEnable() {
            StartRecord();
        }
        private void OnDisable() {
            StopRecord();
        }
        private void OnDestroy() {
            StopRecord();
        }
        public override void Init(int Length) {
            base.Init(Length);
            RequestUserAuthorization();
            if(!f_Authorization) {
                Debug.LogError("Init UCL_MicrophoneStream Fail!!!f_Authorization");
                return;
            }
            if(Microphone.devices.Length <= m_DeviceID) {
                Debug.LogError("Microphone.devices.Length:" + Microphone.devices.Length + ",m_DiviceID:" + m_DeviceID);
                return;
            }
            StartRecord();
        }
        public void StartRecord() {
            if(!m_Recording) return;

            if(m_Clip != null) StopRecord();
            m_ReadPosition = 0;
            m_DeviceName = Microphone.devices[m_DeviceID];
            m_Clip = Microphone.Start(m_DeviceName, m_Loop, m_LengthSec, m_Frequency);
            if(m_RecordQue == null) {
                m_RecordQue = new Queue<float[]>();
            } else {
                m_RecordQue.Clear();
            }
            
        }
        public void StopRecord() {
            if(m_Clip == null) return;
            while(m_RecordQue.Count > 0) {
                Return(m_RecordQue.Dequeue());
            }
            Microphone.End(m_DeviceName);
            Destroy(m_Clip);
            m_Clip = null;
        }

        protected int GetPosition() {
            if(m_Clip == null) return 0;
            return Microphone.GetPosition(m_DeviceName);
        }
        /// <summary>
        /// require Init first to set length of array
        /// please call Dispose(float[] arr) after using the array!!
        /// </summary>
        /// <returns></returns>
        override public float[] Load() {
            if(m_Pool == null || m_Clip == null) return null;
            if(m_RecordQue.Count == 0) return null;

            return m_RecordQue.Dequeue();
        }

        virtual protected void RecordUpdate() {
            if(!m_Recording) {
                if(m_Clip != null) {
                    StopRecord();
                    return;
                }
            } else {
                if(m_Clip == null) {
                    StartRecord();
                    return;
                }
            }
            if(m_Pool == null || m_Clip == null) return;

            int Position = GetPosition();
            int del = Position - m_ReadPosition;
            if(del < 0) {//loop!!
                del = (m_Clip.samples - m_ReadPosition) + Position;
            }
            
            if(del > m_Length) {
                
                float[] data = Rent();
                m_Clip.GetData(data, m_ReadPosition);
                bool skip = false;
                if(m_ClippingThreshold > 0) {
                    float max = 0;
                    for(int i = 0; i < data.Length; i++) {
                        max = Mathf.Max(max, Mathf.Abs(data[i]));
                    }
                    
                    if(max < m_ClippingThreshold) {//Skip
                        if(m_ClippingTimer > 2) {
                            //Debug.LogWarning("Skip Max:" + max);
                            skip = true;
                        } else {
                            //Debug.LogWarning("Max:" + max + ",m_ClippingTimer:"+ m_ClippingTimer);
                            m_ClippingTimer++;
                        }
                    } else {
                        m_ClippingTimer = 0;
                    }
                }

                if(!skip) {
                    if(m_RecordQue.Count >= m_MaxBufferCount) {
                        Return(m_RecordQue.Dequeue());
                    }
                    m_RecordQue.Enqueue(data);
                } else {
                    Return(data);
                }

                m_ReadPosition += m_Length;
                if(m_ReadPosition >= m_Clip.samples) {
                    m_ReadPosition -= m_Clip.samples;
                }
                //Debug.LogWarning("Pos:" + Position+",Record!!:"+m_RecordQue.Count);
            }
        }
        private void Update() {
            RecordUpdate();
        }
    }
}