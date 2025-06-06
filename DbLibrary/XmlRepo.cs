using System.Globalization;
using System.Xml;
using Microsoft.Data.SqlClient;

namespace DbLibrary;

public interface IXmlRepo
{
    string GetElementById(int id, string path = ".");
    string GetGeologyElementById(int id);
    string GetGeographyElementById(int id);
    string GetElementByName(string name);
    int GetIdWithName(string name);
    int AddXmlToDb(string xml);
    SortedDictionary<int, string> GetXmlValues(string path);

    int ModifyLatitude(double latitude, int id);
    int ModifyLongitude(double longitude, int id);
    int ModifyRadius(double radius, int id);
    int ModifyName(string name, int id);
    int ModifyEstimatedVolume(double estimatedVolume, int id);
    int ModifyGeography(string geographyXml, int id);
    int ModifyGeology(string geologyXml, int id);
    int DeleteByName(string name);
    int DeleteForLocation(string location);
    int DeleteForQuantityLessThan(double maxValue);
}

public class XmlRepo : IXmlRepo
{
    public DbConnectionManager db_connect;
    public XmlRepo(DbConnectionManager dbmanager)
    {
        db_connect = dbmanager;
    }

    ////////////////////////////////////////////////////////////////////////////////
    // ------------------------- SEKCJA: Pobieranie danych ------------------------
    ////////////////////////////////////////////////////////////////////////////////

