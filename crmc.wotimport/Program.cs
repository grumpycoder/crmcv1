﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DuoVia.FuzzyStrings;
using FuzzyString;
using WotImport.Properties;

namespace WotImport
{
    class Program
    {
        static IList<Censor> censorList;
        private static ApplicationDbContext db; 

        static void Main(string[] args)
        {
            db = new ApplicationDbContext();

            //censorList = db.Censors.OrderByDescending(c => c.Word).ToList();

            //PopulateCensorList();
            //PopulatePersonList();

            CleanDatabasename();
            Console.WriteLine("Done");
            Console.ReadLine();

        }

        private static void CleanDatabasename()
        {
            
            string[] prefixes =
                        {
                            "dr. & mrs.", "mr. & mrs.", "mr & mrs", "mr. ", "dr. & mrs. ", "mr. & mrs. ", "mr & mrs ",
                            "mr", "mr.", "ms.", "ms", "mrs.", "mrs", "dr.", "dr", "drs.", "drs", "miss", "md", "m.d.", "md.", "phd", "ph.d", "dds", "dds.", 
                            "mr ", "mr. ", "ms. ", "ms ", "mrs. ", "mrs ", "dr. ", "dr ", "drs. ", "drs ", "miss ", "md ", "m.d. ", "md. ", "phd ", "ph.d ", "dds ", "dds. ", 
                            "inc.", ",inc", ",inc.", ", inc", ", inc.", "l.l.c.", ", llc", ", l.l.c",
                            ",inc", "m.t.t", "csc", "esq.", ", esq.", ", esq", ",esq.", ",esq", 
                            "p.a.", "md,", ", m.d.", ", md", ", r.n.", ", rn", 
                            "the rev", "rev and mrs.", "rev and mrs", "rev & mrs.", "rev & mrs", "rev.", "rev", "reverend", "the reverend", "the rev.", "the rt. rev.", "the revs", "the revs.", "reverend", 
                            "pastor", "pastor/teacher", "pastora", "father", "sister", "ed.d.", "ed. d.", "ed.d", "esq.", "llc", "p.c.", "ph.d.", "d.v.m.", ",p.c", ",p.c.", "m.a.", 
                            "attorney at law", "attorney", "atty.", "atty", "fr.", "capt.", "captain", "col.", "colonel", "lt/col", "lt.", "lcdr.", "lcdr ", "cdr.", "cdr ",
                            "usn", "usnr-r", "usnr", "(ret.)", "ret.", "maj.", "m. sgt.", "sgt.", "smsgt", "sgt", "cpt.", "usaf (ret)", "usaf(ret)", "usaf ret", 
                            "usaf", "prof.", "professor", "d.o.", "o.p.", "o.f.m", "(retired)", "(r)", "(ret)", "msgr.", "msgr", "sfcc.", "sfcc", "sfc.", "sfc", ",md", 
                            "gen.", "general", "cpl.", "cpl", "usmcr", "usmc", "in memory of:", "in memory of :", "(in memory of)", "in memory of" , "j.d.", "ll.m.", "d.m.d", "d.d.s", "r.n.",
                            "p.a", ", p.a.", ", p.c.", ",p.c.", "cmdr.", "cmdr. ", 
                            "rn,", "psy.d.", "o.s.b", "ph d", ", ph.d", ", ph.d.", ", phd" , "bsee", "&", ","
                        };
            //string[] prefixes =
            //{
            //    "jr", 
            //    "jr.", 
            //    "Jr.", 
            //    "Jr", 
            //    ", jr"
            //};

            var personList = db.Persons.Where(p => p.Firstname != "" && p.Lastname != " ").OrderBy(p => p.Id).Skip(0).Take(100000);
            //var personList = db.Persons.Where(p => prefixes.Contains(p.Lastname.ToLower()) || prefixes.Contains(p.Firstname.ToLower()) && p.Firstname == null).OrderBy(p => p.Id).Skip(0).Take(100);
            //var personList = db.Persons.Where(p => prefixes.Contains(p.Lastname.ToLower()) && p.Id == 233718).OrderBy(p => p.Id); //.Skip(0).Take(100);
            //var personList = db.Persons.Where(p => prefixes.Contains(p.Lastname.ToLower())).Where(i => i.Firstname == null).OrderBy(p => p.Id); //.Skip(0).Take(100);
            //var personList = db.Persons.Where(p => p.Firstname == null).OrderBy(p => p.Id).Skip(0).Take(100);
            //var personList = db.Persons.Where(p => prefixes.Contains(p.Lastname.ToLower())).OrderBy(p => p.Id);
            //var personList = db.Persons.Where(p => p.Lastname.Length == 1).OrderBy(p => p.Id);
            //var personList = db.Persons.Where(p => p.Lastname.Contains("d.v.m.")).OrderBy(p => p.Id);
            //var personList = db.Persons.OrderBy(p => p.Id).Skip(400000).Take(100000);

            foreach (var person in personList)
            {
                
                var first = person.Firstname.Trim() ?? "";
                var last = person.Lastname.Trim() ?? "";

                var fullname = first + " " + last; 

                var fullArray = fullname.Trim().Split(' ');
                var cleanFullName = String.Join(" ", fullArray.Where(s => !prefixes.Contains(s.ToLower().Trim())));
                var 
                    temp = new List<string>(cleanFullName.Split(' '));

                var fn = temp.First();
                temp.Remove(temp.First());
                var ln = string.Join(" ", temp);

                //var fn = string.Join(" ", temp);

                //var ln = temp.Last();
                //temp.Remove(temp.Last());
                //var fn = string.Join(" ", temp);

                var fuzzyMatchValue = (decimal) GetMatchValue(cleanFullName);

                person.Firstname = fn.Replace(',', ' ');
                person.Lastname = ln.Replace(',', ' ');
                person.FuzzyMatchValue = fuzzyMatchValue;
                if (person.EmailAddress != null)
                    person.EmailAddress = person.EmailAddress.ToLower();

                //Console.WriteLine("Updating {0}:{1} : {2}", person.Id, fullname, person.Firstname + " " + person.Lastname);
                Console.WriteLine(string.Format("FullName: {0}", fullname));
                Console.WriteLine(string.Format("First: {0}  Last: {1}", person.Firstname, person.Lastname));
                Debug.WriteLine(person.Firstname);
                //Console.WriteLine("{0}: {1}", fn + " " + ln, fuzzyMatchValue);
                db.SaveChangesAsync();

            }
            Console.WriteLine("Save database changes ...");
            //db.SaveChangesAsync();
            Console.WriteLine("Done");
        }

