﻿using System;
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
        public override float ClipTime => (float)DataSize / (float)ByteRate;
        
        public int[] Datas;

        System.Text.ASCIIEncoding s_Encoder = new System.Text.ASCIIEncoding();
        int m_ReadAt = 0;
        byte[] m_Wav = null;

        string ReadString(int iReadCount)
        {
            string aResult = null;
            try
            {
                aResult = s_Encoder.GetString(m_Wav, m_ReadAt, iReadCount);
            }catch(System.Exception iE)
            {
                Debug.LogException(iE);
                Debug.LogError("ReadString m_ReadAt:" + m_ReadAt + ",iReadCount:" + iReadCount + ",m_Wav:" + m_Wav.Length);
            }
            
            m_ReadAt += iReadCount;
            return aResult;
        }
        int ReadInt()
        {
            int aResult = System.BitConverter.ToInt32(m_Wav, m_ReadAt);
            m_ReadAt += 4;
            return aResult;
        }
        long ReadLong()
        {
            long aResult = System.BitConverter.ToInt64(m_Wav, m_ReadAt);
            m_ReadAt += 8;
            return aResult;
        }
        short ReadShort()
        {
            short aResult = System.BitConverter.ToInt16(m_Wav, m_ReadAt);
            m_ReadAt += 2;
            return aResult;
        }
        /// <summary>
        /// List type INFO
        /// Info ID (4 byte ASCII text) for information 1
        /// Size of text 1
        /// Text 1
        /// Info ID (4 byte ASCII text) for information 2
        /// Size of text 2
        /// Text 2
        /// </summary>
        private void LoadListChunk()
        {
            string aInfoID = ReadString(4);

            int aTextSize = ReadInt();
            string aInfo = ReadString(aTextSize - 2);
            Debug.LogError("ListChunk aInfoID:" + aInfoID+ ",aTextSize:"+ aTextSize+ ",aInfo:\""+ aInfo+"\"");
        }
        private void LoadChunk()//System.IO.FileStream fs
        {
            
            string aChunkID = ReadString(4);
            Debug.LogError("sChunkID:" + aChunkID);

            switch (aChunkID)
            {
                case "RIFF"://ChunkID "RIFF"
                    {
                        FileSize = ReadInt();
                        Format = ReadString(4);
                        //Debug.LogError("FileSize:" + FileSize + ",Format:" + Format);
                        break;
                    }
                case "fmt "://ChunkID "fmt "
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
                case "LIST"://ref https://www.recordingblogs.com/wiki/list-chunk-of-a-wave-file
                    {
                        
                        int aSize = ReadInt();//The size of the sub-chunk less 8 (less the "chunk ID" and the "size")
                        int aAt = m_ReadAt;
                        int aListTypeID = ReadInt();//Various ASCII character strings. A common one is "INFO"
                        Debug.LogError("LIST aSize:" + aSize + ",aListTypeID:" + aListTypeID+ ",aAt:"+ aAt);
                        try
                        {
                            while (m_ReadAt - aAt < aSize)
                            {
                                LoadListChunk();
                                Debug.LogError("LIST aSize:" + aSize + ",m_ReadAt:" + m_ReadAt + ",aAt:" + aAt);
                            }
                        }catch(System.Exception iE)
                        {
                            Debug.LogException(iE);
                        }

                        m_ReadAt = aAt + aSize;
                        break;
                    }
                case "data"://ChunkID "data"
                    {
                        DataSize = ReadInt();

                        SamplesCount = (int)DataSize / ChannelsCount;// * BytesPerSample
                        Debug.LogError("data DataSize:" + DataSize + ",ChannelsCount:"+ ChannelsCount + ",SamplesCount:" + SamplesCount);

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
            int aLoadChunkTimes = 0;
            const int MaxLoadChunkTimes = 1000;
            while (m_ReadAt < iWav.Length)
            {
                try
                {
                    LoadChunk();
                }catch(System.Exception iE)
                {
                    Debug.LogException(iE);
                    return;
                }
                if (++aLoadChunkTimes > MaxLoadChunkTimes)
                {
                    Debug.LogError("UCL_Wav aLoadChunkTimes > MaxLoadChunkTimes!! aLoadChunkTimes:" + aLoadChunkTimes);
                    break;
                }
            }
            //LoadChunk(0);
            //LoadChunk(1);
            //LoadChunk(2);
        }
        public UCL_Wav(byte[] iWav, string iName = "New Wav")
        {
            Init(iWav, iName);
        }
    }

}