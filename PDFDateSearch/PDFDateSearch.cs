using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using foxit.common;
using foxit.pdf;
using DateTime = System.DateTime;

namespace PDFDateSearch
{
    class PDFSearchDate
    {
        static bool partialYear = false;

        const string sn = "MS0+qHbJccR+YbgV9YX+I27nWYaMEcp5Sny1Lf1226Cx3T/KqYOmNA==";
        const string key = "8f0YFcONvRkN+ldwlpAFW0tF1H0GzlrLMVDAylEs33uto3HGtEGA9eejH/ZPa351Et2u3lzvgUSRKd/bI0TAR76EWxG18jbe78pajMQ6HU+ZlRygi+EM+YZCX5CIRqpoSNRtH+6xYSINhewD5DFUagF80M5NQnhSH4i8EdLEpLtooBfIcWMYiS7mplf64ORLDG7w1sHFcxA+YLMrUGG54xX00BnMmQO9cLOHeCJjiRebuxcviHDNe5GEwqHPKZqtkaYg0tavrGqLCGHWBPt3pA5uPIAPCOvBm88oXVuLC5rvXjkhpXMHmvglO5yIpqWnEY/Fn01XwFw3KtZr5ZX912dQRTQUkmP4xeGwl+9TG2IGRIKnByWEyGNX7QNH0L/i1CErGHY4MpDFohSz45rYCHiasPydEefz4IbStGi7xMRodlpND0WZco2DGWrfQDvkEwWko99rTXC89HZp3pjfCgwNzJxtGe+4vkOkN1UugqJK+mdlZzH0+ryzcMtk+A1ZvwNyPuAQ6ds83wRKcwfU7tvJTKrkEWPbO/Ek7tjf2sq/5RKO9IDqJiam4oSW0t03cVusOX2kn4f8mONXUSS4MYrf/f01Z5+t/mDR2jxqv6mr/Zk3CpoEOfDco39N2gGXjTQHc5s5zuwg7pBrHSJltRsrw53pD8rg0TtKV8LfrLPQtVDLf7Q55Fv7wZ5oJHgWOE7j4Z3radh6kHPZrqqMdhazX05Odc//Z1wP/ronaIvojiW1dGu7hSlk9LdjvVPSZaecRoMkSi4n3hUk3oyZjC8aKRdKWLr3I6aaOybd8I8CUD8r7fjlV8nKoyHvh7lXz2doxxkq5/dITLgvm8Uusnvcf8z+i0IPXBa9b982UIdN+M/gyhfiaEqdbjjY6lCv+TFc3TvB7Od/fjU28BR0LvVFr4AObx7Qugl8AMm1Xg7fZy6uHGR2q9MBOxd/SiccdXO8sZ1JMZlIi5tSCBszVE1n4W4RXrZv+ksFrnqrbOYObKqp5Z8riXyk0sc2LDavyn5Y0sHtdDgc12AUuAsRtaGpB2oQaRVlFQCqUrE0aMNJjNeqHl7zi85zKQORM1XrIAXFGaKVX6KInXS9hOcl8flnBTAPfjf8Up0UtwueTWjGJaVIJ69sAI1h0u7Az5M40oG270amt+k4yLORtS/ph/FF/CUHFHws7hn7E84oY5Z0M/rshYOxBcW9IkiY0Biig3qDeRd6tXa9f3TtxuptX7sCv9GS/UY1bLz0CGYLn3TBrMMSJ4ZQ";

        private static void CreateResultFolder(String output_path)
        {
            if (!Directory.Exists(output_path))
            {
                Directory.CreateDirectory(output_path);
            }
        }

        static List<DateTime> GetDates(string text)
        {

            //Stripping out all the spaces and new lines in the text
            //space separated dates 
            text = Regex.Replace(text, @"\s+|\n+|((\r\n)+)", "");

            const string months = @"January|February|March|April|May|June|July|August|September|October|November|December";
            const string mons = @"(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)(\.{0,1})";

            var digit_date_patterns = new Dictionary<String, String>
            {
                {@"(19|20)\d{2}[\\\/\-\.]((0[1-9])|1(0|1|2)|[1-9])[\\\/\-\.](0[1-9]|(1|2)\d|3(0|1)|[1-9](?!(\d|\.)))",@"(?<year>(19|20)\d{2})[\\\/\-\.](?<month>((0[1-9])|1(0|1|2)|[1-9]))[\\\/\-\.](?<day>(0[1-9]|(1|2)\d|3(0|1)|[1-9]$))"},
                {@"(0[1-9]|(1|2)\d|3(0|1)|(?<!(\d|\.))\d)[\\\/\-\.]((0[1-9])|1(0|1|2)|[1-9])[\\\/\-\.](19|20)\d{2}",@"(?<day>(0[1-9]|(1|2)\d|3(0|1)|^\d))[\\\/\-\.](?<month>((0[1-9])|1(0|1|2)|[1-9]))[\\\/\-\.](?<year>(19|20)\d{2})"}
            };

            var mixed_date_patterns = new Dictionary<String, String>
            {
                {@"(?i)(19|20)\d{2}[\\\/\-\.]{0,1}("+months+@"|"+mons+@")[\\\/\-\.]{0,1} (0[1-9]|(1|2)\d|3(0|1)|[1-9](?!(\d|\.)))",@"(?i)(?<year>(19|20)\d{2})[\\\/\-\.]{0,1}(?<month> ("+months+@"|"+mons+@"))[\\\/\-\.]{0,1}(?<day>(0[1-9]|(1|2)\d|3(0|1)|[1-9]$))"},
                {@"(?i)("+months+@"|"+mons+@")(0[1-9]|(1|2)\d|3(0|1)|[1-9])(st|nd|th){0,1}(\,){0,1}(19|20)\d{2}",@"(?i)(?<month>("+months+@"|"+mons+@"))(?<day>(0[1-9]|(1|2)\d|3(0|1)|[1-9]))(st|nd|th){0,1}(\,){0,1}(?<year>(19|20)\d{2})"},
                {@"(?i)(0[1-9]|(1|2)\d|3(0|1)|(?<!(\d|\.))\d)[\\\/\-\.]{0,1}("+months+@"|"+mons+@")[\\\/\-\.]{0,1}(19|20)\d{2}",@"(?i)(?<day>(0[1-9]|(1|2)\d|3(0|1)|^\d))[\\\/\-\.]{0,1}(?<month>("+months+@"|"+mons+@"))[\\\/\-\.]{0,1}(?<year>(19|20)\d{2})"},
                {@"(?i)(0[1-9]|(1|2)\d|3(0|1)|(?<!(\d|\.))\d)(st|nd|th){0,1}(of|dayof){0,1}("+months+@"|"+mons+@")(\,){0,1}(19|20)\d{2}",@"(?i)(?<day>(0[1-9]|(1|2)\d|3(0|1)|^\d))(st|nd|th){0,1}(of|dayof){0,1}(?<month>("+months+@"|"+mons+@"))(\,){0,1}(?<year>(19|20)\d{2})"}
            };

            var empty_day_patterns = new Dictionary<String, String>
            {
                {@"(?i)("+months+@"|"+mons+@")(19|20)\d{2}",@"(?i)(?<month>("+months+@"|"+mons+@"))(?<year>(19|20)\d{2})"},
                {@"(?i)(19|20)\d{2}("+months+@"|"+mons+@")",@"(?i)(?<year>(19|20)\d{2})(?<month>("+months+@"|"+mons+@"))"}
            };

            var partial_year_date_patterns = new Dictionary<String, String>
            {
                {@"(?<!(19|20))\d{2}[\\\/\-\.](0[1-9]|1(0|1|2)|[1-9])[\\\/\-\.](0[1-9]|(1|2)\d|3(0|1)|[1-9](?!(\d|\.)))",@"(?<year>\d{2})(?<separator1>[\\\/\-\.])(?<month>(0[1-9]|1(0|1|2)|[1-9]))(?<separator2>[\\\/\-\.])(?<day>(0[1-9]|(1|2)\d|3(0|1)|[1-9]$))"},
                {@"(0[1-9]|(1|2)\d|3(0|1)|(?<!(\d|\.))\d)[\\\/\-\.]((0[1-9])|1(0|1|2)|[1-9])[\\\/\-\.]\d{2}(?!(\d\d))",@"(?<day>(0[1-9]|(1|2)\d|3(0|1)|^\d))(?<separator1>[\\\/\-\.])(?<month>((0[1-9])|1(0|1|2)|[1-9]))(?<separator2>[\\\/\-\.])(?<year>\d{2})"}
            };

            var myDates = new List<DateTime?>();
            var provider = CultureInfo.InstalledUICulture;


            foreach (var pattern in digit_date_patterns)
            {
                myDates = myDates.Concat(Regex.Matches(text, pattern.Key).Cast<Match>().Select(m =>
                {
                    var year = int.Parse(Regex.Replace(m.Value, pattern.Value, @"${year}"));
                    var month = int.Parse(Regex.Replace(m.Value, pattern.Value, @"${month}"));
                    var day = int.Parse(Regex.Replace(m.Value, pattern.Value, @"${day}"));

                    //                    Console.WriteLine("Date matched: " + m.Value);
                    //                    Console.WriteLine("year: "+year+"  ||  month: "+month+ "  ||  day: "+day);
                    return new DateTime?(new DateTime(year, month, day));
                }).ToList()).ToList().Distinct().ToList();
            }

            foreach (var pattern in mixed_date_patterns)
            {
                //Add all occurrences of strings with the pattern to the result
                myDates = myDates.Concat(Regex.Matches(text, pattern.Key).Cast<Match>().Select(m =>
                {
                    //                    Console.WriteLine("Date matched: "+m.Value);
                    var year = int.Parse(Regex.Replace(m.Value, pattern.Value, @"${year}"));
                    var monthMatched = Regex.Replace(m.Value, pattern.Value, @"${month}");
                    var day = int.Parse(Regex.Replace(m.Value, pattern.Value, @"${day}"));

                    //                    Console.WriteLine("year: "+year+"  ||  month: "+monthMatched+ "  ||  day: "+day);

                    var month = DateTime.TryParseExact(monthMatched, new[] { "M", "MM", "MMM", "MMMM" }, provider,
                        DateTimeStyles.None, out var dateTime) ? dateTime.Month : 1; ;

                    return new DateTime?(new DateTime(year, month, day));
                }).ToList()).ToList().Distinct().ToList();
            }


            foreach (var pattern in empty_day_patterns)
            {
                myDates = myDates.Concat(Regex.Matches(text, pattern.Key).Cast<Match>().Select(m =>
                {
                    var year = int.Parse(Regex.Replace(m.Value, pattern.Value, @"${year}"));
                    var monthMatched = Regex.Replace(m.Value, pattern.Value, @"${month}");

                    //                    Console.WriteLine("Date matched: " + m.Value);
                    //                    Console.WriteLine("year: "+year+"  ||  month: "+monthMatched+ "  ||  day: empty");
                    int month;
                    int day;
                    if (DateTime.TryParseExact(monthMatched, new[] { "M", "MM", "MMM", "MMMM" }, provider,
                        DateTimeStyles.None, out var dateTime))
                    {
                        month = dateTime.Month;
                        day = DateTime.DaysInMonth(year, dateTime.Month);
                    }
                    else
                    {
                        month = 1;
                        day = 1;
                    };
                    return new DateTime?(new DateTime(year, month, day));
                }).ToList()).ToList().Distinct().ToList();
            }

            if (partialYear)
            {
                foreach (var pattern in partial_year_date_patterns)
                {
                    myDates = myDates.Concat(Regex.Matches(text, pattern.Key).Cast<Match>().Select(m =>
                    {
                        var year = int.Parse(Regex.Replace(m.Value, pattern.Value, @"19${year}"));
                        var month = int.Parse(Regex.Replace(m.Value, pattern.Value, @"${month}"));
                        var day = int.Parse(Regex.Replace(m.Value, pattern.Value, @"${day}"));
                        var sep1 = int.Parse(Regex.Replace(m.Value, pattern.Value, @"${separator1}"));
                        var sep2 = int.Parse(Regex.Replace(m.Value, pattern.Value, @"${separator2}"));

                        if (day > DateTime.DaysInMonth(year, month))
                        {
                            day = DateTime.DaysInMonth(year, month);
                        }
                        //                        Console.WriteLine("Date matched: "+m.Value);
                        //                        Console.WriteLine("year: "+year+"  ||  month: "+month+ "  ||  day: "+day);
                        return sep1.Equals(sep2) ? new DateTime?(new DateTime(year, month, day)) : null;
                    }).ToList()).ToList().Distinct().ToList();
                }
            }

            var result = new List<DateTime>();
            foreach (var myDate in myDates)
            {
                if (myDate.HasValue)
                {
                    result.Add(myDate.Value);
                }
            }
            return result.Distinct().OrderBy(x => x.Date).ToList();
        }

        private static void GrepDates(String pdf_file)
        {
            Console.WriteLine("Search dates in " + pdf_file);

            string text_name = pdf_file.Replace(".pdf", ".txt");
            string input_file = pdf_file;

            ErrorCode error_code = Library.Initialize(sn, key);
            if (error_code != ErrorCode.e_ErrSuccess)
            {
                Console.WriteLine("Library Initialize Error: {0}", error_code);
                return;
            }

            var text = "";

            try
            {

                using (var doc = new PDFDoc(input_file))
                {
                    error_code = doc.Load(null);
                    if (error_code != ErrorCode.e_ErrSuccess)
                    {
                        Console.WriteLine("The PDFDoc " + input_file + " Error: " + error_code);
                        return;
                    }
                    using (var writer = new StreamWriter(text_name, false, System.Text.Encoding.Unicode))
                    {
                        int pageCount = doc.GetPageCount();
                        for (int i = 0; i < pageCount; i++)
                        {
                            using (var page = doc.GetPage(i))
                            {
                                page.StartParse((int)PDFPage.ParseFlags.e_ParsePageNormal, null, false);
                                // Get the text select object.
                                using (var text_select = new TextPage(page, (int)TextPage.TextParseFlags.e_ParseTextNormal))
                                {
                                    int count = text_select.GetCharCount();
                                    if (count > 0)
                                    {
                                        String chars = text_select.GetChars(0, count);
                                        text = text + chars;
                                    }
                                }
                            }
                        }

                        foreach (var dateTime in GetDates(text))
                        {
                            writer.WriteLine(dateTime.Date.ToString("yyyy-MM-dd"));
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Library.Release();
        }

        static void Main(String[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please specify a folder for date search");
                return;
            }
            else if (!Directory.Exists(args[0]))
            {
                Console.WriteLine("The folder does not exist");
                return;
            }

            if (args.Length >= 2)
            {
                partialYear = args[1].ToUpper().Equals("PARTIALYEAR");
            }
            var input_path = args[0];
            var input_dir = new DirectoryInfo(input_path);
            var files = input_dir.GetFiles("*.pdf", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                GrepDates(file.FullName);
            }
        }
    }
}

