using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib {
    public class UCL_AudioFilter : MonoBehaviour {
        virtual public bool Read(float[] data) { return false; }
        virtual public void Write(float[] data) { }
    }
}