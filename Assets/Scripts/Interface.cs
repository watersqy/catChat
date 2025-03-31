using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityWebSocket;
using UnityEngine.UI;
using System.Dynamic;

//�Ի���¼��ʽ
public class Content
{
    public string role { get; set; }
    public string content { get; set; }
}

//�ӿڿ���
public class Interface : MonoBehaviour
{
    public static Interface Instance;
    
    static public AudioSource audioSource;

    public InputField inputField;// �ı������
    public Button sendBtn;//���Ͱ�ť
    public GameObject blackCat;
    public GameObject whiteCat;
    
    
    static List<Content> black;//��è�����¼
    static List<Content> white;//��è�����¼

    const string appId = "a16f9685";// Ӧ��APPID
    static float Volume;//����

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Screen.fullScreen = false;
        audioSource = GetComponent<AudioSource>();
        Volume = 50;
        sendBtn.onClick.AddListener(ClickSendBtn);
        white = new List<Content> { new Content() { role = "system", content = "������С�ף���һֻ�ɰ��İ�è" } };
        black = new List<Content> { new Content() { role = "system", content = "������С�ڣ���һֻ��è" } };
    }

    //������Ϣ
    void ClickSendBtn()
    {
        if (whiteCat.activeInHierarchy)
        {
            Debug.Log("С��");
            white.Add(new Content() { role = "user", content = inputField.text });
            UItext.Instance.CreateQuestion(inputField.text);
            SparkMax s = new SparkMax();
            s.Send(white, "x4_lingxiaolu_em_v2");
        }
        else if (blackCat.activeInHierarchy)
        {
            Debug.Log("С��");
            black.Add(new Content() { role = "user", content = inputField.text });
            UItext.Instance.CreateQuestion(inputField.text);
            SparkMax s = new SparkMax();
            s.Send(black, "x4_lingfeichen_emo");
        }
    }

    //����ӿ�

    public void Speech(string Vcn, string Txt)
    {
        Volume = menu.Instance.volumeSlider.value;
        MakeAudio m = new MakeAudio();
        m.Send(Vcn, Txt);
    }

    //�ǻ��ģ��
    private class SparkMax{
        static ClientWebSocket webSocket;
        static CancellationToken cancellation;
        
        // �ӿ���Կ
        const string apiSecret = "YWJkYmMzODcyY2ZjMDBlNGM1MDIxOTcw";
        const string apiKey = "9a3f45477943e6137af483723203a91c";

        static string hostUrl = "wss://spark-api.xf-yun.com/v3.5/chat";

        async public void Send(List<Content> history, string vcn)
        {
            string authUrl = GetAuthUrl(hostUrl, apiSecret, apiKey);
            string url = authUrl.Replace("http://", "ws://").Replace("https://", "wss://");
            using (webSocket = new ClientWebSocket())
            {
                try
                {
                    await webSocket.ConnectAsync(new Uri(url), cancellation);
                    Debug.Log("chat���ӳɹ�");
                    JsonRequest request = new JsonRequest();
                    request.header = new Header()
                    {
                        app_id = appId,
                        uid = "12345"
                    };
                    request.parameter = new Parameter()
                    {
                        chat = new Chat()
                        {
                            domain = "generalv3.5",//ģ������Ĭ��Ϊ�ǻ�ͨ�ô�ģ��
                            temperature = 0.5,//�¶Ȳ�����ֵ�����ڿ����������ݵ�����ԺͶ����ԣ�ֵԽ�������Խ�ߣ���Χ��0��1��
                            max_tokens = 1024,//�������ݵ���󳤶ȣ���Χ��0��4096��
                        }
                    };
                    request.payload = new Payload()
                    {
                        message = new Message()
                        {
                            text = history
                        }
                    };
                    string jsonString = JsonConvert.SerializeObject(request);
                    
                    //���ӳɹ�����ʼ��������
                    var frameData2 = System.Text.Encoding.UTF8.GetBytes(jsonString.ToString());
                    await webSocket.SendAsync(new ArraySegment<byte>(frameData2), WebSocketMessageType.Text, true, cancellation);

                    // ������ʽ���ؽ�����н���
                    byte[] receiveBuffer = new byte[1024];
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), cancellation);
                    String resp = "";
                    while (!result.CloseStatus.HasValue)
                    {
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
                            //���������Ϊjson
                            JObject jsonObj = JObject.Parse(receivedMessage);
                            int code = (int)jsonObj["header"]["code"];


                            if (0 == code)
                            {
                                int status = (int)jsonObj["payload"]["choices"]["status"];


                                JArray textArray = (JArray)jsonObj["payload"]["choices"]["text"];
                                string content = (string)textArray[0]["content"];
                                resp += content;

                                if (status == 2)
                                {
                                    Debug.Log($"chat���һ֡�� {receivedMessage}");
                                    int totalTokens = (int)jsonObj["payload"]["usage"]["text"]["total_tokens"];
                                    UItext.Instance.CreateAnswer(resp);
                                    history.Add(new Content { role = "assistant", content = resp });
                                    Instance.Speech(vcn, resp);
                                    break;
                                }
                            }
                            else
                            {
                                Debug.LogError($"chat���󱨴� {receivedMessage}");
                            }
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Console.WriteLine("�ѹر�chat����");
                            break;
                        }
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), cancellation);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public class JsonRequest
        {
            public Header header { get; set; }
            public Parameter parameter { get; set; }
            public Payload payload { get; set; }
        }

        public class Header
        {
            public string app_id { get; set; }
            public string uid { get; set; }
        }

        public class Parameter
        {
            public Chat chat { get; set; }
        }

        public class Chat
        {
            public string domain { get; set; }
            public double temperature { get; set; }
            public int max_tokens { get; set; }
        }

        public class Payload
        {
            public Message message { get; set; }
        }

        public class Message
        {
            public List<Content> text { get; set; }
        }
    }

    //�����ϳ�
    private class MakeAudio
    {
        UnityWebSocket.WebSocket webSocket;

        // �ӿ���Կ
        const string apiSecret = "YWJkYmMzODcyY2ZjMDBlNGM1MDIxOTcw";
        const string apiKey = "9a3f45477943e6137af483723203a91c";
        static string hostUrl = "wss://tts-api.xfyun.cn/v2/tts";

        List<float> AudionData = new List<float>();//��������
        string vcn;
        string text;

        public void Send(string Vcn, string Txt)
        {
            AudionData.Clear();
            vcn = Vcn;
            text = Txt;

            string authUrl = GetAuthUrl(hostUrl, apiSecret, apiKey);
            string url = authUrl.Replace("http://", "ws://").Replace("https://", "wss://");
            webSocket = new UnityWebSocket.WebSocket(url);
            webSocket.OnOpen += Socket_OnOpen;
            webSocket.OnMessage += Socket_OnMessage;
            webSocket.OnError += Socket_OnError;
            webSocket.OnClose += Socket_OnClose;
            Connect();
        }

        void Connect()
        {
            if (webSocket.ReadyState != UnityWebSocket.WebSocketState.Open)
            {
                webSocket.ConnectAsync();
            }
        }

        void Socket_OnOpen(object sender, OpenEventArgs e)
        {
            JsonRequest request = new JsonRequest();
            request.common = new Common()
            {
                app_id = appId
            };
            request.business = new Business()
            {
                aue = "raw",
                vcn = vcn,
                volume = Volume,
                tte = "UTF8"
            };
            request.data = new Data()
            {
                status = 2,
                text = Convert.ToBase64String(Encoding.UTF8.GetBytes(text))
            };
            string jsonString = JsonConvert.SerializeObject(request);
            Debug.Log(jsonString);
            webSocket.SendAsync(jsonString);
            Debug.Log("�����ϳ����ӳɹ���");
        }

        void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            //Debug.Log(e.Data);
            if (e.IsText)
            {
                JObject js = JObject.Parse(e.Data);
                if (js["message"].ToString() == "success")
                {
                    if (js["data"] != null)
                    {
                        if (js["data"]["audio"] != null)
                        {
                            string audioData = js["data"]["audio"].ToString();
                            byte[] audioByteData = Convert.FromBase64String(audioData);
                            float[] fs = bytesToFloat(audioByteData);
                            foreach (float f in fs)
                                AudionData.Add(f);

                            if ((int)js["data"]["status"] == 2) //2Ϊ������־��
                            {
                                audioSource.Stop();
                                webSocket.CloseAsync();//�ر�
                                audioSource.clip = AudioClip.Create("video", AudionData.Count, 1, 16000, false);
                                audioSource.clip.SetData(AudionData.ToArray(), 0);
                                audioSource.Play();
                                
                                AnimControll.Instance.SpeechAnim(audioSource.clip.length);
                            }
                        }
                    }
                }
            }
        }

        void Socket_OnClose(object sender, CloseEventArgs e)
        {
            Debug.Log($"�����ϳ����ӹرգ�{e.StatusCode}, {e.Reason}");
        }

        void Socket_OnError(object sender, ErrorEventArgs e)
        {
            Debug.LogError($"�����ϳɴ�����Ϣ�� {e.Message}");
        }

        //byte[]ת16���Ƹ�ʽstring
        static string ToHexString(byte[] bytes)
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();
                foreach (byte b in bytes)
                {
                    strB.AppendFormat("{0:x2}", b);
                }
                hexString = strB.ToString();
            }
            return hexString;
        }

        //����
        static string EncodeBase64(string code_type, string code)
        {
            string encode = "";
            byte[] bytes = Encoding.GetEncoding(code_type).GetBytes(code);
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = code;
            }
            return encode;
        }

        //byte[]����ת��ΪAudioClip�ɶ�ȡ��float[]����
        static float[] bytesToFloat(byte[] byteArray)
        {
            float[] sounddata = new float[byteArray.Length / 2];
            for (int i = 0; i < sounddata.Length; i++)
            {
                sounddata[i] = bytesToFloat(byteArray[i * 2], byteArray[i * 2 + 1]);
            }
            return sounddata;
        }
        static float bytesToFloat(byte firstByte, byte secondByte)
        {
            // convert two bytes to one short (little endian)
            //С�˺ʹ��˳��Ҫ����
            short s;
            if (BitConverter.IsLittleEndian)
                s = (short)((secondByte << 8) | firstByte);
            else
                s = (short)((firstByte << 8) | secondByte);
            // convert to range from -1 to (just below) 1
            return s / 32768.0F;
        }

        public class JsonRequest
        {
            public Common common { get; set; }
            public Business business { get; set; }
            public Data data { get; set; }
        }

        public class Common
        {
            public string app_id { get; set; }
        }

        public class Business
        {
            public string aue { get; set; }
            public string vcn { get; set; }
            public float volume { get; set; }
            public string tte { get; set; }
        }

        public class Data
        {
            public int status { get; set; }
            public string text { get; set; }
        }
    }

    //�����㷨HmacSHA256
    static string HMACsha256(string apiSecretIsKey, string buider)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(apiSecretIsKey);
        System.Security.Cryptography.HMACSHA256 hMACSHA256 = new System.Security.Cryptography.HMACSHA256(bytes);
        byte[] date = System.Text.Encoding.UTF8.GetBytes(buider);
        date = hMACSHA256.ComputeHash(date);
        hMACSHA256.Clear();

        return Convert.ToBase64String(date);
    }

    //��ȨURL
    static string GetAuthUrl(string hostUrl, string api_secret, string api_key)
    {
        string date = DateTime.UtcNow.ToString("r");

        Uri uri = new Uri(hostUrl);
        StringBuilder builder = new StringBuilder("host: ").Append(uri.Host).Append("\n").//
                                Append("date: ").Append(date).Append("\n").//
                                Append("GET ").Append(uri.LocalPath).Append(" HTTP/1.1");

        string sha = HMACsha256(api_secret, builder.ToString());
        string authorization = string.Format("api_key=\"{0}\", algorithm=\"{1}\", headers=\"{2}\", signature=\"{3}\"", api_key, "hmac-sha256", "host date request-line", sha);
        //System.Web.HttpUtility.UrlEncode

        string NewUrl = "https://" + uri.Host + uri.LocalPath;

        string path1 = "authorization" + "=" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(authorization));
        date = date.Replace(" ", "%20").Replace(":", "%3A").Replace(",", "%2C");
        string path2 = "date" + "=" + date;
        string path3 = "host" + "=" + uri.Host;

        NewUrl = NewUrl + "?" + path1 + "&" + path2 + "&" + path3;
        return NewUrl;
    }
}
