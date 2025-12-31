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

    // BELANGRIJK: Base URL wijst naar de HOTEL controller op poort 7221 (HTTPS)
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

        // Check verbinding bij start (UX)
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
                Console.WriteLine($"   Check of de API draait (dotnet run in LeMarconnes.API).");
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
    // ==== INPUT HELPER METHODS (VERBETERD) ====
    // ============================================================

    /// <summary>
    /// Vraagt datum met formaat-hint en optionele default waarde bij Enter.
    /// </summary>
    private static DateTime VraagDatum(string prompt, DateTime? defaultDatum = null)
    {
        while (true)
        {
            // Bouw de prompt string (bijv: "Startdatum [2025-07-01]: ")
            string defaultStr = defaultDatum.HasValue ? $" [{defaultDatum.Value:yyyy-MM-dd}]" : " (yyyy-MM-dd)";
            Console.Write($"{prompt}{defaultStr}: ");

            var input = Console.ReadLine()?.Trim();

            // Als leeg en er is een default -> gebruik default
            if (string.IsNullOrWhiteSpace(input) && defaultDatum.HasValue)
            {
                return defaultDatum.Value;
            }

            // Probeer te parsen
            if (DateTime.TryParse(input, out var datum))
            {
                // Extra check: Hotel Seizoen (maart-okt) - Waarschuwing geven
                if (datum.Month < 3 || datum.Month > 10)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("   Let op: Deze datum valt buiten het hotelseizoen (1 mrt - 31 okt).");
                    Console.ResetColor();
                }
                return datum;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("   Ongeldige datum. Gebruik formaat: jjjj-mm-dd (bijv. 2025-07-20)");
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
                if (min.HasValue && waarde < min.Value) { Console.WriteLine($"   Waarde moet minimaal {min} zijn."); continue; }
                if (max.HasValue && waarde > max.Value) { Console.WriteLine($"   Waarde mag maximaal {max} zijn."); continue; }
                return waarde;
            }
            Console.WriteLine("   Ongeldig getal. Probeer opnieuw.");
        }
    }

    private static string VraagString(string prompt, bool verplicht = true, string? defaultWaarde = null)
    {
        while (true)
        {
            string defaultStr = !string.IsNullOrEmpty(defaultWaarde) ? $" [{defaultWaarde}]" : "";
            Console.Write($"{prompt}{defaultStr}: ");

            var input = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(input)) return input;
            if (!string.IsNullOrWhiteSpace(defaultWaarde)) return defaultWaarde;
            if (!verplicht) return "";

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("   Dit veld is verplicht.");
            Console.ResetColor();
        }
    }

    private static string? VraagOptional(string prompt)
    {
        Console.Write($"{prompt} (optioneel): ");
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
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"   Ongeldige keuze. Kies uit ID: {string.Join(", ", geldigeIds.OrderBy(x => x))}");
            Console.ResetColor();
        }
    }

    // ============================================================
    // ==== UI METHODS ====
    // ============================================================

    private static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Magenta; // Hotel Stijl
        Console.WriteLine("================================================================");
        Console.WriteLine("           LE MARCONNES - HOTEL BEHEERSYSTEEM                  ");
        Console.WriteLine("             (EF Core | Thin Client)                           ");
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
            var typeStr = unit.TypeID == 3 ? "Hotel" : "Anders";
            Console.WriteLine($"{unit.EenheidID,-5} {unit.Naam,-35} {typeStr,-10} {unit.MaxCapaciteit,-5}");
        }
    }

    private static async Task CheckBeschikbaarheidAsync()
    {
        PrintSectionHeader("BESCHIKBAARHEID CHECKEN");

        // Default: Morgen t/m overmorgen
        var morgen = DateTime.Today.AddDays(1);

        var startDatum = VraagDatum("Startdatum", morgen);
        var eindDatum = VraagDatum("Einddatum ", startDatum.AddDays(1));

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

        // 1. Data verzamelen (Met slimme defaults)
        Console.WriteLine("\n-- Periode --");
        // We stellen standaard een weekend in juli voor als voorbeeld
        var defaultStart = new DateTime(DateTime.Today.Year, 7, 1);
        if (defaultStart < DateTime.Today) defaultStart = defaultStart.AddYears(1); // Volgend jaar als juli al voorbij is

        var startDatum = VraagDatum("Startdatum", defaultStart);
        var eindDatum = VraagDatum("Einddatum ", startDatum.AddDays(7)); // Default 1 week

        // 2. Beschikbare kamers ophalen
        var url = $"{_baseUrl}/beschikbaarheid?startDatum={startDatum:yyyy-MM-dd}&eindDatum={eindDatum:yyyy-MM-dd}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) { Console.WriteLine($"Fout: {await response.Content.ReadAsStringAsync()}"); return; }

        var units = await response.Content.ReadFromJsonAsync<List<VerhuurEenheidDTO>>(_jsonOptions);
        var beschikbare = units!.Where(u => u.IsBeschikbaar).ToList();

        if (beschikbare.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Geen kamers beschikbaar in deze periode.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine("\nBeschikbare Kamers:");
        foreach (var u in beschikbare) Console.WriteLine($"  [{u.EenheidID}] {u.Naam} (max {u.MaxCapaciteit}p)");

        // 3. Keuzes maken
        var eenheidId = VraagKeuzeUitLijst("\nKies Kamer ID: ", beschikbare.Select(u => u.EenheidID).ToHashSet());
        var gekozenEenheid = beschikbare.First(u => u.EenheidID == eenheidId);

        // Platform
        var platforms = await (await _httpClient.GetAsync($"{_baseUrl}/platformen")).Content.ReadFromJsonAsync<List<PlatformDTO>>(_jsonOptions);
        foreach (var p in platforms!) Console.WriteLine($"  [{p.PlatformID}] {p.Naam}");
        var platformId = VraagKeuzeUitLijst("Kies Platform ID: ", platforms.Select(p => p.PlatformID).ToHashSet());

        // Gast
        Console.WriteLine("\n-- Gast Informatie --");
        var boeking = new BoekingRequestDTO
        {
            StartDatum = startDatum,
            EindDatum = eindDatum,
            EenheidID = eenheidId,
            PlatformID = platformId,
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
        Console.WriteLine($"\nSamenvatting: {gekozenEenheid.Naam}, {boeking.AantalPersonen} pers, {(eindDatum - startDatum).Days} nachten.");
        if (!VraagBevestiging("Boeking definitief maken?")) return;

        var postResponse = await _httpClient.PostAsJsonAsync($"{_baseUrl}/boek", boeking);
        var result = await postResponse.Content.ReadFromJsonAsync<BoekingResponseDTO>(_jsonOptions);

        if (result?.Succes == true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✅ BOEKING GESLAAGD! ID: {result.ReserveringID}");
            Console.WriteLine($"   Totaalprijs: € {result.TotaalPrijs:N2} (Incl. Toeristenbelasting)");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ MISLUKT: {result?.FoutMelding}");
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
            var gastNaam = r.Gast?.Naam ?? r.GastID.ToString();
            var kamerNaam = r.Eenheid?.Naam ?? r.EenheidID.ToString();

            // Kleur op status
            if (r.Status == "Geannuleerd") Console.ForegroundColor = ConsoleColor.DarkGray;
            else if (r.Status == "Ingecheckt") Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine($"{r.ReserveringID,-5} {gastNaam,-20} {kamerNaam,-20} {periode,-25} {r.Status}");
            Console.ResetColor();
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
        PrintSectionHeader("HOTEL TARIEVEN (2025)");
        var tarieven = await (await _httpClient.GetAsync($"{_baseUrl}/tarieven")).Content.ReadFromJsonAsync<List<TariefDTO>>(_jsonOptions);

        Console.WriteLine($"\n{"ID",-5} {"Cat",-10} {"Platform",-15} {"Prijs",-10} {"TaxStatus",-10}");
        Console.WriteLine(new string('-', 60));

        foreach (var t in tarieven!)
        {
            var cat = t.CategorieID == 1 ? "Logies" : "Tax";
            var plat = t.Platform?.Naam ?? "Standaard";
            Console.WriteLine($"{t.TariefID,-5} {cat,-10} {plat,-15} €{t.Prijs,-9:N2} {(t.TaxStatus ? "Incl" : "Excl")}");
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
        Console.WriteLine(response.IsSuccessStatusCode ? "Geannuleerd." : "Mislukt (Bestond hij wel?).");
    }

    private static async Task BekijkLogsAsync()
    {
        PrintSectionHeader("LOGS");
        var logs = await (await _httpClient.GetAsync($"{_baseUrl}/logs")).Content.ReadFromJsonAsync<List<LogboekDTO>>(_jsonOptions);
        foreach (var l in logs!) Console.WriteLine($"{l.Tijdstip:HH:mm} - {l.Actie} (Rec: {l.RecordID})");
    }
}