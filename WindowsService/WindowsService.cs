using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.IO;
using System.Collections.Generic;


namespace WindowsService {
    class WindowsService : ServiceBase {

        private bool systemShuttingdown;
        private Dictionary<string, string> envs;
        private ServiceDescriptor descriptor;

        public WindowsService() {
            this.ServiceName = "winws";
            this.EventLog.Source = "winws";
            this.EventLog.Log = "Application" ;          
            this.CanHandlePowerEvent = true;
            this.CanHandleSessionChangeEvent = true;
            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.CanStop = true;

            this.descriptor = new ServiceDescriptor();
            this.ServiceName = descriptor.Id;
            this.systemShuttingdown = false;
            if (!EventLog.SourceExists(this.EventLog.Source))
                EventLog.CreateEventSource(this.EventLog.Source, this.EventLog.Log);
        }

        static void Main() {
            ServiceBase.Run(new WindowsService());
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
        }

        protected override void OnStart(string[] args) {

            envs = descriptor.EnvironmentVariables;
            foreach (string key in envs.Keys) {
                LogEvent("envar " + key + "=" + envs[key]);
            }

            base.OnStart(args);
        }

        protected override void OnStop() {
            base.OnStop();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnContinue() {
            base.OnContinue();
        }

        protected override void OnShutdown() {
            base.OnShutdown();
        }

        protected override void OnCustomCommand(int command)
        {
            //  A custom command can be sent to a service by using this method:
            //#  int command = 128; //Some Arbitrary number between 128 & 256
            //#  ServiceController sc = new ServiceController("NameOfService");
            //#  sc.ExecuteCommand(command);

            base.OnCustomCommand(command);
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return base.OnPowerEvent(powerStatus);
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);
        }

        public void LogEvent(String message)
        {
            if (systemShuttingdown)
            {
                /* NOP - cannot call EventLog because of shutdown. */
            }
            else
            {
                EventLog.WriteEntry(message);
            }
        }

        public void LogEvent(String message, EventLogEntryType type)
        {
            if (systemShuttingdown)
            {
                /* NOP - cannot call EventLog because of shutdown. */
            }
            else
            {
                EventLog.WriteEntry(message, type);
            }
        }

        private void WriteEvent(Exception exception)
        {
            WriteEvent(exception.Message + "\nStacktrace:" + exception.StackTrace);
        }

        private void WriteEvent(String message, Exception exception)
        {
            WriteEvent(message + "\nMessage:" + exception.Message + "\nStacktrace:" + exception.StackTrace);
        }

        private void WriteEvent(String message)
        {
            string logfilename = Path.Combine(descriptor.LogDirectory, descriptor.BaseName + ".wrapper.log");
            StreamWriter log = new StreamWriter(logfilename, true);

            log.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + message);
            log.Flush();
            log.Close();
        }


    }
}
