using Godot;
using System;
using System.Linq;

public static class StringUtils {
    public static string RemoveWhitespace(this string input) {
        return new string([.. input.Where(c => !Char.IsWhiteSpace(c))]);
    }
}
