using System.Diagnostics;
using ClassLibrary;

namespace Run
{
    public class Program
    {
        static bool MEMORY_MONITORING = true;
        const string DEFAULT_INPUT_FILE = "A.txt";
        static long initMemory; 

        public static void PrintUsedMemory()
        {
            while (MEMORY_MONITORING)
            {
                Thread.Sleep(2000);
                Process process = Process.GetCurrentProcess();
                long memory = process.WorkingSet64;
                Console.WriteLine($"Used memory by algo is: {(memory - initMemory) / 1024.0 / 1024.0}+- MB");
            }
        }

        public static bool ValidateResult(string file1Name, string file2Name)
        {
            using (StreamReader file1 = new StreamReader(file1Name))
            using (StreamReader file2 = new StreamReader(file2Name))
            {
                string strNum;
                List<long> list1 = new List<long>();
                List<long> list2 = new List<long>();

                while ((strNum = file1.ReadLine()) != null)
                {
                    list1.Add(long.Parse(strNum));
                }

                while ((strNum = file2.ReadLine()) != null)
                {
                    list2.Add(long.Parse(strNum));
                }

                list1.Sort();

                return list1.SequenceEqual(list2);
            }
        } 

        public static void Run()
        {
            Console.WriteLine("Ready file (in), fully random file (r)");
            string fileType = Console.ReadLine();
            string path;
            long size;

            if (fileType == "in")
            {
                Console.WriteLine("File path");
                path = Console.ReadLine();
                size = new FileInfo(path).Length;
            }
            else if (fileType == "r")
            {
                path = DEFAULT_INPUT_FILE;
                Console.WriteLine("Enter file size (MB)");
                size = long.Parse(Console.ReadLine()) * 1024 * 1024;
                FileCreator.GenerateRandomFile(path, size);
            }
            else
            {
                throw new ArgumentException("Invalid input");
            }

            Console.WriteLine("Enhanced algo (y/n)?");
            string algoType = Console.ReadLine();

            PolyPhaseSort algo;
            if (algoType == "y") {
                algo = new PolyPhaseSort(path, size / 8);
            } else
            {
                algo = new PolyPhaseSort(path);
            }

            Thread memoryThread = new Thread(PrintUsedMemory);
            memoryThread.Start();

            Stopwatch algoStart = Stopwatch.StartNew();
            string resultFileName = algo.Sort();
            algoStart.Stop();

            Console.WriteLine($"Result is in {resultFileName}, Runtime: {algoStart.Elapsed.TotalSeconds} s, " +
                              $"Running speed: {size / 1024.0 / 1024.0 / algoStart.Elapsed.TotalSeconds * 60} MB/m");

            MEMORY_MONITORING = false;

            bool resultIsCorrect = ValidateResult(path, resultFileName);
            Console.WriteLine($"Is result correct? {resultIsCorrect}");
        }

        static void Main(string[] args)
        {
            Process process = Process.GetCurrentProcess();
            initMemory = process.WorkingSet64;

            Run(); 
        }
    }
}
