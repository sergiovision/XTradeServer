/*
 Navicat Premium Data Transfer

 Source Server         : localhost
 Source Server Type    : MySQL
 Source Server Version : 80013
 Source Host           : localhost
 Source Database       : xtrade

 Target Server Type    : MySQL
 Target Server Version : 80013
 File Encoding         : utf-8

 Date: 01/17/2019 20:27:06 PM
*/

SET NAMES utf8;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
--  Table structure for `account`
-- ----------------------------
DROP TABLE IF EXISTS `account`;
CREATE TABLE `account` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Number` int(11) NOT NULL DEFAULT '0',
  `Description` varchar(256) DEFAULT NULL,
  `CurrencyId` int(11) NOT NULL DEFAULT '1' COMMENT 'Account currency',
  `WalletId` int(11) NOT NULL,
  `TerminalId` int(11) DEFAULT NULL,
  `Balance` decimal(10,2) DEFAULT '0.00',
  `Equity` decimal(10,2) DEFAULT NULL,
  `LastUpdate` datetime DEFAULT CURRENT_TIMESTAMP,
  `Retired` bit(1) NOT NULL DEFAULT b'0',
  `PersonId` int(1) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `CurrencyId` (`CurrencyId`),
  KEY `WalletId` (`WalletId`),
  KEY `PersonId` (`PersonId`),
  KEY `TerminalId` (`TerminalId`),
  CONSTRAINT `account_fx_wallet` FOREIGN KEY (`WalletId`) REFERENCES `wallet` (`id`),
  CONSTRAINT `account_ibfk_3` FOREIGN KEY (`CurrencyId`) REFERENCES `currency` (`id`),
  CONSTRAINT `personid_fk` FOREIGN KEY (`PersonId`) REFERENCES `person` (`id`),
  CONSTRAINT `terminal_fk` FOREIGN KEY (`TerminalId`) REFERENCES `terminal` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=119 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `accountstate`
-- ----------------------------
DROP TABLE IF EXISTS `accountstate`;
CREATE TABLE `accountstate` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AccountId` int(11) NOT NULL,
  `Date` datetime NOT NULL,
  `Balance` decimal(10,2) NOT NULL DEFAULT '0.00',
  `Comment` varchar(4096) DEFAULT NULL,
  `Formula` varchar(4096) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `AccountId` (`AccountId`),
  CONSTRAINT `accountid_ibfk_1` FOREIGN KEY (`AccountId`) REFERENCES `account` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=3624 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `adviser`
-- ----------------------------
DROP TABLE IF EXISTS `adviser`;
CREATE TABLE `adviser` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(128) DEFAULT NULL,
  `TerminalId` int(11) DEFAULT NULL,
  `SymbolId` int(11) DEFAULT NULL,
  `Timeframe` varchar(64) DEFAULT NULL,
  `Running` bit(1) DEFAULT b'0',
  `Disabled` bit(1) DEFAULT b'0',
  `State` text,
  `Lastupdate` datetime DEFAULT CURRENT_TIMESTAMP,
  `Closereason` int(11) DEFAULT NULL,
  `ClusterId` int(11) DEFAULT NULL,
  `SaveOrders` text,
  PRIMARY KEY (`Id`),
  KEY `TerminalId` (`TerminalId`),
  KEY `SymbolId` (`SymbolId`),
  KEY `Id` (`Id`),
  KEY `Id_2` (`Id`),
  KEY `Id_3` (`Id`),
  KEY `ClusterId` (`ClusterId`),
  KEY `Cluster_Id_idx` (`ClusterId`),
  KEY `ClusterId_2` (`ClusterId`),
  KEY `ClusterId_3` (`ClusterId`),
  CONSTRAINT `adviser_ibfk_1` FOREIGN KEY (`TerminalId`) REFERENCES `terminal` (`id`),
  CONSTRAINT `adviser_ibfk_2` FOREIGN KEY (`SymbolId`) REFERENCES `symbol` (`id`),
  CONSTRAINT `cluster_fk` FOREIGN KEY (`ClusterId`) REFERENCES `expertcluster` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=118 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `country`
