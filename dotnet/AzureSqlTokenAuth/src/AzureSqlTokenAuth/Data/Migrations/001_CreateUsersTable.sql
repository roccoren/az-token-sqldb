IF NOT EXISTS (SELECT *
FROM sys.tables
WHERE name = 'users' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[users]
    (
        [id] BIGINT PRIMARY KEY IDENTITY(1,1),
        [name] NVARCHAR(100) NOT NULL,
        [email] NVARCHAR(255) NOT NULL,
        [created_at] DATETIME2(0) NOT NULL DEFAULT GETUTCDATE(),
        [updated_at] DATETIME2(0) NOT NULL DEFAULT GETUTCDATE()
    );

    -- Add a unique constraint on email
    CREATE UNIQUE INDEX [IX_users_email] ON [dbo].[users] ([email]);

    -- Add comments
    IF NOT EXISTS (
        SELECT *
    FROM sys.extended_properties
    WHERE major_id = OBJECT_ID('dbo.users')
        AND name = 'MS_Description'
    )
    BEGIN
        EXEC sys.sp_addextendedproperty
            @name = N'MS_Description',
            @value = N'User accounts table',
            @level0type = N'SCHEMA', @level0name = N'dbo',
            @level1type = N'TABLE',  @level1name = N'users';
    END
END