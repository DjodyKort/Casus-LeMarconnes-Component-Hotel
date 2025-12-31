// ======== Imports ========
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LeMarconnes.Shared.DTOs;

// ======== Namespace ========
namespace LeMarconnes.Client;

// Console client voor het Hotel Systeem.
class Program
{
    // ==== Properties ====
    private static readonly HttpClient _httpClient = new HttpClient();
    
    // BELANGRIJK: Base URL wijst nu naar de HOTEL controller
    private static readonly string _baseUrl = "https://localhost:7221/api/hotel";
    
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    // ============================================================
    // ==== MAIN ENTRY POINT ====
    // ============================================================

    static async Task Main(string[] args)
    {
        // ==== Start of Function ====
        Console.OutputEncoding = Encoding.UTF8;
        PrintHeader();

        // Check of de API bereikbaar is bij opstarten (optioneel, voor UX)
        Console.WriteLine($"Verbinden met API op {_baseUrl}...");

        while (true)
        {
            try
            {
                PrintMainMenu();
                var choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1": await BekijkEenhedenAsync(); break;
                    case "2": await CheckBeschikbaarheidAsync(); break;
                    case "3": await MaakBoekingAsync(); break;
                    case "4": await BekijkReserveringenAsync(); break;
                    case "5": await BekijkGastenAsync(); break;
                    case "6": await BekijkTarievenAsync(); break;
                    case "7": await BekijkPlatformenAsync(); break;
                    case "8": await AnnuleerReserveringAsync(); break;
                    case "9": await BekijkLogsAsync(); break;
                    case "0":
                        Console.WriteLine("\nTot ziens!");
                        return;
                    default:
                        Console.WriteLine("\nOngeldige keuze. Probeer opnieuw.");
                        break;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nVerbindingsfout: Kan geen verbinding maken met de API.");
                Console.WriteLine($"   Check of de API draait.");
                Console.WriteLine($"   Details: {ex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nOnverwachte Fout: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nDruk op een toets om door te gaan...");
            Console.ReadKey();
            Console.Clear();
            PrintHeader();
        }
    }

    // ============================================================
    // ==== INPUT HELPER METHODS ====
    // ============================================================

    private static DateTime VraagDatum(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (DateTime.TryParse(input, out var datum)) return datum;
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("   Ongeldige datum. (yyyy-MM-dd)");
            Console.ResetColor();
        }
    }

