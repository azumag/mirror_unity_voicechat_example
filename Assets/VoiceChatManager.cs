using System;
using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class VoiceChatManager : NetworkBehaviour
{
    AudioSource audioSource;
    public AudioSource audioSourceEar;
    // float[] audioData;
    byte[] audioData;
    // float[] audioFloat;
    int sampleRate = 22500;

    // Start is called before the first frame update
    void Start()
    {
        if (!isLocalPlayer) return;
        foreach (var device in Microphone.devices) {
            Debug.Log("Name: " + device);
        }
        audioSourceEar.clip = AudioClip.Create("ear", sampleRate, 1, sampleRate, false);
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = Microphone.Start("USB MICROPHONE", true, 1, sampleRate);
        while (!(Microphone.GetPosition("") > 0)) {}
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (!audioSource.isPlaying) {
            // audioSource.Play();
        }
        if (audioData != null) {
            CmdSendAudio(audioData);
        }
        // if (audioFloat != null) {
        //     CmdSendAudio(audioFloat);
        // }
    }

    void Update() {
        if (!isLocalPlayer) return;
        float[] floatData = new float[sampleRate];
        audioSource.clip.GetData(floatData, 0); 

        // convert to byte array
        byte[] byteData = new byte[floatData.Length * 4];
        Buffer.BlockCopy(floatData, 0, byteData, 0, byteData.Length);
        audioData = byteData;

        // audioFloat = floatData;
        // float[] floatData = new float[audioSource.clip.samples * audioSource.clip.channels];
   //     Debug.Log("floatData;;;" + floatData.Length);
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendAudio(byte[] _data)
    // private void CmdSendAudio(float[] _data)
    {
        byte[] compressedData = Compress(_data);
        Debug.Log("CmdSend: " + compressedData.Length);
        RpcSendAudio(compressedData);
	}

     // "includeOwner" player should apply their own data, and not wait to send and recieve it back
    [ClientRpc(channel = Channels.Unreliable)]
    // [ClientRpc(channel = Channels.Unreliable, includeOwner = false)]
    private void RpcSendAudio(byte[] _data)
    // private void RpcSendAudio(float[] _data)
    {
        // if (_data.Length > 0)  {
        //     // Debug.Log("RPCSend " + _data[0]);
        // }
        // Debug.Log("RpcSend " + _data[0]);
        Debug.Log("RpcSend " + audioSourceEar.isPlaying);
        if (!audioSourceEar.isPlaying) {
            if (audioSourceEar.clip != null) {
                float[] floatData = ToFloatArray(Decompress(_data));
                audioSourceEar.clip.SetData(floatData, 0);
                audioSourceEar.Play();
            } else {
				audioSourceEar.clip = AudioClip.Create("ear", sampleRate, 1, sampleRate, false);
            }
        }
    }

    public static byte[] Compress(byte[] src)
    {
        using (var ms = new MemoryStream())
        {
            using (var ds = new DeflateStream(ms, CompressionMode.Compress, true/*msは*/))
            {
                ds.Write(src, 0, src.Length);
            }

            // 圧縮した内容をbyte配列にして取り出す
            ms.Position = 0;
            byte[] comp = new byte[ms.Length];
            ms.Read(comp, 0, comp.Length);
            return comp;
        }
    }

    public static byte[] Decompress(byte[] src)
    {
        using (var ms = new MemoryStream(src))
        using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
        {
            using (var dest = new MemoryStream())
            {
                ds.CopyTo(dest);

                dest.Position = 0;
                byte[] decomp = new byte[dest.Length];
                dest.Read(decomp, 0, decomp.Length);
                return decomp;
            }
        }
    }

    public float[] ToFloatArray(byte[] byteArray) {
        int len = byteArray.Length / 4;
        float[] floatArray = new float[len];
        for (int i = 0; i < byteArray.Length; i+=4) {
            floatArray[i/4] = System.BitConverter.ToSingle(byteArray, i);
        }
        return floatArray;
    }

    // void OnAudioFilterRead(float[] data, int channels) {
    //     if (!isLocalPlayer) return;
    //     Debug.Log("datalen " + data.Length);
    //     // if (data.Length > 0)  {
    //     //     Debug.Log(data[0]);
    //     // }

    //     // convert to byte array
    //     // byte[] byteData = new byte[data.Length * 4];
        
    //     // byte[] byteData = new byte[100];
    //     // Buffer.BlockCopy(data, 0, byteData, 0, 100);
    //     // audioData = byteData;

    //     // float[] audioFloatTmp = new float[100];
    //     // Array.Copy(data, 0, audioFloatTmp, 0, 100);
    //     // Debug.Log("data; "+data[0]);
    //     // Debug.Log("float; "+audioFloatTmp[0]);
    //     // audioFloat = audioFloatTmp;
    // }
}
