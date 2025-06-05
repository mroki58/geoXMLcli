using System.Xml;
using Microsoft.Data.SqlClient;

namespace DbLibrary;

interface IXmlRepo
{
    public int AddXmlToDb(string xml);
    public int DeleteXmlForNode(string path, string nodeValue);
    public int DeleteXmlForAttribute(string path, string attrName, string attrValue);
    public int DeleteXmlWhereNodeLessThan(string path, double maxValue);
    public int ModifyNodeValue(string path, string value, int id); 
    public int ModifyAttrValue(string path, string attr, string value, int id);
}

class XmlRepo : IXmlRepo
{
    public DbConnectionManager db_connect;
    public XmlRepo(DbConnectionManager dbmanager)
    {
        db_connect = dbmanager;
    }


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
    //////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="nodeValue"></param>
    /// <returns></returns>
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
    ///////////////////////////////////////////////////////////////////////////////////////////////////////

    public int ModifyNodeValue(string path, string value, int id)
    {
        Utils.validateXPath(path);
        Utils.validateValue(value);

        string query = "UPDATE dbo.xmltable " +
                       $"SET Content.modify('replace value of ({path}/text())[1] with \"{value}\"') " +
                       "WHERE id= @id ";

        return Modify(query, id);

    }

    public int ModifyAttrValue(string path, string attr, string value, int id)
    {
        Utils.validateXPath(path);
        Utils.validateValue(value);
        Utils.validateValue(attr);

        string query = "UPDATE dbo.xmltable " +
                       $"SET Content.modify('replace value of ({path}/@{attr})[1] with \"{value}\"') " +
                       "WHERE id = @id ";
        return Modify(query, id);

    }

    private int Modify(string query, int id)
    {
        SqlCommand cmd = new SqlCommand(query);
        cmd.Parameters.AddWithValue("@id", id);
        int rowsAffected;

        using (var connection = db_connect.GetConnection())
        {
            connection.Open();
            cmd.Connection = connection;
            rowsAffected = cmd.ExecuteNonQuery();
        }
        return rowsAffected;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    // może być użyte do każdego typu XML-owego zapytania, które możnaby wykonać
    public SortedDictionary<int, string> GetXmlValuesWithId(string path)
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



}