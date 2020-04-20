using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    static public class Lib {
        /// <summary>
        /// Convert PCM Byte to Float
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public static void PCM_ShortToFloat(short[] src, float[] dst) {
            if(src == null || dst == null) return;
            int len = src.Length;
            if(len > dst.Length) len = dst.Length;

            for(int i = 0; i < len; i++) {
                dst[i] = (float)src[i] / 32768;//short.MaxValue
            }
        }
        /// <summary>
        /// Convert Float to PCM Byte 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public static void PCM_FloatToShort(float[] src, short[] dst) {
            if(src == null || dst == null) return;
            int len = src.Length;
            if(len > dst.Length) len = dst.Length;

            for(int i = 0; i < len; i++) {
                int val = Mathf.RoundToInt(src[i] * short.MaxValue);
                if(val > short.MaxValue) val = short.MaxValue;
                else if(val < short.MinValue) val = short.MinValue;
                dst[i] = (short)val;
            }
        }
    }
}