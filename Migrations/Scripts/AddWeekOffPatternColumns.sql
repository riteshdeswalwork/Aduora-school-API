IF COL_LENGTH('WeekOffs', 'PatternType') IS NULL
BEGIN
    ALTER TABLE [WeekOffs] ADD [PatternType] nvarchar(20) NOT NULL CONSTRAINT [DF_WeekOffs_PatternType] DEFAULT N'EveryWeek';
END

IF COL_LENGTH('WeekOffs', 'WeekOccurrences') IS NULL
BEGIN
    ALTER TABLE [WeekOffs] ADD [WeekOccurrences] nvarchar(50) NOT NULL CONSTRAINT [DF_WeekOffs_WeekOccurrences] DEFAULT N'';
END
