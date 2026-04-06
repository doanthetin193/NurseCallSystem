using System;

namespace NurseServer.Models
{
    public class CallLog
    {
        public int Id { get; set; }
        public string PatientBedName { get; set; }
        public string CallType { get; set; }
        public DateTime RequestTime { get; set; }
        public DateTime? ResolvedTime { get; set; }
    }
}
