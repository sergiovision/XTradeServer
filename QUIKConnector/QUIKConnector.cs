using BusinessObjects;
using Ecng.Collections;
using log4net;
using StockSharp.BusinessEntities;
using StockSharp.Logging;
using StockSharp.Messages;
using StockSharp.Quik;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogManager = StockSharp.Logging.LogManager;

namespace QUIK
{
    public class QUIKConnector : ITerminalConnector
    {
        //private static readonly ILog log = log4net.LogManager.GetLogger(typeof(QUIKConnector));
        private readonly LogManager _logManager = new LogManager();
        protected bool _isConnected;
        public Dictionary<int, IExpert> advisers;
        public bool bStopJobs;
        protected IWebLog log;

        public IMainService service;

        public Terminal terminal;

        public QuikTrader Trader;

        public QUIKConnector(IMainService serv, IWebLog l)
        {
            Portfolios = new ListEx<Portfolio>();
            Positions = new ListEx<BasePosition>();
            Orders = new ListEx<Order>();
            Securities = new SynchronizedList<Security>();
            advisers = new Dictionary<int, IExpert>();
            service = serv;
            log = l;
        }

        //List<SecurityInfo> listSecurityInfo;
        //List<DepoLimitEx> listDepoLimits;
        //List<PortfolioInfoEx> listPortfolio;
        //List<MoneyLimit> listMoneyLimits;
        //List<MoneyLimitEx> listMoneyLimitsEx;
        //FormOutputTable toolCandlesTable;
        //
        // Summary:
        //     Список портфелей, добавленных в таблицу.
        public ListEx<Portfolio> Portfolios { get; }

        //
        // Summary:
        //     Список позиций, добавленных в таблицу.
        public ListEx<BasePosition> Positions { get; }
        public ListEx<Order> Orders { get; }
        public SynchronizedList<Security> Securities { get; }

        public bool Connect(Terminal toTerminal)
        {
            try
            {
                if (Trader == null)
                {
                    // создаем подключение
                    Trader = new QuikTrader {IsDde = false, Path = toTerminal.FullPath};
                    /*
                    {
                        LuaFixServerAddress = Address.Text.To<EndPoint>(),
                        LuaLogin = Login.Text,
                        LuaPassword = Password.Password.To<SecureString>()
                    };
                    */

                    Trader.LogLevel = LogLevels.Info;

                    _logManager.Sources.Add(Trader);
                    _logManager.Listeners.Add(new FileLogListener("XTrade.Quik.log"));

                    // отключение автоматического запроса всех инструментов.
                    Trader.RequestAllSecurities = false;

                    // возводим флаг, что соединение установлено
                    _isConnected = true;

                    // переподключение будет работать только во время работы биржи РТС
                    // (чтобы отключить переподключение когда торгов нет штатно, например, ночью)
                    Trader.ReConnectionSettings.WorkingTime = ExchangeBoard.Forts.WorkingTime;

                    // подписываемся на событие об успешном восстановлении соединения
                    Trader.Restored += () =>
                    {
                        Log("Connection restored");
                    }; // MessageBox.Show(this, LocalizedStrings.Str2958));

                    // подписываемся на событие разрыва соединения
                    Trader.ConnectionError += error =>
                    {
                        Log(error.ToString());
                    }; //this.GuiAsync(() => MessageBox.Show(this, error.ToString()));

                    // подписываемся на ошибку обработки данных (транзакций и маркет)
                    Trader.Error += error => { Log(error.ToString()); };
                    //	this.GuiAsync(() => MessageBox.Show(this, error.ToString(), "Ошибка обработки данных"));

                    // подписываемся на ошибку подписки маркет-данных
                    Trader.MarketDataSubscriptionFailed += (security, msg, error) => { Log(error.ToString()); };
                    // this.GuiAsync(() => MessageBox.Show(this, error.ToString(), LocalizedStrings.Str2956Params.Put(msg.DataType, security)));

                    Trader.NewSecurity += Securities.Add;
                    //Trader.NewMyTrade += _myTradesWindow.TradeGrid.Trades.Add;
                    //Trader.NewTrade += _tradesWindow.TradeGrid.Trades.Add;
                    Trader.NewOrder += Orders.Add;
                    Trader.NewStopOrder += Orders.Add;
                    //Trader.OrderRegisterFailed += _ordersWindow.OrderGrid.AddRegistrationFail;
                    //Trader.StopOrderRegisterFailed += _stopOrdersWindow.OrderGrid.AddRegistrationFail;
                    Trader.OrderCancelFailed += fail => { Log(fail.Error.Message); };
                    // this.GuiAsync(() => MessageBox.Show(this, fail.Error.Message, LocalizedStrings.Str2981));
                    Trader.StopOrderCancelFailed += fail => { Log(fail.Error.Message); };
                    //this.GuiAsync(() => MessageBox.Show(this, fail.Error.Message, LocalizedStrings.Str2981));
                    Trader.NewPortfolio += Portfolios.Add;
                    Trader.NewPosition += Positions.Add;

                    Trader.MassOrderCancelFailed += (transId, error) => { Log(error.ToString()); };
                    //this.GuiAsync(() => MessageBox.Show(this, error.ToString(), LocalizedStrings.Str716));

                    // устанавливаем поставщик маркет-данных
                    // _securitiesWindow.SecurityPicker.MarketDataProvider = Trader;

                    //ShowSecurities.IsEnabled = ShowTrades.IsEnabled =
                    //    ShowMyTrades.IsEnabled = ShowOrders.IsEnabled =
                    //        ShowPortfolios.IsEnabled = ShowStopOrders.IsEnabled = true;

                    Trader.Connect();

                    _isConnected = true;

                    bStopJobs = false;

                    terminal = toTerminal;
                    //var advs = MainService.thisGlobal.GetAdvisersByTerminal(terminal.Id);
                    Portfolio portfolio = null;
                    try
                    {
                        var res = service.GetAdvisers().Where(x => x.TerminalId == toTerminal.Id);
                        foreach (var adv in res)
                            if (!adv.Disabled)
                            {
                                QUIKExpert quikE = new QUIKExpert(adv);
                                advisers.Add(adv.Id, quikE);
                                service.SubscribeToSignals(adv.Id);
                                if (string.IsNullOrEmpty(adv.State))
                                {
                                    adv.State = quikE.Serialize();
                                    service.UpdateAdviser(adv);
                                }

                                if (portfolio == null)
                                    portfolio = Portfolios.Where(x => x.Name == quikE.PortfolioName).FirstOrDefault();
                            }

                        Log("Successfully connected to <QUIK>");
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                    }

                    return true;
                }

                Trader.Disconnect();

                _isConnected = false;
                return false;
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }

            return false;
        }


