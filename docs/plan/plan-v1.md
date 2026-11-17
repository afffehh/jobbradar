# Jobbsökningsbot – plan för version 1

17 november 2026 · Alfred Eneström

## Mål

Version 1 hämtar varje morgon nya .NET-jobb i Stockholm från Platsbanken och mejlar dig en kort lista med länkar – helt automatiskt, utan server.

**Gör:**

- Söker i Platsbanken via JobTechs öppna API med dina sökord
- Filtrerar bort det som inte passar (t.ex. "senior", "7 år", "Java")
- Kommer ihåg vilka annonser du redan fått, så att bara nya skickas
- Mejlar en sammanfattning: titel, företag, ort, sista ansökningsdag och länk
- Körs varje vardagsmorgon via GitHub Actions

**Gör inte (än):** poängsätter mot ditt CV med AI (version 2), har webbgränssnitt eller databas (version 3), eller söker jobb åt dig.

## Repo och teknikval

Repot heter **`jobbradar`** och byggs som ett konsolprogram i C#/.NET – samma stack som i ditt CV, så projektet blir en bra portfolio-del.

| Val | Vad | Varför |
| --- | --- | --- |
| Reponamn | `jobbradar` (publikt på GitHub) | Kort, lätt att förstå, syns på din profil |
| Språk | C# med .NET 10 (aktuell LTS) | Din huvudstack, visar .NET-kunskap |
| Projekttyp | Konsolapp (`dotnet new console`) | Enklast att köra i GitHub Actions |
| HTTP och JSON | `HttpClient` + `System.Text.Json` | Inbyggt, inga extra paket |
| Mejl | NuGet-paketet `MailKit` via Gmail SMTP | Stabilt och vanligt sätt att skicka mejl från .NET |
| Tester | xUnit | Visar testtänk, kopplar till ditt examensarbete |
| Schemaläggning | GitHub Actions (cron) | Gratis, ingen server, körs även när datorn är av |
| Minne | `data/seen.json` som committas tillbaka | Ingen databas behövs i version 1 |

## Varför vi gör så här

Varje val i version 1 är det enklaste som fungerar på riktigt – och som går att bygga vidare på till version 2 och 3 utan att börja om.

**Arkitektur och upplägg**

- **API i stället för att skrapa sajter.** JobTechs API är öppet och gjort för det här; att skrapa LinkedIn eller Indeed bryter mot deras villkor och går sönder så fort sidan ändras.
- **Konsolapp i stället för webbapp.** Programmet ska bara köra, mejla och avsluta – ingen behöver klicka i det. Ett gränssnitt kommer först i version 3.
- **GitHub Actions i stället för egen server.** Gratis, körs i molnet även när din dator är av, och du slipper drift. Dessutom får du riktig CI/CD-erfarenhet att visa upp.
- **En JSON-fil i stället för databas.** Vi behöver bara komma ihåg vilka id:n som redan skickats. En databas vore onödig komplexitet nu; i version 3 byts filen mot PostgreSQL.
- **Mejl i stället för app-notiser.** Mejl fungerar överallt, kräver ingen app och listan finns kvar att gå tillbaka till.
- **Filtrera i egen kod, inte bara i API-anropet.** Uteslutningsord som "senior" och "7 år" står ofta i annonstexten, inte i titeln – det kan API:et inte filtrera på lika bra. Egen kod går också att enhetstesta.
- **En klass per ansvar** (`JobTechClient`, `JobFilter`, `SeenStore`, `EmailSender`). Varje del går att testa och byta ut separat – i version 2 läggs en `AiScorer` till utan att resten ändras.
- **Hemligheter som miljövariabler, aldrig i koden.** Repot är publikt; lösenord i `appsettings.json` skulle synas för alla.

**Teknikerna och varför**

- **C# och .NET 10** – din huvudstack och det du söker jobb inom. .NET 10 är aktuell LTS-version med lång support.
- **`HttpClient` och `System.Text.Json`** – inbyggt i .NET, så inga extra beroenden för det mest grundläggande.
- **MailKit** – Microsofts egen `SmtpClient` rekommenderas inte längre för nya projekt; MailKit är standardvalet i .NET-världen.
- **Gmail SMTP med applösenord** – gratis och fungerar direkt; applösenordet kan spärras separat om det läcker.
- **xUnit** – det vanligaste testramverket i .NET; tester visar arbetsgivare att du tar kvalitet på allvar.
- **GitHub Actions** – samma verktyg många arbetsgivare använder för CI/CD, så erfarenheten är direkt överförbar.

