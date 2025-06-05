using System.Xml;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;

namespace DbLibrary;

class Deposit
{
    private Deposit(string _name)
    {
        geology = new Geology();
        geography = new Geography();
        name = _name;

    }

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



    public string name { get; set; }
    public Geology geology { get; set; }
    public Geography geography { get; set; }

}

class Geology
{
    public string? Type { get; set; }
    public double EstimatedVolume { get; set; }
    public double Depth { get; set; }
    public string? Status { get; set; } // może być enumem
}

class Geography
{
    public string? Location { get; set; }
    public string? Region { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Radius { get; set; }
}