# FXMindNET
Forex robot written in .NET, apache thrift and MQL, that helps you train with Forex robot or earn money while you sleep.

Project built using following technologies and libraries.

MQL - Forex robot logic
C# - Server and Client app

Apache Thrift - for Metatrader extension and C# code to fetch data from the C# part of the code to MQL robot algorithm
Quartz.NET - to scheduled jobs execution. Get Currencies Rates, News,  Financial data on scheduled basis using this wonderful library.
SQLServer - to store financial data in DB and for trading history. 

Folder lists:

BusinessObjects - definition of C# entities

FXBusinessLoigic - main business logic of C# part of the robot. Fetching/storing Financial data in DB 
MainServer - Windows Service code. C# part of the robots runs as a background Windows service.

MQL - MQL part of the robot - most interesting algorithms that may earn you money and good for developing/modifying Forex robots.

ThriftMQL - bridge between MQL and C# using Apache Thrift C# library.

WinClient - Winforms client written using Devexpress to configure robot and see financial data. Written using Winforms and Devexpress. Devexpress 17 or later needed to be installed with license.


Project developed completely by me - Sergei Zhuravlev - Experienced Software Developer - as my hobby project. Because I worked for one of the forex brokers 
and for private trader and got engough experience to write this robot myself.
For learning forex trading, robots and perform automated trading.

Code tested and works stable on the server so you can sleep calm when robot runs. But you still have to properly configure Forex robot algorithm and sure that your server has permanent internet connection and forex dealer is reliable. Robot better works with fixed spreads.

If you need help building and setting up robot write me to my email: sergewinner @ gmail com

If robot help you earn money you can thank me using Donation of any amount on my PayPal account sergeiwinner @ gmail  com or donate to my BTC wallet: 1NZfLceM6fMd8iSeY5PdJv1Q4z65MaqBud

Allowed to use in your programs and enhance your robots, just mention my name in your products.

Copyright (C) Sergei Zhuravlev 2018