## Filstruktur

Femton filer räcker för version 1 – en tydlig uppdelning i klient, filter, minne och mejl gör koden lätt att bygga vidare på.

```text
jobbradar/
├─ .github/workflows/daily.yml
├─ src/JobbRadar/
│  ├─ JobbRadar.csproj
│  ├─ Program.cs
│  ├─ appsettings.json
│  ├─ Models/JobAd.cs
│  └─ Services/
│     ├─ JobTechClient.cs
│     ├─ JobFilter.cs
│     ├─ SeenStore.cs
│     └─ EmailSender.cs
├─ tests/JobbRadar.Tests/
│  ├─ JobbRadar.Tests.csproj
│  └─ JobFilterTests.cs
├─ data/seen.json
├─ docs/plan/plan-v1.md
├─ JobbRadar.sln
├─ .gitignore
└─ README.md
```

| Fil | Innehåll |
| --- | --- |
| `Program.cs` | Startpunkt: läser inställningar, hämtar annonser, filtrerar, mejlar, sparar sedda id:n |
| `appsettings.json` | Sökord, region, uteslutningsord, max antal annonser – inga hemligheter här |
| `Models/JobAd.cs` | En `record` med id, titel, arbetsgivare, ort, sista ansökningsdag och länk |
| `Services/JobTechClient.cs` | Anropar JobTech-API:et och gör om JSON till `JobAd` |
| `Services/JobFilter.cs` | Tar bort annonser med uteslutningsord i titel eller text |
| `Services/SeenStore.cs` | Läser och skriver `data/seen.json` så att samma annons inte skickas två gånger |
| `Services/EmailSender.cs` | Bygger ett HTML-mejl och skickar det med MailKit |
| `tests/.../JobFilterTests.cs` | Enhetstester för filtret, t.ex. att "Senior Java Developer" sorteras bort |
| `data/seen.json` | Lista med id:n på redan skickade annonser, börjar som `[]` |
| `.github/workflows/daily.yml` | Kör programmet varje vardagsmorgon och committar `seen.json` |
| `.gitignore` | Standardmallen för .NET (`dotnet new gitignore`) |
| `README.md` | Vad projektet gör, hur man kör det lokalt och vilka secrets som behövs |

## JobTech-API:et

