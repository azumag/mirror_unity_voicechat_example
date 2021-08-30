using System;
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
    float[] audioFloat;
    int sampleRate = 3000;

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
        Debug.Log("test");
        // if (audioData != null) {
            // CmdSendAudio(audioData);
        // }
        ;
        if (audioFloat != null) {
            CmdSendAudio(audioFloat);
        }
    }

    void Update() {
        if (!isLocalPlayer) return;
        float[] floatData = new float[sampleRate];
        audioSource.clip.GetData(floatData, 0); 
        audioFloat = floatData;
        // float[] floatData = new float[audioSource.clip.samples * audioSource.clip.channels];
   //     Debug.Log("floatData;;;" + floatData.Length);
    }

    [Command(channel = Channels.Unreliable)]
    // private void CmdSendAudio(byte[] _data)
    private void CmdSendAudio(float[] _data)
    {
        Debug.Log("CmdSend");
        RpcSendAudio(_data);
	}

     // "includeOwner" player should apply their own data, and not wait to send and recieve it back
    // [ClientRpc(channel = Channels.Unreliable)]
    [ClientRpc(channel = Channels.Unreliable, includeOwner = false)]
    // private void RpcSendAudio(byte[] _data)
    private void RpcSendAudio(float[] _data)
    {
        // if (_data.Length > 0)  {
        //     // Debug.Log("RPCSend " + _data[0]);
        // }
        Debug.Log("RpcSend " + _data[0]);
        Debug.Log("RpcSend " + audioSourceEar.isPlaying);
        if (!audioSourceEar.isPlaying) {
            if (audioSourceEar.clip != null) {
                audioSourceEar.clip.SetData(_data, 0);
                audioSourceEar.Play();
            } else {
                audioSourceEar.clip = AudioClip.Create("ear", sampleRate, 1, sampleRate, false);
            }
        }
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
