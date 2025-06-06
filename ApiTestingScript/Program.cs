using System;
using System.Diagnostics;
using DbLibrary;

class Program
{
    static void Main()
    {
        // stworzenie obiektu złoża
        Deposit deposit = Deposit.CreateDeposit("Złoto");
        deposit.setGeology("Złoto", 1000, 50, "Eksploatacja");
        deposit.setGeography("Złota Góra", "Złotoryja", 51.0, 16.0, 10.0);

        // utworzenie połaczenia z bazą danych
        DbConnectionManager db = new DbConnectionManager();
        XmlRepo xmlRepo = new XmlRepo(db);

        int rowsAffected;
        rowsAffected = xmlRepo.DeleteByName("Złoto");
        Console.WriteLine(rowsAffected > 0
            ? $"Usunięto {rowsAffected} elementów o nazwie Złoto."
            : "Nie znaleziono elementów o nazwie Złoto do usunięcia.");

        // stworzenie XML przy użyciu bazy danych
        string? xml = deposit.createXmlUsingDb(db);
        Console.WriteLine("Wygenerowany XML:");
        Console.WriteLine(xml);

        // Oczekiwany XML jako string
        string expectedGeology = "<geology><type>Złoto</type><estimatedVolume>1000.0</estimatedVolume><depth>50.0</depth><status>Eksploatacja</status></geology>";
        string expectedGeography = "<geography><location>Złota Góra</location><region>Złotoryja</region><latitude>51.000000</latitude><longitude>16.000000</longitude><radius>10.0</radius></geography>";
        string expectedXml = "<deposit name=\"Złoto\">" + expectedGeology + expectedGeography + "</deposit>";

        Debug.Assert(xml == expectedXml, "Generated XML does not match expected XML!");
        Console.WriteLine("Xml zgadza się z oczekiwanym formatem.");

        xmlRepo.AddXmlToDb(xml);
        int id = xmlRepo.GetIdWithName("Złoto");

        Debug.Assert(id > 0, "Element o nazwie Złoto nie istnieje w bazie danych!");
        Console.WriteLine($"Element o nazwie Złoto istnieje w bazie danych z ID: {id}");

        string geologyXml = xmlRepo.GetGeologyElementById(id);
        string geographyXml = xmlRepo.GetGeographyElementById(id);
        Debug.Assert(geologyXml != "" && geographyXml != "", "Nie udało się pobrać danych geologicznych lub geograficznych!");
        Console.WriteLine("Dane geologiczne i geograficzne zostały pobrane pomyślnie.");
        Debug.Assert(geologyXml == expectedGeology && geographyXml == expectedGeography, "Pobrane dane geologiczne lub geograficzne nie zgadzają się z oczekiwanymi danymi!");
        Console.WriteLine("Pobrane dane geologiczne i geograficzne zgadzają się z oczekiwanymi danymi.");

        string xml1 = xmlRepo.GetElementById(id);
        string xml2 = xmlRepo.GetElementByName("Złoto");

        Debug.Assert(xml1 == xml2 && xml1 != "", "XML pobrane po ID i po nazwie nie są takie same!");
        Console.WriteLine("XML pobrane po ID i po nazwie są takie same.");

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        var xmlValues = xmlRepo.GetXmlValues(".");
        Debug.Assert(xmlValues[id] == xml, "Pobrane wartości XML nie zgadzają się z oczekiwanym XML!");
        Console.WriteLine("Pobrane wartości XML zgadzają się z oczekiwanym XML.");

        //////////////////////////////////////////////////////////

        rowsAffected = xmlRepo.ModifyLatitude(52.0, id);
        Debug.Assert(rowsAffected > 0, "Nie udało się zmodyfikować szerokości geograficznej!");
        Console.WriteLine($"Zmodyfikowano szerokość geograficzną dla ID {id}. Liczba zmodyfikowanych wierszy: {rowsAffected}");

        Debug.Assert(xmlRepo.GetGeographyElementById(id).Contains("<latitude>52.000000</latitude>"), "Szerokość geograficzna nie została zmodyfikowana poprawnie!");
        Console.WriteLine("Szerokość geograficzna została zmodyfikowana poprawnie.");

        rowsAffected = xmlRepo.ModifyName("Złoto Nowe", id);
        Debug.Assert(rowsAffected > 0, "Nie udało się zmodyfikować nazwy!");
        Console.WriteLine($"Zmodyfikowano nazwę dla ID {id}. Liczba zmodyfikowanych wierszy: {rowsAffected}");
        Debug.Assert(xmlRepo.GetElementById(id).Contains("Złoto Nowe"), "Nazwa nie została zmodyfikowana poprawnie!");
        Console.WriteLine("Nazwa została zmodyfikowana poprawnie.");
        xmlRepo.ModifyName("Złoto", id); // Przywrócenie nazwy do pierwotnej wartości

        ////////////////////////////////////////////////////////////////////////
        var newGeology = "<geology><type>Nowe Złoto</type><estimatedVolume>2000.0</estimatedVolume><depth>60.0</depth><status>Eksploatacja</status></geology>";
        rowsAffected = xmlRepo.ModifyGeology(newGeology, id);
        Debug.Assert(rowsAffected > 0, "Nie udało się zmodyfikować danych geologicznych!");
        Console.WriteLine($"Zmodyfikowano dane geologiczne dla ID {id}. Liczba zmodyfikowanych wierszy: {rowsAffected}");
        Debug.Assert(xmlRepo.GetGeologyElementById(id) == newGeology, "Dane geologiczne nie zostały zmodyfikowane poprawnie!");
        Console.WriteLine("Dane geologiczne zostały zmodyfikowane poprawnie.");
    }
}