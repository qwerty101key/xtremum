using System;
using System.Text.RegularExpressions;

namespace ContentManagement
{
    public class TimerHasher : IHasher
    {
        public string Hash(string source)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[:. ]", options);
            return regex.Replace(DateTime.Now.ToString("yyyyMMddHHmmssFFF"), "_");
        }
    }
}