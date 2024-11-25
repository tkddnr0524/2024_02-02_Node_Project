using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;              //WebSocket Ȱ��
using TMPro;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;


//�޼��� Ÿ�� ����
[Serializable]
public class NetworkMessage
{
    public string type;
    public string playerId;
    public string message;
    public Vector3Data position;
}
//Vector3 ����ȭ�� ���� Ŭ����
[Serializable]
public class Vector3Data
{
    public float x;
    public float y;
    public float z;

    public Vector3Data(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}
public class NetworkManager : MonoBehaviour
{

    private WebSocket webSocket;
    [SerializeField] private string serverUrl = "ws://localhost:3000";

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button SendButton;
    [SerializeField] private TextMeshProUGUI chatLog;
    [SerializeField] private TextMeshProUGUI statusText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleMessage(string json)
    {
        try
        {
            NetworkMessage message = JsonConvert.DeserializeObject<NetworkMessage>(json);

            switch (message.type)
            {
                case "connettion":
                    break;
                case "chat":
                    AddToChatLog(message.message);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"�޼��� ó�� �� ���� : {e.Message}");   
        }
    }

    private async void ConnetToServer()
    {
        webSocket = new WebSocket(serverUrl);

        webSocket.OnOpen += () =>
        {
            Debug.Log("���� ����!");
            UpdateStatusText("�����", Color.green);
        };

        webSocket.OnError += (e) =>
        {
            Debug.Log($"���� : {e}");
            UpdateStatusText("���� �߻�", Color.red);
        };

        webSocket.OnClose += (e) =>
        {
            Debug.Log("���� ����");
            UpdateStatusText("���� ����", Color.red);
        };

        webSocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            HandleMessage(message);
        };

        await webSocket.Connect();
    }

    private async void SendChatMessage()
    {
        if (string.IsNullOrEmpty(messageInput.text)) return;

        if (webSocket.State == WebSocketState.Open)
        {
            NetworkMessage message = new NetworkMessage
            {
                type = "chat",
                message = messageInput.text
            };

            await webSocket.SendText(JsonConvert.SerializeObject(message));
            messageInput.text = "";
        }
    }
    private void UpdateStatusText(string status, Color color)
    {
        if (statusText != null)
        {
            statusText.text = status;
            statusText.color = color;   
        }
    }
    
    private void AddToChatLog(string message)
    {
        if (chatLog != null)
        {
            chatLog.text += $"\n{message}";
        }
    }

    private async void OnApplicationQuit()
    {
        if(webSocket != null && webSocket.State == WebSocketState.Open)
        {
            await webSocket.Close();
        }
    }
}
