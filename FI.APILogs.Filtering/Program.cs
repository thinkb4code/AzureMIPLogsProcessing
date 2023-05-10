using CsvHelper;
using CsvHelper.Configuration;
using FI.APILogs.Filtering.Models;
using System.Configuration;
using System.Globalization;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Enter Path of directory to process: ");
        var path = ConfigurationManager.AppSettings["DataFolderPath"];

        var fileName = Directory.GetFiles(path, "*.json");
        var claimsNumber = new string[] { "7003", "5008", "5009", "5010", "5011", "7004", "7002", "5007", "5006", "7001", "7000", "5015", "5014", "3012", "3013", "5016", "5012", "5013", "7900", "5018", "5017", "7800" };

        var peopleFilePath = Directory.GetFiles(path, ConfigurationManager.AppSettings["PeopleManifest"]);
        List<ClaimPeopleRecord> peopleRecords = File.ReadLines(peopleFilePath[0]).Select((l, i) => new ClaimPeopleRecord(l, i)).ToList(); //File.ReadAllText(peopleFilePath.FirstOrDefault());

        var managementReportFileName = $"{path}\\Output\\Management_Report_{DateTime.Now.ToString("MM-dd-yyyTHH-mm-ss")}.csv";
        var claimsReportFileName = $"{path}\\Output\\Claims_Report_{DateTime.Now.ToString("MM-dd-yyyTHH-mm-ss")}.csv";
        bool printHeader = true;

        var startTime = DateTime.Now;
        
        Console.WriteLine($"Labeled content report generation started at: ${startTime}");

        foreach (var file in fileName)
        {
            var fileContent = File.ReadAllText(file);
            List<AuditLog>? data = JsonSerializer.Deserialize<List<AuditLog>>(fileContent);

            if (data != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Processing file: {file} at {DateTime.Now}");

                var mipEvents = data.Where(d => d.RecordType == 43);

                Console.WriteLine($"Total records found: {data.Count} and MIP events are {mipEvents.Count()}");

                // Match on claim number prefixes in the email subject line
                //var claimsRecords = mipEvents.Where(m => Array.FindAll(claimsNumber, s => m.ItemName.Contains(s)).Length > 0).ToList();

                // Filter records
                string date = file.Replace(path+"\\", string.Empty).Replace("Audit.Exchange_", string.Empty).Replace("_12-00-00.json", string.Empty);
                //var report = FilterMIPRecords(claimsRecords, date);
                var report = FilterRecords(mipEvents.ToList(), date);

                // Create output folder if not exists
                if (!Directory.Exists($"{path}\\Output"))
                {
                    Directory.CreateDirectory($"{path}\\Output");
                }

                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = printHeader,
                    Delimiter = ",",
                    Encoding = Encoding.UTF8
                };

                if (report[0].Count() > 0)
                {
                    using (StreamWriter writer = File.AppendText(managementReportFileName))
                    using (var csv = new CsvWriter(writer, csvConfig))
                    {
                        csv.WriteRecords(report[0]);
                    }
                }

                if (report[1].Count() > 0)
                {
                    using (StreamWriter writer = File.AppendText(claimsReportFileName))
                    using (var csv = new CsvWriter(writer, csvConfig))
                    {
                        csv.WriteRecords(report[1]);
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Processing completed at {DateTime.Now}");
            }
            printHeader = false; 
        }
        Console.WriteLine($"Labeled content report generation completed at {DateTime.Now}. Execution time: {DateTime.Now.Subtract(startTime).TotalMinutes}");
        
        // Build unlabeled content reports
        managementReportFileName = $"{path}\\Output\\Management_Report_Unlabeled_{DateTime.Now.ToString("MM-dd-yyyTHH-mm-ss")}.csv";
        claimsReportFileName = $"{path}\\Output\\Claims_Report_Unlabeled_{DateTime.Now.ToString("MM-dd-yyyTHH-mm-ss")}.csv";
        printHeader = true;

        startTime = DateTime.Now;
        Console.WriteLine($"Unlabeled content report generation started at: ${startTime}");

        foreach (var file in fileName)
        {
            var fileContent = File.ReadAllText(file);
            List<AuditLog>? data = JsonSerializer.Deserialize<List<AuditLog>>(fileContent);

            if (data != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Processing file: {file} at {DateTime.Now}");

                var mipEvents = data.Where(d => d.RecordType == 2);

                Console.WriteLine($"Total records found: {data.Count} and non-MIP events are {mipEvents.Count()}");

                // Filter records
                string date = file.Replace(path + "\\", string.Empty).Replace("Audit.Exchange_", string.Empty).Replace("_12-00-00.json", string.Empty);
                //var report = FilterRecords(claimsRecords, date);
                var report = FilterNonMIPRecords(mipEvents.ToList(), date);

                // Create output folder if not exists
                if (!Directory.Exists($"{path}\\Output"))
                {
                    Directory.CreateDirectory($"{path}\\Output");
                }

                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = printHeader,
                    Delimiter = ",",
                    Encoding = Encoding.UTF8
                };

                if (report[0].Count() > 0)
                {
                    using (StreamWriter writer = File.AppendText(managementReportFileName))
                    using (var csv = new CsvWriter(writer, csvConfig))
                    {
                        csv.WriteRecords(report[0]);
                    }
                }

                if (report[1].Count() > 0)
                {
                    using (StreamWriter writer = File.AppendText(claimsReportFileName))
                    using (var csv = new CsvWriter(writer, csvConfig))
                    {
                        csv.WriteRecords(report[1]);
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Processing completed at {DateTime.Now}");
            }
            printHeader = false;
        }
        Console.WriteLine($"Unlabeled content report generation completed at {DateTime.Now}. Execution time: {DateTime.Now.Subtract(startTime).TotalMinutes}");

        Console.ReadKey();

        List<MIPEvent>[] FilterMIPRecords(List<AuditLog> logs, string date)
        {
            var claimsReport = new List<MIPEvent>();
            var mgmtReport = new List<MIPEvent>();

            logs.ForEach(l =>
            {
                var checkPerson = peopleRecords.FindAll(p => p.Email.Equals(l.Sender, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (checkPerson != null && !checkPerson.LOB.ToLower().Contains("executive"))
                {
                    if (checkPerson.Company.Equals("FNWL", StringComparison.OrdinalIgnoreCase) || checkPerson.Company.Equals("Exchange", StringComparison.OrdinalIgnoreCase))
                    {
                        claimsReport.Add(new MIPEvent()
                        {
                            Date = date,
                            Subject = l.ItemName.Replace("\n", " "),
                            LabelName = GetGenericLabel(l.LabelId),
                            Receivers = String.Join("; ", l.Receivers),
                            Sender = l.Sender,
                            JobTitle = checkPerson.JobTitle,
                            LOB = GetNormalLob(checkPerson.LOB),
                            SenderManager = checkPerson.N3,
                            Company = checkPerson.Company
                        });
                    }
                    else if (checkPerson.Company.Equals("Management", StringComparison.OrdinalIgnoreCase))
                    {
                        mgmtReport.Add(new MIPEvent()
                        {
                            Date = date,
                            Subject = l.ItemName.Replace("\n", " "),
                            LabelName = GetGenericLabel(l.LabelId),
                            Receivers = String.Join("; ", l.Receivers),
                            Sender = l.Sender,
                            JobTitle = checkPerson.JobTitle,
                            LOB = checkPerson.LOB,
                            SenderManager = checkPerson.N3,
                            Company = checkPerson.Company
                        });
                    }
                }
            });

            return new[] { mgmtReport, claimsReport };
        }

        List<NonMIPEvent>[] FilterNonMIPRecords(List<AuditLog> logs, string date)
        {
            var claimsReport = new List<NonMIPEvent>();
            var mgmtReport = new List<NonMIPEvent>();

            logs.ForEach(l =>
            {
                var checkPerson = peopleRecords.FindAll(p => p.Email.Equals(l.UserId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (checkPerson != null && !checkPerson.LOB.ToLower().Contains("executive"))
                {
                    if (checkPerson.Company.Equals("FNWL", StringComparison.OrdinalIgnoreCase) || checkPerson.Company.Equals("Exchange", StringComparison.OrdinalIgnoreCase))
                    {
                        claimsReport.Add(new NonMIPEvent()
                        {
                            Date = date,
                            //Subject = l.Item.Subject,
                            Operation = l.Operation,
                            Sender = l.UserId,
                            JobTitle = checkPerson.JobTitle,
                            LOB = GetNormalLob(checkPerson.LOB),
                            SenderManager = checkPerson.N3,
                            Company = checkPerson.Company
                        });
                    }
                    else if (checkPerson.Company.Equals("Management", StringComparison.OrdinalIgnoreCase))
                    {
                        mgmtReport.Add(new NonMIPEvent()
                        {
                            Date = date,
                            //Subject = l.Item.Subject,
                            Operation = l.Operation,
                            Sender = l.Sender,
                            JobTitle = checkPerson.JobTitle,
                            LOB = checkPerson.LOB,
                            SenderManager = checkPerson.N3,
                            Company = checkPerson.Company
                        });
                    }
                }
            });

            return new[] { mgmtReport, claimsReport };
        }

        string GetGenericLabel(string labelId)
        {
            switch (labelId)
            {
                case "bb2efefb-464c-4091-bdae-906d0eb48c19":
                    return "Public";
                case "3d57c56a-3489-401f-8ad7-eb0ac8309de4":
                    return "Proprietary";
                case "bbedcbcb-ab54-401f-bf77-cdeb80bf94b4":
                    return "Confidential";
                case "48016cbc-46d7-4f4f-a78e-0c4d5ab7be9e":
                    return "Confidential Personal Information";
                case "890c764a-bed2-40f7-a031-1e0531882b10":
                    return "Highly Confidential";
                case "ad7b2396-85f7-4b7e-ad67-9b01f313d350":
                    return "Highly Confidential Sensitive Personal Information";
                default:
                    return "";
            }
        }

        string GetNormalLob(string lob)
        {
            switch (lob.Trim().ToLower())
            {
                case "bristol west":
                    return "Bristol West";
                case "claims auto operations":
                    return "Auto";
                case "claims blo":
                    return "BLO";
                case "claims business insurance":
                    return "Business";
                case "claims property":
                case "claims workspace solutions":
                    return "Property";
                case "claims shared services":
                    return "Shared Services";
                case "claims compliance":
                //case "claims customer experience":
                //case "claims executives":
                case "claims legal services":
                case "claims strategy":
                //case "farmers brokerered solutions":
                //case "farmers new world life":
                case "finance":
                    //case "office of general counsel":
                    //case "service operations":
                    return "Miscellaneous";
                default:
                    return string.Empty;
            }
        }
    }
}