using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using System.Linq;
using BusinessObjects;
using log4net;
using System.Diagnostics;
using FXBusinessLogic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security;
using FXBusinessLogic.BusinessObjects;
using Quartz;
using System.Threading;

namespace FXBusinessLogic
{
    public class ProcessImpersonation
    {
        public ProcessImpersonation( ILog l)
        {
            log = l;
        }

        #region DLL Imports

        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        internal const int TOKEN_ASSIGN_PRIMARY = 0x0001;
        internal const int TOKEN_DUPLICATE = 0x0002;
        internal const int TOKEN_IMPERSONATE = 0X00000004;
        internal const int TOKEN_ADJUST_DEFAULT = 0x0080;
        internal const int TOKEN_ADJUST_SESSIONID = 0x0100;
        internal const int MAXIMUM_ALLOWED = 0x2000000;
        internal const int GENERIC_ALL_ACCESS = 0x10000000;
        
        internal const int CREATE_UNICODE_ENVIRONMENT = 0x00000400;
        internal const int NORMAL_PRIORITY_CLASS = 0x20;
        internal const int CREATE_NEW_CONSOLE = 0x00000010;

        internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        internal const string SE_TCB_NAME = "SeTcbPrivilege";
        internal const string SE_RESTORE_NAME = "SeRestorePrivilege";