        private static string getString(object o)
        {
            if (o == DBNull.Value) return null;
            return (string)o;
        }

        private static void PopulatePersonList()
        {

            var connStr = @"Data Source=.\dev2012;Initial Catalog=sandbox;Integrated Security=True";

            var conn = new SqlConnection(connStr);

            conn.Open();

            //var wallnames = new SqlDataAdapter("SELECT * FROM dbfl_current", conn);

            //var ds = new DataSet("sandbox");
            //wallnames.FillSchema(ds, SchemaType.Source, "dbfl_current");
            //wallnames.Fill(ds, "dbfl_current");
            //var dt = ds.Tables["dbfl_current"];

            var wallnames = new SqlDataAdapter("select * from wall_out where id > 99852 order by id", conn);

            var ds = new DataSet("sandbox");
            wallnames.FillSchema(ds, SchemaType.Source, "wall_out");
            wallnames.Fill(ds, "wall_out");
            var dt = ds.Tables["wall_out"];

            //foreach (DataRow row in dt.Rows)
            //{
            //    Console.WriteLine(row["name"].ToString());
            //}


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

            //const string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\temp\SPLCWOT.mdb;User Id=;Password=;";
            const string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\temp\\SPLCWOT.accdb;";
            //const string connectinString = Settings.Default.SPLCWOT2ConnectionString; 

            const string queryString = "SELECT row_id, account_id, NAME_LINE, ZIP_CODE FROM wall_out where id >= 5740";

            using (var connection = new OleDbConnection(connectionString))
            using (var command = new OleDbCommand(queryString, connection))
            {
                try
                {
                    var count = 1;
                    //connection.Open();
                    //var reader = command.ExecuteReader();
                    foreach (DataRow row in dt.Rows)
                    {
                        //var name = reader[2].ToString().Split(' ');
                        //var cleanName = String.Join(" ", name.Where(s => !prefixes.Contains(s.ToLower().Trim())));
                        //var temp = new List<string>(cleanName.Split(' '));

                        string tempname;
                        if (String.IsNullOrEmpty(row["firstname"].ToString()))
                        {
                            tempname = row["lastname"].ToString();
                        }
                        else
                        {
                            tempname = row["firstname"].ToString() + " " + row["lastname"].ToString();
                        }
                        var name = tempname.Split(' ');
                        var cleanName = String.Join(" ", name.Where(s => !prefixes.Contains(s.ToLower().Trim())));
                        var temp = new List<string>(cleanName.Split(' '));

                        //var name = row["name"].ToString().Split(' ');
                        //var cleanName = String.Join(" ", name.Where(s => !prefixes.Contains(s.ToLower().Trim())));
                        //var temp = new List<string>(cleanName.Split(' '));


                        var ln = temp.Last();
                        temp.Remove(temp.Last());
                        var fn = String.Join(" ", temp);

                        var person = new Person()
                        {
                            Firstname = fn,
                            Lastname = ln,
                            AccountId = row["lookupid"].ToString(),
                            FuzzyMatchValue = (decimal)GetMatchValue(cleanName),
                            Zipcode = row["zip"].ToString(),
                            DateCreated = DateTime.Now,
                            IsDonor = true,
                            IsPriority = false
                        };

                        Console.WriteLine("Adding to list: {0}", person.Firstname + person.Lastname);
                        //personList.Add(person);
                        //if (personList.Count != 100) continue;
                        Console.WriteLine("Saving data...{0}", count);
                        count += 1;
                        db.Persons.Add(person);
     
                        //db.Persons.AddRange(personList);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException validation)
                        {
                            Debug.WriteLine(validation.Message);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error: {0}: {1}", person.AccountId, e.Message);
                            Debug.WriteLine("Error: {0}", person.AccountId);
                            Debug.WriteLine(e.Message);
                        }
                        person = null; 
                        Console.WriteLine("Done... Moving on");
                        personList.Clear();
                    }
                    //reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            db.Persons.AddRange(personList);
            Console.WriteLine("Saving to database...");
            db.SaveChanges();
            Console.WriteLine("Finished ...");
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
                if (string.IsNullOrEmpty(censor.Word)) continue;
                var diceArray =
                    nameArray.Select(name => name.ToLower().DiceCoefficient(censor.Word.ToLower())).ToList();
                if (diceArray.Max() > maxValue)
                {
                    maxValue = diceArray.Max();
                }
            }
            return maxValue;
        }

    }
}
