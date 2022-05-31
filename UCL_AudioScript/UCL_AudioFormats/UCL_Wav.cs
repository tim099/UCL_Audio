using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCL.AudioLib
{
    /// <summary>
    /// reference http://soundfile.sapp.org/doc/WaveFormat/
    /// </summary>
    public class UCL_Wav : UCL_AudioClip
    {
        /// <summary>
        /// 36 + SubChunk2Size, or more precisely:
        /// 4 + (8 + SubChunk1Size) + (8 + SubChunk2Size)
        /// This is the size of the rest of the chunk
        /// following this number.This is the size of the
        /// entire file in bytes minus 8 bytes for the
        /// two fields not included in this count:
        /// ChunkID and ChunkSize.
        /// </summary>
        public int FileSize { get; internal set; }


        /// <summary>
        /// Contains the letters "WAVE"
        /// (0x57415645 big-endian form).
        /// </summary>
        public string Format { get; internal set; }
       
        /// <summary>
        /// PCM = 1 (i.e. Linear quantization)
        /// Values other than 1 indicate some form of compression.
        /// </summary>
        public int AudioFormat { get; internal set; }
        /// <summary>
        /// SampleRate * NumChannels * BitsPerSample/8
        /// </summary>
        public int ByteRate { get; internal set; }
        /// <summary>
        /// NumChannels * BitsPerSample/8
        /// The number of bytes for one sample including all channels.
        /// </summary>
        public int BlockAlign { get; internal set; }
        /// <summary>
        /// 8 bits = 8, 16 bits = 16, etc.
        /// </summary>
        public int BitsPerSample { get; internal set; }

        /// <summary>
        /// NumSamples * NumChannels * BitsPerSample/8
        /// This is the number of bytes in the data.
        /// You can also think of this as the size
        /// of the read of the subchunk following thisnumber.
        /// </summary>
        public int DataSize { get; internal set; }


        /// <summary>
        /// Total play time in seconds
        /// </summary>
        public override float ClipTime => DataSize / (float)ByteRate;
        
        public int[] Datas;

        System.Text.ASCIIEncoding s_Encoder = new System.Text.ASCIIEncoding();
        int m_ReadAt = 0;
        byte[] m_Wav = null;

        string ReadString(int iReadCount)
        {
            string aResult = s_Encoder.GetString(m_Wav, m_ReadAt, iReadCount);
            m_ReadAt += iReadCount;
            return aResult;
        }
        int ReadInt()
        {
            int aResult = System.BitConverter.ToInt32(m_Wav, m_ReadAt);
            m_ReadAt += 4;
            return aResult;
        }
        short ReadShort()
        {
            short aResult = System.BitConverter.ToInt16(m_Wav, m_ReadAt);
            m_ReadAt += 2;
            return aResult;
        }
        float ReadFloatShort()
        {
            return ReadShort() / 32768.0f;//short.MaxValue + 1;
        }
        float ReadFloat()
        {
            return ReadInt() / 2147483648f;// int.MaxValue;+ 1;
        }
        private void LoadChunk(int iChunckID = 0)//System.IO.FileStream fs
        {
            
            string sChunkID = ReadString(4);
            //Debug.LogError("sChunkID:" + sChunkID);

            switch (iChunckID)
            {
                case 0://ChunkID "RIFF"
                    {
                        FileSize = ReadInt();
                        Format = ReadString(4);
                        //Debug.LogError("FileSize:" + FileSize + ",Format:" + Format);
                        break;
                    }
                case 1://ChunkID "fmt "
                    {
                        var aFMTChunkSize = ReadInt();
                        int aChunckStartAt = m_ReadAt;
                        AudioFormat = ReadShort();
                        ChannelsCount = ReadShort();

                        Frequency = ReadInt();

                        ByteRate = ReadInt();
                        BlockAlign = ReadShort();
                        BitsPerSample = ReadShort();
                        //Debug.LogError("FMTChunkSize:" + aFMTChunkSize + ",AudioFormat:" + AudioFormat + ",ChannelCount:" + ChannelsCount);
                        //Debug.LogError("Frequency:" + Frequency + ",ByteRate:" + ByteRate + ",BlockAlign:" + BlockAlign + ",BitsPerSample:" + BitsPerSample);

                        m_ReadAt = aChunckStartAt + aFMTChunkSize;//Set position to Chunck End
                        break;
                    }
                case 2://ChunkID "data"
                    {
                        DataSize = ReadInt();
                        SamplesCount = DataSize / ChannelsCount;// * BytesPerSample
                        //Debug.LogError("data DataSize:" + DataSize + ",SamplesCount:" + SamplesCount);
                        //aDataLen = SampleCount * ChannelsCount;
                        Datas = new int[SamplesCount];
                        m_ChannelDatas = new float[SamplesCount];
                        switch (BitsPerSample)
                        {
                            case 8:
                                {
                                    for (int i = 0; i < SamplesCount; i++)
                                    {
                                        Datas[i] = m_Wav[m_ReadAt++];
                                        m_ChannelDatas[i] = Datas[i] / 256f;
                                    }
                                    break;
                                }
                            case 16:
                                {
                                    for (int i = 0; i < SamplesCount; i++)
                                    {
                                        Datas[i] = ReadShort();
                                        m_ChannelDatas[i] = Datas[i] / 32768.0f;
                                    }
                                    break;
                                }
                            case 32:
                                {
                                    for (int i = 0; i < SamplesCount; i++)
                                    {
                                        Datas[i] = ReadInt();
                                        m_ChannelDatas[i] = Datas[i] / 2147483648f;
                                    }
                                    break;
                                }
                            default:
                                {
                                    Debug.LogError("ClipName:" + ClipName + "BitsPerSample:" + BitsPerSample + ", Not support yet!!");
                                    break;
                                }
                        }
                        break;
                    }
            }
        }
        public void Init(byte[] iWav, string iName = "New Wav")
        {
            m_Wav = iWav;
            m_ReadAt = 0;
            ClipName = iName;

            LoadChunk(0);
            LoadChunk(1);
            LoadChunk(2);

            




            
            //if(BytesPerSample == 2)
            //{
            //    for (int i = 0; i < aDataLen; i++)
            //    {
            //        ChannelDatas[i] = ReadFloatShort();
            //        //if (aDataLen - i < 30)
            //        //{
            //        //    //Debug.LogError(i.ToString()+",ChannelDatas[i]:" + ChannelDatas[i]);
            //        //}
            //    }
            //}
            //else if(BytesPerSample == 4)
            //{
            //    for (int i = 0; i < aDataLen; i++)
            //    {
            //        ChannelDatas[i] = ReadFloat();
            //    }
            //}
            //else
            //{
            //    Debug.LogError("Wave File" + iName + "BytesPerSample:" + BytesPerSample + " !!");
            //}
        }
        public UCL_Wav(byte[] iWav, string iName = "New Wav")
        {
            Init(iWav, iName);
        }
    }

}