-- ----------------------------
DROP TABLE IF EXISTS `country`;
CREATE TABLE `country` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Code` varchar(2) NOT NULL,
  `CurrencyId` int(6) DEFAULT NULL,
  `Description` varchar(256) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `ID_IDX` (`Id`) USING BTREE,
  KEY `CurrencyId` (`CurrencyId`),
  CONSTRAINT `Country_Currency_fk` FOREIGN KEY (`CurrencyId`) REFERENCES `currency` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=254 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `currency`
-- ----------------------------
DROP TABLE IF EXISTS `currency`;
CREATE TABLE `currency` (
  `Id` int(11) NOT NULL,
  `Name` varchar(32) NOT NULL,
  `Enabled` tinyint(1) DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `ID` (`Id`),
  KEY `ID_2` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `deals`
-- ----------------------------
DROP TABLE IF EXISTS `deals`;
CREATE TABLE `deals` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `OrderId` bigint(18) DEFAULT '0',
  `DealId` bigint(18) DEFAULT NULL,
  `TerminalId` int(11) NOT NULL,
  `Opentime` datetime NOT NULL,
  `SymbolId` int(11) NOT NULL,
  `Typ` tinyint(6) NOT NULL,
  `Volume` decimal(11,2) DEFAULT NULL,
  `Price` decimal(18,8) DEFAULT NULL,
  `Closetime` datetime DEFAULT NULL,
  `comment` varchar(256) DEFAULT NULL,
  `AdviserId` int(11) DEFAULT NULL,
  `Commission` decimal(11,2) DEFAULT NULL,
  `Swap` decimal(11,2) DEFAULT NULL,
  `Profit` decimal(11,2) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `DealId` (`DealId`),
  KEY `SymbolId` (`SymbolId`),
  KEY `AdviserId` (`AdviserId`),
  KEY `TerminalId` (`TerminalId`),
  CONSTRAINT `Adviser_fk` FOREIGN KEY (`AdviserId`) REFERENCES `adviser` (`id`),
  CONSTRAINT `SymId_fk` FOREIGN KEY (`SymbolId`) REFERENCES `symbol` (`id`),
  CONSTRAINT `Terminal_fx` FOREIGN KEY (`TerminalId`) REFERENCES `terminal` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=910 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `expertcluster`
-- ----------------------------
DROP TABLE IF EXISTS `expertcluster`;
CREATE TABLE `expertcluster` (
  `Id` int(11) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `MetasymbolId` int(11) NOT NULL,
  `AdviserId` int(11) NOT NULL,
  `Typ` smallint(6) DEFAULT '0',
  `Retired` bit(1) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `MetasymbolId` (`MetasymbolId`),
  KEY `AdviserId` (`AdviserId`),
  CONSTRAINT `metasym_fk` FOREIGN KEY (`MetasymbolId`) REFERENCES `metasymbol` (`id`),
  CONSTRAINT `refadviser_fk` FOREIGN KEY (`AdviserId`) REFERENCES `adviser` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `jobs`
-- ----------------------------
DROP TABLE IF EXISTS `jobs`;
CREATE TABLE `jobs` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Classpath` varchar(255) NOT NULL,
  `Grp` varchar(255) NOT NULL DEFAULT 'MANUAL',
  `Name` varchar(255) NOT NULL,
  `Cron` varchar(128) NOT NULL DEFAULT '0 0 0 1 1 ? 2100',
  `Description` varchar(1000) DEFAULT NULL,
  `Statmessage` longtext,
  `Prevdate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Nextdate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Params` longtext,
  `Disabled` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `metasymbol`
