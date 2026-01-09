using MentorEval.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;

namespace MentorEval.UnitTests.Infrastructure;
public static class DbTestHelper
{
    public sealed class DbHandle : IDisposable
    {
        public SqliteConnection Connection { get; }
        public AppDbContext Db { get; }

        public DbHandle(SqliteConnection connection, AppDbContext db)
        {
            Connection = connection;
            Db = db;
        }

        public void Dispose()
        {
            Db.Dispose();
            Connection.Dispose();
        }
    }

    public static DbHandle CreateSqliteInMemoryDb()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();

        return new DbHandle(connection, db);
    }
}
