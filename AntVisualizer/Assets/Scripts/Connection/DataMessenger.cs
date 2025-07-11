using System;
using System.Collections;
using Newtonsoft.Json;
using Server.Net.Models;
using UnityEngine;
using WebSocketSharp;

namespace Connection
{
    public class DataMessenger : MonoBehaviour
    {
        private WebSocket ws;

        private JsonSerializer _serializer;
        private void Start()
        {
            ws = new WebSocket("ws://37.48.249.190:8080/echo");

            ws.OnMessage += (sender, e) =>
            {
                Debug.Log("Received: " + e.Data);
            };

            ws.Connect();
            ws.Send("subscribe");
        }

        public void SendMessage(Message message)
        {
            var str = JsonConvert.SerializeObject(message);
            
            ws.Send(str);
        }

        private void OnReceiveMessage(string message)
        {
            var msg = JsonConvert.DeserializeObject<Message>(message);
        }

        private IEnumerator SendIntSequence(int max)
        {
            for (int i = 0; i < max; i++)
            {
                ws.Send(i.ToString());
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
    
}