using System;

namespace NurseServer.Models
{
    public class PatientBed
    {
        public string MacAddress { get; set; }
        public string RoomName { get; set; }
        public string BedName { get; set; }
        public string IpAddress { get; set; }
        public string Status { get; set; } // "Normal", "Offline", "CẤP CỨU", "THAY DỊCH TRUYỀN", "HỖ TRỢ VỆ SINH"
        public DateTime LastSeen { get; set; }
    }
}
