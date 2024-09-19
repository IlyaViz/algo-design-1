namespace ClassLibrary
{
    public class FileCreator
    {
        public static void GenerateRandomFile(string fileName, long size)
        {
            Random random = new Random();

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                long bytesWritten = 0;
                long num;
                string line;

                while (bytesWritten < size)
                {
                    num = random.NextInt64();

                    if (num >= Constants.MIN_NUM)
                    {
                        line = num.ToString() + '\n';
                        writer.Write(line);
                        bytesWritten += line.Length;
                    }
                }
            }
        }
    }
}
