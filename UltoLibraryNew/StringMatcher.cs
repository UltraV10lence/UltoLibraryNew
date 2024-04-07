﻿namespace UltoLibraryNew; 

public static class StringMatcher {
    // ReSharper disable once StringLiteralTypo
    public static readonly Shape Name = new ("qwertyuiopasdfghjklzxcvbnm", true);
    public static readonly Shape Numbers = new ("1234567890");
    public static readonly Shape SpecialChars = new (".,`!@&*()&?^:;$#[]/\\-+=");
    public static readonly Shape Space = new(" ");
    public static readonly Shape ServiceChars = new ("\t\n\r" + (char)13);
    public static readonly Shape NickName = Name.Append(Numbers);
    public static readonly Shape EMail = NickName.Append("@.");
    
    public class Shape(string included, bool ignoreCase = false) {
        public readonly string Included = included;
        public readonly bool IgnoreCase = ignoreCase;

        public Shape? Lower {
            get;
            private init;
        }

        public Shape Append(Shape other) {
            var sh = new Shape(Included, IgnoreCase) {
                Lower = other
            };
            return sh;
        }
        public Shape Append(string other, bool ignoreCase = false) {
            return Append(new Shape(other, ignoreCase));
        }

        public bool Contains(char c) {
            if (IgnoreCase) c = char.ToLower(c);
            var cont = Included.ToLower().Contains(c);
            if (!cont && Lower != null) return Lower.Contains(c);
            return cont;
        }
        
        public bool Matches(string str) {
            return str.All(Contains);
        }
    }
}