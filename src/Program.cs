using System;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
class Program{
    private static string connectionStringSTT = @"Server=localhost\SQLEXPRESS;Database=STT;password=Derpderp;Trusted_Connection=True;TrustServerCertificate=True;";
    private static void initializeDB(){
        var connectionStringMaster = @"Server=localhost\SQLEXPRESS;Database=master;password=Derpderp;Trusted_Connection=True;TrustServerCertificate=True;";
        using var connection = new SqlConnection(connectionStringMaster);
        connection.Open();

        var dbName = "STT";
        var dropDbSql = $"IF DB_ID('{dbName}') IS NOT NULL DROP DATABASE {dbName}";
        var createDbSql = $"IF DB_ID('{dbName}') IS NULL CREATE DATABASE {dbName}";
        try{
            using var dropCommand = new SqlCommand(dropDbSql, connection);
            dropCommand.ExecuteNonQuery();
            Console.WriteLine($"Database '{dbName}' deleted successfully.");
            using var createCommand = new SqlCommand(createDbSql, connection);
            createCommand.ExecuteNonQuery();
            Console.WriteLine($"Database '{dbName}' created successfully.");
        }
        catch (Exception ex){
            Console.WriteLine($"Error: {ex.Message}");
        }

        string schema = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "schema.sql"));
        string seed = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "seed.sql"));
        using var command = connection.CreateCommand();
        command.CommandText = schema;
        command.ExecuteNonQuery();
        command.CommandText = seed;
        command.ExecuteNonQuery();
        Console.WriteLine("Database initialized.");

        // Show inserted data for Users
        command.CommandText = "SELECT * FROM Users;";
        using var reader = command.ExecuteReader();
        while (reader.Read()){
            Console.WriteLine($"UserId: {reader["UserId"]}, Username: {reader["Username"]}");
        }
    }

    static void Main(string[] args){
        Console.WriteLine("Hello, World!");
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();
        app.UseMiddleware<LoggingMiddleware>();

        bool startFromScratch = true;
        if (startFromScratch){
            initializeDB();
        }

        app.Run(async context =>{
            if (context.Request.Path == "/favicon.ico"){return;}
            await context.Response.WriteAsync("Hello from endpoint!\n");
            
            //Create all Users from current database information
            List<User> allUsers = [];
            string usersCommand = "SELECT UserId, Username, CreatedAt FROM Users";
            using var connection = new SqlConnection(connectionStringSTT);
            await connection.OpenAsync();
            using var cmd = new SqlCommand(usersCommand, connection);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()){
                var user = new User{
                    UserId = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    CreatedAt = reader.GetDateTime(2)
                };
                allUsers.Add(user);
            }
            //Simulate each user creating and liking posts, as well as influx of new users
            List<Task> tasks = [];
            foreach (var user in allUsers){
                tasks.Add(user.SimulateUserActivityAsync());
            }
            DateTime startTime = DateTime.Now;
            while((DateTime.Now - startTime).TotalSeconds < 10){
                await Task.Delay(500);
                int allUsersLength = allUsers.Count;
                var user = new User{
                    UserId = allUsersLength,
                    Username = StringGenerator.GenerateName(),
                    CreatedAt = DateTime.Now
                };
                await user.AddUserToDB();
                allUsers.Add(user);
                tasks.Add(user.SimulateUserActivityAsync());
            }
            User.shutdown = true;
            await Task.WhenAll(tasks);
            HistoryService historyService = new(){};
            List<Task<string>> historyTasks = [];
            foreach (var user in allUsers){
                historyTasks.Add(historyService.GenerateHistory(user));
            }
            var results = await Task.WhenAll(historyTasks);
            foreach (var result in results){
                Console.WriteLine(result + "\n");
            }
        });
        app.Run();
    }
}