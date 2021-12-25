using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace DuplicatesLibrary
{
    interface IDublettenprüfung
    {
        IEnumerable Sammle_Kandidaten(string pfad);
        IEnumerable Sammle_Kandidaten(string pfad, Vergleichsmodi modus);
        IEnumerable Prüfe_Kandidaten(IEnumerable kandidaten);
    }

    interface IDublette
    {
        IEnumerable Dateipfade { get; }
    }

    public enum Vergleichsmodi
    {
        Größe_und_Name,
        Größe
    }

    /// <summary>
    /// Die Klasse Dublette realisiert die Auflistung der Dublikatten als FullName (string)
    /// </summary>
    public class Dublette : IDublette
    {
        public IEnumerable Dateipfade { get; }

        // Der Konstruktor macht die Zuweisung 
        public Dublette(List<string> dublicates)
        {
            Dateipfade = dublicates;
        }
    }

    /// <summary>
    /// Die Klasse SearchDublicates hat drei Methoden am Bord
    /// </summary>
    public class SearchDublicates : IDublettenprüfung 
    {

        /// <summary>
        /// Die Methode Sammle_Kandidaten findet die möglichen Dublikatte
        /// Es gibt 2 Arten oder Modi diese zu sammeln: wenn die Datein gleiche Größe (in Byte) haben
        /// oder gleiche Größe und gleichen Namen.
        /// Zuerst werden alle im Verzeichnisbaum gefundene Dateien in eine Dictionary geschrieben,
        /// Diese Dictionary hat Schlüssel-Wert Paar: 
        /// --- Für Modus Größe: --- 
        /// Schlüssel: Größe der gefundenen Datei in Byte als long, 
        /// Wert: Pfad zu dieser Datei inkl. Name) als Liste aus Strings 
        ///  --- Für Modus Größe und Name: --- 
        /// Schlüssel: Größe der gefundenen Datei in Byte und Name OHNE Pfad als tupel (long, string), 
        /// Wert: Pfad zu dieser Datei inkl. Name) als Liste aus Strings 
        /// Danach wird geprüft ob der Wert der Dictionary (also die Liste) für entspechenden Schlüssel mehr als 1 Eintrag hat
        /// Alle Einträge (mit Count > 1), die unter einem Schlüssel liegen, werden ins Exemplar der Klasse Dublette (die wiederum Interface IDublette implementiert) geschrieben.
        /// So ergibt sich ein Exemplar der Duplette mit Dateipfaden von möglichen Dublikatten einer Datei
        /// Da möglicherweise mehrere Dublikatten von mehreren Datei existieren können, werden die Exemplare (ein Exemplar pro Gruppe der Dublikatten) 
        /// in die Liste geschriben. 
        /// Also Rückgabewert ist praktisch die Liste der Listen, oder ganz grob - zweidimensionales Array.
        /// </summary>
        /// <param name="pfad"></param>
        /// <returns></returns>
        public IEnumerable Sammle_Kandidaten(string pfad)
        {
            return Sammle_Kandidaten(pfad, Vergleichsmodi.Größe_und_Name);
        }

        public IEnumerable Sammle_Kandidaten(string pfad, Vergleichsmodi modus)
        {
            // Dictionary für Modus Name
            Dictionary<long, List<string>> workDict = new();

            // Dictionary für Modus Name und Größe
            Dictionary<(long , string), List<string>> workDict_withNames = new();

            // Output Variable
            List<Dublette> dubl_list = new();

            // Erst alle Datein aus dem Verzeichnisbaum holen
            var f = Directory.EnumerateFiles(pfad, "*", SearchOption.AllDirectories);

            switch (modus)
            {
                case Vergleichsmodi.Größe:
                    {                        
                        foreach (string file in f)
                        {
                            var fi = new FileInfo(file);

                            // Hier wird geprüft ob Dictionary bereits eine Datei mit dieser Größe hat
                            if (workDict.ContainsKey(fi.Length))
                            {
                                // Falls ja - füge zu diesem Schlüssel eine neue Datei in der Liste hinzu
                                workDict[fi.Length].Add(fi.FullName);
                            }
                            else
                            {
                                // Falls nein - erstelle eine neue Paar 
                                workDict.Add(fi.Length, new List<string> { fi.FullName });
                            }
                        }

                        // Jetzt kopiere alle Einträge unter einem Schlüssel, deren Anzahl größer als 1 ist in einer Liste
                        foreach (var v in workDict.Where(el => el.Value.Count > 1))
                        {
                            dubl_list.Add(new Dublette(v.Value));
                        }
                        break;
                    }
                case Vergleichsmodi.Größe_und_Name:
                    {  
                        foreach (string file in f)
                        {
                            var fi = new FileInfo(file);

                            // Hilfsvariable Tuple erstellen
                            (long, string) t = (fi.Length, fi.Name);

                            // Hier wird geprüft ob Dictionary bereits eine Datei mit dieser Größe und Name hat
                            if (workDict_withNames.ContainsKey(t))
                            {
                                // Falls ja - füge zu diesem Schlüssel eine neue Datei in der Liste hinzu
                                workDict_withNames[t].Add(fi.FullName);
                            }
                            else
                            {
                                // Falls nein - erstelle eine neue Paar 
                                workDict_withNames.Add(t, new List<string> { fi.FullName });
                            }
                        }
                        // Jetzt kopiere alle Einträge unter einem Schlüssel, deren Anzahl größer als 1 ist in einer Liste
                        foreach (var v in workDict_withNames.Where(el => el.Value.Count > 1))
                        {
                            dubl_list.Add(new Dublette(v.Value));
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return dubl_list;
        }

        /// <summary>
        /// Die Methode Prüfe_Kandidaten vergleicht die Hashwerte von Kandidaten und falls diese Warte gleich sind - gibt die Datenpfade zurück
        /// Der Rückgabe Wert ist auch praktisch die Liste der Listen
        /// </summary>
        /// <param name="kandidaten"></param>
        /// <returns></returns>
        public IEnumerable Prüfe_Kandidaten(IEnumerable kandidaten)
        {
            // Output Value
            List<Dublette> dubl_list = new();

            // Dictionary zum Sammmeln
            Dictionary<string, List<string>> workDict = new();

            foreach (Dublette dublette in kandidaten)
            {
                foreach (string file in dublette.Dateipfade)
                {
                    // Öffne die Datei als Stream (da Hash Funktion einen Stream-Wert annimmt)
                    // Using - damit nach verlassen dieses Feldes Zugriff wieder frei wird
                    using (var stream = File.OpenRead(file))
                    {
                        System.Text.StringBuilder sb = new();
                                                
                        string string_hash;

                        // Hier werden die Hashwerte berechnet
                        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                        {
                            var hash = md5.ComputeHash(stream);

                            // Alle Hash-bytes zu String umwandeln
                            for (int i = 0; i < hash.Length; i++)
                            {
                                sb.Append(hash[i].ToString("X2"));
                            }
                            string_hash = sb.ToString();
                        }

                        // Hier erfolgt das selbe Überprüfungsmechanismus wie in der Methode Sammle_Kandidaten
                        if (workDict.ContainsKey(string_hash))
                        {
                            workDict[string_hash].Add(file);
                        }
                        else
                        {
                            workDict.Add(string_hash, new List<string> { file });
                        }
                    }
                }
            }

            foreach (var v in workDict.Where(el => el.Value.Count > 1))
            {
                dubl_list.Add(new Dublette(v.Value));
            }
            return dubl_list;        
        }
    }
}
