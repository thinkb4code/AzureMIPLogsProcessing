using CsvHelper;
using CsvHelper.Configuration;
using FI.APILogs.Filtering.Models;
using System.Configuration;
using System.Globalization;
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

        var peopleMgmtFilePath = Directory.GetFiles(path, ConfigurationManager.AppSettings["PeopleManifest"]);
        List<ClaimPeopleRecord> peopleMgmtRecords = File.ReadLines(peopleMgmtFilePath[0]).Select((l, i) => new ClaimPeopleRecord(l, i)).ToList(); //File.ReadAllText(peopleMgmtFilePath.FirstOrDefault());

        var startTime = DateTime.Now;
        Console.WriteLine($"Program started at: ${startTime}");

        var managementReportFileName = $"{path}\\Output\\Management_Report_{DateTime.Now.ToString("MM-dd-yyyTHH-mm-ss")}.csv";
        var claimsReportFileName = $"{path}\\Output\\Claims_Report_{DateTime.Now.ToString("MM-dd-yyyTHH-mm-ss")}.csv";
        bool printHeader = true;

        foreach (var file in fileName)
        {
            var fileContent = File.ReadAllText(file);
            List<AuditLog>? data = JsonSerializer.Deserialize<List<AuditLog>>(fileContent);

            if (data != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Processing file: {file} at {DateTime.Now}");

                var mipEvent = data.Where(d => d.RecordType == 43);

                Console.WriteLine($"Total records found: {data.Count} and MIP events are {mipEvent.Count()}");

                var claimsRecords = mipEvent.Where(m => Array.FindAll(claimsNumber, s => m.ItemName.Contains(s)).Length > 0).ToList();

                // Filter records
                var report = FilterRecords(claimsRecords);

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
        Console.WriteLine($"Program completed at {DateTime.Now}. Total Execution time: {DateTime.Now.Subtract(startTime).TotalMinutes}");
        Console.ReadKey();

        List<MIPReport>[] FilterRecords(List<AuditLog> logs)
        {
            var mipClaimReportPeople = new List<MIPReport>();
            var mipClaimReportMgmt = new List<MIPReport>();

            logs.ForEach(l =>
            {
                var checkPerson = peopleMgmtRecords.FindAll(p => p.Email.Equals(l.Sender, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (checkPerson != null)
                {
                    if (checkPerson.Company.Equals("FNWL", StringComparison.OrdinalIgnoreCase) || checkPerson.Company.Equals("Exchange", StringComparison.OrdinalIgnoreCase))
                    {
                        mipClaimReportPeople.Add(new MIPReport()
                        {
                            Date = l.CreationTime,
                            Subject = l.ItemName,
                            LabelName = GetGenericLabel(l.LabelName),
                            Receivers = String.Join("; ", l.Receivers),
                            Sender = l.Sender,
                            JobTitle = checkPerson.JobTitle,
                            LOB = checkPerson.LOB,
                            SenderManager = checkPerson.N3,
                            Company = checkPerson.Company
                        });
                    }
                    else if (checkPerson.Company.Equals("Management", StringComparison.OrdinalIgnoreCase))
                    {
                        mipClaimReportMgmt.Add(new MIPReport()
                        {
                            Date = l.CreationTime,
                            Subject = l.ItemName,
                            LabelName = l.LabelName,
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

            return new[] { mipClaimReportMgmt, mipClaimReportPeople };
        }

        string GetGenericLabel(string labelName)
        {
            switch (labelName)
            {
                case "Personal Information (Confidential)":
                    return "Confidential Personal Information";
                case "Sensitive Personal Information (Highly Confidential)":
                    return "Highly Confidential Sensitive Personal Information";
                default:
                    return labelName;
            }
        }
    }
}