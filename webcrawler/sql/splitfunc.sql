CREATE FUNCTION Split(@STRING nvarchar(4000), @Delimiter char(1))
RETURNS @Results TABLE (Items nvarchar(4000))
AS
BEGIN
DECLARE @INDEX INT
DECLARE @SLICE nvarchar(4000)
SELECT @INDEX = 1
        IF @STRING IS NULL RETURN
        WHILE @INDEX !=0
        BEGIN
            SELECT @INDEX = CHARINDEX(@Delimiter,@STRING)
             
           IF @INDEX !=0

                SELECT @SLICE = RTRIM(LTRIM(LEFT(@STRING,@INDEX - 1)))

            ELSE

                SELECT @SLICE = RTRIM(LTRIM(@STRING))

             
            INSERT INTO @Results(Items) VALUES(@SLICE)
         
            SELECT @STRING = RIGHT(@STRING,LEN(@STRING) - @INDEX)
             

            IF LEN(@STRING) = 0 BREAK

        END

    RETURN

END