using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        Console.WriteLine("*******************************************************************************");
        Console.WriteLine("* word-list-gen                                                               *");
        Console.WriteLine("*******************************************************************************");
        Console.WriteLine();

        if (args.Length <= 0)
        {
            Console.WriteLine("Wrong parameters!");
            Console.WriteLine("Usage:");
            Console.WriteLine("word-list-gen N");
            Environment.Exit(1);
            return;
        }

        var n = int.Parse(args[0]);
        if (n <= 2)
        {
            Console.WriteLine("Wrong parameters!");
            Console.WriteLine("N should be > 2");
            Environment.Exit(1);
            return;
        }

        var source = GetSourceFile();
        Console.WriteLine("Generating partial dictionary...");

        var target = GetOutputFileName(n);

        var totalCount = 0;
        var filteredCount = 0;

        using (var stream = File.OpenWrite(target))
        using (var writer = new StreamWriter(stream, Encoding.UTF8))
        {
            var lines = File.ReadLines(source, Encoding.GetEncoding("windows-1251"));
            foreach (var line in lines)
            {
                totalCount++;

                if (!FilterWord(line, n))
                {
                    continue;
                }

                writer.WriteLine(line.ToLowerInvariant());
                filteredCount++;
            }
        }

        Console.WriteLine($"Completed!");
        Console.WriteLine($"  total_count    = {totalCount}");
        Console.WriteLine($"  filtered_count = {filteredCount}");
        Console.WriteLine($"  file_name      = {target}");
    }

    static bool FilterWord(string word, int n)
    {
        if (word.Length != n)
        {
            return false;
        }

        if (word.Any(c => !char.IsLetterOrDigit(c)))
        {
            return false;
        }

        return true;
    }

    static string GetSourceFile()
    {
        const string txtFileName = "data/source.txt";
        const string zipFileName = "data/source.zip";
        const string URL = "http://speakrus.ru/dict/zdf-win.zip";

        if (!File.Exists(txtFileName))
        {
            Console.WriteLine("Downloading source dictionary...");
            Download(URL, zipFileName);
            Unzip(zipFileName, txtFileName);
        }

        return txtFileName;
    }

    static void Download(string url, string filename)
    {
        using (var http = new HttpClient())
        {
            Console.WriteLine($" request  = GET {url}");

            var response = http.GetAsync(url).Result;
            Console.WriteLine($" response = {response.StatusCode:G} {response.ReasonPhrase}");

            var stream = response.Content.ReadAsStreamAsync().Result;
            EnsureParentDirExists(filename);

            using (var file = File.OpenWrite(filename))
            {
                stream.CopyTo(file);
            }
        }
    }

    static void Unzip(string zipFileName, string txtFileName)
    {
        using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
        {
            var entry = zip.Entries.First();
            entry.ExtractToFile(txtFileName);
        }
    }

    static string GetOutputFileName(int n)
    {
        var filename = $"out/dictionary-{n}.txt";
        EnsureParentDirExists(filename);
        return filename;
    }

    static void EnsureParentDirExists(string filename)
    {
        var dirName = Path.GetDirectoryName(filename);
        if (!Directory.Exists(dirName))
        {
            Directory.CreateDirectory(dirName);
        }
    }
}
