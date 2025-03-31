using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityWebSocket;
using static loadAudio;

public class loadAudio : MonoBehaviour
{
    //¼����ť-����
    bool micConnected = false;//�Ƿ�����˷�
    AudioSource speakSource;//��ȡ¼��
    WebSocket webSocket;

    int last_length;
    float[] volumeData;
    short[] intData;
    bool isRunning;
    string txt;

    void Start()
    {
        //��ʼʱ�ȼ����û����˷�
        if (Microphone.devices.Length <= 0)
        {
            Debug.LogWarning("û����˷��豸");
        }
        else
        {
            micConnected = true; //�Ƿ�����˷�
            Debug.Log(Microphone.devices[0]);
            speakSource = GetComponent<AudioSource>();//���
        }
        last_length = -1;
        volumeData = new float[9999];
        intData = new short[9999];
        isRunning = false;
    }

    void Update()
    {
        if (isRunning)
        {
            byte[] voiceData = GetVoiveData();
            if (voiceData != null)
            {
                if (webSocket != null && webSocket.ReadyState == WebSocketState.Open)
                {
                    webSocket.SendAsync(voiceData);
                }
            }

        }
    }

    public void ButtonOnPress()
    {
        //�����ʱ
        Debug.Log("��ס��ť");
        if (micConnected)
        {
            speakSource.Stop();
            if (!Microphone.IsRecording(null))
            {
                ////����˷�û����¼��
                //speakSource.clip = Microphone.Start(null, true, 60, 16000);//��ʼ¼��
                //while (!(Microphone.GetPosition(null) > 0))
                //{
                //}
                if (webSocket != null && webSocket.ReadyState == WebSocketState.Open)
                {
                    Debug.LogWarning("��ʼ����ʶ��ʧ�ܣ����ȴ��ϴ�ʶ�����ӽ���");
                    return;
                }
                speakSource.clip = Microphone.Start(null, false, 60, 16000);
                isRunning = true;
                txt = "";
                Connect();
            }
        }
    }

    public void ButtonUp()
    {
        //����ť���ɿ�ʱ
        Debug.Log("̧��");
        isRunning = false;
        if (!Microphone.IsRecording(null))
            return;
        Microphone.End(null); //ֹͣ��˷�
        speakSource.mute = false;
        if (webSocket != null)
        {
            string endMsg = "{\"end\": true}";
            byte[] data = Encoding.UTF8.GetBytes(endMsg);
            webSocket.SendAsync(data);
        }
    }

    void Connect()
    {
        webSocket = new WebSocket(GetWebSocketUrl());

        webSocket.OnOpen += (sender, e) =>{
            Debug.Log("����ʶ�����ӳɹ�");
            webSocket.OnClose += Socket_OnClose;
        };
        webSocket.OnMessage += Socket_OnMessage;
        webSocket.OnError += Socket_OnError;
        webSocket.ConnectAsync();
        //StartCoroutine(SendVoiceData());
    }

    void Socket_OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log("����ʶ�����ӹر�");
    }

    void Socket_OnMessage(object sender, MessageEventArgs e)
    {
        Debug.Log("����ʶ�����ݷ��أ�" + e.Data);
        if(e.IsText)
        {
            JObject js = JObject.Parse(e.Data);
            if (js["code"].ToString() == "0" && js["data"].ToString()!="")
            {
                var result = getResult(js["data"].ToString());
                if (result.IsFinal)
                {
                    txt += result.Text;
                    Debug.Log("Text����:" + txt);
                    UItext.Instance.inputField.text = txt;
                }
                else
                {
                    Debug.Log("Text�м�:" + result.Text);
                    UItext.Instance.inputField.text = txt + result.Text;
                }
            }
        }
    }

    void Socket_OnError(object sender, ErrorEventArgs e)
    {
        Debug.LogError("����ʶ��������:" + e.Message);
    }

    byte[] GetVoiveData()
    {
        if (speakSource.clip == null)
        {
            return null;
        }
        int new_length = Microphone.GetPosition(null);
        if (new_length == last_length)
        {
            if (Microphone.devices.Length == 0)
            {
                isRunning = false;
            }
            return null;
        }
        int length = new_length - last_length;
        int offset = last_length + 1;
        last_length = new_length;
        if (offset < 0)
        {
            return null;
        }
        if (length < 0)
        {
            float[] temp = new float[speakSource.clip.samples];
            speakSource.clip.GetData(temp, 0);
            int lengthTail = speakSource.clip.samples - offset;
            int lengthHead = new_length + 1;
            try
            {
                Array.Copy(temp, offset, volumeData, 0, lengthTail);
                Array.Copy(temp, 0, volumeData, lengthTail + 1, lengthHead);
                length = lengthTail + lengthHead;

            }
            catch (Exception)
            {
                return null;
            }
        }
        else
        {
            if (length > volumeData.Length)
            {
                volumeData = new float[length];
                intData = new short[length];
            }
            speakSource.clip.GetData(volumeData, offset);
        }
        byte[] bytesData = new byte[length * 2];
        int rescaleFactor = 32767; //to convert float to Int16
        for (int i = 0; i < length; i++)
        {
            intData[i] = (short)(volumeData[i] * rescaleFactor);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }
        return bytesData;
    }

    Result getResult(string dataJson)
    {
        StringBuilder builder = new StringBuilder();
        Result res = new Result();
        try
        {
            JsonData data = JsonMapper.ToObject(dataJson);
            JsonData cn = data["cn"];
            JsonData st = cn["st"];
            if (st["ed"].ToString().Equals("0"))
            {
                res.IsFinal = false;
            }
            else
            {
                res.IsFinal = true;
            }
            JsonData rtArry = st["rt"];
            foreach (JsonData rtObject in rtArry)
            {
                JsonData wsArr = rtObject["ws"];
                foreach (JsonData wsObject in wsArr)
                {
                    JsonData cwArr = wsObject["cw"];
                    foreach (JsonData cwObject in cwArr)
                    {
                        builder.Append(cwObject["w"].ToString());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        res.Text = builder.ToString();
        return res;
    }

    string GetWebSocketUrl()
    {
        string appid = "a16f9685";
        string ts = GetCurrentUnixTimestampMillis().ToString();
        string baseString = appid + ts;
        string md5 = GetMD5Hash(baseString);
        UnityEngine.Debug.Log("baseString:" + baseString + ",md5:" + md5);
        string sha1 = CalculateHmacSha1(md5, "33b18c9d7b9835f0be77871df04f65b1");
        string signa = sha1;
        string url = string.Format("wss://rtasr.xfyun.cn/v1/ws?appid={0}&ts={1}&signa={2}", appid, ts, signa);
        return url;
    }

    long GetCurrentUnixTimestampMillis()
    {
        DateTime unixStartTime = new DateTime(1970, 1, 1).ToLocalTime();
        DateTime now = DateTime.Now;// DateTime.UtcNow;
        TimeSpan timeSpan = now - unixStartTime;
        long timestamp = (long)timeSpan.TotalSeconds;
        return timestamp;
    }

    string GetMD5Hash(string input)
    {
        MD5 md5Hasher = MD5.Create();
        byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
        StringBuilder sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }

    string CalculateHmacSha1(string data, string key)
    {
        HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key));
        byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hashBytes);
    }

    public class Result
    {
        public string Text { get; set; }
        public bool IsFinal { get; set; }
    }
}
