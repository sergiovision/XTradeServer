XTrade is a good project for managing trading experience and personal capital state.
Project consists from Server XTradeServer app and XTrade Web App. XTrade Main Server is a Windows service which manages all trading terminals, process all data and signals, stores everything in MySQL database, hosts XTrade Web app as a self host. 
XTrade Web App is an Angular Web application which administrates XTrade Server.

Application features list is in [Readme.pdf](https://github.com/sergiovision/XTradeWeb/blob/master/Readme.pdf) file in root folder.

How to build server application:
1. Clone this repository
2. Run from command line: build.bat

To make build succeeded the following apps should be installed: Visual Studio 2017, Visual Studio 2017 Build tools.
Applications need to be installed to run server properly: 

1. MySQL Server version 5 or later.
2. Metatrader 5 Terminal
3. Optionally - QUIK terminal.
4. Optionally StockSharp applications in case if you trade with QUIK and/or Cryptos. StockSharp is free and can be downloaded here http://stocksharp.com.

MySQL database script located in /DB folder. Create database and name it “xtrade”. Run both sql scripts in DB folder.
Open Settings table and set the following variables

XTrade.TerminalUser - should be set to windows user login name where trading terminals will be running

XTrade.InstallDir - XTrade installation folder.

Metatrader.CommonFiles - path to MT5 common files folder

MQL.Sources - path to MQL folder where your MQL robots stored

XTrade Server folders structure:

[/bin](https://github.com/sergiovision/XTradeServer/tree/master/bin) - binary folder where server binaries stored.

/dist - location of XTeade WebApp production build files.

[/BusinessLogic](https://github.com/sergiovision/XTradeServer/tree/master/BusinessLogic) - main app logic

[/BusinessObjects](https://github.com/sergiovision/XTradeServer/tree/master/BusinessLogic/BusinessObjects) - shared business objects

[/MainServer](https://github.com/sergiovision/XTradeServer/tree/master/MainServer) - main server self host and WebAPI controllers

[/MQL](https://github.com/sergiovision/XTradeServer/tree/master/MQL) - MQL sources of trading robot.

[/QUIKConnector](https://github.com/sergiovision/XTradeServer/tree/master/QUIKConnector) - connector library to QUIK terminal using StockSharp libraries.

[/UnitTests](https://github.com/sergiovision/XTradeServer/tree/master/UnitTests) - Tests of server WebAPI

To run application MySQL database should be created from DB folder.
[/bin/XTrade.config](https://github.com/sergiovision/XTradeServer/blob/master/bin/XTrade.config) should point to proper MySQL server.

run: XTrade.MainServer.exe install 

to install windows service.

Go to services.msc 
and run XTrade Main Server.

If you have problems running check XTrade.MainServer.log to see errors.


***Warning***:
It is a free version of application. Application works and can be used on real trading accounts. This repository contains free alfa version. You can base your trading software solutions on this app. If you need help with application installation/run/clarification on your trade server you can write me, but consultation is not free. Also better version of this application available which can be purchased or modified to your needs for money, contact me if you like application idea and ready to invest money/time to adapt this application to your trading goals.

If you are a programmer and want to start learn trading this application is a good free choice.
If you are a professional trader then you can hire me to improve this app and apapt it to your trading goals.

XTrade Web app repository and build instructions here [XTradeWeb](https://github.com/sergiovision/XTradeWeb)

