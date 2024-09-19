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
            long memory;

            while (MEMORY_MONITORING)
            {
                Thread.Sleep(2000);
                Process process = Process.GetCurrentProcess();
                memory = process.WorkingSet64;
                Console.WriteLine($"Used memory by algo is: {(memory - initMemory) / 1024.0 / 1024.0}+- MB");
            }
        }

        public static bool ValidateResult(string inFileName, string outFileName)
        {
            using (StreamReader inFile = new StreamReader(inFileName))
            using (StreamReader outFile = new StreamReader(outFileName))
            {
                string strNum;
                List<long> inList = new List<long>();
                List<long> outList = new List<long>();

                while ((strNum = inFile.ReadLine()) != null)
                {
                    inList.Add(long.Parse(strNum));
                }

                while ((strNum = outFile.ReadLine()) != null)
                {
                    outList.Add(long.Parse(strNum));
                }

                inList.Sort();

                bool equal = true;

                for (int i = 0; i < inList.Count; i++)
                {
                    if (inList[i] != outList[i])
                    {
                        equal = false;
                        break;
                    }
                }

                return equal;
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
            if (algoType == "y")
            {
                algo = new PolyPhaseSort(path, size / 10); // 4 series
            }
            else
            {
                algo = new PolyPhaseSort(path);
            }

            Process process = Process.GetCurrentProcess();
            initMemory = process.WorkingSet64;
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
            Run();
        }
    }
}
