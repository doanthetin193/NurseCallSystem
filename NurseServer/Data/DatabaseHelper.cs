using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using NurseServer.Models;

namespace NurseServer.Data
{
    public static class DatabaseHelper
    {
        private const string DbFileName = "NurseCallSystem.sqlite";

        private static string GetConnectionString() => $"Data Source={DbFileName};Version=3;";

        // Chương 7: Lock object dùng chung — bảo vệ các thao tác GHI DB khỏi race condition
        // khi IcmpHealthCheckLoop (Thread nền) và HandleTcpClientAsync (nhiều Task song song) cùng ghi đồng thời
        private static readonly object _dbLock = new object();

        public static void InitDatabase()
        {
            if (!File.Exists(DbFileName)) SQLiteConnection.CreateFile(DbFileName);

            using (var conn = new SQLiteConnection(GetConnectionString()))
            {
                conn.Open();
                
                string createBedTable = @"
                    CREATE TABLE IF NOT EXISTS PatientBeds (
                        RoomName TEXT,
                        BedName TEXT,
                        MacAddress TEXT,
                        IpAddress TEXT,
                        Status TEXT,
                        LastSeen DATETIME,
                        PRIMARY KEY (RoomName, BedName)
                    );";
                using (var cmd = new SQLiteCommand(createBedTable, conn)) { cmd.ExecuteNonQuery(); }

                string createLogTable = @"
                    CREATE TABLE IF NOT EXISTS CallLogs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        PatientBedName TEXT,
                        CallType TEXT,
                        RequestTime DATETIME,
                        ResolvedTime DATETIME
                    );";
                using (var cmd = new SQLiteCommand(createLogTable, conn)) { cmd.ExecuteNonQuery(); }
                
                // Pre-seed 32 phòng nếu database rỗng
                string countCheck = "SELECT COUNT(*) FROM PatientBeds";
                bool shouldSeed = true;
                using (var cmdCount = new SQLiteCommand(countCheck, conn)) {
                    if (Convert.ToInt32(cmdCount.ExecuteScalar()) > 0) shouldSeed = false;
                }
                
                if (shouldSeed)
                {
                    using (var transaction = conn.BeginTransaction())
                    {
                        string insertSql = "INSERT INTO PatientBeds (RoomName, BedName, MacAddress, IpAddress, Status, LastSeen) VALUES (@room, @bed, '', '', 'Offline', @last)";
                        for (int f = 1; f <= 5; f++)
                        {
                            for (int r = 1; r <= 5; r++)
                            {
                                foreach (char b in new[] { 'A', 'B', 'C', 'D', 'E' })
                                {
                                    using (var cmdIns = new SQLiteCommand(insertSql, conn))
                                    {
                                        cmdIns.Parameters.AddWithValue("@room", $"Phòng {f}0{r}");
                                        cmdIns.Parameters.AddWithValue("@bed", $"Giường {b}");
                                        cmdIns.Parameters.AddWithValue("@last", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                        cmdIns.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
        }

        public static void ResetAllBedsToOffline()
        {
            lock (_dbLock)
            {
                try
                {
                    using (var conn = new SQLiteConnection(GetConnectionString()))
                    {
                        conn.Open();
                        using (var cmdClear = new SQLiteCommand("UPDATE PatientBeds SET Status = 'Offline', MacAddress = '', IpAddress = ''", conn))
                        {
                            cmdClear.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
            }
        }

        public static void UpsertBed(PatientBed bed)
        {
            // Chương 7: lock serializes writes — IcmpHealthCheck + HandleTcpClient gọi method này đồng thời
            lock (_dbLock)
            {
                using (var conn = new SQLiteConnection(GetConnectionString()))
                {
                    conn.Open();
                    string query = @"
                        INSERT INTO PatientBeds (RoomName, BedName, MacAddress, IpAddress, Status, LastSeen)
                        VALUES (@Room, @Bed, @Mac, @Ip, @Status, @LastSeen)
                        ON CONFLICT(RoomName, BedName) DO UPDATE SET
                        MacAddress = @Mac, IpAddress = @Ip, Status = @Status, LastSeen = @LastSeen;";
                    
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Mac", bed.MacAddress ?? "");
                        cmd.Parameters.AddWithValue("@Room", bed.RoomName);
                        cmd.Parameters.AddWithValue("@Bed", bed.BedName);
                        cmd.Parameters.AddWithValue("@Ip", bed.IpAddress ?? "");
                        cmd.Parameters.AddWithValue("@Status", bed.Status);
                        cmd.Parameters.AddWithValue("@LastSeen", bed.LastSeen);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        
        public static void DeleteBed(string macAddress)
        {
            lock (_dbLock)
            {
                try
                {
                    using (var conn = new SQLiteConnection(GetConnectionString()))
                    {
                        conn.Open();
                        using (var cmd = new SQLiteCommand("UPDATE PatientBeds SET Status = 'Offline', MacAddress = '', IpAddress = '' WHERE MacAddress = @mac", conn))
                        {
                            cmd.Parameters.AddWithValue("@mac", macAddress);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
            }
        }

        public static string GetBedStatus(string roomName, string bedName)
        {
            try
            {
                using (var conn = new SQLiteConnection(GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT Status FROM PatientBeds WHERE RoomName = @room AND BedName = @bed", conn))
                    {
                        cmd.Parameters.AddWithValue("@room", roomName);
                        cmd.Parameters.AddWithValue("@bed", bedName);
                        var obj = cmd.ExecuteScalar();
                        if (obj != null && obj != DBNull.Value) return obj.ToString();
                    }
                }
            }
            catch { }
            return "Offline";
        }

        public static PatientBed GetBedDetail(string roomName, string bedName)
        {
            try
            {
                using (var conn = new SQLiteConnection(GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT * FROM PatientBeds WHERE RoomName = @room AND BedName = @bed", conn))
                    {
                        cmd.Parameters.AddWithValue("@room", roomName);
                        cmd.Parameters.AddWithValue("@bed", bedName);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new PatientBed
                                {
                                    RoomName = reader["RoomName"].ToString(),
                                    BedName = reader["BedName"].ToString(),
                                    MacAddress = reader["MacAddress"].ToString(),
                                    IpAddress = reader["IpAddress"].ToString(),
                                    Status = reader["Status"].ToString(),
                                    LastSeen = Convert.ToDateTime(reader["LastSeen"])
                                };
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        public static void UpdateBedStatus(string roomName, string bedName, string status)
        {
            lock (_dbLock)
            {
                try
                {
                    using (var conn = new SQLiteConnection(GetConnectionString()))
                    {
                        conn.Open();
                        using (var cmd = new SQLiteCommand("UPDATE PatientBeds SET Status = @status WHERE RoomName = @room AND BedName = @bed", conn))
                        {
                            cmd.Parameters.AddWithValue("@status", status);
                            cmd.Parameters.AddWithValue("@room", roomName);
                            cmd.Parameters.AddWithValue("@bed", bedName);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
            }
        }

        public static void AddCallLog(CallLog log)
        {
            lock (_dbLock)
            {
                try
                {
                    using (var conn = new SQLiteConnection(GetConnectionString()))
                    {
                        conn.Open();
                        using (var cmd = new SQLiteCommand("INSERT INTO CallLogs (PatientBedName, CallType, RequestTime) VALUES (@bed, @type, @time)", conn))
                        {
                            cmd.Parameters.AddWithValue("@bed", log.PatientBedName);
                            cmd.Parameters.AddWithValue("@type", log.CallType);
                            cmd.Parameters.AddWithValue("@time", log.RequestTime.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
            }
        }

        public static System.Collections.Generic.List<CallLog> GetAllCallLogs()
        {
            var list = new System.Collections.Generic.List<CallLog>();
            try
            {
                using (var conn = new SQLiteConnection(GetConnectionString()))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("SELECT * FROM CallLogs ORDER BY Id DESC", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new CallLog
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                PatientBedName = reader["PatientBedName"].ToString(),
                                CallType = reader["CallType"].ToString(),
                                RequestTime = Convert.ToDateTime(reader["RequestTime"]),
                                ResolvedTime = reader["ResolvedTime"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["ResolvedTime"]) : null
                            });
                        }
                    }
                }
            }
            catch { }
            return list;
        }

        public static void MarkResolvedLogs(string roomName, string bedName)
        {
            lock (_dbLock)
            {
                try
                {
                    using (var conn = new SQLiteConnection(GetConnectionString()))
                    {
                        conn.Open();
                        string caller = $"{roomName} - {bedName}";
                        // Cập nhật tất cả các log chưa có thời gian xử lý của Giường này
                        using (var cmd = new SQLiteCommand("UPDATE CallLogs SET ResolvedTime = @time WHERE PatientBedName = @caller AND ResolvedTime IS NULL", conn))
                        {
                            cmd.Parameters.AddWithValue("@time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@caller", caller);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
            }
        }

        public static void SetBedOffline(string roomName, string bedName)
        {
            lock (_dbLock)
            {
                try
                {
                    using (var conn = new SQLiteConnection(GetConnectionString()))
                    {
                        conn.Open();
                        // Clear IpAddress so ICMP Ping loop doesn't resurrect it on Localhost
                        using (var cmd = new SQLiteCommand("UPDATE PatientBeds SET Status = 'Offline', IpAddress = '' WHERE RoomName = @room AND BedName = @bed", conn))
                        {
                            cmd.Parameters.AddWithValue("@room", roomName);
                            cmd.Parameters.AddWithValue("@bed", bedName);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
            }
        }

        public static void DeleteBedAbsolute(string roomName, string bedName)
        {
            lock (_dbLock)
            {
                try
                {
                    using (var conn = new SQLiteConnection(GetConnectionString()))
                    {
                        conn.Open();
                        using (var cmd = new SQLiteCommand("DELETE FROM PatientBeds WHERE RoomName = @room AND BedName = @bed", conn))
                        {
                            cmd.Parameters.AddWithValue("@room", roomName);
                            cmd.Parameters.AddWithValue("@bed", bedName);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch { }
            }
        }
        
        public static bool AddNewBed(string roomName, string bedName)
        {
            lock (_dbLock)
            {
                try
                {
                    using (var conn = new SQLiteConnection(GetConnectionString()))
                    {
                        conn.Open();
                        string insertSql = "INSERT INTO PatientBeds (RoomName, BedName, MacAddress, IpAddress, Status, LastSeen) VALUES (@room, @bed, '', '', 'Offline', @last)";
                        using (var cmd = new SQLiteCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@room", roomName);
                            cmd.Parameters.AddWithValue("@bed", bedName);
                            cmd.Parameters.AddWithValue("@last", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.ExecuteNonQuery();
                        }
                        return true;
                    }
                }
                catch { return false; }
            }
        }



        public static List<PatientBed> GetAllBeds()
        {
            var list = new List<PatientBed>();
            try 
            {
                using (var conn = new SQLiteConnection(GetConnectionString()))
                {
                    conn.Open();
                    string query = "SELECT MacAddress, RoomName, BedName, IpAddress, Status, LastSeen FROM PatientBeds ORDER BY RoomName ASC";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new PatientBed {
                                    MacAddress = reader["MacAddress"].ToString(),
                                    RoomName = reader["RoomName"].ToString(),
                                    BedName = reader["BedName"].ToString(),
                                    IpAddress = reader["IpAddress"].ToString(),
                                    Status = reader["Status"].ToString(),
                                    LastSeen = Convert.ToDateTime(reader["LastSeen"])
                                });
                            }
                        }
                    }
                }
            }
            catch { }
            return list;
        }
    }
}