JobSearch är Arbetsförmedlingens öppna API över alla annonser i Platsbanken, med fritextsök och filter på yrke och geografi, och svarar med JSON ([JobTech Dev](https://jobtechdev.se/en/components/jobsearch)).

Detaljerna nedan är skrivna ur minnet och kunde inte testas – stäm av dem mot API:ets Swagger-dokumentation på jobtechdev.se innan du kodar.

| Del | Ungefär så här |
| --- | --- |
| Bas-URL | `https://jobsearch.api.jobtechdev.se` |
| Endpoint | `GET /search` |
| Fritext | `q=.NET utvecklare` (eller `q=C#`, `q=Blazor`) |
| Geografi | `region=<concept-id för Stockholms län>` eller `municipality=<id>` |
| Bara nya | `published-after=<datum/tid>` |
| Antal | `limit` (max cirka 100) och `offset` |
| API-nyckel | Krävdes tidigare som headern `api-key` – kolla om den fortfarande behövs |

Fält du behöver ur varje träff i `hits`: `id`, `headline`, `employer.name`, `workplace_address.municipality`, `application_deadline`, `webpage_url`.

Tips: gör första anropet i webbläsaren eller Postman och spara ett svar som `sample.json` – då kan du bygga `JobAd`-modellen mot riktig data.

## Steg för steg

Bygg i den här ordningen – varje steg går att köra och testa innan nästa, och hela version 1 ryms på en helg (cirka 4–8 timmar).

1. ~~**Skapa repot** på GitHub (`jobbradar`, publikt, med README) och klona det.~~ Klart – repot och README finns.
2. **Sätt upp lösningen:**
   - `dotnet new sln -n JobbRadar`
   - `dotnet new console -o src/JobbRadar`
   - `dotnet new xunit -o tests/JobbRadar.Tests`
   - `dotnet new gitignore`
   - `dotnet sln add src/JobbRadar tests/JobbRadar.Tests` och en projektreferens från testerna till appen
3. **Testa API:et för hand** i webbläsare eller Postman och spara ett svar som `sample.json`.
4. **Skriv `JobAd` och `JobTechClient`** – målet är att `Program.cs` skriver ut 10 annonser i terminalen.
5. **Lägg till `appsettings.json`** med sökord, region och uteslutningsord, och läs in det i `Program.cs`.
6. **Skriv `JobFilter` och dess tester** – kör `dotnet test` tills de är gröna.
7. **Skriv `SeenStore`** – kör programmet två gånger; andra gången ska inga gamla annonser komma.
8. **Skriv `EmailSender`** och skicka ett testmejl till dig själv – skapa först ett eget Gmail-konto för boten, slå på tvåstegsverifiering och skapa ett applösenord, med uppgifterna som miljövariabler lokalt.
9. **Lägg till `daily.yml`** och secrets i GitHub (nästa avsnitt) och starta en körning manuellt.
10. **Uppdatera README** med en skärmdump av mejlet.

## GitHub Actions och secrets

En workflow-fil kör testerna och sedan programmet kl. 07 svensk vintertid varje vardag, och sparar `seen.json` tillbaka i repot.

```yaml
name: Daily job radar

on:
  schedule:
    - cron: "0 6 * * 1-5"   # 06:00 UTC = 07:00 vintertid, 08:00 sommartid
  workflow_dispatch:         # gör att du kan starta den för hand

permissions:
  contents: write            # behövs för att committa seen.json

jobs:
  run:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "10.0.x"
      - run: dotnet test
      - run: dotnet run --project src/JobbRadar
        env:
          SMTP_USER: ${{ secrets.SMTP_USER }}
          SMTP_PASSWORD: ${{ secrets.SMTP_PASSWORD }}
          MAIL_TO: ${{ secrets.MAIL_TO }}
      - name: Spara sedda annonser
        run: |
          git config user.name "jobbradar-bot"
          git config user.email "jobbradar-bot@users.noreply.github.com"
          git add data/seen.json
          git commit -m "Update seen jobs" || echo "Inga nya annonser"
          git push
```

Lägg in de här under **Settings → Secrets and variables → Actions** i repot:

| Secret | Värde |
| --- | --- |
| `SMTP_USER` | Ett eget Gmail-konto skapat bara för boten |
| `SMTP_PASSWORD` | Ett Gmail-applösenord (kräver tvåstegsverifiering), aldrig ditt vanliga lösenord |
| `MAIL_TO` | Din vanliga mejladress, där listan landar varje morgon |
| `JOBTECH_API_KEY` | Bara om API:et visar sig kräva nyckel |

GitHub Actions kör ibland schemalagda jobb några minuter sent – det spelar ingen roll här.

## Klart när

Version 1 är klar när ett mejl med nya .NET-jobb landar i din inkorg en vardagsmorgon utan att du gjort något.

- [x] Repot `jobbradar` finns på GitHub med README
- [ ] Lösning, app och testprojekt är uppsatta
- [ ] Programmet hämtar och skriver ut annonser lokalt
- [ ] Filtret sorterar bort uteslutningsord, och `dotnet test` är grönt
- [ ] Samma annons skickas aldrig två gånger
- [ ] Ett testmejl har kommit fram
- [ ] Secrets är inlagda och en manuell körning i Actions lyckas
- [ ] Den schemalagda körningen har gått av sig själv minst en morgon
- [ ] README har en skärmdump av mejlet

**Nästa steg, version 2:** lägg till en `AiScorer` som skickar varje annons och ditt CV till ett AI-API, får tillbaka en poäng 1–10 med motivering och sorterar mejlet efter den.

## Källor

- [JobSearch – JobTech Dev](https://jobtechdev.se/en/components/jobsearch)