        public void Dispose()
        {
            if (_isConnected)
                try
                {
                    bStopJobs = true;
                    if (Trader != null)
                    {
                        Trader.Disconnect();
                        Trader.Dispose();
                        Trader = null;
                    }

                    Log("Disconnected from <QUIK>");
                }
                catch (Exception e)
                {
                    Log("Problem Disconnecting <QUIK>" + e);
                }
        }

        public void MarketOrder(SignalInfo signal, IExpert adv)
        {
            try
            {
                if (adv == null)
                    return;
                // decimal priceInOrder = Math.Round(tool.LastPrice + tool.Step * 5, tool.PriceAccuracy);
                // decimal priceInOrder = 0;// Math.Round(tool.LastPrice, tool.PriceAccuracy);
                int qty = (int) adv.Volume();

                var portfolio = Portfolios.Where(x => x.Name == adv.AccountName()).FirstOrDefault();
                if (!Trader.RegisteredPortfolios.Contains(portfolio)) Trader.RegisterPortfolio(portfolio);

                Order order = new Order();
                order.Type = OrderTypes.Market;
                var securities = Securities.Where(x => x.Code == adv.Symbol());
                if (securities != null && securities.Count() > 0)
                {
                    order.Security = securities.FirstOrDefault();
                }
                else
                {
                    order.Security = new Security();
                    order.Security.Code = adv.Symbol();
                    order.Security.Id = adv.Symbol() + "@FORTS";
                }

                order.Comment = adv.Comment();
                order.Portfolio = portfolio;

                if (!Trader.RegisteredSecurities.Contains(order.Security))
                {
                    Trader.RegisterSecurity(order.Security);
                    Trader.RegisterTrades(order.Security);
                }

                order.Volume = qty;
                order.Direction = signal.Value == 0 ? Sides.Buy : Sides.Sell;

                Log(
                    $"Expert <{adv.AccountName()}> On {adv.Symbol()} {order.Direction.ToString()} Register order: lots=" +
                    qty);
                Trader.RegisterOrder(order);
            }
            catch (Exception e)
            {
                Log($"Expert <{adv.AccountName()}> Error registering order: " + e);
            }
        }

        public List<PositionInfo> GetActivePositions()
        {
            if (_isConnected == false)
                return null;
            List<PositionInfo> positionsList = new List<PositionInfo>();
            return positionsList;
        }

        public Dictionary<int, IExpert> GetRunningAdvisers()
        {
            return advisers;
        }

        public bool IsStopped()
        {
            return bStopJobs;
        }

        private void Log(string message)
        {
            log.Log(message);
        }
    }
}