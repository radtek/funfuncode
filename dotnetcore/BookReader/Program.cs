﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using WordAnalyser.Model;

namespace BookReader
{
    /// <summary>
    /// - 仅从目前有的字库中进行统计.
    /// - 使用Top1020 作为图书ID，同时保存前5行单词作为书籍简介，这样可以留待进一步人工
    /// 完善书籍基础信息、勘误。
    /// </summary>
    class Parser
    {
        private double TotalTime = 0;
        private Int64 TotalBook = 0;
        private Int64 TotalWords = 0;

        public void ReadTxtFile(string filePathName, Language language, BookCategory category, int skipLines, string connectionString, ref Dictionary<string, WordStatistics> wordsRepo)
        {
            var sw = new Stopwatch();

            var fi = new FileInfo(filePathName);
            if (fi.Extension != ".txt")
                return;

            sw.Start();
            var lines = File.ReadLines(filePathName);
            var cntdict = new Dictionary<string, Int64>();
            Int64 wc = 0;
            Int64 tc = 0;
            Int64 linenum = 0;
            // 定义enumerable 避免可能的对IEnumerable的遍历，这是ReSharper提示的优化项。
            var enumerable = lines as IList<string> ?? lines.ToList();
            foreach (var line in enumerable)
            {
                ++linenum;
                if (linenum <= skipLines) // todo: this is temp for downloaded book
                    continue;
                var cline = GetLetters(line, language);
                foreach (var letter in cline)
                {
                    if (cntdict.ContainsKey(letter))
                    {
                        cntdict[letter] += 1;
                    }
                    else
                    {
                        cntdict[letter] = 1;
                        ++wc;
                    }
                    ++tc;
                }
            }

            if (wc < 50)
            {
                Console.WriteLine(
                    $"{TotalBook}: Parse total words: {tc}, different words: {wc}, less than 50!!!!! {fi.Name.Replace(".txt", "")}");
                return;
            }
            List<KeyValuePair<string, Int64>> myList = cntdict.ToList();
            myList.Sort(
                (pair1, pair2) => pair2.Value.CompareTo(pair1.Value)
            );
            var top1020 = string.Join("", myList.Skip(10).Take(10).Select(p => p.Key).ToArray());
            bool findbook = true;

            try
            {
                var context = Program.GetBookContext(connectionString);
                // 添加跟踪信息
                var booktrack = new BookTrack
                {
                    Top1020 = top1020,
                    OriginalPath = filePathName,
                    BookInfo = string.Join('\n', enumerable.Skip(skipLines).Take(5))
                };
                context.Add(booktrack);
                context.SaveChanges();

                var book = context.Books.SingleOrDefault(p => p.TopIndexWords == top1020); // Include(p => p.Language).Include(p => p.BookCategory).
                if (book != null)
                {
                    Console.WriteLine($"book already exists: {fi.Name}, tracked in BookTrack and not calculate statistics.");
                    return;
                }
                if (book == null)
                {
                    findbook = false;
                    book = new Book
                    {
                        LanguageId = language.LanguageId,
                        BookName = fi.Name.Replace(".txt", ""),
                        LastDateTime = DateTime.UtcNow,
                        BookInfo = string.Join('\n', enumerable.Skip(skipLines).Take(5))
                    };
                }
                book.PrevWordCount = book.LastWordCount;
                book.PrevTotalCount = book.LastTotalCount;
                book.LastWordCount = wc;
                book.LastTotalCount = tc;
                if (findbook) context.Update(book);
                else
                {
                    book.TopIndexWords = top1020;
                    // 2018年4月17日 EF Core Add function returns negative id，why?
                    context.Add(book);
                }
                context.SaveChanges();
                MapBookCategory map = new MapBookCategory
                {
                    BookId =  book.BookId,
                    BookCategoryId = category.BookCategoryId
                };
                context.Add(map);
                context.SaveChanges();
                var br = new BookResult
                {
                    Book = book,
                    BookId = findbook ? book.BookId : 0,
                    LanguageId = language.LanguageId,
                    BookTrack = booktrack.BookTrackId,
                    ResultDateTime = DateTime.UtcNow,
                    WordCount = wc,
                    TotalCount = tc,
                    Top10 = string.Join("", myList.Take(10).Select(p => p.Key).ToArray()),
                    Top1020 = top1020,
                    Top50 = string.Join("", myList.Take(50).Select(p => p.Key).ToArray())
                };
                context.Add(br);
                context.SaveChanges();

                var newwords = new List<WordStatistics>();
                var wrs = new List<WordResult>();
                foreach (var dic in cntdict)
                {
                    wrs.Add(new WordResult
                    {
                        Book = book,
                        BookId = book.BookId,
                        WordCount = dic.Value,
                        WordLetter = dic.Key
                    });
                    if (!wordsRepo.ContainsKey(dic.Key))
                    {
                        wordsRepo[dic.Key] = new WordStatistics
                        {
                            WordUnicode = dic.Key,
                            TotalBook = 1,
                            TotalWords = wc,
                            TotalOccur = dic.Value,
                            MaxRatio = (double)dic.Value / (double)wc,
                            MaxWords = wc,
                            MaxOccur = dic.Value,
                            BookCategory = category,
                            BookCategoryId = category.BookCategoryId,
                            FirstBookId = book.BookId,
                            WordLength = dic.Key.Length
                        };
                    }
                    else
                    {
                        var wordsta = wordsRepo[dic.Key];
                        wordsta.TotalBook += 1;
                        wordsta.MaxOccur = Math.Max(wordsta.MaxOccur, dic.Value);
                        wordsta.MaxWords = Math.Max(wordsta.MaxWords, wc);
                        wordsta.MaxRatio = Math.Max(wordsta.MaxRatio, (double)dic.Value / (double)wc);
                        wordsta.TotalOccur += dic.Value;
                        wordsta.TotalWords += wc;
                        //if (findbook) todo 这里的计算有误！
                        //{
                        //    wordsta.TotalOccur -= ?? not book.PrevWordCount;
                        //    wordsta.TotalWords -= ?? not book.PrevTotalCount;
                        //}
#if false
#else
                        wordsRepo[dic.Key] = wordsta;
#endif
                    }
                }
#if false
                context.AddRange(wrs);
                context.UpdateRange(wordsrepo.Values);
                context.SaveChanges();
                context.AddRange(newwords);
                context.SaveChanges();
#else

#endif
                sw.Stop();
                ++TotalBook;
                TotalWords += wc;
                TotalTime += sw.ElapsedMilliseconds;
                Console.WriteLine(
                    $"{TotalBook}: Parse finished, total words: {tc}, different words: {wc}, average elapsed milliseconds: {(double) TotalTime / (double) TotalBook}, average words: {(double) TotalWords / (double) TotalBook}. {book.BookName}");
                Int64 i = 0;
                foreach (var count in myList)
                {
                    if (i++ > 10)
                        break;
                    Console.WriteLine($"{i}: {count.Key}: {count.Value}");
                }
            }
            catch (Exception e)
            {
                // must create new context since old context has dirty data
                var context = Program.GetBookContext(connectionString);
                context?.Add(new BookException
                {
                    Top1020 = top1020,
                    ErrorMsg = e.ToString(),
                    BookInfo = fi.FullName
                });
                context?.SaveChanges();
                Console.WriteLine($"{TotalBook}: Parse failed: {e}");
            }
        }

