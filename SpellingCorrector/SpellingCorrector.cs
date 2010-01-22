using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Spelling_Code_Challenge
{
    class SpellingCorrector
    {
        public const string NO_SUGGESTION = "NO_SUGGESTION";
        Dictionary<string, int> words;

        public SpellingCorrector()
        {
            long bytesBefore = GC.GetTotalMemory(true);
            words = new Dictionary<string, int>(500000);
            long bytesAfter = GC.GetTotalMemory(true);

            Console.WriteLine("Before Allocation {0:0.00}MB, After {1:0.00}MB, diff = {2:0.00}MB\n",
                BytesToMB(bytesBefore), BytesToMB(bytesAfter), BytesToMB(bytesAfter - bytesBefore));
        }       

        /// <summary>
        /// Method from http://www.codegrunt.co.uk/code/spell.cs,
        /// original is at http://norvig.com/spell-correct.html
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal string GetSpelling(string str)
        {
            var possibleMatches = new List<string> {str.ToLower()};

            Func<string, IEnumerable<string>> edits1 =
                    // Deletion, i.e. go thru string deleting 1 letter at a time
                    w => (from i in Enumerable.Range(0, w.Length)
                          select w.Substring(0, i) + w.Substring(i + 1))
                    // Transposition
                    .Union(from i in Enumerable.Range(0, w.Length - 1)
                           select w.Substring(0, i) + w.Substring(i + 1, 1) + w.Substring(i, 1) + w.Substring(i + 2))
                    // Alteration
                    //.Union(from i in Enumerable.Range(0, w.Length) 
                    //    from c in "abcdefghijklmnopqrstuvwxyz" 
                    //    select w.Substring(0, i) + c.ToString() + w.Substring(i + 1))
                    // Insertion                    
                    //.Union(from i in Enumerable.Range(0, w.Length + 1) 
                    //    from c in "abcdefghijklmnopqrstuvwxyz" 
                    //    select w.Substring(0, i) + c.ToString() + w.Substring(i));
                    ;

            const string fileName = @"H:\Dev\__Small projects (trying stuff out)__\Word List Stuff\Spelling Code Challenge\Spelling Code Challenge\WordLists\Norvig - big.txt";
            var features = from Match m in Regex.Matches(File.ReadAllText(fileName).ToLower(), "[a-z]+", RegexOptions.Compiled) 
                           select m.Value;
            //var nWords = (from f in features
            //              group f by f
            //                  into g
            //                  select g).ToDictionary(g => g.Key, g => g.Count());            

            //Non-Linq equivalent, split into seperate statements
            var featuresGrouped = features.GroupBy(f => f);            
            var featuresGroupedSelected = featuresGrouped.Select(g => g);            
            var nWords = featuresGroupedSelected.ToDictionary(g => g.Key, g => g.Count());

            //IEnumerable<string> candidates = from w in edits1("testing") where nWords.ContainsKey(w) select w;            
            var candidates = from w in edits1(str) select w;
            int temp = candidates.Count();

            Console.WriteLine("\nThere are {0} possible candidates for input {1} :", candidates.Count(), str);
            foreach (var candidate in candidates)
            {
                Console.Write("{" + candidate + "}, ");  
            } 
            Console.WriteLine("\n");

            //Console.WriteLine("\nThere are {0} possible matches:", possibleMatches.Count);
            //possibleMatches.ForEach(x => Console.Write("{" + x + "}, "));
            //Console.WriteLine("\n");

            var matches = possibleMatches.Where(possible => words.ContainsKey(possible)).ToList();

            return matches.Count > 0 ? matches[0] : NO_SUGGESTION;
        }

        internal void ParseStream(TextReader textStream)
        {
            //int counter = 0, 
            int wordsAdded = 0;
            string line;            

            // ^ (anchor to start of string)
            // [ ] (any character in the "\p{L}")
            // \p{L} (matches a single character that is a Unicode letter)
            // + (one or more times)
            // $ (anchor to end of string)
            //var regex = new Regex(@"^[\p{L}]+$");
            var regex = new Regex(@"[a-z]+");

            Console.Write("Parsing stream....");
            var timer = new Stopwatch();
            long bytesBefore = GC.GetTotalMemory(true);
            timer.Start();
            while ((line = textStream.ReadLine()) != null)
            {
                //if (regex.IsMatch(line)) //much faster to create regex once and just match each time                    
                foreach (Match match in regex.Matches(line.ToLowerInvariant()))                                                    
                {
                    var word = match.Value; //.ToLowerInvariant();
                    if (words.ContainsKey(word))
                        words[word]++;
                    else
                        words.Add(word, 1);                                                                
                    wordsAdded++;
                }
                //counter++;
                //if (counter % 100000 == 0)
                //    Console.Write(".");
            }
            timer.Stop();
            long bytesAfter = GC.GetTotalMemory(true);

            Stopwatch linqTimer = new Stopwatch();
            linqTimer.Start();
            const string fileName = @"H:\Dev\__Small projects (trying stuff out)__\Word List Stuff\Spelling Code Challenge\Spelling Code Challenge\WordLists\Norvig - big.txt";
            var features = from Match m in Regex.Matches(File.ReadAllText(fileName).ToLower(), "[a-z]+", RegexOptions.Compiled)
                           select m.Value;
            var nWords = (from f in features
                          group f by f
                              into g
                              select g).ToDictionary(g => g.Key, g => g.Count());    
            linqTimer.Stop();

            words.Remove(words.Keys.First());
            AreDictionariesTheSame(words, nWords);                        

            Console.WriteLine("completed\n");

            Console.WriteLine("Before Insertion {0:0.00}MB, After {1:0.00}MB, diff = {2:0.00}MB\n",
                    BytesToMB(bytesBefore), BytesToMB(bytesAfter), BytesToMB(bytesAfter - bytesBefore));

            Console.WriteLine("Took {0:0.00} ms ({1:0.00} secs) to read {2} lines\n",
                timer.ElapsedMilliseconds, timer.ElapsedMilliseconds / 1000.0, wordsAdded);

            Console.WriteLine("LINQ Took {0:0.00} ms ({1:0.00} secs) to read {2} lines\n",
                linqTimer.ElapsedMilliseconds, linqTimer.ElapsedMilliseconds / 1000.0, wordsAdded);
        }

        private static bool AreDictionariesTheSame(Dictionary<string, int> dict1, Dictionary<string, int> dict2)
        {
            if (dict1.Count() != dict2.Count())
                Console.WriteLine("Different number is items {0} and {1}", dict1.Count(), dict2.Count());

            var areTheSame = true;
            foreach (var item in dict1.Where(item => dict2[item.Key] != item.Value))
            {
                Console.WriteLine("Mismatch dict1[{0}] = {1}, dict2[{2}] = {3}",
                                  item.Key, item.Value, item.Key, dict2[item.Key]);
                areTheSame = false;
            }

            if (areTheSame)
                Console.WriteLine("The 2 dictionaries are the same");

            return areTheSame;
        }

        //public void ParseFile(string fileName)
        //{
        //    int counter = 0, wordsAdded = 0;
        //    string line;
        //    long bytesBefore, bytesAfter;

        //    Console.Write("Parsing file");
        //    Stopwatch timer = new Stopwatch();
        //    timer.Start();
        //    using (StreamReader file = new StreamReader(fileName))
        //    {
        //        bytesBefore = GC.GetTotalMemory(true);
        //        Regex regex = new Regex(@"^[\p{L}]+$");
        //        while ((line = file.ReadLine()) != null)
        //        {
        //            if (regex.IsMatch(line)) //much faster to create regex once and just match each time                    
        //            {
        //                words.Add(line.ToLower(), counter);                        
        //                wordsAdded++;
        //            }
        //            counter++;
        //            if (counter % 100000 == 0)
        //                Console.Write(".");
        //        }               
        //        bytesAfter = GC.GetTotalMemory(true);                
        //    }
        //    timer.Stop();
        //    Console.WriteLine("Completed\n");

        //    Console.WriteLine("Before Insertion {0:0.00}MB, After {1:0.00}MB, diff = {2:0.00}MB\n",
        //            BytesToMB(bytesBefore), BytesToMB(bytesAfter), BytesToMB(bytesAfter - bytesBefore));

        //    Console.WriteLine("Took {0:0.00} ms ({1:0.00} secs) to read {2} lines, out of {3}\n", 
        //        timer.ElapsedMilliseconds, timer.ElapsedMilliseconds / 1000.0, wordsAdded, counter);
        //}

        static private double BytesToMB(long bytesBefore)
        {
            return bytesBefore / 1024.0 / 1024.0;
        }       
    }
}
