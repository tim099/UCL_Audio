using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_MicrophoneStream : UCL_AudioStream {
        public class RecordData {
            public RecordData(UCL_MicrophoneStream _parent) {
                m_Parent = _parent;
            }
            public RecordData SetData(float[] _data) {
                m_Data = _data;
                return this;
            }
            public RecordData SetAudioListenerData(float[] _data) {
                m_AudioListenerData = _data;
                return this;
            }
            public void Dispose() {
                if(m_Data != null) m_Parent.Return(m_Data);
                if(m_AudioListenerData != null) m_Parent.Return(m_AudioListenerData);
            }
            public void Dispose(float[] _data) {
                Dispose();
            }
            public float[] m_Data;
            //this will be null if not enable RecordAudioListenerData
            public float[] m_AudioListenerData;

            UCL_MicrophoneStream m_Parent;
        }
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
        [UCL.Core.PA.UCL_StrList(typeof(Microphone), "devices")] public string m_DeviceName = "";//{ get; protected set; }

        /// <summary>
        /// m_DeviceID only work when m_DeviceName cant be find in Microphone.devices
        /// </summary>
        [Header(@"Only work when DeviceName can't be find in Microphone.devices")]
        public uint m_DeviceID = 0;

        public bool m_Loop = true;
        public bool m_Recording = true;
        public bool m_RecordAudioListenerData = false;
        public int m_LengthSec = 4;
        //[Range(1.0f,20.0f)] public float m_Test;
        [Range(0,128)] public int m_EchoCancellationOffSet;//[UCL.Core.PA.UCL_IntSlider(0, 128)] 
        /// <summary>
        /// Max record time in milisecond
        /// </summary>
        [Header("Max record time in milisecond")]
        public int m_MaxRecordTime = 128;


        #region Clipping
        [Header("Clipping Setting")]
        /// <summary>
        /// if audio sample abs value > m_ClippingThreshold then valid_count++
        /// </summary>
        public float m_ClippingThreshold = 0;
        /// <summary>
        /// if valid_count less than m_ClippingValidCount
        /// than clip the sample
        /// </summary>
        public int m_ClippingValidCount = 4;
        /// <summary>
        /// if unvalid count more then MinValidSegCount then start clipping
        /// </summary>
        public int m_MinValidSegCount = 2;
        //[Space(20)]
        #endregion



        #region ReadOnly

        [UCL.Core.PA.UCL_ReadOnly] public int m_ReadPosition = 0;
        /// <summary>
        /// interval of each audio frame in milisecond
        /// </summary>
        [UCL.Core.PA.UCL_ReadOnly] public int m_FrameTime;

        /// <summary>
        /// Max m_AudioDatas count
        /// </summary>
        [UCL.Core.PA.UCL_ReadOnly] public int m_MaxBufferCount = 4;

        #endregion
        protected int m_MicPosition = 0;
        protected int m_ClippingTimer = 0;
        protected Queue<RecordData> m_RecordQue;
        protected AudioClip m_Clip;

        protected float[] m_AudioListenerData;

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
        protected void UpdateReadOnlyData() {
            m_FrameTime = Mathf.RoundToInt((1000.0f * m_Length) / m_Frequency);
            m_MaxBufferCount = Mathf.CeilToInt((float)m_MaxRecordTime / m_FrameTime);
        }
        public void StartRecord() {
            if(!m_Recording) return;

            if(m_Clip != null) StopRecord();
            UpdateReadOnlyData();

            if(!string.IsNullOrEmpty(m_DeviceName)) {
                for(uint i = 0,len = (uint)Microphone.devices.Length ; i < len; i++) {
                    if(m_DeviceName == Microphone.devices[i]) {
                        m_DeviceID = i;
                        //Debug.LogWarning("Device find:" + Microphone.devices[i]+",at:"+i);
                        break;
                    }
                }
            }
            m_DeviceName = Microphone.devices[m_DeviceID];
            m_Clip = Microphone.Start(m_DeviceName, m_Loop, m_LengthSec, m_Frequency);
            if(m_RecordQue == null) {
                m_RecordQue = new Queue<RecordData>();
            } else {
                ClearRecordData();
            }
            m_AudioListenerData = new float[m_Length * (m_MaxBufferCount+1)];
            m_ReadPosition = 0;//m_Clip.samples - m_Frequency/2;//skip 0.5 sec
        }
        void ClearRecordData() {
            if(m_RecordQue == null) return;
            while(m_RecordQue.Count > 0) {
                m_RecordQue.Dequeue().Dispose();
            }
        }
        public void StopRecord() {
            if(m_Clip == null) return;
            ClearRecordData();
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

            return m_RecordQue.Dequeue().m_Data;
        }

        /// <summary>
        /// Please call Dispose after using the RecordData
        /// </summary>
        /// <returns></returns>
        virtual public RecordData LoadData() {
            if(m_Pool == null || m_Clip == null) return null;
            if(m_RecordQue.Count == 0) return null;
            return m_RecordQue.Dequeue();
        }

        //float timer = 0;
        virtual protected bool RecordUpdate() {
            if(!m_Recording) {
                if(m_Clip != null) {
                    StopRecord();
                    return false;
                }
            } else {
                if(m_Clip == null) {
                    StartRecord();
                    return false;
                }
            }
            if(m_Pool == null || m_Clip == null) return false;
            m_ReadTimes = 0;

            if(m_RecordAudioListenerData) {
                AudioListener.GetOutputData(m_AudioListenerData,0);
            }

            m_MicPosition = GetPosition();
            //Debug.LogWarning("m_MicPosition:" + m_MicPosition);
            int i = 0;
            bool flag = false;
            while(ReadRecordData() && ++i < 100) {
                flag = true;
            }
            return flag;
        }
        public void GetOutputData(float[] data) {
            if(m_Clip == null) return;
            int pos = GetPosition() - data.Length;
            if(pos < 0) {
                pos += m_Clip.samples;
            }
            m_Clip.GetData(data, pos);
        }
        /// <summary>
        /// ReadRecordData times in this Fixed Update
        /// </summary>
        protected int m_ReadTimes = 0;
        protected bool ReadRecordData() {
            ++m_ReadTimes;

            int del = m_MicPosition - m_ReadPosition;
            if(del < 0) {//loop!!
                del = (m_Clip.samples - m_ReadPosition) + m_MicPosition;
            }
            //timer += Time.deltaTime;
            if(del > m_Length) {
                //Debug.LogWarning("Record Time:" + timer);
                //timer = 0;
                float[] data = Rent();
                m_Clip.GetData(data, m_ReadPosition);
                #region check clipping
                bool skip = false;
                if(m_ClippingThreshold > 0) {
                    int valid_count = 0;
                    if(m_ClippingValidCount > data.Length) m_ClippingValidCount = data.Length;

                    for(int i = 0; i < data.Length; i++) {
                        if(Mathf.Abs(data[i]) > m_ClippingThreshold) {
                            valid_count++;
                        }
                    }

                    if(valid_count < m_ClippingValidCount) {//Skip
                        if(m_ClippingTimer > m_MinValidSegCount) {
                            //Debug.LogWarning("Skip sample_times:" + sample_times);
                            skip = true;
                        } else {
                            //Debug.LogWarning("Max:" + max + ",m_ClippingTimer:"+ m_ClippingTimer);
                            m_ClippingTimer++;
                        }
                    } else {
                        m_ClippingTimer = 0;
                    }
                }
                #endregion

                if(!skip) {
                    int pos = m_AudioListenerData.Length - del;
                    if(m_RecordQue.Count < m_MaxBufferCount &&(!m_RecordAudioListenerData || pos >= 0)) {
                        var record_data = new RecordData(this).SetData(data);
                        if(m_RecordAudioListenerData) {
                            var lis_data = Rent();
                            //m_AudioListenerData.Length - m_ReadTimes * lis_data.Length;
                            /*
                            if(pos < 0) {
                                Debug.LogWarning("m_AudioListenerData.Length:" + m_AudioListenerData.Length + ",m_ReadTimes:" +
                                    (m_ReadTimes) + ",lis_data.Length:" + lis_data.Length + ",pos:" + pos + ",del:"+ del);
                                pos = 0;
                            }
                            */
                            int len = lis_data.Length;
                            if(pos + len > m_AudioListenerData.Length) {
                                len = m_AudioListenerData.Length - pos;
                            }
                            System.Array.Copy(m_AudioListenerData, pos, lis_data, 0, len);
                            record_data.m_AudioListenerData = lis_data;
                        }
                        m_RecordQue.Enqueue(record_data);
                    }
                } else {
                    Return(data);
                }

                m_ReadPosition += m_Length;
                if(m_ReadPosition >= m_Clip.samples) {
                    m_ReadPosition -= m_Clip.samples;
                }
                return true;
                //Debug.LogWarning("Pos:" + Position+",Record!!:"+m_RecordQue.Count);
            }
            return false;
        }
        private void Update() {
            //RecordUpdate();
        }
        private void FixedUpdate() {
            RecordUpdate();
        }
#if UNITY_EDITOR
        private void OnValidate() {
            UpdateReadOnlyData();
        }
#endif
    }
}