        private static WindowsImpersonationContext impersonatedUser;
        public static IntPtr hToken = IntPtr.Zero;
        public static IntPtr dupeTokenHandle = IntPtr.Zero;
        const string SE_INCREASE_QUOTA_NAME = "SeIncreaseQuotaPrivilege";

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }
        public enum ShowCommands : int
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11
        }

        [DllImport("shell32.dll")]
        static extern IntPtr ShellExecute(
            IntPtr hwnd,
            string lpOperation,
            string lpFile,
            string lpParameters,
            string lpDirectory,
            ShowCommands nShowCmd);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern int ImpersonateLoggedOnUser(IntPtr hToken);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("kernel32", SetLastError = true), SuppressUnmanagedCodeSecurityAttribute]
        static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public extern static bool DuplicateToken(IntPtr ExistingTokenHandle, int SECURITY_IMPERSONATION_LEVEL, ref IntPtr DuplicateTokenHandle);

        [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
        static extern bool DuplicateTokenEx(IntPtr hExistingToken, Int32 dwDesiredAccess,
                            ref SECURITY_ATTRIBUTES lpThreadAttributes,
                            Int32 ImpersonationLevel, Int32 dwTokenType,
                            ref IntPtr phNewToken);

        [DllImport("userenv.dll", SetLastError = true)]
        static extern bool CreateEnvironmentBlock(out IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

        public enum WTS_INFO_CLASS
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType,
            WTSIdleTime,
            WTSLogonTime,
            WTSIncomingBytes,
            WTSOutgoingBytes,
            WTSIncomingFrames,
            WTSOutgoingFrames,
            WTSClientInfo,
            WTSSessionInfo
        }
        internal static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

        [DllImport("Wtsapi32.dll")]
        public static extern bool WTSQuerySessionInformation(
            System.IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out System.IntPtr ppBuffer, out uint pBytesReturned);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);
        #endregion

        public static ILog log;

        private static void WriteToLog(string message)
        {
            log.Info(message);
        }

        /// <summary>
        /// Duplicates the token information derived 
        /// from the logged in user's credentials. This 
        /// is required to run the application on the 
        /// logged in users desktop.
        /// </summary>
        /// <returns>Returns true if the application was successfully started in the user's desktop.</returns>
        public bool ExecuteAppAsLoggedOnUser(string AppName, string CmdLineArgs)
        {
            //WriteToLog("In ExecuteAppAsLoggedOnUser for all users.");
            IntPtr LoggedInUserToken = IntPtr.Zero;
            IntPtr DuplicateToken = IntPtr.Zero;
            IntPtr ShellProcessToken = IntPtr.Zero;

            Process currProc = Process.GetCurrentProcess();
            bool result = OpenProcessToken(currProc.Handle, TOKEN_QUERY | TOKEN_ADJUST_PRIVILEGES, ref LoggedInUserToken);
            if (!result)
            {
                WriteToLog("OpenProcessToken failed: " + Marshal.GetLastWin32Error());
                return false;
            }
            else
            {
                //Below part for increasing the UAC previleges to the token.
                TokPriv1Luid tp = new TokPriv1Luid();
                tp.Count = 1;
                tp.Luid = 0;
                if (!LookupPrivilegeValue(null, SE_INCREASE_QUOTA_NAME, ref tp.Luid))
                {
                    WriteToLog("LookupPrivilegeValue failed: " + Marshal.GetLastWin32Error());
                    return false;
                }

                tp.Attr = SE_PRIVILEGE_ENABLED;
                if (!AdjustTokenPrivileges(LoggedInUserToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
                {
                    WriteToLog("OpenProcessToken failed: " + Marshal.GetLastWin32Error());
                    return false;
                }
                CloseHandle(LoggedInUserToken);
            }

            List<Process> explorerProcessList = new List<Process>();
            string trayProcessName = Path.GetFileNameWithoutExtension(AppName); // AppName.Substring(AppName.LastIndexOf(@"\") + 1, AppName.Length - AppName.LastIndexOf(@"\") - 5);
            string userName = "";
            foreach (Process explorerProcess in Process.GetProcessesByName("explorer"))
            {
                userName = GetProcessUser(explorerProcess);
                bool IsProcessRunningForUser = userName.Equals(MainService.MTTerminalUserName, StringComparison.InvariantCultureIgnoreCase );
                //foreach (Process PHTrayProcess in Process.GetProcessesByName(trayProcessName))
                //{
                //    if (explorerProcess.SessionId == PHTrayProcess.SessionId )
                //    {
                //        if (log.IsDebugEnabled)
                //            log.Debug(trayProcessName + " is already running for user SessionId " + explorerProcess.SessionId);
                //        IsProcessRunningForUser = true;
                //        break;
                //    }
                //}

                if (((Environment.OSVersion.Version.Major > 5 && explorerProcess.SessionId > 0)
                    || Environment.OSVersion.Version.Major == 5)
                    && IsProcessRunningForUser)
                {
                    if (MainService.thisGlobal.IsDebug())
                        log.Info($"Desktop is running for user {userName} and SessionId " + explorerProcess.SessionId);
                    explorerProcessList.Add(explorerProcess);
                    break;
                }
            }

            if (null != explorerProcessList && explorerProcessList.Count > 0)
            {
                foreach (Process explorerProcess in explorerProcessList)
                {
                    Process ShellProcess = explorerProcess;
                    ShellProcess.StartInfo.LoadUserProfile = true;

                    try
                    {
                        int tokenRights = MAXIMUM_ALLOWED; //TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY | TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_ADJUST_DEFAULT | TOKEN_ADJUST_SESSIONID;
                        if (!OpenProcessToken(ShellProcess.Handle, tokenRights, ref ShellProcessToken))
                        {
                            WriteToLog("Unable to OpenProcessToken " + Marshal.GetLastWin32Error());
                            return false;
                        }

                        SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
                        sa.nLength = Marshal.SizeOf(sa);

                        if (Environment.UserInteractive && System.Diagnostics.Debugger.IsAttached)
                        {   
                            // In debug mode tokens obtained differently
                            IntPtr hToken = WindowsIdentity.GetCurrent().Token;
                            if (!DuplicateTokenEx(hToken, GENERIC_ALL_ACCESS, ref sa, 1, 1, ref DuplicateToken))
                            {
                                WriteToLog("Unable to duplicate token " + Marshal.GetLastWin32Error());
                                return false;
                            }

                        }
                        else
                        {
                            if (!DuplicateTokenEx(ShellProcessToken, tokenRights, ref sa, 2, 1, ref DuplicateToken))
                            {
                                WriteToLog("Unable to duplicate token " + Marshal.GetLastWin32Error());
                                return false;
                            }
                        }

                        //if (MainService.thisGlobal.IsDebug())
                        //    log.Info("Duplicated the token " + WindowsIdentity.GetCurrent().Name);
                        //WriteToLog("Duplicated the token " + WindowsIdentity.GetCurrent().Name);

                        SECURITY_ATTRIBUTES processAttributes = new SECURITY_ATTRIBUTES();
                        SECURITY_ATTRIBUTES threadAttributes = new SECURITY_ATTRIBUTES();
                        PROCESS_INFORMATION pi;
                        STARTUPINFO si = new STARTUPINFO();
                        si.cb = (uint)Marshal.SizeOf(si);

                        IntPtr UserEnvironment = IntPtr.Zero;
                        uint dwCreationFlags = NORMAL_PRIORITY_CLASS | CREATE_NEW_CONSOLE;
                        if (!CreateEnvironmentBlock(out UserEnvironment, ShellProcessToken, true))
                        {
                            WriteToLog("Unable to create user's enviroment block " + Marshal.GetLastWin32Error());
                        }
                        else
                        {
                            dwCreationFlags |= CREATE_UNICODE_ENVIRONMENT;
                        }

                        string WorkingDir = Path.GetDirectoryName(AppName);

                        if (!CreateProcessAsUser(DuplicateToken, AppName, CmdLineArgs, ref processAttributes, ref threadAttributes, true, dwCreationFlags, UserEnvironment, WorkingDir, ref si, out pi))
                        {
                            WriteToLog("Unable to create process " + Marshal.GetLastWin32Error());
                            if (Marshal.GetLastWin32Error() == 740)
                            {
                                WriteToLog("Please check the installation as some elevated permissions is required to execute the binaries");
                            }
                            return false;
                        }
                        log.InfoFormat("Process {0} started under user {1} successfully", AppName, userName);
                        Process trayApp = Process.GetProcessById(Convert.ToInt32(pi.dwProcessId));
                        trayApp.StartInfo.LoadUserProfile = true;
                        break;
                    }
                    finally
                    {
                        if (ShellProcessToken != null) CloseHandle(ShellProcessToken);
                        if (DuplicateToken != null) CloseHandle(DuplicateToken);
                    }
                }
            }
            else
            {
                WriteToLog("No user has been identified to have logged into the system.");
                return false;
            }
            //WriteToLog("Finished ExecuteAppAsLoggedOnUser for all users.");
            return true;
        }

        public string GetProcessUser(Process process)
        {
            IntPtr AnswerBytes;
            uint AnswerCount;
            if (WTSQuerySessionInformation(WTS_CURRENT_SERVER_HANDLE,
                                              process.SessionId,
                                              WTS_INFO_CLASS.WTSUserName,
                                              out AnswerBytes,
                                              out AnswerCount))
            {
                string userName = Marshal.PtrToStringAnsi(AnswerBytes);
                return userName;
            }
            else
            {
                return "";
            }
        }

    /// <summary>
    /// Impersonate the user credentials. This would be required by 
    /// the service applications to impersonate the logged in user
    /// credentials to launch certain applications or applying the
    /// power scheme.
    /// </summary>
    /// <returns>Returns true if the impersonation is successful.</returns>
    public  bool ImpersonateUser()
        {
            // For simplicity I'm using the PID of System here
            //if (log.IsDebugEnabled) log.Debug("GetaProcess for Explorer"); 
            Process Pname = GetaProcess("explorer");
            //This can be null if no user has not logged into the system.
            if (Pname == null) return false;

            int pid = Pname.Id;
            Process proc = Process.GetProcessById(pid);
            if (OpenProcessToken(proc.Handle, TOKEN_QUERY | TOKEN_IMPERSONATE | TOKEN_DUPLICATE, ref hToken)) // != 0)
            {
                WindowsIdentity newId = new WindowsIdentity(hToken);
                //log.Debug(newId.Owner);
                try
                {
                    const int SecurityImpersonation = 2;
                    dupeTokenHandle = DupeToken(hToken,
                    SecurityImpersonation);
                    if (IntPtr.Zero == dupeTokenHandle)
                    {
                        string s = String.Format("Dup failed {0}, privilege not held",
                        Marshal.GetLastWin32Error());
                        throw new Exception(s);
                    }

                    impersonatedUser = newId.Impersonate();
                    return true;
                }
                finally
                {
                    CloseHandle(hToken);
                }
            }
            else
            {
                string s = String.Format("OpenProcess Failed {0}, privilege not held", Marshal.GetLastWin32Error());
                throw new Exception(s);
            }
        }

        /// <summary>
        /// Duplicate the token for user impersonation.
        /// </summary>
        /// <param name="token">Token to duplicate for impersonation</param>
        /// <param name="Level">Impersonation security level, currently hardcored to 2</param>
        /// <returns>Returns duplicated token</returns>
        public static IntPtr DupeToken(IntPtr token, int Level)
        {
            IntPtr dupeTokenHandle = IntPtr.Zero;
            bool retVal = DuplicateToken(token, Level, ref dupeTokenHandle);
            return dupeTokenHandle;
        }

        /// <summary>
        /// Get the process running locally on the machine.
        /// If the specified process does not exists, it 
        /// returns back the current process.
        /// </summary>
        /// <param name="processname">Process name to get</param>
        /// <returns>Returns back the process</returns>
        public static Process GetaProcess(string processname)
        {
            Process[] aProc = Process.GetProcessesByName(processname);
            if (aProc.Length > 0) return aProc[0];
            else
            {
                //if (log.IsDebugEnabled) log.Debug("Explorer is not running");
                Process currentProcess = Process.GetCurrentProcess();
                return currentProcess;
            }
        }

        /// <summary>
        /// Roleback the impersonation if applied previously.
        /// </summary>
        public static void UndoImpersonate()
        {
            impersonatedUser.Undo();
            if (hToken != IntPtr.Zero) CloseHandle(hToken);
            if (dupeTokenHandle != IntPtr.Zero) CloseHandle(dupeTokenHandle);
            return;
        }

        public void StartProcessInNewThread(string fileName, string logFile, string appname)
        {
            log.Info($"Starting deploy script {fileName}");
            var runTime = SystemTime.UtcNow();
            Thread newThread = new Thread(Func => {
                try
                {
                    CloseTerminal(appname);

                    ProcessStartInfo process = new ProcessStartInfo();
                    process.FileName = fileName;
                    process.Arguments = $" > {logFile}";
                    process.CreateNoWindow = true;
                    process.ErrorDialog = false;
                    process.RedirectStandardError = true;
                    process.RedirectStandardInput = true;
                    process.RedirectStandardOutput = true;
                    process.UseShellExecute = false;
                    process.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    Process p = Process.Start(process);
                    p.WaitForExit();
                    DateTimeOffset now = SystemTime.UtcNow();
                    TimeSpan duration = now - runTime;
                    log.Info($"Deploying finished for script {fileName} for {duration.Seconds} seconds.");
                }
                catch (Exception e)
                {
                    log.Error("Thread run Process Error: " + e.ToString());
                }
            });
            newThread.Start();
        }

        public  void CloseTerminal(string AppName)
        {
            System.Diagnostics.Process[] processlist = null;
            try
            {
                string trayProcessName = Path.GetFileNameWithoutExtension(AppName);
                processlist = Process.GetProcessesByName(trayProcessName);
                var processL = processlist.Where(d => d.MainModule.FileName.Equals(AppName, StringComparison.InvariantCultureIgnoreCase));
                if ((processL != null) && (processL.Count() > 0))
                {
                    var process = processL.FirstOrDefault();
                    if ( !process.HasExited )
                    {
                        // process.CloseMainWindow();
                        process.Kill();
                        //Thread.Sleep(2000);
                        //if (process != null)
                        //    if (!process.HasExited)
                        //        process.Kill();
                    }
                    if (!process.HasExited)
                        process.WaitForExit();
                    log.Info($"Terminal closed for deployment {AppName}");
                }
            }
            catch (Exception e)
            {
                log.Info("Error in CloseTerminal: " + e.ToString());
            }
            finally
            {
                if (processlist != null)
                {
                    foreach (Process p in processlist)
                    {
                        p.Dispose();
                    }
                }
            }
        }


    }
}
