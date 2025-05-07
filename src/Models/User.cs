using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Update.Internal;

public class User {
    public int UserId;
    public string Username = string.Empty;
    public DateTime CreatedAt;
    public static bool shutdown = false;
    private static readonly Random _random = new();
    private static string connectionString = @"Server=localhost\SQLEXPRESS;Database=STT;password=Derpderp;Trusted_Connection=True;TrustServerCertificate=True;";
    public async Task AddUserToDB(){
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        string addCommand =
            "INSERT INTO Users (Username, CreatedAt) " +
            "OUTPUT INSERTED.UserId " +
            "VALUES (@Username, @CreatedAt)";
        var cmd = new SqlCommand(addCommand, connection);
        cmd.Parameters.AddWithValue("@Username", Username);
        cmd.Parameters.AddWithValue("@CreatedAt", CreatedAt);
        object? result = await cmd.ExecuteScalarAsync();
    }
    
    public async Task SimulateUserActivityAsync() {
        while (!shutdown){
            await Task.Delay(1000);
            int postOrLike = _random.Next(2);
            switch (postOrLike){
                case 0:
                    await CreatePostAsync();
                    break;
                case 1:
                    await LikePostAsync();
                    break;
                default:
                    Console.WriteLine($"Unexpected value in SimulateUserActivityAsync for user {UserId}");
                    break;
            }
        }
    }
    public async Task CreatePostAsync(){
        string content = StringGenerator.GeneratePost();
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        
        string postCommand =
            "INSERT INTO Posts (UserId, Content, CreatedAt) " +
            "OUTPUT INSERTED.PostId " +
            "VALUES (@UserId, @Content, GETDATE())";
        var cmd = new SqlCommand(postCommand, connection);
        cmd.Parameters.AddWithValue("@UserId", UserId);
        cmd.Parameters.AddWithValue("@Content", content);
        object? result = await cmd.ExecuteScalarAsync();
        if (result is not int postId){
            Console.WriteLine($"Failure to create post by user{UserId}");
            return;
        }
        //Do something with postId if neccessary
    }
    public async Task LikePostAsync(){
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        string postCountCommand = "SELECT COUNT(*) FROM Posts";
        var cmd = new SqlCommand(postCountCommand, connection);
        object? result = await cmd.ExecuteScalarAsync();
        if (result is not int numPosts){
            Console.WriteLine($"Failure to retrieve post count by user{UserId}");
            return;
        }
        int chosenPost = _random.Next(numPosts) + 1; //Indexing from 1
        try{
            string likeCommand =
            "IF NOT EXISTS (SELECT 1 FROM Likes WHERE PostId = @PostId AND UserId = @UserId) " +
            "INSERT INTO Likes (PostId, UserId, CreatedAt) VALUES (@PostId, @UserId, GETDATE())";
            cmd = new SqlCommand(likeCommand, connection);
            cmd.Parameters.AddWithValue("@PostId", chosenPost);
            cmd.Parameters.AddWithValue("@UserId", UserId);
            result = await cmd.ExecuteScalarAsync();
            if (result is int rowsAffected){
                if (rowsAffected == 0){
                    Console.WriteLine($"User {UserId} already liked Post {chosenPost}");
                }
                else{
                    Console.WriteLine($"User {UserId} now likes Post {chosenPost}");
                }
            }
        }
        catch (Exception ex){
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }
}