-- ----------------------------
DROP TABLE IF EXISTS `metasymbol`;
CREATE TABLE `metasymbol` (
  `Id` int(11) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `C1` varchar(32) DEFAULT NULL,
  `C2` varchar(32) DEFAULT NULL,
  `Typ` smallint(6) DEFAULT '0',
  `Retired` bit(1) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `Id` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `newsevent`
-- ----------------------------
DROP TABLE IF EXISTS `newsevent`;
CREATE TABLE `newsevent` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `CurrencyId` int(6) NOT NULL,
  `HappenTime` datetime NOT NULL,
  `Name` varchar(500) NOT NULL,
  `Importance` tinyint(3) unsigned NOT NULL,
  `ActualVal` varchar(127) DEFAULT NULL,
  `ForecastVal` varchar(127) DEFAULT NULL,
  `PreviousVal` varchar(127) DEFAULT NULL,
  `ParseTime` datetime NOT NULL,
  `Raised` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `iCurrencyId_NewsEvent` (`CurrencyId`),
  CONSTRAINT `News_Currency_fk` FOREIGN KEY (`CurrencyId`) REFERENCES `currency` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=33957 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `person`
-- ----------------------------
DROP TABLE IF EXISTS `person`;
CREATE TABLE `person` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `LanguageId` mediumint(9) NOT NULL DEFAULT '0',
  `Credential` longtext NOT NULL,
  `RegIp` longtext NOT NULL,
  `Mail` varchar(255) NOT NULL,
  `CountryId` int(11) DEFAULT NULL,
  `Privilege` varchar(50) DEFAULT NULL,
  `Uuid` longtext,
  `Activated` bit(1) DEFAULT NULL,
  `Retired` bit(1) DEFAULT b'0',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uc_PersonMail` (`Mail`),
  KEY `country_idx` (`CountryId`) USING BTREE,
  CONSTRAINT `person_ibfk_1` FOREIGN KEY (`CountryId`) REFERENCES `country` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `rates`
-- ----------------------------
DROP TABLE IF EXISTS `rates`;
CREATE TABLE `rates` (
  `Id` int(11) NOT NULL,
  `MetaSymbolId` int(10) NOT NULL,
  `Ratebid` decimal(19,8) NOT NULL,
  `Rateask` decimal(19,8) DEFAULT NULL,
  `Lastupdate` datetime DEFAULT CURRENT_TIMESTAMP,
  `Retired` bit(1) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `MetaSymbolId` (`MetaSymbolId`),
  CONSTRAINT `MetaSymbol_idx` FOREIGN KEY (`MetaSymbolId`) REFERENCES `metasymbol` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `settings`
-- ----------------------------
DROP TABLE IF EXISTS `settings`;
CREATE TABLE `settings` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Propertyname` varchar(255) NOT NULL,
  `Value` longtext,
  `Description` longtext,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `NAME_IDX` (`Propertyname`)
) ENGINE=InnoDB AUTO_INCREMENT=29 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `site`
-- ----------------------------
DROP TABLE IF EXISTS `site`;
CREATE TABLE `site` (
  `ID` int(11) NOT NULL,
  `Name` varchar(50) DEFAULT NULL,
  `Description` varchar(255) DEFAULT NULL,
  `URL` varchar(1024) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `statsdate`
-- ----------------------------
DROP TABLE IF EXISTS `statsdate`;
CREATE TABLE `statsdate` (
  `Id` int(11) NOT NULL,
  `Typ` int(10) NOT NULL,
  `DailyValue` decimal(11,2) DEFAULT NULL,
  `WeeklyValue` decimal(11,2) DEFAULT NULL,
  `Lastupdate` datetime DEFAULT CURRENT_TIMESTAMP,
  `Monthly` decimal(11,2) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `statsymbol`
-- ----------------------------
DROP TABLE IF EXISTS `statsymbol`;
CREATE TABLE `statsymbol` (
  `Id` int(11) NOT NULL,
  `MetasymbolId` int(11) NOT NULL,
  `AverageValue` decimal(11,2) DEFAULT NULL,
  `Lastupdate` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `MetasymbolId` (`MetasymbolId`),
  CONSTRAINT `MetaSymbol_fk` FOREIGN KEY (`MetasymbolId`) REFERENCES `metasymbol` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `symbol`
-- ----------------------------
DROP TABLE IF EXISTS `symbol`;
CREATE TABLE `symbol` (
  `Id` int(11) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `MetaSymbolId` int(11) DEFAULT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `Retired` tinyint(1) DEFAULT NULL,
  `Expiration` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  KEY `MetaSymbolId` (`MetaSymbolId`),
  KEY `ID` (`Id`),
  CONSTRAINT `MetasymbolId` FOREIGN KEY (`MetaSymbolId`) REFERENCES `metasymbol` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `terminal`
-- ----------------------------
DROP TABLE IF EXISTS `terminal`;
CREATE TABLE `terminal` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `AccountNumber` int(11) DEFAULT '0',
  `AccountId` int(11) DEFAULT NULL,
  `Broker` varchar(128) NOT NULL,
  `Fullpath` longtext NOT NULL,
  `Codebase` varchar(4096) DEFAULT NULL,
  `Disabled` bit(1) DEFAULT b'0',
  `Demo` bit(1) DEFAULT b'0',
  `Stopped` bit(1) DEFAULT b'0',
  PRIMARY KEY (`Id`),
  KEY `AccountId` (`AccountId`),
  KEY `Id` (`Id`),
  CONSTRAINT `Account_fk` FOREIGN KEY (`AccountId`) REFERENCES `account` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

-- ----------------------------
--  Table structure for `wallet`
-- ----------------------------
DROP TABLE IF EXISTS `wallet`;
CREATE TABLE `wallet` (
  `Id` int(11) NOT NULL,
  `Name` varchar(127) NOT NULL COMMENT 'Income name',
  `Shortname` varchar(64) DEFAULT NULL,
  `Link` varchar(4096) DEFAULT NULL,
  `Retired` bit(1) DEFAULT NULL,
  `PersonId` int(11) DEFAULT NULL,
  `SiteId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `PersonId` (`PersonId`),
  KEY `SiteId` (`SiteId`),
  CONSTRAINT `PersonFk` FOREIGN KEY (`PersonId`) REFERENCES `person` (`id`),
  CONSTRAINT `SiteId` FOREIGN KEY (`SiteId`) REFERENCES `site` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

SET FOREIGN_KEY_CHECKS = 1;
