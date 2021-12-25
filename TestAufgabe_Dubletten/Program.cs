using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DuplicatesLibrary; 



namespace TestAufgabe_Schleuppen
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Geben Sie bitte den Pfad ein, wo die Duplikate gesucht werden sollen: ");
            string dir = Console.ReadLine().ToString();

            Console.WriteLine("Welcher Parameter soll ubereinstimmen? (0 - Größe und Name, 1 - nur Größe)");
            string mode = Console.ReadLine().ToString();

            while (!mode.Equals("0") & !mode.Equals("1"))
            {
                Console.WriteLine("Die Eingabe 0 oder 1 ist möglich, bitte wiederholen:");                
                mode = Console.ReadLine().ToString();
            }

            SearchDublicates dr = new();
         
            try
            {
                Vergleichsmodi emode = (Vergleichsmodi)Enum.Parse(typeof(Vergleichsmodi), mode);

                // Zuerst die möglichen Dublikatten finden
                IEnumerable list_prim = dr.Sammle_Kandidaten(dir, emode);

                // Danach die Kandidaten mittels Hashwertberehnung prüfen
                IEnumerable list_final = dr.Prüfe_Kandidaten(list_prim);

                int i = 0;

                // Die einzelnen Dublikatten gruppenweise anzeigen
                foreach (Dublette v in list_final)
                {
                    Console.WriteLine($"Group {++i}:");

                    Console.WriteLine(string.Join('\n', v.Dateipfade as List<string>));

                }
                
                if (i == 0)
                    Console.WriteLine("Keine Dubletten gefunden");

                Console.WriteLine("Das war die TestAufgabe für Schleupen");
            }
            // Falls der eingegebene Pfad nicht existiert, direkt Fehlermeldung anzeigen
            catch (System.IO.DirectoryNotFoundException ex1)
            {
                Console.WriteLine($"Das Verzeichnis existiert nicht: {ex1.Message}");
            }
            catch (ArgumentException ex2)
            {
                Console.WriteLine($"Die Eingabe für Modus wurde nicht erkannt, nur die Eingaben 0 der 1 sind zugelassen: {ex2.Message}");
            }
        }
    }
}
