﻿using System.Security.Cryptography;
using System.Text;

namespace CorsairLink;

public static class Utils
{
    public static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }
        else if (value > max)
        {
            return max;
        }

        return value;
    }

    public static byte ToFractionalByte(int value) => (byte)((value * byte.MaxValue + 50) / 100);

    public static int FromFractionalByte(byte value) => (100 * value + byte.MaxValue / 2) / byte.MaxValue;

    private static readonly char[] HEX_CHARS = "0123456789ABCDEF".ToCharArray();

    public static string ToHexString(this ReadOnlySpan<byte> bytes)
    {
        char[] hexChars = new char[bytes.Length * 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            int v = bytes[i] & 0xff;
            hexChars[i * 2] = HEX_CHARS[v >> 4];
            hexChars[i * 2 + 1] = HEX_CHARS[v & 0x0f];
        }
        return new string(hexChars);
    }

    public static string ToHexString(this byte[] bytes) => ToHexString(bytes.AsSpan());

    public static float FromLinear11(ReadOnlySpan<byte> bytes)
    {
        int value = bytes[1] << 8 | bytes[0];

        int mantissa = value & 0x7FF;
        if (mantissa > 1023)
            mantissa -= 2048;

        int exponent = value >> 11;
        if (exponent > 15)
            exponent -= 32;

        return mantissa * (float)Math.Pow(2, exponent);
    }

    public static bool GetEnvironmentFlag(string flagName)
    {
        var variableValue = Environment.GetEnvironmentVariable(flagName);
        if (string.IsNullOrEmpty(variableValue))
        {
            variableValue = Environment.GetEnvironmentVariable(flagName, EnvironmentVariableTarget.Machine);
        }
        return !string.IsNullOrEmpty(variableValue) && (variableValue.ToLower() == "true" || variableValue == "1");
    }

    public static void SyncWait(int milliseconds)
    {
        Task.Delay(milliseconds).GetAwaiter().GetResult();
    }

    public static string ParseString(byte[] data)
    {
        return Encoding.ASCII.GetString(data.TakeWhile(bt => bt != 0).ToArray()).Trim();
    }

    public static string ToMD5HexString(string str)
    {
        var hash = MD5.Create().ComputeHash(Encoding.Default.GetBytes(str));
        return hash.ToHexString();
    }

    public static string FormatForLogging(this Exception exception)
        => exception.FormatForLogging(0);

    private static string FormatForLogging(this Exception exception, int indentLevel = 0)
    {
        static string GetIndent(int level)
        {
            const int INDENT_SIZE = 4;
            return new string(' ', level * INDENT_SIZE);
        }

        StringBuilder sb = new();

        if (indentLevel == 0)
        {
            sb.AppendLine();
        }

        sb.AppendLine($"{GetIndent(indentLevel)}Type: {exception.GetType().FullName}");
        sb.AppendLine($"{GetIndent(indentLevel)}Message: {exception.Message}");
        sb.AppendLine($"{GetIndent(indentLevel)}Source: {exception.Source}");
        sb.AppendLine($"{GetIndent(indentLevel)}Stack Trace:");
        sb.AppendLine(exception.StackTrace);

        if (exception.Data.Count > 0)
        {
            sb.AppendLine($"{GetIndent(indentLevel)}Data:");
            foreach (var key in exception.Data.Keys)
            {
                sb.AppendLine($"{GetIndent(indentLevel + 1)}{key}: {exception.Data[key]}");
            }
        }

        if (exception.InnerException != null)
        {
            sb.AppendLine($"{GetIndent(indentLevel)}Inner Exception:");
            sb.AppendLine(exception.InnerException.FormatForLogging(indentLevel + 1));
        }

        return sb.ToString();
    }
}