        private List<string> GetLetters(string line, Language language)
        {
            var lst = new List<string>(line.Length);
            // 简体汉字
            if (language.LanguageCode == "zh_CN")
            {
                foreach (var c in line)
                {
                    if (c > 0x4E00 && c < 0x9FCB)
                    {
                        lst.Add(c.ToString());
                    }
                }
            }
            else //if (language.LanguageCode == "en_US")
            {
                Regex rgx = new Regex("[^a-zA-Z -]");
                // 都用小写字符// 不能直接把“—”放进第一个regex
                line = rgx.Replace(line, " ").ToLower(); // only trim '-' postfix or prefix

                line = line.Replace("--", " ").Replace("—", " ");
                // 避免有些符号黏在单词上
                // new char[]{' ', ',', '.', '\'', ';','\"', '’', '‘','?','!' }
                lst.AddRange(line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(t => t.Length > 1)
                    .Select(p => p.Trim('-')));
            }
            return lst;
        }
    }
    class Program
    {
        /// <summary>
        /// 将文本数据读取到系统中。
        /// https://carlos.mendible.com/2016/07/11/step-by-step-dotnet-core-and-entity-framework-core/
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            BookContext context = null;
            if (args.Length == 2 && args[0].Trim().ToLower() == "initsqlite")
            {
                context = GetBookContext("DataSource=" + args[1]);
                if (context != null)
                {
                    Console.WriteLine("success!");
                }
                else
                {
                    Console.WriteLine("Fail!");
                }
                return;
            }
            if (args.Length == 2 && args[0].Trim().ToLower() == "initmysql")
            {
                context = GetBookContext(args[1]);
                if (context != null)
                {
                    Console.WriteLine("success!");
                }
                else
                {
                    Console.WriteLine("Fail!");
                }
                return;
            }
            // <program> <dir> <language>
            if (args.Length != 6)
            {
                Console.WriteLine("Usage-1: <program> <dir(\"E:\\xiabook\")> <language(zh_CN|en_US)> <category(MordernMasterwork|MordernMasterwork|AcientMasterbook|MordernChildrenNobel|...)> <skip-lines> sqlite <dbfile(book.db)>");
                Console.WriteLine("Usage-2: <program> <dir(\"E:\\xiabook\")> <language(zh_CN|en_US)> <category(MordernMasterwork|MordernMasterwork|AcientMasterbook|MordernChildrenNobel|...)> <skip-lines> mysql <connection-string(server=localhost;port=3306;database=sakila;user=test;password=test)>");
                Console.WriteLine("Usage-3: <program> initsqlite <dbfile(book.db)>");
                Console.WriteLine("Usage-4: <program> initmysql <connection-string(server=localhost;port=3306;database=sakila;user=test;password=test)>");
                return;
            }
            var datapath = args[0];//@"E:\xiabook";
            string connectionString = "";
            if(args[4] == "sqlite")
                connectionString = "DataSource=" + args[5];
            else if (args[4] == "mysql")
                connectionString = args[5];
            else
            {
                return;
            }
            context = GetBookContext(connectionString);
            if (context == null)
            {
                Console.WriteLine("Failed to get book db context!");
                return;
            }
            var lang = context.Languages.SingleOrDefault(p => p.LanguageCode == args[1]);
            if (lang == null)
            {
                Console.WriteLine("Please init language database using LanguageInit tool.");
                return;
            }
            var cate = context.BookCategories.SingleOrDefault(p => p.CategoryType == args[2]);
            if (cate == null)
            {
                cate = new BookCategory
                {
                    CategoryType = args[2]
                };
                context.Add(cate);
                context.SaveChanges();
            }


