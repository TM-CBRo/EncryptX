using System;

namespace EncryptX.Entities
{
    public class EncryptionLog
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FileName { get; set; }  // Numele fisierului sau "Text"
        public string ActionType { get; set; } // "ENCRYPT" sau "DECRYPT"
        public DateTime Timestamp { get; set; }
    }
}