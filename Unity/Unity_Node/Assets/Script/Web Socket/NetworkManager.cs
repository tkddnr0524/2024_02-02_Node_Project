using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;                              //WebSocket 활용
using TMPro;
using Newtonsoft.Json;
using UnityEngine.UI;
using System;

//메세지 타입 정의 
[Serializable]
public class NetworkMessage
{
    public string type;
    public string playerId;
    public string message;
    public Vector3Data position;
}

//Vecotr3 직렬화를 위한 클래스
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

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;  // Inspector에서 플레이어 프리팹 할당용
    private string myPlayerId;                         // 내 플레이어 ID 저장
    private GameObject myPlayer;                       // 내 플레이어 객체
    private Dictionary<string, GameObject> otherPlayers = new Dictionary<string, GameObject>();  // 다른 플레이어들 관리

    private float syncInterval = 0.1f;    // 위치 동기화 주기
    private float syncTimer = 0f;         // 타이머

    // Start is called before the first frame update
    void Start()
    {
        // 서버 연결 시작
        ConnectToServer();

        //// UI 이벤트 연결
        //messageInput.onEndEdit.AddListener((message) => {
        //    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        //    {
        //        SendChatMessage();
        //    }
        //});

        //SendButton.onClick.AddListener(SendChatMessage);
    }

    // Update is called once per frame
    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (webSocket != null)
        {
            webSocket.DispatchMessageQueue();
        }
#endif
        // 위치 동기화 타이머
        if (myPlayer != null)
        {
            syncTimer += Time.deltaTime;
            if (syncTimer >= syncInterval)
            {
                SendPositionUpdate();
                syncTimer = 0f;
            }
        }
    }

    private async void ConnectToServer()
    {
        webSocket = new WebSocket(serverUrl);

        webSocket.OnOpen += () =>
        {
            Debug.Log("연결 성공!");
            UpdateStatusText("연결됨", Color.green);
        };

        webSocket.OnError += (e) =>
        {
            Debug.Log($"에러 : {e}");
            UpdateStatusText("에러 발생", Color.red);
        };

        webSocket.OnClose += (e) =>
        {
            Debug.Log("연결 종료");
            UpdateStatusText("연결 끊김", Color.red);
        };

        webSocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            HandleMessage(message);
        };
        await webSocket.Connect();
    }

    private void HandleMessage(string json)
    {
        try
        {
            Debug.Log("받은 메시지: " + json);  // 추가: 받은 메시지 출력

            NetworkMessage message = JsonConvert.DeserializeObject<NetworkMessage>(json);
            Debug.Log("메시지 타입: " + message.type);  // 추가: 메시지 타입 출력




            switch (message.type)
            {
                case "connection":
                    HandleConnection(message);  // 이제 이 메서드를 새로 만들어야 함
                    break;

                case "chat":
                    AddToChatLog(message.message);
                    break;

                // 새로 추가하는 케이스들
                case "playerPosition":
                    UpdatePlayerPosition(message);  // 새로 만들 메서드
                    break;

                case "playerDisconnect":
                    RemovePlayer(message.playerId); // 새로 만들 메서드
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"메세지 처리 중 에러 : {e.Message}");
        }
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
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            await webSocket.Close();
        }
    }

    // 연결 처리 메서드
    private void HandleConnection(NetworkMessage message)
    {
        myPlayerId = message.playerId;
        AddToChatLog($"서버에 연결됨 (ID: {myPlayerId})");

        // 플레이어 생성
        Vector3 spawnPosition = new Vector3(0, 1, 0);
        myPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        myPlayer.name = $"Player_{myPlayerId}";

        // 내 플레이어 설정
        PlayerController controller = myPlayer.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.SetAsMyPlayer();
        }
    }

    // 위치 전송 메서드
    private async void SendPositionUpdate()
    {
        if (webSocket.State == WebSocketState.Open && myPlayer != null)
        {
            NetworkMessage message = new NetworkMessage
            {
                type = "playerPosition",
                playerId = myPlayerId,
                position = new Vector3Data(myPlayer.transform.position)
            };

            await webSocket.SendText(JsonConvert.SerializeObject(message));
        }
    }

    // 다른 플레이어 위치 업데이트 메서드
    private void UpdatePlayerPosition(NetworkMessage message)
    {
        if (message.playerId == myPlayerId) return; // 자신의 위치는 무시

        if (!otherPlayers.ContainsKey(message.playerId))
        {
            // 새로운 플레이어 생성
            GameObject newPlayer = Instantiate(playerPrefab);
            newPlayer.name = $"Player_{message.playerId}";
            otherPlayers.Add(message.playerId, newPlayer);
        }

        // 위치 업데이트
        otherPlayers[message.playerId].transform.position = message.position.ToVector3();
    }

    // 플레이어 제거 메서드
    private void RemovePlayer(string playerId)
    {
        if (otherPlayers.ContainsKey(playerId))
        {
            Destroy(otherPlayers[playerId]);
            otherPlayers.Remove(playerId);
            AddToChatLog($"플레이어 {playerId} 퇴장");
        }
    }
}