using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_AudioStream : MonoBehaviour {
        public int m_Frequency = 8000;//{ get; protected set; }
        public int m_Channels = 1;// { get; protected set; } = 1;
        protected int m_Length = 1024;
        protected UCL.Core.Container.UCL_ArrayPoolFixed<float> m_Pool = null;
        virtual public void Init(int Length) {
            m_Length = Length;
            m_Pool = new UCL.Core.Container.UCL_ArrayPoolFixed<float>(m_Length);
        }
        /// <summary>
        /// Load data from AudioStream
        /// return null if no data avaliable
        /// </summary>
        /// <returns></returns>
        virtual public float[] Load() {
            return null;
        }
        virtual public float[] Rent() {
            if(m_Pool == null) return null;
            return m_Pool.Rent();
        }

        /// <summary>
        /// Return array create by LoadFromArrayPool()!!
        /// </summary>
        /// <param name="arr"></param>
        public void Return(float[] arr) {
            if(m_Pool == null) return;
            m_Pool.Return(arr);
        }
    }
}