// See https://aka.ms/new-console-template for more information
using CsvHelper;
using FI.APILogs.Filtering.Models;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

Console.WriteLine("Enter Path of directory to process: ");
// var path = Console.ReadLine();
var path = "C:\\Users\\mrana\\OneDrive\\Documents\\Farmers Insurance";

var fileName = Directory.GetFiles(path, "*.json");
var claimsNumber = new string[] { "7003", "5008", "5009", "5010", "5011", "7004", "7002", "5007", "5006", "7001", "7000", "5015", "5014", "3012", "3013", "5016", "5012", "5013", "7900", "5018", "5017", "7800" };

var peopleMgmtFilePath = Directory.GetFiles(path, "Management_Claims.csv");
List<ClaimPeopleRecord> peopleMgmtRecords = File.ReadLines(peopleMgmtFilePath[0]).Select((l, i) => new ClaimPeopleRecord(l, i)).ToList(); //File.ReadAllText(peopleMgmtFilePath.FirstOrDefault());

var startTime = DateTime.Now;
Console.WriteLine($"Program started at: ${startTime}");

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

        // Filter Mgmt Records
        //var mgmtRecords = FilterRecords(claimsRecords, new[] { "Management" });
        //// Filter Claims Records
        //var peopleRecords = FilterRecords(claimsRecords, new[] { "FNWL", "Exchange" });
        var report = FilterRecords(claimsRecords);

        // Create output folder if not exists
        if (!Directory.Exists($"{path}\\Output"))
        {
            Directory.CreateDirectory($"{path}\\Output");
        }


        if (report[0].Count() > 0)
        {
            using (StreamWriter writer = File.AppendText($"{path}\\Output\\Management_Report_{DateTime.Now.ToString("MM-dd-yyyTHH-mm-ss")}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(report[0]);
            }
        }

        if (report[1].Count() > 0)
        {
            using (StreamWriter writer = File.AppendText($"{path}\\Output\\People_Report_{DateTime.Now.ToString("MM-dd-yyyTHH-mm-ss")}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(report[1]);
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Processing completed at {DateTime.Now}");
    }

    Console.WriteLine($"Program completed at {DateTime.Now}. Total Execution time: {DateTime.Now.Subtract(startTime).TotalMinutes}");
    Console.ReadKey();
}

List<MIPClaimReport>[] FilterRecords(List<AuditLog> logs)
{
    var mipClaimReportPeople = new List<MIPClaimReport>();
    var mipClaimReportMgmt = new List<MIPClaimReport>();

    logs.ForEach(l => {
        var checkPerson = peopleMgmtRecords.FindAll(p => p.Email.Equals(l.Sender, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        if (checkPerson != null)
        {
            if (checkPerson.Company.Equals("FNWL", StringComparison.OrdinalIgnoreCase) || checkPerson.Company.Equals("Exchange", StringComparison.OrdinalIgnoreCase))
            {
                mipClaimReportPeople.Add(new MIPClaimReport() {
                    CreationTime = l.CreationTime,
                    ItemName = l.ItemName,
                    LabelName = GetGenericLabel(l.LabelName),
                    Receivers = l.Receivers,
                    Sender = l.Sender,
                    JobTitle = checkPerson.JobTitle,
                    LOB = checkPerson.LOB,
                    N3 = checkPerson.N3,
                    Company = checkPerson.Company
                });
            }else if(checkPerson.Company.Equals("Management", StringComparison.OrdinalIgnoreCase)){
                mipClaimReportMgmt.Add(new MIPClaimReport()
                {
                    CreationTime = l.CreationTime,
                    ItemName = l.ItemName,
                    LabelName = l.LabelName,
                    Receivers = l.Receivers,
                    Sender = l.Sender,
                    JobTitle = checkPerson.JobTitle,
                    LOB = checkPerson.LOB,
                    N3 = checkPerson.N3,
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
            return "Personal Information (Confidential)";
        case "Sensitive Personal Information (Highly Confidential)":
            return "Highly Confidential Sensitive Personal Information";
        default:
            return labelName;
    }
}