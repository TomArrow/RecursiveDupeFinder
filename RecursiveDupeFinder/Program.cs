using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace RecursiveDupeFinder
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("welcome");

            string currentPlace = Directory.GetCurrentDirectory();

            string[] subDirs = Directory.GetDirectories(currentPlace);

            List<string> duplicates = new List<string>();

            foreach(string subDir in subDirs)
            {
                duplicates.AddRange(findDuplicateFiles(subDir));
            }

            File.WriteAllLines("dupesFound.txt",duplicates.ToArray());


            Console.ReadKey();
        }

        static string[] findDuplicateFiles(string directory, string[] filesToCompare = null)
        {
            List<string> duplicates = new List<string>();

            string[] filesHere = Directory.GetFiles(directory);

            List<string> notDupes = new List<string>();

            if(filesToCompare != null)
            {
                notDupes.AddRange(filesToCompare);
                foreach (string fileHere in filesHere)
                {
                    foreach(string fileToCompare in filesToCompare)
                    {
                        if (FilesAreEqualVectorized(new FileInfo(fileHere),new FileInfo(fileToCompare)))
                        {
                            Console.WriteLine(fileHere +" is a dupe.");
                            duplicates.Add(fileHere);
                        } else
                        {
                            notDupes.Add(fileHere);
                        }
                    }
                }
            } else
            {
                notDupes.AddRange(filesHere);
            }

            string[] subDirs = Directory.GetDirectories(directory);

            foreach(string subDir in subDirs)
            {
                duplicates.AddRange(findDuplicateFiles(subDir, notDupes.ToArray()));
            }


            return duplicates.ToArray();
        }



        // Following file comparison code taken from: https://stackoverflow.com/a/1359947
        const int BYTES_TO_READ = sizeof(Int64);

        static bool FilesAreEqual(FileInfo first, FileInfo second)
        {
            if (first.Length != second.Length)
                return false;

            if (string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase))
                return true;

            int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

            using (FileStream fs1 = first.OpenRead())
            using (FileStream fs2 = second.OpenRead())
            {
                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];

                for (int i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BYTES_TO_READ);
                    fs2.Read(two, 0, BYTES_TO_READ);

                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        return false;
                }
            }

            return true;
        }

        static int BYTES_TO_READ_COUNT = Vector<Int64>.Count;
        static int BYTES_TO_READ_TOTAL = BYTES_TO_READ * BYTES_TO_READ_COUNT;

        static bool FilesAreEqualVectorized(FileInfo first, FileInfo second)
        {
            if (first.Length != second.Length)
                return false;

            if (string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase))
                return true;

            int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ_TOTAL);

            using (FileStream fs1 = first.OpenRead())
            using (FileStream fs2 = second.OpenRead())
            {
                byte[] one = new byte[BYTES_TO_READ_TOTAL];
                byte[] two = new byte[BYTES_TO_READ_TOTAL];

                Vector<Int64> oneV,twoV;

                for (int i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BYTES_TO_READ_TOTAL);
                    fs2.Read(two, 0, BYTES_TO_READ_TOTAL);

                    oneV = new Vector<long>(one);
                    twoV = new Vector<long>(two);
                    if (oneV != twoV)
                        return false;
                }
            }

            return true;
        }
    }
}
