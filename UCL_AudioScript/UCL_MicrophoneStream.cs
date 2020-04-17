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
        public int m_LengthSec = 4;
        public string m_DeviceName { get; protected set; }
        public AudioSource m_AudioSource;
        //public int m_Position = 0;
        public int m_ReadPosition =0;
        protected Queue<float[]> m_RecordQue;
        protected AudioClip m_Clip;
        public override void Init(int Length) {
            Debug.Log("Init UCL_MicrophoneStream");
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
            m_DeviceName = Microphone.devices[m_DeviceID];
            m_Clip = Microphone.Start(m_DeviceName, m_Loop, m_LengthSec, m_Frequency);
            if(m_AudioSource) {
                m_AudioSource.clip = m_Clip;
                m_AudioSource.loop = true;
                m_AudioSource.Play();
                Debug.Log("Set UCL_MicrophoneStream m_AudioSource!!");
            }
            m_RecordQue = new Queue<float[]>();
            Debug.Log("Init UCL_MicrophoneStream End!!");
        }
        public void Stop() {
            if(m_Clip == null) return;

            Microphone.End(m_DeviceName);
            //Destroy(m_Clip);
            //m_Clip = null;
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
            if(m_Pool == null || m_Clip == null) return;
            int Position = GetPosition();
            int del = Position - m_ReadPosition;
            if(del < 0) {//loop!!
                del = (m_Clip.samples - m_ReadPosition) + Position;
            }
            
            if(del > m_Length) {
                
                float[] data = Rent();
                m_Clip.GetData(data, m_ReadPosition);
                m_RecordQue.Enqueue(data);

                m_ReadPosition += m_Length;
                if(m_ReadPosition > m_Clip.samples) {
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