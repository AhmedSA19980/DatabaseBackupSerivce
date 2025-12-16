using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace DatabaseBackupSerivce
{
    [RunInstaller(true)]
    public partial class BackupSerivceInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller serviceProcessInstaller;
        private ServiceInstaller serviceInstaller;
        public BackupSerivceInstaller()
        {
            InitializeComponent();

            // Configure the Service Process Installer
            serviceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem // Adjust as needed (e.g., NetworkService, LocalService)
            };

            // Configure the Service Installer
            serviceInstaller = new ServiceInstaller
            {
                ServiceName = "DatabaseBackupService", // Must match the ServiceName in your ServiceBase class
                DisplayName = "DatabaseBackup_Service",
                StartType = ServiceStartMode.Manual // Or Automatic, depending on requirements
            };


            serviceInstaller.ServicesDependedOn = new string[]
            {
                "MSSQLSERVER",
                "RpcSs",
                "EventLog"
            };
            // Add installers to the installer collection
            Installers.Add(serviceProcessInstaller);
            Installers.Add(serviceInstaller);
        }

    }
}