    public string GetElementById(int id, string path = ".")
    {
        Utils.validateXPath(path);

        using (var connection = db_connect.GetConnection())
        {
            connection.Open();
            string query = $"SELECT Content.query('{path}') as result FROM dbo.xmltable WHERE id = @id";
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);

            SqlDataReader? reader = cmd.ExecuteReader();
            if (reader.Read())
            {

                if (!reader.IsDBNull(0))
                {
                    return reader.GetString(0);
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }
    }
    
    public string GetGeologyElementById(int id)
    {
        return GetElementById(id, "/deposit/geology");
    }

    public string GetGeographyElementById(int id)
    {
        return GetElementById(id, "/deposit/geography");
    }

    public string GetElementByName(string name)
    {
        using (var connection = db_connect.GetConnection())
        {
            connection.Open();
            string query = "SELECT Content.query('.') as result FROM dbo.xmltable WHERE Content.exist('/deposit[@name=sql:variable(\"@name\")]') = 1";
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@name", name);

            SqlDataReader? reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                if (!reader.IsDBNull(0))
                {
                    return reader.GetString(0);
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public int GetIdWithName(string name)
    {
        using (var connection = db_connect.GetConnection())
        {
            connection.Open();
            string query = "SELECT id FROM dbo.xmltable WHERE Content.exist('/deposit[@name=sql:variable(\"@name\")]') = 1";
            SqlCommand cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@name", name);

            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                if (!reader.IsDBNull(0))
                {
                    return reader.GetInt32(0);
                }
                else
                {
                    return -1; 
                }
            }
            else
            {
                return -1;
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////////
    // ------------------------- SEKCJA: Dodawanie danych -------------------------
    ////////////////////////////////////////////////////////////////////////////////

    public int AddXmlToDb(string xml)
    {
        // sprawdzamy czy to prawidłowy XML plus przerabiamy go na dokument XML
        XmlDocument xmlDocument = Utils.validateXML(xml);
        // zapytanie wstawiające element do bazy danych
        string query = "INSERT INTO dbo.xmltable (Content) VALUES (@xml_value)";
        SqlCommand cmd = new SqlCommand(query);
        cmd.Parameters.AddWithValue("@xml_value", xmlDocument.OuterXml);

        int rowsAffected = 0;

        using (var connection = db_connect.GetConnection())
        {
            connection.Open();
            cmd.Connection = connection;
            rowsAffected = cmd.ExecuteNonQuery();
        }

        if (rowsAffected > 0)
        {
            return rowsAffected;
        }

        return 0;
    }

    public SortedDictionary<int, string> GetXmlValues(string path)
    {
        Utils.validateXPath(path);

        string query = "SELECT id, C.query('.') as result " +
                        "FROM dbo.xmltable  " +
                        $"CROSS APPLY Content.nodes('{path}') as T(C) ";

        SqlCommand cmd = new SqlCommand(query);
        SqlDataReader? reader;

        SortedDictionary<int, string> data = new SortedDictionary<int, string>();

        using (var connection = db_connect.GetConnection())
        {
            connection.Open();
            cmd.Connection = connection;
            reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    int idValue = reader.GetInt32(0);
                    string? value = reader.GetString(1);
                    if (value != null)
                    {
                        data.Add(idValue, value);
                    }
                }
            }
        }

        return data;
    }

    public int ModifyLatitude(double latitude, int id)
    {
        return ModifyNodeValue("/deposit/geography/latitude", latitude.ToString("F6", CultureInfo.InvariantCulture), id);
    }

    public int ModifyLongitude(double longitude, int id)
    {
        return ModifyNodeValue("/deposit/geography/longitude", longitude.ToString("F6", CultureInfo.InvariantCulture), id);
    }

    public int ModifyRadius(double radius, int id)
    {
        return ModifyNodeValue("/deposit/geography/radius", radius.ToString("F1", CultureInfo.InvariantCulture), id);
    }

    public int ModifyName(string name, int id)
    {
        return ModifyAttrValue("/deposit", "name", name, id);
    }

    public int ModifyEstimatedVolume(double estimatedVolume, int id)
    {
        return ModifyNodeValue("/deposit/geology/estimatedVolume", estimatedVolume.ToString("F1", CultureInfo.InvariantCulture), id);
    }

    public int ModifyGeography(string geographyXml, int id)
    {
        return ModifyNodeXml("/deposit/geography", geographyXml, id);
    }

    public int ModifyGeology(string geologyXml, int id)
    {
        return ModifyNodeXml("/deposit/geology", geologyXml, id);
    }
    public int ModifyNodeXml(string path, string xml, int id)
    {
        Utils.validateXPath(path);
        XmlDocument xmlDocument = Utils.validateXML(xml);

        string query = "UPDATE dbo.xmltable " +
                       $"SET Content.modify('replace value of ({path}/text())[1] with sql:variable(\"@xml_value\")') " +
                       "WHERE id = @id";

        SqlParameter[] parameters =
        {
            new SqlParameter("@xml_value", xmlDocument.OuterXml),
            new SqlParameter("@id", id)
        };

        return Modify(query, parameters);
    }

    public int ModifyNodeValue(string path, string value, int id)
    {
        Utils.validateXPath(path);
        Utils.validateValue(value);

        string query = "UPDATE dbo.xmltable " +
                       $"SET Content.modify('replace value of ({path}/text())[1] with sql:variable(\"@value\")') " +
                       "WHERE id= @id ";

        SqlParameter[] parameters =
        {
            new SqlParameter("@id", id),
            new SqlParameter("@value", value)
        };

        return Modify(query, parameters);

    }

    public int ModifyAttrValue(string path, string attr, string value, int id)
    {
        Utils.validateXPath(path);
        Utils.validateValue(value);
        Utils.validateValue(attr);

        string query = "UPDATE dbo.xmltable " +
                       $"SET Content.modify('replace value of ({path}/@{attr})[1] with sql:variable(\"@value\")') " +
                       "WHERE id = @id ";

        SqlParameter[] parameters =
        {
            new SqlParameter("@id", id),
            new SqlParameter("@value", value)
        };

        return Modify(query, parameters);

    }

    ////////////////////////////////////////////////////////////////////////////////
    // ------------------------- SEKCJA: Usuwanie danych --------------------------
    ////////////////////////////////////////////////////////////////////////////////

    public int DeleteByName(string name)
    {
        return DeleteXmlForAttribute("/deposit", "name", name);
    }

    public int DeleteForLocation(string location)
    {
        return DeleteXmlForNode("/deposit/geography/location", location);
    }

    public int DeleteForQuantityLessThan(double maxValue)
    {
        return DeleteXmlWhereNodeLessThan("/deposit/geology/estimatedVolume", maxValue);
    }

    public int DeleteXmlForNode(string path, string nodeValue)
    {
        string query = "DELETE FROM dbo.xmltable " +
                     $"WHERE Content.exist('{path}[text()=sql:variable(\"@nodeValue\")]') = 1";

        SqlParameter[] parameters =
        {
            new SqlParameter("@nodeValue", nodeValue)
        };

        return Delete(query, parameters);
    }

    public int DeleteXmlWhereNodeLessThan(string path, double maxValue)
    {
        string query = @"
            DELETE FROM dbo.xmltable
            WHERE Content.exist(" +
               $"'{path}[text() < sql:variable(\"@maxValue\")]' = 1 ";

        SqlParameter[] parameters =
        {
            new SqlParameter("@maxValue", maxValue)
        };

        return Delete(query, parameters);
    }


    public int DeleteXmlForAttribute(string path, string attrName, string attrValue)
    {
        string query = "DELETE FROM dbo.xmltable " +
                     $"WHERE Content.exist('{path}[@{attrName}=sql:variable(\"@attrValue\")]') = 1";

        SqlParameter[] parameters =
        {
            new SqlParameter("@attrValue", attrValue)
        };

        return Delete(query, parameters);
    }


    private int Delete(string query, SqlParameter[] parameters)
    {
        int rowsAffected = 0;

        using (var connection = db_connect.GetConnection())
        {
            connection.Open();
            using (var cmd = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                rowsAffected = cmd.ExecuteNonQuery();
            }
        }

        return rowsAffected;
    }

    private int Modify(string query, SqlParameter[] parameters)
    {
        int rowsAffected;
        using (var connection = db_connect.GetConnection())
        {
            connection.Open();
            using (var cmd = new SqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                rowsAffected = cmd.ExecuteNonQuery();
            }
        }
        return rowsAffected;
    }

}