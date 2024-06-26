using System;
using System.Collections.Generic;

namespace Cajero.ATM
{
    public class Message
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public TypeMessages Type { get; set; }

        public override string ToString() => $"Code: {ID}, Description: {Description}, Type: {Type}";
    }

    public static class MessageHelper
    {
        public static void AddMessage(this List<Message> messages, int ID, string Description, TypeMessages typeMe)
        {
            Message tmp = new Message
            {
                ID = ID,
                Description = Description,
                Type = typeMe
            };
            messages.Add(tmp);
        }
    }

    public enum TypeMessages
    {
        success = 0,
        warning = 1,
        info = 2,
        danger = 3,
        error = 999,
    }
}
