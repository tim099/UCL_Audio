using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace UCL.AudioLib {
    public class UCL_PCMConverter {
        protected int m_Length;
        protected Core.MarshalLib.UCL_IntPtr m_Ptr;
        protected short[] m_Buffer;
        public UCL_PCMConverter(int _len) {
            m_Length = _len;
            m_Ptr = new Core.MarshalLib.UCL_IntPtr(sizeof(short) * m_Length);
            m_Buffer = new short[m_Length];
        }
        ~UCL_PCMConverter() {
            m_Ptr.Free();
        }
        public IntPtr ToPtr(float[] data) {
            UCL.AudioLib.Lib.PCM_FloatToShort(data, m_Buffer);
            m_Ptr.ToPtr(m_Buffer);
            return m_Ptr.m_Ptr;
        }
        public void ToPCM(IntPtr ptr, float[] arr) {
            Marshal.Copy(ptr, m_Buffer, 0, m_Buffer.Length);
            UCL.AudioLib.Lib.PCM_ShortToFloat(m_Buffer, arr);
        }
    }
}