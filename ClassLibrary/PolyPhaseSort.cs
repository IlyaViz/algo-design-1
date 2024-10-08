﻿using System.Buffers;
using System.Diagnostics;

namespace ClassLibrary
{
    public class PolyPhaseSort
    {
        private readonly string _inFileName;
        private readonly long _memoryLimit = 0;

        private const string B1_FILE_NAME = $"B1.txt";
        private const string B2_FILE_NAME = $"B2.txt";
        private const string B3_FILE_NAME = $"B3.txt";
        private const long MIN_SYS_NUM = long.MinValue;
        private const string TEMP_FILE_NAME = "TEMP.txt";
        private const long NUM_SIZE = sizeof(long);

        public PolyPhaseSort(string inFileName)
        {
            _inFileName = inFileName;
        }

        public PolyPhaseSort(string inFileName, long memoryLimit)
        {
            _inFileName = inFileName;
            _memoryLimit = memoryLimit;
        }

        private static long GetNextFibonacciNum(long n, HashSet<long> unavailable)
        {
            if (n == 0) return 0;

            long prev = 0;
            long current = 1;
            long temp;

            while (current < n || unavailable.Contains(current))
            {
                temp = current;
                current += prev;
                prev = temp;
            }

            return current;
        }

        private void DistributeInFile()
        {
            using StreamWriter file1 = new StreamWriter(B1_FILE_NAME);
            using StreamWriter file2 = new StreamWriter(B2_FILE_NAME);
            List<StreamWriter> files = new List<StreamWriter> { file1, file2 };
            using StreamReader inFile = new StreamReader(_inFileName);
            long[] lastNums = { MIN_SYS_NUM, MIN_SYS_NUM };
            int lastUsedFileIndex = 0;
            string strNum;
            long num;

            while ((strNum = inFile.ReadLine()) != null)
            {
                num = long.Parse(strNum);

                if (num < lastNums[lastUsedFileIndex])
                {
                    lastUsedFileIndex = 1 - lastUsedFileIndex;
                }

                files[lastUsedFileIndex].WriteLine(num);
                lastNums[lastUsedFileIndex] = num;
            }

            using (new StreamWriter(B3_FILE_NAME)) { };
        }

        private void AddFictiousSeries()
        {
            long[] seriesCount = new long[2];
            long lastNum = MIN_SYS_NUM;
            long num;
            string strNum;

            using (StreamReader file1 = new StreamReader(B1_FILE_NAME))
            using (StreamReader file2 = new StreamReader(B2_FILE_NAME))
            {
                List<StreamReader> files = new List<StreamReader> { file1, file2 };

                for (int fileIndex = 0; fileIndex < files.Count; fileIndex++)
                {
                    if ((strNum = files[fileIndex].ReadLine()) != null)
                    {
                        lastNum = long.Parse(strNum);
                        seriesCount[fileIndex] += 1;
                    }

                    while ((strNum = files[fileIndex].ReadLine()) != null)
                    {
                        num = long.Parse(strNum);

                        if (num < lastNum)
                        {
                            seriesCount[fileIndex] += 1;
                        }
                        lastNum = num;
                    }
                }
            }

            using (StreamWriter file1 = File.AppendText(B1_FILE_NAME))
            using (StreamWriter file2 = File.AppendText(B2_FILE_NAME))
            {
                List<StreamWriter> files = new List<StreamWriter> { file1, file2 }; 

                HashSet<long> usedFibonacciNums = new HashSet<long>();
                long fibonacciNum;
                long extraSeries;

                for (int fileIndex = 0; fileIndex < 2; fileIndex++)
                {
                    fibonacciNum = GetNextFibonacciNum(seriesCount[fileIndex], usedFibonacciNums);
                    usedFibonacciNums.Add(fibonacciNum);

                    extraSeries = fibonacciNum - seriesCount[fileIndex];

                    for (int counter = 0; counter < extraSeries; counter++)
                    {
                        files[fileIndex].WriteLine(Constants.MIN_NUM - 1 - counter);
                    }
                }
            }
        }

        private void FilesInit()
        {
            DistributeInFile();
            AddFictiousSeries();
        }

        private static bool FileIsEmpty(string fileName)
        {
            using StreamReader file = new StreamReader(fileName);
            return file.ReadLine() == null;
        }

        private static void KeepLeftData(long extraNum, StreamReader file)
        {
            string fileName = (file.BaseStream as FileStream).Name;
            string strNum;

            using (StreamWriter tempFile = new StreamWriter(TEMP_FILE_NAME))
            {
                tempFile.WriteLine(extraNum);

                while ((strNum = file.ReadLine()) != null)
                {
                    tempFile.WriteLine(strNum);
                }
            }

            file.Close();
            File.Delete(fileName);
            File.Move(TEMP_FILE_NAME, fileName);
        }
        private static void CleanFile(StreamReader file)
        {
            string fileName = (file.BaseStream as FileStream).Name;

            file.Close();
            File.Create(TEMP_FILE_NAME).Close();
            File.Delete(fileName);
            File.Move(TEMP_FILE_NAME, fileName);
        }
        private static void RemoveBelowMinNums(string fileName)
        {
            string strNum;

            using (StreamWriter tempFile = new StreamWriter(TEMP_FILE_NAME))
            using (StreamReader file = new StreamReader(fileName))
            {
                long num;

                while ((strNum = file.ReadLine()) != null)
                {
                    num = long.Parse(strNum);

                    if (num >= Constants.MIN_NUM)
                    {
                        tempFile.WriteLine(num);
                    }
                }
            }

            File.Delete(fileName);
            File.Move(TEMP_FILE_NAME, fileName);
        }

