using System;
using System.Linq;
using System.Text.RegularExpressions;

class Program
{
    /*
    Converts parameterized dynamic SQL from SQL Profiler to normal T-SQL. Usage example: first copy the dynamic sql to clipboard, then run following command:
      gnuwin32\bin\getclip.exe | convert-sp_executesql.exe | sqlformatter.exe | clip
      
      sqlformatter.exe can be found at http://architectshack.com/PoorMansTSqlFormatter.ashx
    */
    static void Main()
	{
        var re = new Regex(@"exec*\s*sp_executesql\s+N'([\s\S]*)',\s*N'(@[\s\S]*?)',\s*([\s\S]*)", RegexOptions.IgnoreCase); // 1: the sql, 2: the declare, 3: the setting
	    var input = Console.In.ReadToEnd();
	    var match = re.Match(input);
        if (match.Success)
        {
            var sql = match.Groups[1].Value.Replace("''", "'");
            var declare = match.Groups[2].Value;
            var setting = match.Groups[3].Value + ',';
            
            // to deal with comma or single quote in variable values, we can use the variable name to split
            var re2 = new Regex(@"@[\s\S]*?\s*=");
            var variables = re2.Matches(setting).Cast<Match>().Select(m => m.Value).ToArray();
            var values = re2.Split(setting).Where(s=>!string.IsNullOrWhiteSpace(s)).ToArray();

            Console.WriteLine("BEGIN\nDECLARE {0};", declare);
            for (int i = 0; i < variables.Length; i++)
            {
                Console.WriteLine("SET {0}{1};", variables[i], values[i].Substring(0, values[i].Length-1));
            }
            Console.WriteLine("{0}\nEND", sql);
        }
	}
}
