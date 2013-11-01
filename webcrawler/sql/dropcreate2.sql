USE [filmreviews]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE [dbo].[filteredreviews];

CREATE TABLE [dbo].[filteredreviews](
	[frid] [bigint] IDENTITY(1,1) NOT NULL,
	[rid] [bigint] NULL,
	[frating] [float] NULL,
	[ffiltered] [nvarchar](max) NULL,
	[fresidue] [nvarchar](max) NULL,
 CONSTRAINT [PK_filteredreviews] PRIMARY KEY CLUSTERED 
(
	[frid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


USE [filmreviews]
GO

/****** Object:  Table [dbo].[reviewwords]    Script Date: 17.10.2013 02:36:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE [dbo].[reviewwords]
CREATE TABLE [dbo].[reviewwords](
	[wid] [bigint] IDENTITY(1,1) NOT NULL,
	[rid] [int] NULL,
	[rating] [float] NULL,
	[word] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO



