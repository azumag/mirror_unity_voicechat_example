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
    byte[] audioData;
    int sampleRate = 20000;
    public int micSamplePacketSize = 8350;
    int lastSample;

    void Start()
    {
        if (!isLocalPlayer) return;
        foreach (var device in Microphone.devices) {
            Debug.Log("Name: " + device);
        }
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = Microphone.Start("USB MICROPHONE", true, 120, sampleRate);
        while (Microphone.GetPosition(null) < 0) { }
    }


    void Update() {
        if (!isLocalPlayer) return;

        int pos = Microphone.GetPosition(null);
        int diff = pos - lastSample;

        if (diff >= micSamplePacketSize) {
            float[] floatData = new float[diff];
            audioSource.clip.GetData(floatData, lastSample); 

            // convert to byte array
            byte[] byteData = new byte[floatData.Length * 4];
            Buffer.BlockCopy(floatData, 0, byteData, 0, byteData.Length);
            CmdSendAudio(byteData);
            lastSample = pos;
        }
    }

    [Command(channel = Channels.Unreliable)]
    private void CmdSendAudio(byte[] _data)
    {
        Debug.Log("CmdSend:src " + _data.Length);
        byte[] compressedData = Compress(_data);
        Debug.Log("CmdSend:dst " + compressedData.Length);
        RpcSendAudio(compressedData);
	}

    // [ClientRpc(channel = Channels.Unreliable)]
    [ClientRpc(channel = Channels.Unreliable, includeOwner = false)]
    private void RpcSendAudio(byte[] _data)
    {
        Debug.Log("RpcSend " + audioSourceEar.isPlaying);
        float[] floatData = ToFloatArray(Decompress(_data));
        AudioClip ac = AudioClip.Create("ear", floatData.Length, 1, sampleRate, false);
        ac.SetData(floatData, 0);
        audioSourceEar.clip = ac;
        if (!audioSourceEar.isPlaying) {
            audioSourceEar.Play();
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
    // }
}
