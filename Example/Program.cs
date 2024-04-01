using MilitiaDataParsing;
using System;
using System.Collections.Generic;

namespace Example
{
    class Program
    {
        static void Main()
        {
            ParserHandler parser = new ParserHandler(ParserOnOutput);

            Organization org = new Organization("Parsing \" Inc", true, new Person("Donna", "Kearney", 33, Gender.Female, new Details("3572 Poling Farm Road", "402-354-7168", "donna.kearney@parsing.inc")), new Details("2891 Jessie Street", "740-663-1757", "business@parsing.inc"));
            org.Owner.Devices.Add(new Device("UPhone 80E", "2017-03-21"));
            org.Owner.Devices.Add(new Device("PakBook 13", "2016-07-30"));
            org.Employees.Add(new Person("Dawn", "Mack", 46, Gender.Male, new Details("2725 Giraffe Hill Drive", "972-252-3109", "DawnGMack@jourrapide.com")));
            org.Employees[0].Devices.Add(new Device("MePad", "2016-11-09"));
            org.Employees.Add(new Person("Christina", "Bates", 32, Gender.Female, new Details("2714 Heavner Avenue", "770-762-5351", "ChristinaCBates@dayrep.com")));
            org.Employees.Add(new Person("Shane", "Osborne", 35, Gender.Male, new Details("1880 Timber Oak Drive", "805-926-2039", "ShaneKOsborne@teleworm.us")));

            string orgData = parser.Export(org);
            Console.WriteLine("Exporting org as orgData:");
            Console.WriteLine(orgData);

            string orgData2 = parser.Export(org);
            Console.WriteLine("Exporting org as orgData2:");
            Console.WriteLine(orgData2);

            Organization org2 = parser.Import<Organization>(orgData2) as Organization;
            string orgData3 = parser.Export(org2);
            Console.WriteLine("Imported orgData2 and exported as orgData3:");
            Console.WriteLine(orgData3);

            if (orgData == orgData2 && orgData == orgData3)
            {
                Console.WriteLine("All data is identical.");
            }

            Console.ReadLine();
        }

        private static void ParserOnOutput(object sender, OutputEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        public enum Gender { Male, Female }

        public class Details : IParsable
        {
            public string Address { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }

            public Details() { }

            public Details(string address, string phone, string email)
            {
                Address = address;
                Phone = phone;
                Email = email;
            }

            public virtual string Header { get { return "details"; } }

            public virtual void Parsing(Parser parse)
            {
                Address = parse.Auto("address", Address);
                Phone = parse.Auto("phone", Phone);
                Email = parse.Auto("email", Email);
            }
        }

        public class Device : IParsable
        {
            public string Title { get; set; }
            public string Date { get; set; }

            public Device() { }

            public Device(string title, string date)
            {
                Title = title;
                Date = date;
            }

            public string Header { get { return "device"; } }

            public void Parsing(Parser p)
            {
                Title = p.Auto("title", Title);
                Date = p.Auto("date", Date);
            }
        }

        public class Person : IParsable
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int Age { get; set; }
            public Gender Gender { get; set; }
            public Details Details { get; set; }
            public List<Device> Devices { get; set; }

            public Person()
            {
                Devices = new List<Device>();
            }

            public Person(string firstName, string lastName, int age, Gender gender, Details details, List<Device> devices)
            {
                FirstName = firstName;
                LastName = lastName;
                Age = age;
                Gender = gender;
                Details = details;
                Devices = devices;
            }

            public Person(string firstName, string lastName, int age, Gender gender, Details details) : this(firstName, lastName, age, gender, details, new List<Device>()) { }

            public string Header { get { return "person"; } }

            public void Parsing(Parser p)
            {
                FirstName = p.Auto("first_name", FirstName);
                LastName = p.Auto("last_name", LastName);
                Age = p.Auto("age", Age);
                Gender = p.Auto("gender", Gender);
                Details = p.Auto("details", Details);
                Devices = p.List("devices", Devices);
            }
        }

        public class Organization : IParsable
        {
            public string Title { get; set; }
            public bool Active { get; set; }
            public Person Owner { get; set; }
            public List<Person> Employees { get; set; }

            public Details Details { get; set; }

            public Organization()
            {
                Employees = new List<Person>();
            }

            public Organization(string title, bool active, Person owner, Details details) : this()
            {
                Title = title;
                Active = active;
                Owner = owner;
                Details = details;
            }

            public string Header { get { return "organization"; } }

            public void Parsing(Parser p)
            {
                Title = p.Auto("title", Title);
                Active = p.Auto("active", Active);
                Owner = p.Auto("owner", Owner);
                Details = p.Auto("details", Details);
                Employees = p.List("employees", Employees);
            }
        }
    }
}