    private static int VraagInteger(string prompt, int? min = null, int? max = null)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out var waarde))
            {
                if (min.HasValue && waarde < min.Value) { Console.WriteLine($"Minimaal {min}"); continue; }
                if (max.HasValue && waarde > max.Value) { Console.WriteLine($"Maximaal {max}"); continue; }
                return waarde;
            }
            Console.WriteLine("   Ongeldig getal.");
        }
    }

    private static string VraagString(string prompt, bool verplicht = true, string? defaultWaarde = null)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input)) return input;
            if (!verplicht) return defaultWaarde ?? "";
            Console.WriteLine("   Verplicht veld.");
        }
    }

    private static string? VraagOptional(string prompt)
    {
        Console.Write(prompt);
        var input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? null : input;
    }

    private static bool VraagBevestiging(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} (j/n): ");
            var input = Console.ReadLine()?.ToLower().Trim();
            if (input == "j" || input == "ja") return true;
            if (input == "n" || input == "nee") return false;
        }
    }

    private static int VraagKeuzeUitLijst(string prompt, HashSet<int> geldigeIds)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out var id) && geldigeIds.Contains(id)) return id;
            Console.WriteLine($"   Ongeldige keuze. Kies uit: {string.Join(", ", geldigeIds.OrderBy(x => x))}");
        }
    }

    // ============================================================
    // ==== UI METHODS ====
    // ============================================================

    private static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Magenta; // Andere kleur voor Hotel
        Console.WriteLine("================================================================");
        Console.WriteLine("           LE MARCONNES - HOTEL BEHEERSYSTEEM                  ");
        Console.WriteLine("                  (Powered by EF Core)                         ");
        Console.WriteLine("================================================================");
        Console.ResetColor();
    }

    private static void PrintMainMenu()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n HOTEL MENU");
        Console.WriteLine("---------------------------------");
        Console.ResetColor();
        Console.WriteLine("  1. Bekijk Kamers");
        Console.WriteLine("  2. Check Beschikbaarheid");
        Console.WriteLine("  3. Boek een Kamer");
        Console.WriteLine("  4. Bekijk Reserveringen");
        Console.WriteLine("  5. Bekijk Gasten");
        Console.WriteLine("  6. Bekijk Tarieven");
        Console.WriteLine("  7. Bekijk Platformen");
        Console.WriteLine("  8. Annuleer Reservering");
        Console.WriteLine("  9. Bekijk Logs");
        Console.WriteLine("  0. Afsluiten");
        Console.WriteLine("---------------------------------");
        Console.Write("Keuze: ");
    }

    private static void PrintSectionHeader(string title)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n=== {title} ===");
        Console.ResetColor();
    }

    // ============================================================
    // ==== API METHODS ====
    // ============================================================

    private static async Task BekijkEenhedenAsync()
    {
        PrintSectionHeader("HOTEL KAMERS");
        
        var response = await _httpClient.GetAsync($"{_baseUrl}/eenheden");
        response.EnsureSuccessStatusCode();
        var units = await response.Content.ReadFromJsonAsync<List<VerhuurEenheidDTO>>(_jsonOptions);

        if (units == null || units.Count == 0) { Console.WriteLine("Geen kamers gevonden."); return; }

        Console.WriteLine($"\n{"ID",-5} {"Naam",-35} {"Type",-10} {"Cap",-5}");
        Console.WriteLine(new string('-', 60));
        
        foreach (var unit in units)
        {
            // TypeID 3 = Hotelkamer
            var typeStr = unit.TypeID == 3 ? "Hotel" : (unit.TypeID == 1 ? "Geh" : "Bed");
            Console.WriteLine($"{unit.EenheidID,-5} {unit.Naam,-35} {typeStr,-10} {unit.MaxCapaciteit,-5}");
        }
    }

    private static async Task CheckBeschikbaarheidAsync()
    {
        PrintSectionHeader("BESCHIKBAARHEID CHECKEN");
        
        var startDatum = VraagDatum("\nStartdatum: ");
        var eindDatum = VraagDatum("Einddatum:  ");

        // Seizoen Validatie (Client-side pre-check, API doet definitieve check)
        if (startDatum.Month < 3 || startDatum.Month > 10)
            Console.WriteLine("Let op: U zoekt buiten het seizoen (1 mrt - 31 okt).");

        var url = $"{_baseUrl}/beschikbaarheid?startDatum={startDatum:yyyy-MM-dd}&eindDatum={eindDatum:yyyy-MM-dd}";
        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode) 
        { 
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Fout: {await response.Content.ReadAsStringAsync()}"); 
            Console.ResetColor();
            return; 
        }

        var units = await response.Content.ReadFromJsonAsync<List<VerhuurEenheidDTO>>(_jsonOptions);
        
        Console.WriteLine($"\nBeschikbaarheid {startDatum:dd-MM} t/m {eindDatum:dd-MM}:\n");
        Console.WriteLine($"{"ID",-5} {"Naam",-35} {"Status",-12}");
        Console.WriteLine(new string('-', 55));

        foreach (var unit in units!)
        {
            var status = unit.IsBeschikbaar ? "VRIJ" : "BEZET";
            Console.ForegroundColor = unit.IsBeschikbaar ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"{unit.EenheidID,-5} {unit.Naam,-35} {status}");
            Console.ResetColor();
        }
    }

    private static async Task MaakBoekingAsync()
    {
        PrintSectionHeader("KAMER BOEKEN");
        
        // 1. Data verzamelen
        Console.WriteLine("\n-- Periode --");
        var startDatum = VraagDatum("Startdatum: ");
        var eindDatum = VraagDatum("Einddatum:  ");

        // 2. Beschikbare kamers ophalen
        var url = $"{_baseUrl}/beschikbaarheid?startDatum={startDatum:yyyy-MM-dd}&eindDatum={eindDatum:yyyy-MM-dd}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) { Console.WriteLine($"Fout: {await response.Content.ReadAsStringAsync()}"); return; }
        
        var units = await response.Content.ReadFromJsonAsync<List<VerhuurEenheidDTO>>(_jsonOptions);
        var beschikbare = units!.Where(u => u.IsBeschikbaar).ToList();

        if (beschikbare.Count == 0) { Console.WriteLine("Geen kamers beschikbaar."); return; }

        Console.WriteLine("\nBeschikbaar:");
        foreach (var u in beschikbare) Console.WriteLine($"  [{u.EenheidID}] {u.Naam} (max {u.MaxCapaciteit}p)");

        // 3. Keuzes maken
        var eenheidId = VraagKeuzeUitLijst("\nKies Kamer ID: ", beschikbare.Select(u => u.EenheidID).ToHashSet());
        var gekozenEenheid = beschikbare.First(u => u.EenheidID == eenheidId);

        // Platform
        var platforms = await (await _httpClient.GetAsync($"{_baseUrl}/platformen")).Content.ReadFromJsonAsync<List<PlatformDTO>>(_jsonOptions);
        foreach (var p in platforms!) Console.WriteLine($"  [{p.PlatformID}] {p.Naam}");
        var platformId = VraagKeuzeUitLijst("Kies Platform ID: ", platforms.Select(p => p.PlatformID).ToHashSet());

        // Gast
        Console.WriteLine("\n-- Gast --");
        var boeking = new BoekingRequestDTO
        {
            StartDatum = startDatum, EindDatum = eindDatum, EenheidID = eenheidId, PlatformID = platformId,
            GastNaam = VraagString("Naam: "),
            GastEmail = VraagString("Email: "),
            GastTel = VraagOptional("Tel: "),
            GastStraat = VraagString("Straat: "),
            GastHuisnr = VraagString("Huisnr: "),
            GastPostcode = VraagString("Postcode: "),
            GastPlaats = VraagString("Plaats: "),
            AantalPersonen = VraagInteger($"Aantal Personen (max {gekozenEenheid.MaxCapaciteit}): ", 1, gekozenEenheid.MaxCapaciteit)
        };

        // 4. Versturen
        if (!VraagBevestiging("Boeking versturen?")) return;

        var postResponse = await _httpClient.PostAsJsonAsync($"{_baseUrl}/boek", boeking);
        var result = await postResponse.Content.ReadFromJsonAsync<BoekingResponseDTO>(_jsonOptions);

        if (result?.Succes == true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nBOEKING GESLAAGD! ID: {result.ReserveringID}");
            Console.WriteLine($"Totaalprijs (Excl tax opgeslagen, incl berekend): € {result.TotaalPrijs:N2}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nMISLUKT: {result?.FoutMelding}");
        }
        Console.ResetColor();
    }

    private static async Task BekijkReserveringenAsync()
    {
        PrintSectionHeader("RESERVERINGEN");
        var res = await (await _httpClient.GetAsync($"{_baseUrl}/reserveringen")).Content.ReadFromJsonAsync<List<ReserveringDTO>>(_jsonOptions);
        
        if (res == null || res.Count == 0) { Console.WriteLine("Geen data."); return; }

        Console.WriteLine($"\n{"ID",-5} {"Gast",-20} {"Kamer",-20} {"Periode",-25} {"Status",-12}");
        Console.WriteLine(new string('-', 90));
        
        foreach (var r in res)
        {
            var periode = $"{r.Startdatum:dd/MM} - {r.Einddatum:dd/MM}";
            // Gebruik Gast/Eenheid objecten als ze via EF Core Include zijn meegekomen, anders ID fallback
            var gastNaam = r.Gast?.Naam ?? r.GastID.ToString();
            var kamerNaam = r.Eenheid?.Naam ?? r.EenheidID.ToString();

            Console.WriteLine($"{r.ReserveringID,-5} {gastNaam,-20} {kamerNaam,-20} {periode,-25} {r.Status}");
        }
    }

    private static async Task BekijkGastenAsync()
    {
        PrintSectionHeader("GASTEN");
        var gasten = await (await _httpClient.GetAsync($"{_baseUrl}/gasten")).Content.ReadFromJsonAsync<List<GastDTO>>(_jsonOptions);
        foreach (var g in gasten!) Console.WriteLine($"{g.GastID,-5} {g.Naam,-20} {g.Email}");
    }

    private static async Task BekijkTarievenAsync()
    {
        PrintSectionHeader("HOTEL TARIEVEN");
        var tarieven = await (await _httpClient.GetAsync($"{_baseUrl}/tarieven")).Content.ReadFromJsonAsync<List<TariefDTO>>(_jsonOptions);
        
        Console.WriteLine($"\n{"ID",-5} {"Cat",-10} {"Platform",-15} {"Prijs",-10} {"TaxStatus",-10}");
        Console.WriteLine(new string('-', 60));

        foreach (var t in tarieven!)
        {
            var cat = t.CategorieID == 1 ? "Logies" : "Tax";
            var plat = t.Platform?.Naam ?? "Alle";
            Console.WriteLine($"{t.TariefID,-5} {cat,-10} {plat,-15} €{t.Prijs,-9:N2} {(t.TaxStatus?"Incl":"Excl")}");
        }
    }

    private static async Task BekijkPlatformenAsync()
    {
        PrintSectionHeader("PLATFORMEN");
        var list = await (await _httpClient.GetAsync($"{_baseUrl}/platformen")).Content.ReadFromJsonAsync<List<PlatformDTO>>(_jsonOptions);
        foreach (var item in list!) Console.WriteLine($"{item.PlatformID,-5} {item.Naam} ({item.CommissiePercentage}%)");
    }

    private static async Task AnnuleerReserveringAsync()
    {
        PrintSectionHeader("ANNULEREN");
        var id = VraagInteger("Reservering ID: ");
        if (!VraagBevestiging("Zeker weten?")) return;

        var response = await _httpClient.PutAsync($"{_baseUrl}/reserveringen/{id}/annuleer", null);
        Console.WriteLine(response.IsSuccessStatusCode ? "Geannuleerd." : "Mislukt.");
    }

    private static async Task BekijkLogsAsync()
    {
        PrintSectionHeader("LOGS");
        var logs = await (await _httpClient.GetAsync($"{_baseUrl}/logs")).Content.ReadFromJsonAsync<List<LogboekDTO>>(_jsonOptions);
        foreach (var l in logs!) Console.WriteLine($"{l.Tijdstip:HH:mm} - {l.Actie} (Rec: {l.RecordID})");
    }
}