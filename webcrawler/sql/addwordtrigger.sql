USE [filmreviews]
GO
/****** Object:  Trigger [dbo].[addword]    Script Date: 17.10.2013 03:29:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TRIGGER [dbo].[addword]
   ON [dbo].[filteredreviews]
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	DECLARE @STRING nvarchar(max)
	DECLARE @SLICE nvarchar(max)
	DECLARE @Delimiter char(1)=' '
	DECLARE @INDEX INT
	DECLARE @RID INT
	DECLARE @RATING FLOAT
	SELECT @INDEX=1
	SELECT @STRING=RTRIM(LTRIM(ffiltered)) FROM inserted
	SELECT @RID=rid FROM inserted
	SELECT @RATING=frating FROM inserted
	WHILE @INDEX !=0
	BEGIN	
		IF LEN(@STRING) = 0 BREAK
		SELECT @INDEX = CHARINDEX(@Delimiter,@STRING)             
		IF @INDEX !=0
			SELECT @SLICE = RTRIM(LTRIM(LEFT(@STRING,@INDEX - 1)))
		ELSE
			SELECT @SLICE = RTRIM(LTRIM(@STRING))             
		INSERT INTO reviewwords(rid,rating,word) VALUES(@RID,@RATING, @SLICE)  
		IF (LEN(@STRING) - @INDEX)>0       
			SELECT @STRING = RIGHT(@STRING,LEN(@STRING) - @INDEX)
		IF LEN(@STRING) = 0 BREAK
	END
END
