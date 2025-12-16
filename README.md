# DatabaseBackupSerivce


This project is designed to build, develop, and deploy a Windows Service that automates the periodic backup of a SQL Server database.

The service logs all backup activities and handles errors gracefully. Configuration values—such as database connection details, backup intervals, and folder paths—are dynamically managed through the App.config file.



1. Core Functionalities
### Automated Backups:
 Perform a full backup of a specified SQL Server database.
Save the backup file in a designated folder, appending a timestamp to the file name.
### Dynamic Configuration:
  Use App.config to configure:
      * Database connection string.
      * Backup folder path.
      *  Log folder path.
 Backup interval (in minutes).


 ## Example Configurations:

 ```
<appSettings>
	
	<connectionStrings>
		<add name="DbConn"
		connectionString="yourdatabaseconnectionaddress"/> //* for example :Server=localhost;Database=databasename;Integrated Security=True;TrustServerCertificate=True;
	</connectionStrings>
    <add key="BackupFolder" value="C:\DatabaseBackups" />
    <add key="LogFolder" value="C:\DatabaseBackups\Logs" />
    <add key="BackupIntervalMinutes" value="60" />
</appSettings>
```
