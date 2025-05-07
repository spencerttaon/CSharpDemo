using System;
using System.Text;

public static class StringGenerator
{
    private static readonly Random _random = new();
    public const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string Generate(int length){
        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++){
            sb.Append(chars[_random.Next(chars.Length)]);
        }
        return sb.ToString();
    }

    public static string GenerateName(){
        int name_length = _random.Next(2, 16);
        return Generate(name_length);
    }
    public static string GeneratePost(){
        int post_length = _random.Next(20, 40);
        return Generate(post_length);
    }
}