            var parser = new Parser();
            DirSearch(datapath, s =>
            {
                if (s.EndsWith("rar"))
                {
                    UnRar(s);
                }
            });
            //DirSearch(datapath, s =>
            //{
            //    FileInfo fi = new FileInfo(s);
            //    var newname = fi.FullName.Replace("_下书网www.xiabook.com", "");
            //    File.Move(s, newname);
            //});

            // todo 2018年4月14日 利用缓存加快了10倍速度（比直接使用context.WordStatisticses.SingleOrDefault），
            // 但是带来了多个程序同时访问数据库带来的数据不一致问题。
            Dictionary<string, WordStatistics> wordsRepo = context.WordStatisticses.ToDictionary(p => p.WordUnicode, p => p);

            DirSearch(datapath, s =>
            {
                parser.ReadTxtFile(s, lang, cate, Convert.ToInt32(args[3]), connectionString, ref wordsRepo);
            });
            Console.WriteLine($"{DateTime.Now:yyyyMMdd HH:mm:ss.fff} Book recursive visit finished. starting commit!");
            using (var targetconn = new SqliteConnection(
                "" +
                new SqliteConnectionStringBuilder(connectionString)))
            {
                targetconn.Open();
                using (var transaction = targetconn.BeginTransaction())
                {
                    foreach (var item in wordsRepo.Values)
                    {
                        var insertCommand = targetconn.CreateCommand();
                        insertCommand.Transaction = transaction;
                        insertCommand.CommandText =
                            $"INSERT or replace into WordStatisticses ( WordUnicode, WordLength, TotalBook,TotalWords,TotalOccur,MaxWords,MaxOccur,MaxRatio,BookCategoryId,FirstBookId )" +
                            $" VALUES ( '{item.WordUnicode}', {item.WordLength}, {item.TotalBook}, {item.TotalWords},{item.TotalOccur}, {item.MaxWords}, {item.MaxOccur},{item.MaxRatio},{item.BookCategoryId},{item.FirstBookId} )";
                        insertCommand.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
            Console.WriteLine($"{DateTime.Now:yyyyMMdd HH:mm:ss.fff} Commit db finished.");
            Directory.CreateDirectory("result");
            using(var writer = new StreamWriter($"result/{DateTime.Now:yyyyMMddHHmmss}.txt"))
            {
                writer.WriteLine("WordUnicode, WordLength, TotalBook,TotalWords,TotalOccur,MaxWords,MaxOccur,MaxRatio,BookCategoryId,FirstBookId");
                var sorted = wordsRepo.Values.ToList();
                sorted.Sort((pair1, pair2) => pair2.TotalOccur.CompareTo(pair1.TotalOccur));
                foreach (var item in sorted)
                {
                    writer.WriteLine($"{item.WordUnicode}, {item.WordLength}, {item.TotalBook}, {item.TotalWords},{item.TotalOccur}, {item.MaxWords}, {item.MaxOccur},{item.MaxRatio},{item.BookCategoryId},{item.FirstBookId}");
                }
            }
        }

        public static BookContext GetBookContext(string connectionString)
        {
            //var builder = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            //IConfigurationRoot configuration = builder.Build();

            //// Get the connection string
            //string connectionString = configuration.GetConnectionString("BookContext");
            return BookContextFactory.Create(connectionString);
        }

        private static void UnRar(string filepath)
        {
            var fi = new FileInfo(filepath);
            // Microsoft.Win32 and System.Diagnostics namespaces are imported
            //Dim objRegKey As RegistryKey
            RegistryKey objRegKey;
            objRegKey = Registry.ClassesRoot.OpenSubKey("WinRAR\\Shell\\Open\\Command");
            // Windows 7 Registry entry for WinRAR Open Command

            // Dim obj As Object = objRegKey.GetValue("");
            Object obj = objRegKey.GetValue("");

            //Dim objRarPath As String = obj.ToString()
            string objRarPath = obj.ToString();
            objRarPath = objRarPath.Substring(1, objRarPath.Length - 7);

            objRegKey.Close();

            //Dim objArguments As String
            string objArguments;
            // in the following format
            // " X G:\Downloads\samplefile.rar G:\Downloads\sampleextractfolder\"
            objArguments = " X " + " " + filepath + " " + " " + fi.DirectoryName;

            // Dim objStartInfo As New ProcessStartInfo()
            ProcessStartInfo objStartInfo = new ProcessStartInfo();

            // Set the UseShellExecute property of StartInfo object to FALSE
            //Otherwise the we can get the following error message
            //The Process object must have the UseShellExecute property set to false in order to use environment variables.
            objStartInfo.UseShellExecute = false;
            objStartInfo.FileName = objRarPath;
            objStartInfo.Arguments = objArguments;
            objStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            objStartInfo.WorkingDirectory = fi.DirectoryName + "\\";

            //   Dim objProcess As New Process()
            Process objProcess = new Process();
            objProcess.StartInfo = objStartInfo;
            objProcess.Start();
            objProcess.WaitForExit();


            try
            {
                fi.Delete();
                if (fi.DirectoryName != null)
                {
                    DirectoryInfo di = new DirectoryInfo(fi.DirectoryName);
                    FileInfo[] urlfiles = di.GetFiles("*.url")
                        .Where(p => p.Extension == ".url").ToArray();
                    foreach (FileInfo file in urlfiles)
                    {
                        try
                        {
                            file.Attributes = FileAttributes.Normal;
                            File.Delete(file.FullName);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                
            }
        }

        static void DirSearch(string sDir, Action<string> doAction)
        {
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    doAction(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    DirSearch(d, doAction);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }
    }
}
