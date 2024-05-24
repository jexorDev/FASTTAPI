USE [FASTTAPI]	
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * 
               FROM INFORMATION_SCHEMA.TABLES 
               WHERE TABLE_SCHEMA = 'dbo'  
			   AND TABLE_NAME = 'Flights')
BEGIN
	CREATE TABLE [dbo].[Flights] (
		[PK]                INT           IDENTITY (0, 1) NOT NULL,
		[Disposition]       BIT           NOT NULL,
		[FlightNumber]      VARCHAR (4)   NOT NULL,
		[Airline]           CHAR (3)      NOT NULL,
		[DateTimeScheduled] DATETIME      NULL,
		[DateTimeEstimated] DATETIME      NULL,
		[DateTimeActual]    DATETIME      NULL,
		[Gate]              VARCHAR (4)   NULL,
		[CityName]          VARCHAR (256) NOT NULL,
		[CityAirportName]   VARCHAR (256) NOT NULL,
		[CityAirportCode]   CHAR (3)      NOT NULL,
		[DateTimeCreated]   DATETIME      NOT NULL,
		[DateTimeUpdated]   DATETIME      NULL
	);
END
GO; 

IF NOT EXISTS (SELECT * 
               FROM INFORMATION_SCHEMA.TABLES 
               WHERE TABLE_SCHEMA = 'dbo' 
               AND  TABLE_NAME = 'FlightsCodeSharePartners')
BEGIN
	CREATE TABLE [dbo].[FlightCodeSharePartners] (
		[PK]                INT           IDENTITY (0, 1) NOT NULL,
		[FlightPK]          INT           NOT NULL,
		[CodeshareID]       VARCHAR (8)   NOT NULL
		CONSTRAINT FK_Flights_PK FOREIGN KEY (FlightPK) REFERENCES dbo.Flights (PK)
	);
END
GO;

IF NOT EXISTS(SELECT 1 FROM sys.columns 
			  WHERE Name = N'IsStale'
              AND Object_ID = Object_ID(N'dbo.Flights'))
BEGIN
    ALTER TABLE [dbo].[Flights]
	ADD IsStale BIT NOT NULL CONSTRAINT DF_Flights_IsStale DEFAULT(0) 
END
GO;

IF NOT EXISTS(SELECT 1 FROM sys.columns 
			  WHERE Name = N'IsStale'
			  AND Object_ID = Object_ID(N'dbo.Flights'))
BEGIN
    ALTER TABLE [dbo].[Flights]
	ADD IsStale BIT NOT NULL CONSTRAINT DF_Flights_IsStale DEFAULT(0) 
END
GO;

IF NOT EXISTS (SELECT * 
               FROM SYS.INDEXES
               WHERE NAME = 'IX_Flights_DateTimeScheduled' 
               AND  OBJECT_ID = OBJECT_ID('Flights'))
BEGIN			   
	CREATE NONCLUSTERED INDEX [IX_Flights_DateTimeScheduled]
		ON [dbo].[Flights]([DateTimeScheduled] ASC);
END