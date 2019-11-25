using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace regexDirScan
{
    class Program
    {
        static void DirSearch(string currentDirectory, Func<string, bool> isNeedDepper)
        {
            try
            {
                foreach (string subdirectoryPath in Directory.GetDirectories(currentDirectory))
                {
                    if(isNeedDepper(subdirectoryPath))
                        DirSearch(subdirectoryPath, isNeedDepper);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
		
		static int Comparison(IComparable[] v1, IComparable[] v2)
		{
			if(v1.Length != v2.Length)
				throw new Exception("Unexpected logic error: length not equal");
			foreach(var elem in v1.Zip(v2, (o1, o2) => new []{o1, o2}))
			{
				var elemCompareResult = elem[0].CompareTo(elem[1]);
				if(elemCompareResult == 0)
					continue;
				return elemCompareResult;
			}
			return 0;
		}
		
		static IEnumerable<string> SubmatchesOrdering(List<KeyValuePair<string, IComparable[]>> matched)
		{
			if(!matched.Any())
				return Enumerable.Empty<string>();
			if(matched.First().Value.Any())
				matched.Sort((v1, v2) => Comparison(v1.Value, v2.Value));
			return matched.Select(kv => kv.Key);
		}

        static int Main(string[] args)
        {
            try
            {
				var defaultRegex = @"(\d+)\.(\d+)\.(\d+)\.(\d+)";
                if (args.Length < 1 || args.Length > 2 || (args[0] is var searchingInDirectory && !Directory.Exists(searchingInDirectory)))
				{
                    Console.Error.WriteLine("Recursive scaning for regex pattern in directory names, extracting submatches and sorting paths only to matched directory level lexicagraphicaly by submatches.");
					Console.Error.WriteLine("If submatch is number, then converting it to integer before comparison.");
					Console.Error.WriteLine("First argument should be directory for scanning.");
					Console.Error.WriteLine($"Second argument should be regexp with submatches for comparison and by default is '{defaultRegex}'.");
					return -1;
				}
                var result = new List<KeyValuePair<string, IComparable[]>>();
                var uniquePathes = new HashSet<string>();
				var submatchesRegexpString = args.Length >= 2 ? args[1] : defaultRegex;
                var submatchesRegexp = new Regex(submatchesRegexpString, RegexOptions.Compiled | RegexOptions.ECMAScript);
                DirSearch(searchingInDirectory, (directoryPath) =>
                {
                    var m = submatchesRegexp.Match(directoryPath);
                    if (!m.Success)
                        return true;
                    var g = m.Groups;
					var endOfMatch = g[0].Index + g[0].Length;
					var endOfMatchedDirectory = directoryPath.IndexOf('\\', endOfMatch);
					if(endOfMatchedDirectory == -1)
						endOfMatchedDirectory = directoryPath.IndexOf('/', endOfMatch);
                    var pathToMatch = endOfMatchedDirectory != -1 ? directoryPath.Substring(0, endOfMatchedDirectory) : directoryPath;
                    if (uniquePathes.Contains(pathToMatch))
                        return true;
                    uniquePathes.Add(pathToMatch);
					var submatches = g.Cast<Group>().Skip(1).Select(g => g.Value).Select(v => int.TryParse(v, out int intValue) ? (IComparable)intValue : (IComparable)v).ToArray();
					var kv = new KeyValuePair<string, IComparable[]>(pathToMatch, submatches);
                    result.Add(kv);
                    return false;
                });
				
                foreach (var paths in SubmatchesOrdering(result))
                    Console.WriteLine(paths);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
            return 0;
        }
    }
}
