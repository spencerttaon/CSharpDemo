using Microsoft.Data.SqlClient;

public class HistoryService {
    private static string connectionString = @"Server=localhost\SQLEXPRESS;Database=STT;password=Derpderp;Trusted_Connection=True;TrustServerCertificate=True;";
    public async Task<string> GenerateHistory(User user){
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        string findUserPostsLikesUnion =
            "SELECT 'Post' AS ActionType, PostId, CreatedAt FROM Posts WHERE UserID = @UserID " +
            "UNION ALL " + 
            "SELECT 'Like' AS ActionType, PostId, CreatedAt FROM Likes WHERE UserID = @UserID " +
            "ORDER BY CreatedAt;";
        var cmd = new SqlCommand(findUserPostsLikesUnion, connection);
        cmd.Parameters.AddWithValue("@UserID", user.UserId);
        string historyString = $"UserId: {user.UserId}\nUsername: {user.Username}\nUser Created At: {user.CreatedAt}\n";
        using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()){
                string actionType = reader.GetString(0);
                int postId = reader.GetInt32(1);
                DateTime createdAt = reader.GetDateTime(2);
                historyString+=$"Action: {actionType} on postId: {postId} at {createdAt}\n";
            }
        return historyString;
    }
}