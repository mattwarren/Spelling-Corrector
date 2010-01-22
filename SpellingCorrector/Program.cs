using System;
using System.IO;
using System.Reflection;

namespace Spelling_Code_Challenge
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] names = Assembly.GetExecutingAssembly().GetManifestResourceNames();          
            const string resourceLocation = "Spelling_Code_Challenge.WordLists.";

            //var wordListStream = GetResource(resourceLocation + "allwords.txt");
            //var wordListStream = GetResource(resourceLocation + "cracklib-words.txt");
            var wordListStream = GetResource(resourceLocation + "Norvig - big.txt");
            //var wordListStream = GetResource(resourceLocation + "ul_words.txt");

            var corrector = new SpellingCorrector();            
            //const string testFileName = @"C:\Users\mattwarren\Desktop\Word List Stuff\ul_words.txt";
            //corrector.ParseFile(testFileName);            
            corrector.ParseStream(wordListStream);
            wordListStream.Close();

            TestString(corrector, "sheeeeep", "sheep");
            TestString(corrector, "peepple", "people");
            TestString(corrector, "sheeple", SpellingCorrector.NO_SUGGESTION);

            //Case (upper/lower) errors
            TestString(corrector, "inSIDE", "inside");
            //Repeated letters:
            TestString(corrector, "jjoobbb", "job");
            //Incorrect vowels: 
            TestString(corrector, "weke", "wake");

            //Any combination of the above types of error in a single word should be corrected 
            TestString(corrector, "CUNsperrICY", "conspiracy");            
        }

        public static TextReader GetResource(string resourceName)
        {
            if (String.IsNullOrEmpty(resourceName))
                return null;

            Assembly assembly = Assembly.GetExecutingAssembly();           
            var resource = assembly.GetManifestResourceStream(resourceName);

            if (resource == null)
                return null;

            TextReader textReader = new StreamReader(resource);
            return textReader;
            //var result = new List<string>();
            //string line;
            //while ((line = textReader.ReadLine()) != null)
            //{
            //    result.Add(line);
            //}
            //textReader.Close();

            //return result.ToArray();
        }

        static private void TestString(SpellingCorrector corrector, string input, string expected)
        {
            string result = corrector.GetSpelling(input);

            if (result.Equals(expected, StringComparison.CurrentCulture))
                Console.WriteLine(" PASSED : \"{0}\" => \"{1}\"", input, result);
            else
                Console.WriteLine("*FAILED : \"{0}\" returned \"{1}\", expected \"{2}\"", input, result, expected);
        }
	}	
}