        private void SortInFile()
        {
            long numsPerIter = _memoryLimit / NUM_SIZE;

            if (numsPerIter == 0)
            {
                return;
            }

            using (StreamWriter tempFile = new StreamWriter(TEMP_FILE_NAME))
            using (StreamReader inFile = new StreamReader(_inFileName))
            {
                long[] array = new long[numsPerIter];
                int index = 0;
                string strNum;
                long num;

                while ((strNum = inFile.ReadLine()) != null)
                {
                    num = long.Parse(strNum);
                    array[index] = num;
                    index += 1;

                    if (index >= numsPerIter)
                    {
                        Array.Sort(array);

                        foreach (long number in array)
                        {
                            tempFile.WriteLine(number);
                        }

                        index = 0;
                    }
                }

                if (index > 0)
                {
                    long[] partArray = new long[index];

                    for (int i = 0; i < index; i++)
                    {
                        partArray[i] = array[i];
                    }

                    Array.Sort(partArray);

                    foreach (long number in partArray)
                    {
                        tempFile.WriteLine(number);
                    }
                }
            }

            File.Delete(_inFileName);
            File.Move(TEMP_FILE_NAME, _inFileName);
        }

        private void MergeFiles(string in1FileName, string in2FileName, string outFileName)
        {
            StreamReader inFile1 = new StreamReader(in1FileName);
            StreamReader inFile2 = new StreamReader(in2FileName);
            StreamReader[] inFiles = { inFile1, inFile2 };
            using StreamWriter outFile = new StreamWriter(outFileName);
            long[] lastNums = { MIN_SYS_NUM, MIN_SYS_NUM };
            long[] newNums = { MIN_SYS_NUM, MIN_SYS_NUM };
            bool[] seriesCompleted = { false, false };

            void GetNextData(int fileIndex)
            {
                lastNums[fileIndex] = newNums[fileIndex];

                string strNum;

                if ((strNum = inFiles[fileIndex].ReadLine()) != null)
                {
                    newNums[fileIndex] = long.Parse(strNum);
                }
                else
                {
                    newNums[fileIndex] = MIN_SYS_NUM;
                }
            }

            void ReadLeftSeries(int fileIndex)
            {
                do
                {
                    outFile.WriteLine(newNums[fileIndex]);
                    GetNextData(fileIndex);
                } while (newNums[fileIndex] >= lastNums[fileIndex]);

                seriesCompleted[fileIndex] = true;
            }

            void Finalize(int fileIndex)
            {
                int anotherFileIndex = 1 - fileIndex;

                CleanFile(inFiles[fileIndex]);

                if (!seriesCompleted[anotherFileIndex])
                {
                    ReadLeftSeries(anotherFileIndex);

                    if (newNums[anotherFileIndex] != MIN_SYS_NUM)
                    {
                        KeepLeftData(newNums[anotherFileIndex], inFiles[anotherFileIndex]);
                    }
                    else
                    {
                        CleanFile(inFiles[anotherFileIndex]);
                    }
                }
                else
                {
                    KeepLeftData(newNums[anotherFileIndex], inFiles[anotherFileIndex]);
                }
            }

            GetNextData(0);
            GetNextData(1);

            while (newNums[0] != MIN_SYS_NUM && newNums[1] != MIN_SYS_NUM)
            {
                if (newNums[0] < lastNums[0] && !seriesCompleted[1])
                {
                    seriesCompleted[0] = true;
                    ReadLeftSeries(1);
                }
                else if (newNums[1] < lastNums[1] && !seriesCompleted[0])
                {
                    seriesCompleted[1] = true;
                    ReadLeftSeries(0);
                }
                else
                {
                    if (newNums[0] < newNums[1])
                    {
                        seriesCompleted[1] = false;
                        outFile.WriteLine(newNums[0]);
                        GetNextData(0);
                    }
                    else
                    {
                        seriesCompleted[0] = false;
                        outFile.WriteLine(newNums[1]);
                        GetNextData(1);
                    }
                }
            }

            if (newNums[0] == MIN_SYS_NUM)
            {
                Finalize(0);
            }
            else
            {
                Finalize(1);
            }
        }

        public string Sort()
        {
            if (FileIsEmpty(_inFileName))
            {
                return _inFileName;
            }

            Stopwatch watch = Stopwatch.StartNew();

            if (_memoryLimit != 0)
            {
                SortInFile();
                watch.Stop();
                Console.WriteLine($"sort in file: {watch.Elapsed.TotalSeconds} s");
            }

            watch.Restart();
            FilesInit();
            watch.Stop();
            Console.WriteLine($"files init: {watch.Elapsed.TotalSeconds} s");

            string[] fileNames = { B1_FILE_NAME, B2_FILE_NAME, B3_FILE_NAME };
            bool[] filesEmpty = fileNames.Select(FileIsEmpty).ToArray();

            int num = 0;
            watch.Restart();
            while (filesEmpty.Count(f => !f) != 1)
            {
                num++;
                if (filesEmpty[0])
                {
                    MergeFiles(fileNames[1], fileNames[2], fileNames[0]);
                }
                else if (filesEmpty[1])
                {
                    MergeFiles(fileNames[0], fileNames[2], fileNames[1]);
                }
                else
                {
                    MergeFiles(fileNames[0], fileNames[1], fileNames[2]);
                }

                filesEmpty = fileNames.Select(FileIsEmpty).ToArray();
            }
            watch.Stop();
            Console.WriteLine($"merging process ({num}): {watch.Elapsed.TotalSeconds} s");

            int resultFileIndex = Array.IndexOf(filesEmpty, false);

            watch.Restart();
            RemoveBelowMinNums(fileNames[resultFileIndex]);
            watch.Stop();
            Console.WriteLine($"removing below mins: {watch.Elapsed.TotalSeconds} s");

            return fileNames[resultFileIndex];
        }
    }
}
