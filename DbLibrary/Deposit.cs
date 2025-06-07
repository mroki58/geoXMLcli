using System.Xml;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using System.Globalization;

namespace DbLibrary;

// Obiekt deposit reprezentujący złoże - typ danych przechowywany w bazie danych
// Zawiera informacje o geologii i geografii złoża
// Posiada metody do tworzenia złoża, ustawiania geologii i geografii, generowania XML z bazy danych
// oraz tworzenia złoża z XML
// Wykorzystuje klasę DbConnectionManager do zarządzania połączeniem z bazą danych
public class Deposit
{
    private Deposit(string _name)
    {
        geology = new Geology();
        geography = new Geography();
        name = _name;

    }

    // Tworzenie złoża przy pomocy wzorca budowniczego
    public static Deposit CreateDeposit(string name)
    {
        return new Deposit(name);
    }

    public void setGeology(string type, double estVolume, double depth, string status)
    {
        geology.Type = type;
        geology.EstimatedVolume = estVolume;
        geology.Depth = depth;
        geology.Status = status;
    }

    public void setGeography(string loc, string reg, double lat, double lon, double rad)
    {
        geography.Location = loc;
        geography.Region = reg;
        geography.Latitude = lat;
        geography.Longitude = lon;
        geography.Radius = rad;
    }

    // Do tworzenia elementu wykorzystuje się procedurę składowaną w bazie danych
    public string? createXmlUsingDb(DbConnectionManager db)
    {
        string? result;
        using (var connection = db.GetConnection())
        {
            var command = new SqlCommand(
                @"SELECT dbo.createXMLForData(
                    @name, @type, @estimatedVolume, @depth, @status, 
                    @location, @region, @latitude, @longitude, @radius
                )", connection);

            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@type", geology.Type);
            command.Parameters.AddWithValue("@estimatedVolume", geology.EstimatedVolume);
            command.Parameters.AddWithValue("@depth", geology.Depth);
            command.Parameters.AddWithValue("@status", geology.Status);
            command.Parameters.AddWithValue("@location", geography.Location);
            command.Parameters.AddWithValue("@region", geography.Region);
            command.Parameters.AddWithValue("@latitude", geography.Latitude);
            command.Parameters.AddWithValue("@longitude", geography.Longitude);
            command.Parameters.AddWithValue("@radius", geography.Radius);

            connection.Open();
            var xmlResult = command.ExecuteScalar();
            result = xmlResult?.ToString();
        }
        return result;
    }

    // Wykorzystuje typ XmlDocument do odczytania XML i utworzenia z niego obiektu Deposit
    public static Deposit createDepositByXml(string xml)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);

        XmlNode? depositNode = doc.SelectSingleNode("/deposit");
        if (depositNode == null)
            throw new Exception("Invalid XML format: missing <deposit> element");

        string depositName = depositNode.Attributes?["name"]?.Value
                         ?? depositNode.SelectSingleNode("name")?.InnerText
                         ?? string.Empty;

        Deposit deposit = new Deposit(depositName);

        XmlNode? geologyNode = depositNode.SelectSingleNode("geology");
        if (geologyNode != null)
        {
            deposit.geology.Type = geologyNode.SelectSingleNode("type")?.InnerText;
            deposit.geology.EstimatedVolume = double.Parse(
                geologyNode.SelectSingleNode("estimatedVolume")?.InnerText ?? "0",
                CultureInfo.InvariantCulture);
            deposit.geology.Depth = double.Parse(
                geologyNode.SelectSingleNode("depth")?.InnerText ?? "0",
                CultureInfo.InvariantCulture);
            deposit.geology.Status = geologyNode.SelectSingleNode("status")?.InnerText;
        }

        XmlNode? geographyNode = depositNode.SelectSingleNode("geography");
        if (geographyNode != null)
        {
            deposit.geography.Location = geographyNode.SelectSingleNode("location")?.InnerText;
            deposit.geography.Region = geographyNode.SelectSingleNode("region")?.InnerText;
            deposit.geography.Latitude = double.Parse(
                geographyNode.SelectSingleNode("latitude")?.InnerText ?? "0",
                CultureInfo.InvariantCulture);
            deposit.geography.Longitude = double.Parse(
                geographyNode.SelectSingleNode("longitude")?.InnerText ?? "0",
                CultureInfo.InvariantCulture);
            deposit.geography.Radius = double.Parse(
                geographyNode.SelectSingleNode("radius")?.InnerText ?? "0",
                CultureInfo.InvariantCulture);
        }

        return deposit;
    }



    public string name { get; set; }
    public Geology geology { get; set; }
    public Geography geography { get; set; }

}

public class Geology
{
    public string? Type { get; set; }
    public double EstimatedVolume { get; set; }
    public double Depth { get; set; }
    public string? Status { get; set; } // może być enumem
}

public class Geography
{
    public string? Location { get; set; }
    public string? Region { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Radius { get; set; }
}