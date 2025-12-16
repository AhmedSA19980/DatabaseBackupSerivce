using System;

using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;


namespace DatabaseBackupSerivce
{
    public partial class DatabaseBackupService : ServiceBase
    {
        
        private static string Connections = ConfigurationManager.ConnectionStrings["Dbconn"].ConnectionString;
        private static string BackupFolder = ConfigurationManager.AppSettings["BackupFolder"];
        private static string LogFolder = ConfigurationManager.AppSettings["LogFolder"];
      
    


        // New-EventLog -LogName "DatabaseBackupLog" -Source "BackupService"
        private static EventLog eventLog;

        Timer backupTimer;
      
        public DatabaseBackupService()
        {
            InitializeComponent();


            eventLog = new EventLog();
            if (!EventLog.SourceExists("DatabaseBackupService"))
            {
                EventLog.CreateEventSource("BackupService", "DatabaseBackupLog");
            }

            eventLog.Source = "BackupService";
            eventLog.Log = "DatabaseBackupLog";
        }

        private bool IsServiceRunning(string serviceName)
        {
            try
            {
                ServiceController sc = new ServiceController(serviceName);
                return sc.Status == ServiceControllerStatus.Running;
            }
            catch
            {
                // service not installed or not accessible
                return false;
            }
        }

        protected override void OnStart(string[] args)
        {
           
            int intervalMinutes = int.Parse(ConfigurationManager.AppSettings["BackupIntervalMinutes"]);


            if (!Directory.Exists(BackupFolder)) Directory.CreateDirectory(BackupFolder);

            if (!Directory.Exists(LogFolder)) Directory.CreateDirectory(LogFolder);

            Log("Service Started !");

            if (!IsServiceRunning("MSSQLSERVER"))
            {
                eventLog.WriteEntry("SQL Server (MSSQLSERVER) is not running.", EventLogEntryType.Error);
                Console.WriteLine("SQL Server (MSSQLSERVER) is not running.");
                Stop();
                return;
            }

            // Check RPC
            if (!IsServiceRunning("RpcSs"))
            {
                eventLog.WriteEntry("RPC Service (RpcSs) is not running.", EventLogEntryType.Error);
                Stop();
                return;
            }

            // Check Event Log
            if (!IsServiceRunning("EventLog"))
            {
                eventLog.WriteEntry("Event Log service is not running.", EventLogEntryType.Error);
                Stop();
                return;
            }


           


            backupTimer = new Timer(intervalMinutes * 60 *1000 );
            backupTimer.Elapsed += (s, e) => BackUpData();
            backupTimer.AutoReset = true;
            backupTimer.Start();
        }


       
        protected override void OnStop()
        {
            // wait for thread to finish
            if (!Environment.UserInteractive) { backupTimer?.Stop();backupTimer.Dispose(); }
            
            Log("Service Stopped !");

        }

        static void BackUpData()
        {

            string filePath = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            string backupPath = Path.Combine(BackupFolder, filePath);
            try
            {
                string backupSql = $"BACKUP DATABASE EmployeesDB TO DISK = '{backupPath}' WITH INIT, FORMAT;"; 
                using (SqlConnection conn = new SqlConnection(Connections))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(backupSql ,  conn)) {
                        cmd.CommandTimeout = 0; // Allow long backups
                        cmd.ExecuteNonQuery();
                       // Console.WriteLine("data is processin ");

                    }

                }
                Log($" Database backup successfully: {backupPath}");
                eventLog.WriteEntry("✔ Backup created successfully.", EventLogEntryType.Information);
            }
            catch (Exception ex) {

                eventLog.WriteEntry(" Error during backup:" +ex.Message, EventLogEntryType.Error  );
                Log($" Error during backup:{ex.Message}");

            }

        }



        static void Log(string msg)
        {
            string LogPath = Path.Combine(LogFolder, "LogFile.txt");
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //Console.WriteLine($"[{dateTime}] {msg}");
            File.AppendAllText(LogPath, $"[{dateTime}] {msg}\n");

        }

        public void StartOnConsole()
        {
            OnStart(null);

            Console.WriteLine("server start on console ");
            Console.WriteLine();
            OnStop();
            Console.ReadLine();
        }
    }
}
