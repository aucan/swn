USE [filmreviews]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Drop table [dbo].[films]
GO

CREATE TABLE [dbo].[films](
	[fid] [int] IDENTITY(1,1) NOT NULL,
	[fname] [nvarchar](max) NULL,
	[flink] [nvarchar](max) NULL,
	[frewcount] [int] NULL,
	[sid] [int] NULL,
	[frate] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

DROP TABLE [dbo].[reviews]
GO 

CREATE TABLE [dbo].[reviews](
	[rid] [int] IDENTITY(1,1) NOT NULL,
	[fid] [int] NULL,
	[rlink] [nvarchar](max) NULL,
	[rcontent] [nvarchar](max) NULL,
	[rrating] [float] NULL,
	[rfiltered] [nvarchar](max) NULL,
	[rresidue] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO