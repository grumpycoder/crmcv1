using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DuoVia.FuzzyStrings;
using FuzzyString;

namespace WotImport
{
    class Program
    {
        static void Main(string[] args)
        {

//            PopulateCensorList();
//            PopulatePersonList();

            Console.WriteLine("Done");
            Console.ReadLine();

        }

        private static void PopulatePersonList()
        {
            var db = new ApplicationDbContext();

            string[] prefixes =
                        {
                            "dr. & mrs.", "mr. & mrs.", "mr & mrs", 
                            "mr", "mr.", "ms.", "ms", "mrs.", "mrs", "dr.", "dr", "drs.", "drs", "miss", "md", "m.d.", "md.", "phd", "ph.d", "dds", "dds.", 
                            "the rev", "rev and mrs.", "rev and mrs", "rev & mrs.", "rev & mrs", "rev.", "rev", "reverend", "the reverend", "the rev.", "the rt. rev.", "the revs", "the revs.", "reverend", 
                            "pastor", "pastor/teacher", "pastora", "father", "sister", "ed.d.", "ed. d.", "ed.d", 
                            "attorney at law", "attorney", "atty.", "atty", "fr.", "capt.", "captain", "col.", "colonel", "lt/col", "lt.", "lcdr.", "lcdr ", "cdr.", "cdr ",
                            "usn", "usnr-r", "usnr", "(ret.)", "ret.", "major", "maj.", "m. sgt.", "sgt.", "smsgt", "sgt", "cpt.", "usaf (ret)", "usaf(ret)", "usaf ret", 
                            "usaf", "prof.", "professor", "d.o.", "o.p.", "o.f.m", "(retired)", "(r)", "(ret)", "msgr.", "msgr", "sfcc.", "sfcc", "sfc.", "sfc", 
                            "gen.", "general", "cpl.", "cpl", "usmcr", "usmc", "in memory of:", "in memory of :", "(in memory of)", "in memory of" , "j.d.", "ll.m.", "d.m.d", "d.d.s", "r.n.",
                            "rn,", "psy.d.", "o.s.b", "ph d" , "bsee", "&" 
                        };

            var personList = new List<Person>();

            const string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\temp\SPLCWOT.mdb;User Id=;Password=;";
            const string queryString = "SELECT row_id, account_id, NAME_LINE, ZIP_CODE FROM wall_out";

            using (var connection = new OleDbConnection(connectionString))
            using (var command = new OleDbCommand(queryString, connection))
            {
                try
                {
                    connection.Open();
                    var reader = command.ExecuteReader();

                    var count = 0;
                    while (reader.Read())
                    {
                        var name = reader[2].ToString().Split(' ');
                        var cleanName = String.Join(" ", name.Where(s => !prefixes.Contains(s.ToLower().Trim())));
                        var temp = new List<string>(cleanName.Split(' '));

                        var ln = temp.Last();
                        temp.Remove(temp.Last());
                        var fn = String.Join(" ", temp);

                        var person = new Person()
                        {
                            Firstname = fn,
                            Lastname = ln,
                            FuzzyMatchValue = (decimal)GetMatchValue(cleanName),
                            Zipcode = reader[3].ToString(),
                            DateCreated = DateTime.Now
                        };
                        Console.WriteLine("Adding to list: {0}", person.Firstname + person.Lastname);
                        personList.Add(person);
                        if (personList.Count != 100) continue;
                        Console.WriteLine("Saving data...");
                        db.Persons.AddRange(personList);
                        db.SaveChanges();
                        Console.WriteLine("Done... Moving on");
                        personList.Clear();
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            db.Persons.AddRange(personList);
            Console.WriteLine("Saving to database...");
            db.SaveChanges();
        }

        private static void PopulateCensorList()
        {
            var db = new ApplicationDbContext();
            var censorList = new List<Censor>();

            using (var sr = new StreamReader(@"c:\temp\blacklist.txt"))
            {
                while (sr.Peek() > 0)
                {
                    Console.WriteLine("Adding {0} to censors", sr.ReadLine());
                    var line = sr.ReadLine();
                    censorList.Add(new Censor() { Word = line });
                }
            }
            db.Censors.AddRange(censorList);
            db.SaveChanges();
        }

        static double GetMatchValue(string fullname)
        {
            var db = new ApplicationDbContext();
            var list = db.Censors.OrderByDescending(o => o.Word);

            double maxValue = 0;
            var nameArray = fullname.Split(' ');
            nameArray[nameArray.Length - 1] = fullname;

            foreach (var censor in list)
            {
                var diceArray = nameArray.Select(name => name.ToLower().DiceCoefficient(censor.Word.ToLower())).ToList();
                if (diceArray.Max() > maxValue) { maxValue = diceArray.Max(); }
            }
            return maxValue;
        }

    }
}
