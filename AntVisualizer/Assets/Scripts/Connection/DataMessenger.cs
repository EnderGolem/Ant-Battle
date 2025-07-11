using System;
using UnityEngine;
using WebSocketSharp;

namespace Connection
{
    public class DataMessenger : MonoBehaviour
    {
        private WebSocket ws;
        private void Start()
        {
            ws = new WebSocket("ws://localhost:8080");

            ws.OnMessage += (sender, e) =>
            {
                Debug.Log("Received: " + e.Data);
            };

            ws.Connect();
            ws.Send("Hello from Unity!");
        }

        private void OnReceiveMessage(string message)
        {
            
        }
    }

    public class VisMessage
    {
        public int msgCode;

        public string serializedValue;
    }
}