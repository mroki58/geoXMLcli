using System;
using System.Diagnostics;
using DbLibrary;

class Program
{
    static void Main()
    {
        int rowsAffected;

        Console.WriteLine("═════════════════════ 📌 START PROGRAMU 📌 ═════════════════════");

        // stworzenie obiektu złoża
        Deposit deposit = Deposit.CreateDeposit("Złoto");
        deposit.setGeology("Złoto", 1000, 50, "Eksploatacja");
        deposit.setGeography("Złota Góra", "Złotoryja", 51.0, 16.0, 10.0);

        // utworzenie połączenia z bazą danych
        DbConnectionManager db = new DbConnectionManager();
        XmlRepo xmlRepo = new XmlRepo(db);

        Console.WriteLine("\n🔹 Czyszczenie tabeli xmltable...");
        xmlRepo.ClearXmlTable();

        Console.WriteLine("\n🔹 Generowanie XML przy użyciu procedury SQL...");
        string? xml = deposit.createXmlUsingDb(db);
        Console.WriteLine("📄 Wygenerowany XML:");
        Console.WriteLine(xml);

        string expectedGeology = "<geology><type>Złoto</type><estimatedVolume>1000.0</estimatedVolume><depth>50.0</depth><status>Eksploatacja</status></geology>";
        string expectedGeography = "<geography><location>Złota Góra</location><region>Złotoryja</region><latitude>51.000000</latitude><longitude>16.000000</longitude><radius>10.0</radius></geography>";
        string expectedXml = "<deposit name=\"Złoto\">" + expectedGeology + expectedGeography + "</deposit>";

        Debug.Assert(xml == expectedXml, "❌ XML niezgodny z oczekiwanym!");
        Console.WriteLine("✅ XML zgadza się z oczekiwanym formatem.");

        xmlRepo.AddXmlToDb(xml);

        Console.WriteLine("\n🔹 Próba dodania tego samego XML ponownie...");
        try
        {
            xmlRepo.AddXmlToDb(xml);
            Debug.Assert(false, "❌ Dodanie duplikatu powinno wyrzucić wyjątek!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("✅ Oczekiwany wyjątek przy dodaniu duplikatu: " + ex.Message);
        }

        Console.WriteLine("\n🔹 Sprawdzenie obecności elementu 'Złoto' w bazie...");
        int id = xmlRepo.GetIdWithName("Złoto");
        Debug.Assert(id > 0, "❌ Element 'Złoto' nie istnieje!");
        Console.WriteLine($"✅ Element 'Złoto' ma ID: {id}");

        Console.WriteLine("\n🔹 Pobieranie danych geologicznych i geograficznych...");
        string geologyXml = xmlRepo.GetGeologyElementById(id);
        string geographyXml = xmlRepo.GetGeographyElementById(id);
        Debug.Assert(geologyXml != "" && geographyXml != "", "❌ Nie udało się pobrać danych!");
        Console.WriteLine("✅ Dane geologiczne i geograficzne pobrane pomyślnie.");
        Debug.Assert(geologyXml == expectedGeology && geographyXml == expectedGeography, "❌ Dane niezgodne z oczekiwaniami!");
        Console.WriteLine("✅ Dane zgodne z oczekiwaniami.");

        Console.WriteLine("\n🔹 Porównanie XML pobranego po ID i po nazwie...");
        string xml1 = xmlRepo.GetElementById(id);
        string xml2 = xmlRepo.GetElementByName("Złoto");
        Debug.Assert(xml1 == xml2 && xml1 != "", "❌ XML-e się różnią!");
        Console.WriteLine("✅ XML pobrane po ID i po nazwie są zgodne.");

        Console.WriteLine("\n🔹 Pobieranie mapy ID → XML...");
        var xmlValues = xmlRepo.GetXmlValues(".");
        Debug.Assert(xmlValues[id] == xml, "❌ Mapa ID → XML nie zgadza się!");
        Console.WriteLine("✅ Mapa XML zgodna z oczekiwanym XML.");

        Console.WriteLine("\n🔹 Modyfikacja szerokości geograficznej...");
        rowsAffected = xmlRepo.ModifyLatitude(52.0, id);
        Debug.Assert(rowsAffected > 0, "❌ Nie udało się zmodyfikować szerokości geograficznej!");
        Console.WriteLine($"✅ Zmieniono szerokość geograficzną (zmienione wiersze: {rowsAffected})");
        Debug.Assert(xmlRepo.GetGeographyElementById(id).Contains("<latitude>52.000000</latitude>"), "❌ Szerokość nie została zmodyfikowana!");
        Console.WriteLine("✅ Szerokość geograficzna zmodyfikowana poprawnie.");

        Console.WriteLine("\n🔹 Modyfikacja nazwy złoża...");
        rowsAffected = xmlRepo.ModifyName("Złoto Nowe", id);
        Debug.Assert(rowsAffected > 0, "❌ Nie udało się zmodyfikować nazwy!");
        Console.WriteLine($"✅ Nazwa zmodyfikowana (zmienione wiersze: {rowsAffected})");
        Debug.Assert(xmlRepo.GetElementById(id).Contains("Złoto Nowe"), "❌ Nazwa nie została zmieniona!");
        Console.WriteLine("✅ Nazwa zmodyfikowana poprawnie.");
        xmlRepo.ModifyName("Złoto", id);

        Console.WriteLine("\n🔹 Dodawanie i usuwanie obiektu 'Srebro'...");
        Deposit deposit2 = Deposit.CreateDeposit("Srebro");
        deposit2.setGeology("Srebro", 500, 30, "Eksploatacja");
        deposit2.setGeography("Srebrna Góra", "Srebrna", 50.0, 15.0, 5.0);

        xml = deposit2.createXmlUsingDb(db);
        if (xml != null)
            xmlRepo.AddXmlToDb(xml);

        int id2 = xmlRepo.GetIdWithName("Srebro");
        rowsAffected = xmlRepo.DeleteByName("Srebro");
        Debug.Assert(xmlRepo.GetElementById(id2) == "" && rowsAffected == 1, "❌ Nie udało się usunąć 'Srebro'!");
        Console.WriteLine("✅ Element 'Srebro' usunięty pomyślnie.");

        Console.WriteLine("\n🔹 Test zbiorczego usuwania (lokacja = 'Miedziana Góra')...");
        Deposit deposit3 = Deposit.CreateDeposit("Miedź");
        deposit3.setGeology("Miedź", 2000, 70, "Eksploatacja");
        deposit3.setGeography("Miedziana Góra", "Miedziana", 52.0, 18.0, 15.0);
        xml = deposit3.createXmlUsingDb(db);
        if (xml != null)
            xmlRepo.AddXmlToDb(xml);

        Deposit deposit4 = Deposit.CreateDeposit("Miedź2");
        deposit4.setGeology("Miedź", 2000, 70, "Eksploatacja");
        deposit4.setGeography("Miedziana Góra", "Miedziana", 52.0, 18.0, 15.0);
        xml = deposit4.createXmlUsingDb(db);
        if (xml != null)
            xmlRepo.AddXmlToDb(xml);

        rowsAffected = xmlRepo.DeleteForLocation("Miedziana Góra");
        Debug.Assert(rowsAffected == 2, "❌ Nie usunięto 2 elementów!");
        Console.WriteLine($"✅ Usunięto {rowsAffected} elementy z lokalizacji 'Miedziana Góra'.");

        Console.WriteLine("\n🔹 Test warunku ilości (quantity)...");
        rowsAffected = xmlRepo.DeleteForQuantityLessThan(1000);
        Debug.Assert(rowsAffected == 0, "❌ Usunięto coś, czego nie powinno się usuwać!");
        Console.WriteLine("✅ Nie usunięto żadnych elementów, jak oczekiwano.");

        rowsAffected = xmlRepo.DeleteForQuantityLessThan(1001);
        Debug.Assert(rowsAffected == 1, "❌ Nie udało się usunąć elementu z ilością < 1001!");
        Console.WriteLine($"✅ Usunięto {rowsAffected} element z ilością < 1001.");

    }
}
