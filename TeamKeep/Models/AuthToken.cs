using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;

namespace TeamKeep.Models
{
    public class AuthToken
    {
        public int UserId { get; set; }
        public long TimeStamp { get; set; }
        public string Key { get; set; }
        public string AsString { get { return string.Format("{0}_{1}_{2}", UserId, TimeStamp, Key); } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uniqueId">Unique ID, usually the user ID</param>
        /// <param name="uniqueString">Unique string, usually the password hash</param>
        /// <returns></returns>
        public static AuthToken Generate(int uniqueId, string uniqueString)
        {
            var timestamp = (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;

            return new AuthToken
            {
                UserId = uniqueId,
                TimeStamp = timestamp,
                Key = Hash(uniqueId + timestamp + uniqueString)
            };
        }

        public static AuthToken Generate(HttpCookieCollection cookies)
        {
            const string cookieName = "teamkeep-token";

            var tokenCookie = cookies.Get(cookieName);
            if (tokenCookie == null) return null;

            var token = tokenCookie.Value;
            var tokenParts = token.Split('_');

            if (tokenParts.Length < 3) return null;

            return new AuthToken
            {
                UserId = Convert.ToInt32(tokenParts[0]),
                TimeStamp = Convert.ToInt64(tokenParts[1]),
                Key = tokenParts[2]
            };
        }

        public static string GenerateKey(string uniqueString, bool useDatetime = true)
        {
            var timestamp = (useDatetime) ? (long)(DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds : 0;
            var hash = Hash(timestamp + uniqueString);
            return new Regex("[^A-Za-z0-9]").Replace(hash, "$"); // Make web safe
        }

        public static AuthToken Generate(int userId, byte[] passwordHash)
        {
            return Generate(userId, Convert.ToBase64String(passwordHash));
        }

        public bool Verify(string passwordHash)
        {
            if (passwordHash == null) return false;
            return Hash(UserId + TimeStamp + passwordHash).Equals(Key);
        }

        public bool Verify(byte[] passwordHash)
        {
            if (passwordHash == null) return false;
            return Hash(UserId + TimeStamp + Convert.ToBase64String(passwordHash)).Equals(Key);
        }

        private static string Hash(string input)
        {
            var bytes = new byte[input.Length * sizeof(char)];
            Buffer.BlockCopy(input.ToCharArray(), 0, bytes, 0, bytes.Length);
            return Convert.ToBase64String(MD5.Create().ComputeHash(bytes));
        }
    }
}