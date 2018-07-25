using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using BusinessObjects;
using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using log4net;

namespace FXMind.WinClient
{
    public partial class MainForm : XtraForm, INotificationUi
    {
        public delegate void InitProgressDelegate(int min, int max);

        private static readonly ILog log = LogManager.GetLogger(typeof(MainForm));

        public static string DATEFORMAT = "yyyy,M,d";
        public static string TIMEFORMAT = "H,m,s";
        public static string DATETIMEFORMAT = "yyyy,M,d,H,m,s";

        public static BarStaticItem statusBar;
        public static TimeZoneInfo g_userTimeZone;

        private Dictionary<string, ScheduledJob> RunningJobs = new Dictionary<string, ScheduledJob>();
        private Dictionary<int, TimeZoneInfo> tz_col;

        public MainForm()
        {
            log.Info("Main Form c-tor...");
            try
            {
                InitializeComponent();
                RegisterContainer();
                log.Info("Main Form c-tor initialized...");
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        private IContainer container { get; set; }

        public void LogStatus(string statMessage)
        {
            if (statusBar != null) statusBar.Caption = statMessage;
        }

        public void ReloadAllViewsNotification()
        {
            if (IsHandleCreated)
                BeginInvoke(new Action(OnReloadAllViews));
        }

        public void InitProgressNotification(int min, int max)
        {
            object[] pars = {min, max};
            if (IsHandleCreated)
                BeginInvoke(new InitProgressDelegate(OnInitProgressBar), pars);
        }

        public void UpdateProgressNotification()
        {
            if (IsHandleCreated)
                BeginInvoke(new Action(OnUpdateProgressStep));
        }

        public void UpdateData(object data)
        {
        }

        public IContainer GetContainer()
        {
            return container;
        }

        private void RegisterContainer()
        {
            try
            {
                var builder = new ContainerBuilder();
                builder.Register(c => new AppServiceClient("localhost", fxmindConstants.AppService_PORT) );  //.SingleInstance();
                container = builder.Build();
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
            }
        }

        protected void InitTimeZones()
        {
            ReadOnlyCollection<TimeZoneInfo> tz;
            tz_col = new Dictionary<int, TimeZoneInfo>();
            using (AppServiceClient app = container.Resolve<AppServiceClient>())
            {
                string gtimezone = app.client.GetGlobalProp(fxmindConstants.SETTINGS_PROPERTY_USERTIMEZONE);

                tz = TimeZoneInfo.GetSystemTimeZones();
                int index = 0;
                if (tz != null && tz.Count > 0)
                    foreach (TimeZoneInfo timezone in tz)
                    {
                        barTimeZone2.Strings.Add(timezone.DisplayName);
                        index++;
                        tz_col.Add(index - 1, timezone);
                        if (gtimezone != null && timezone.StandardName.Equals(gtimezone))
                        {
                            barTimeZone2.ItemIndex = index - 1;
                            g_userTimeZone = tz_col[barTimeZone2.ItemIndex];
                        }
                        else
                        {
                            // default is Mexico
                            if (timezone.BaseUtcOffset == new TimeSpan(-7, 0, 0))
                            {
                                barTimeZone2.ItemIndex = index - 1;
                                g_userTimeZone = tz_col[barTimeZone2.ItemIndex];
                            }
                        }
                    }

                // reset this valuse on start
                //app.client.SetGlobalProp(fxmindConstants.SETTINGS_PROPERTY_USEDATEINTERVAL, "false");
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            log.Info("Main Form OnLoad...");
            base.OnLoad(e);
        }

        public void InitLogger()
        {
            statusBar = barStaticItem6;
        }

        private void TradersParserFrom_Load(object sender, EventArgs e)
        {
            log.Info("Main Form Load...");



            ReloadAllViews += TradersParserForm_ReloadAllViews;
            InitProgress += TradersParserForm_InitProgressBar;
            UpdateProgress += TradersParserForm_UpdateProgressStep;
            InitLogger();

            barProgressParsing.Visibility = BarItemVisibility.Never;

            //InitFXMindServer();

            InitTimeZones();

            RefreshServicePage();

            xtraTabControl1.SelectedTabPageIndex = 2;

            justReloadAllViews();


            Program.LogStatus("Connected to DB.");

            using (var app = container.Resolve<AppServiceClient>())
            {
                if (!app.client.IsDebug()) barButtonTestClient.Enabled = false;
            }

            log.Info("Main Form Load Finished.");
        }

        private void RefreshJobsGrid()
        {
            using (var app = container.Resolve<AppServiceClient>())
            {

                List<ScheduledJob> list = app.client.GetAllJobsList();
                if (list.Count == 0)
                {
                    if (app.client.InitScheduler(false) == false)
                    {
                        LogStatus("!!!Please run FXMind.MainServer service!!!!");
                        return;
                    }

                    list = app.client.GetAllJobsList();
                }

                RunningJobs = app.client.GetRunningJobs();
                gridJobs1.DataSource = list;
            }
        }

        private void barCalcCurrStr_ItemClick(object sender, ItemClickEventArgs e)
        {
        }

        // Events region
        public event EventHandler ReloadAllViews;
        public event EventHandler InitProgress;
        public event EventHandler UpdateProgress;

        protected void OnReloadAllViews()
        {
            // Use this technique to avoid potential race condition.
            EventHandler handler = ReloadAllViews;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void TradersParserForm_ReloadAllViews(object sender, EventArgs e)
        {
            justReloadAllViews();
            repositoryItemProgressBar1.Step = 0;
            barProgressParsing.Visibility = BarItemVisibility.Never;
        }

        protected void OnInitProgressBar(int min, int max)
        {
            EventHandler handler = InitProgress;
            if (handler != null)
                handler(this, new ProgressInitEventArgs(min, max));
        }

        private void TradersParserForm_InitProgressBar(object sender, EventArgs e)
        {
            var pe = (ProgressInitEventArgs) e;
            int min = pe.Min;
            int max = pe.Max;
            repositoryItemProgressBar1.ProgressKind = ProgressKind.Horizontal;
            repositoryItemProgressBar1.Maximum = max;
            repositoryItemProgressBar1.Minimum = min;
            repositoryItemProgressBar1.Step = 0;
            repositoryItemProgressBar1.PercentView = true;
            repositoryItemProgressBar1.ProgressViewStyle = ProgressViewStyle.Solid;
            barProgressParsing.EditValue = repositoryItemProgressBar1.Minimum;
            barProgressParsing.Visibility = BarItemVisibility.Always;
        }

        protected void OnUpdateProgressStep()
        {
            EventHandler handler = UpdateProgress;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void TradersParserForm_UpdateProgressStep(object sender, EventArgs e)
        {
            repositoryItemProgressBar1.Step++;
            barProgressParsing.EditValue = repositoryItemProgressBar1.Step + 1;
        }

        // End of Events 

        protected void justReloadAllViews()
        {
            if (xtraTabControl1.SelectedTabPageIndex == 0)
                RefreshJobsGrid();
//            if (xtraTabControl1.SelectedTabPageIndex == 1)
//                RefreshCurrencyStrength(false);
            if (xtraTabControl1.SelectedTabPageIndex == 2)
                RefreshServicePage();
        }


        private void barTimeZone2SelectionChange_ItemClick(object sender, ListItemClickEventArgs e)
        {
            if (barTimeZone2.ItemIndex >= 0 && barTimeZone2.ItemIndex < tz_col.Count)
            {
                g_userTimeZone = tz_col[barTimeZone2.ItemIndex];

                using (var app = container.Resolve<AppServiceClient>())
                {
                    app.client.SetGlobalProp(fxmindConstants.SETTINGS_PROPERTY_USERTIMEZONE, g_userTimeZone.StandardName);

                }
            }
        }

        private void gridView3_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
        {
            // this event fills Jobs Grid data
            object row = e.Row;
            if (row == null)
                return;
            if (e.IsGetData)
            {
                object valJobName = gridView3.GetListSourceRowCellValue(e.ListSourceRowIndex, colJobName);
                object valJobGroup = gridView3.GetListSourceRowCellValue(e.ListSourceRowIndex, colJobGroup);
                if (valJobName != null && valJobGroup != null)
                {
                    using (var app = container.Resolve<AppServiceClient>())
                    {

                        string jobName = valJobName.ToString();
                        string jobGroup = valJobGroup.ToString();
                        if (e.Column.FieldName == "colPrevTime")
                        {
                            long val = app.client.GetJobPrevTime(jobGroup, jobName);
                            if (val > 0)
                                e.Value = ConvertToLocalDateTime(val);
                            return;
                        }

                        if (e.Column.FieldName == "colNextTime")
                        {
                            long val = app.client.GetJobNextTime(jobGroup, jobName);
                            if (val > 0)
                                e.Value = ConvertToLocalDateTime(val);
                            return;
                        }

                        if (e.Column.FieldName == "colLog")
                        {
                            string val = app.client.GetJobProp(jobGroup, jobName, "log");
                            e.Value = val;
                        }
                    }
                }
            }
        }

        public static DateTime ConvertToLocalDateTime(long datelong)
        {
            DateTime datetime = DateTime.FromBinary(datelong);
            int offset_hours = -7;
            if (g_userTimeZone != null)
                return TimeZoneInfo.ConvertTime(datetime, g_userTimeZone); //datetime.AddHours(offset_hours);
            return datetime.AddHours(offset_hours);
        }

        private void gridView3_RowStyle(object sender, RowStyleEventArgs e)
        {
            try
            {
                var strJobName = (string) gridView3.GetRowCellValue(e.RowHandle, colJobName);
                var strJobGroup = (string) gridView3.GetRowCellValue(e.RowHandle, colJobGroup);
                if (strJobName == null || strJobGroup == null)
                    return;
                if (RunningJobs.ContainsKey(strJobGroup + strJobName))
                {
                    e.Appearance.BackColor = DXColor.LightGreen;
                    return;
                }

                e.Appearance.BackColor = DXColor.White;
            }
            catch (Exception ex)
            {
                LogStatus(ex.Message);
            }
        }


        private void gridView4_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            try
            {
                if (e.CellValue == null)
                    return;
                switch (e.Column.FieldName)
                {
                    case "Min1":
                    case "Min5":
                    case "Min15":
                    case "Min30":
                    case "Hourly":
                    case "Hourly5":
                    case "Daily":
                    case "Monthly":
                        var val = (decimal) e.CellValue;
                        if (val < 0)
                            e.Appearance.BackColor = DXColor.Red;
                        if (val > 0)
                            e.Appearance.BackColor = DXColor.LightGreen;
                        if (val == 0)
                            e.Appearance.BackColor = DXColor.LightGray;
                        break;
                }
            }
            catch (Exception ex)
            {
                LogStatus(ex.Message);
            }
        }


        private void repositoryRunNowButtonEdit1_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            var strJobName = (string) gridView3.GetRowCellValue(gridView3.FocusedRowHandle, colJobName);
            var strJobGroup = (string) gridView3.GetRowCellValue(gridView3.FocusedRowHandle, colJobGroup);
            using (var app = container.Resolve<AppServiceClient>())
            {
                app.client.RunJobNow(strJobGroup, strJobName);
            }
        }

        private void xtraTabControl1_SelectedPageChanged(object sender, TabPageChangedEventArgs e)
        {
            switch (e.Page.Name)
            {
                case "strengthPage":
                    LoadCurrStrengthControls();
                    break;
                case "jobsPage":
                    LoadCronExpressionsList();
                    break;
                //case "autoPage":
                //    LoadMTRelatedControls();
                //    break;
            }

            justReloadAllViews();
        }


        private void LoadCronExpressionsList()
        {
            ComboBoxItemCollection coll = repositoryItemCronExpressions.Items;
            coll.Clear();
            coll.BeginUpdate();
            try
            {
                coll.Add(new CronExpressionInfo("0 0/1 * ? * MON-FRI *", "Every Minute"));
                coll.Add(new CronExpressionInfo("0 0/3 * ? * MON-FRI *", "Every 3 Minutes"));
                coll.Add(new CronExpressionInfo("0 0/5 * ? * MON-FRI *", "Every 5 Minutes"));
                coll.Add(new CronExpressionInfo("0 0/10 * ? * MON-FRI *", "Every 10 Minutes"));
                coll.Add(new CronExpressionInfo("0 0/15 * ? * MON-FRI *", "Every 15 Minutes"));
                coll.Add(new CronExpressionInfo("0 0/30 * ? * MON-FRI *", "Every 30 Minutes"));
                coll.Add(new CronExpressionInfo("0 0 0/1 ? * MON-FRI *", "Every Hour"));
                coll.Add(new CronExpressionInfo("0 0 0/5 ? * MON-FRI *", "Every 5 Hours"));
                coll.Add(new CronExpressionInfo("0 0 9 ? * MON-FRI *", "Every WeekDay at 9 am"));
                coll.Add(new CronExpressionInfo(fxmindConstants.CRON_MANUAL, "Manual run only"));
            }
            finally
            {
                coll.EndUpdate();
            }
        }

        /*
        private void LoadMTRelatedControls()
        {
            
            XPCollection<DBSymbol> symDb = dbservice.GetSymbols();
            repositoryItemEAComboSymbol.Items.Clear();
            foreach (DBSymbol sym in symDb)
            {
                string symbol = sym.C1 + sym.C2;
                repositoryItemEAComboSymbol.Items.Add(symbol);
            }
        }*/

        protected void LoadCurrStrengthControls()
        {
            using (var app = container.Resolve<AppServiceClient>())
            {
                List<Currency> currenciesDb = app.client.GetCurrencies();
                // load currencies list
                repositoryGridLookCurrency1.DataSource = currenciesDb;
                repositoryGridLookCurrency1.View.OptionsSelection.MultiSelect = true;
                repositoryGridLookCurrency1.PopulateViewColumns();

                List<TechIndicator> techIndiDb = app.client.GetIndicators();
                repositoryGridLookIndi1.DataSource = techIndiDb;
                repositoryGridLookIndi1.View.OptionsSelection.MultiSelect = true;
                repositoryGridLookIndi1.PopulateViewColumns();
            }
        }

        private void gridView5_RowCellClick(object sender, RowCellClickEventArgs e)
        {
            if (e.Column.Name.Equals("colEnabled"))
            {
                var gridview = (GridView) sender;
                var val = (bool) e.CellValue;
                val = !val;
                var row = (Currency) gridview.GetRow(e.RowHandle);
                if (row != null && gridview.IsRowLoaded(e.RowHandle))
                {
                    row.Enabled = val;
                    using (var app = container.Resolve<AppServiceClient>())
                    {
                        app.client.SaveCurrency(row);
                    }
                }
            }
        }

        private void gridView3_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (e.Column.Name.Equals("colSchedule"))
            {
                var gridview = (GridView) sender;
                var val = (string) e.Value;

                var strJobName = (string) gridview.GetRowCellValue(gridview.FocusedRowHandle, colJobName);
                var strJobGroup = (string) gridview.GetRowCellValue(gridview.FocusedRowHandle, colJobGroup);
                using (var app = container.Resolve<AppServiceClient>())
                {
                    app.client.SetJobCronSchedule(strJobGroup, strJobName, val);
                }
            }
        }

        private void repositoryItemGridLookUpEdit1View_RowCellClick(object sender, RowCellClickEventArgs e)
        {
            if (e.Column.Name.Equals("colEnabled"))
            {
                var gridview = (GridView) sender;
                var val = (bool) e.CellValue;
                val = !val;
                var row = (TechIndicator) gridview.GetRow(e.RowHandle);
                if (row != null && gridview.IsRowLoaded(e.RowHandle))
                {
                    row.Enabled = val;
                    using (var app = container.Resolve<AppServiceClient>())
                    {

                        app.client.SaveIndicator(row);
                    }
                }
            }
        }

        private void repositoryItemCronExpressions_ParseEditValue(object sender, ConvertEditValueEventArgs e)
        {
            object oVal = e.Value;
            if (oVal == null)
                return;
            if (oVal is string)
            {
                e.Value = oVal.ToString();
            }
            else
            {
                var cron = (CronExpressionInfo) oVal;
//                string[] strings = val.Split("-".ToCharArray());
//                if (strings.Length == 2)
//                    val = strings[0];

                e.Value = cron._cron;
            }

            e.Handled = true;
        }


        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //if (appclient != null)
            //{
            //    appclient.Dispose();
            //    appclient = null;
            //}
        }

        /*
        public static MQLBridge.ExportLib.SOCKET_CLIENT client = new MQLBridge.ExportLib.SOCKET_CLIENT();
        public void TestSocketClient()
        {
            string inputStr = "555|" + "NewsEvent" + "|" + "EURUSD" + "|" + 1 + "|" + 40 + "|" + DateTime.Now.ToString() + "\n";
            string host = "127.0.0.1"; //"192.168.10.2";//"108.161.134.164";//"192.168.10.255";// "172.16.16.1";// "192.168.135.1"; // ";//"127.0.0.1";
            uint ret = 0;
            if (client.status != 1) {
                ret = MQLBridge.ExportLib.SocketOpen(ref client, host, 2010);
            }
            ret = MQLBridge.ExportLib.SocketWriteString(ref client, inputStr);
            System.Text.StringBuilder rawMessage = new StringBuilder();
            ret = MQLBridge.ExportLib.SocketReceiveString(ref client, rawMessage, 1024);
            LogAdviser("Client Received string:" + rawMessage);
            //MQLBridge.ExportLib.SocketClose(ref client);
        }
        */

        private void barButtonTestClient_ItemClick(object sender, ItemClickEventArgs e)
        {
            //TestSocketClient();
            /*
                NewsEventInfo info = null;
                DateTime now = DateTime.Parse("2014/4/24 06:00:00.000");
                //now = now.AddDays(-5);
                //now = now.AddHours(-6);
                //now = DateTime.SpecifyKind(now, DateTimeKind.Local);
            
                bool res = fxmind.GetNewsEventInfo(now, "EURUSD", 150, 0, ref info);
                if (res)
                    log.Info("Result Event on " + now + " " + info.ToString());
             */
            //DateTime now = DateTime.Parse("2014/4/25 06:00:00.000");
            //now = now.AddDays(-5);
            //now = now.AddHours(-6);
            //now = DateTime.SpecifyKind(now, DateTimeKind.Local);
            //double valLong = 0;double valShort = 0;

            //fxmind.GetAverageLastGlobalSentiments(now, "EURUSD", out valLong, out valShort);

            //log.Info("GlobalSentiments long: " + valLong + " Short: " + valShort);
            //TestThriftClientDLL();
            // TestThriftClient();
        }

        public class CronExpressionInfo
        {
            public CronExpressionInfo(string cron, string desc)
            {
                _cron = cron;
                _desc = desc;
            }

            public string _cron { get; set; }
            public string _desc { get; set; }

            public override string ToString()
            {
                return _cron + "  " + _desc;
            }
        }

        public class ProgressInitEventArgs : EventArgs
        {
            public ProgressInitEventArgs(int min, int max)
            {
                Min = min;
                Max = max;
            }
            public int Min { get; internal set; }
            public int Max { get; internal set; }
        }

        /*   public void TestThriftClient()
        {
            FXMindMQLClient client = new FXBusinessLogic.BusinessObjects.Thrift.FXMindMQLClient("127.0.0.1", 2011);
            List<string> list = new List<string>();
            list.Add("Hello from client");
            list = client.ProcessStringData(list);
            log.Info("Client got: " + list[0] + "and " + list[1]);
            
        }*/
        /*
        public void TestThriftClientDLL()
        {
            var tc = new ThriftCalls.THRIFT_CLIENT();
            //tc.host = "127.0.0.1";
            tc.port = 2011;
            tc.Magic = 5555;
            string[] instr = {"hello", "world"};
            string[] res = {"", "", "", ""};

            //ThriftCalls.ProcessStringMethod(ref tc, instr, instr.Length, res);
            //StringBuilder str = new StringBuilder("hellow|world|2");
            //ThriftCalls.ProcessStringData(str);
            string parameters = "func=CurrentSentiments|symbol=USDCHF|time=2014.05.09 23:45";
            double[] d = {2, 4};
            string data = "0|0";
            long retval = ThriftCalls.ProcessDoubleData(d, 2, parameters, data, ref tc);
            log.Info("Retval got: " + retval + "data: " + d[0] + ", " + d[1]);
        }
        */

        #region ServiceFunctionality

        private bool RefreshServicePage()
        {
            return UpdateServiceStatus();
        }

        protected bool UpdateServiceStatus()
        {
            bool status = false;
            labelStatus.Text = AdminServiceManager.GetCurrentServiceStatus();
            if (labelStatus.Text.Contains("Running"))
            {
                status = true;
            }
            else
            {
                using (var app = container.Resolve<AppServiceClient>())
                {
                    if (!app.client.IsDebug())
                        status = false;
                }
            }

            xtraTabControl1.TabPages[0].PageEnabled = true;
            return status;
        }

        private void applyBtn_Click(object sender, EventArgs e)
        {
            if (radioStart.Checked)
            {
                AdminServiceManager.EnableService();
                AdminServiceManager.StartService();
                UpdateServiceStatus();
                return;
            }

            if (radioStop.Checked)
            {
                AdminServiceManager.StopService();
                UpdateServiceStatus();
                return;
            }

            if (radioEnable.Checked)
            {
                AdminServiceManager.EnableService();
                UpdateServiceStatus();
                return;
            }

            if (radioDisable.Checked)
            {
                AdminServiceManager.StopService();
                AdminServiceManager.DisableService();
                UpdateServiceStatus();
            }
        }

        #endregion

        #region CalculateTradersCount

        public static SortedSet<string> tradersCount;

        public static void initTradersCount()
        {
            tradersCount = new SortedSet<string>();
        }

        public static void AddTrader(string strTrader)
        {
            if (tradersCount != null) tradersCount.Add(strTrader);
        }

        public static int GetTradersCountValue()
        {
            if (tradersCount != null) return tradersCount.Count();
            return 0;
        }

        public static void CleanTradersCount()
        {
            tradersCount.Clear();
            tradersCount = null;
        }

        #endregion

        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Deploy button
            using (AppServiceClient app = container.Resolve<AppServiceClient>())
            {
                app.client.Deploy();
            }
        }

        private void barButtonItem7_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}