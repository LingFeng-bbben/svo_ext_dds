using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace svo_ext_dds
{
    class Program
    {
        static string svoHeader = "AVTS";
        static byte[] DDSpattern = { 0x44, 0x44, 0x53, 0x20, 0x7c };
        static int nameStartIndex = 0x480;
        static int nameoffsetIndex = 0x400;
        static int nameLength = 0x40;
        static async Task Main(string[] args)
        {
            foreach(string arg in args)
            {
                Console.WriteLine("Exracting " + arg);
                FileInfo file = new FileInfo(arg);
                byte[] filebyte = File.ReadAllBytes(arg);
                string fileheader = Encoding.UTF8.GetString(filebyte.Take(4).ToArray());
                if (fileheader != svoHeader)
                {
                    Console.WriteLine("WRONG FILE!!!!!!!");
                    continue;
                }

                List<string> filenames = new List<string>();
                int subindex = nameStartIndex;
                while (filebyte[subindex] != 0x59)
                {
                    string subfilename = Encoding.UTF8.GetString(filebyte.Skip(subindex).Take(nameLength).ToArray());
                    Console.WriteLine("file:" + subfilename);
                    filenames.Add(subfilename);
                    subindex += nameoffsetIndex;
                }

                List<int> startIndexs = IndexOf(filebyte, DDSpattern);
                DirectoryInfo outputDir = file.Directory.CreateSubdirectory(file.Name.Replace('.', '_'));
                for (int i=0;i<startIndexs.Count; i++)
                {
                    Console.WriteLine("Found at {0,8:X8}",startIndexs[i]);
                    byte[] ddsFile;
                    if(i+1<startIndexs.Count)
                        ddsFile = filebyte.Skip(startIndexs[i]).Take(startIndexs[i + 1] - startIndexs[i]).ToArray();
                    else
                        ddsFile = filebyte.Skip(startIndexs[i]).ToArray();
                    string outpath = outputDir.FullName + "\\" + filenames[i].Replace("\0","");
                    Console.WriteLine(outpath);
                    File.WriteAllBytes(outpath, ddsFile);
                }
                
            }
            Console.WriteLine("Extract complete,,,");
            await Task.Delay(3000);
        }
        static List<int> IndexOf(byte[] srcBytes, byte[] searchBytes)
        {
            if (srcBytes == null) { return null; }
            if (searchBytes == null) { return null; }
            if (srcBytes.Length == 0) { return null; }
            if (searchBytes.Length == 0) { return null; }
            if (srcBytes.Length < searchBytes.Length) { return null; }
            List<int> indexs = new List<int>();
            for (int i = 0; i < srcBytes.Length - searchBytes.Length; i++)
            {
                if (srcBytes[i] == searchBytes[0])
                {
                    if (searchBytes.Length == 1) { indexs.Add(i); }
                    bool flag = true;
                    for (int j = 1; j < searchBytes.Length; j++)
                    {
                        if (srcBytes[i + j] != searchBytes[j])
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag) { indexs.Add(i); }
                }
            }
            return indexs;
        }
    }
}
