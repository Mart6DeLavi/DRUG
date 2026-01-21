# DRUG - Instrukcja Użytkownika

---

## O grze

**DRUG** to dynamiczna gra platformowa 2D, w której Twoim zadaniem jest przetrwać jak najdłużej, unikając przeciwników i przeszkód. Gra stopniowo przyspiesza, zwiększając wyzwanie. Zbieraj punkty, odblokuj nowe skórki dla swojej postaci i ustanów nowy rekord!

### Główne cechy gry:
- **Dynamiczna rozgrywka** - gra stopniowo przyspiesza wraz z czasem
- **System skoków** - klasyczny skok oraz specjalny podwójny skok
- **Power-upy** - zbieraj bonusy zwiększające prędkość, punkty i możliwości
- **Debuffy** - uważaj na złe efekty, które utrudnią Ci rozgrywkę
- **System walut** - zbieraj monety podczas gry
- **Sklep ze skórkami** - personalizuj swoją postać
- **Tryb offline** - graj bez połączenia z internetem

---

## Wymagania systemowe

### Minimalne wymagania:

#### Windows
- **System operacyjny:** Windows 10 (64-bit) lub nowszy
- **Procesor:** Intel Core i3 / AMD Ryzen 3 lub równoważny
- **Pamięć RAM:** 4 GB
- **Karta graficzna:** Zintegrowana karta graficzna Intel HD Graphics 4000 lub nowsza
- **Miejsce na dysku:** 500 MB wolnego miejsca
- **DirectX:** Wersja 11

#### macOS
- **System operacyjny:** macOS 10.13 (High Sierra) lub nowszy
- **Procesor:** Intel Core i3 / Apple M1 lub nowszy
- **Pamięć RAM:** 4 GB
- **Karta graficzna:** Zintegrowana karta graficzna Intel HD Graphics 4000 / Apple GPU
- **Miejsce na dysku:** 500 MB wolnego miejsca
- **Metal:** Wymagane wsparcie dla Metal API

### Zalecane wymagania:

#### Windows
- **System operacyjny:** Windows 11 (64-bit)
- **Procesor:** Intel Core i5 / AMD Ryzen 5 lub lepszy
- **Pamięć RAM:** 8 GB
- **Karta graficzna:** NVIDIA GeForce GTX 750 / AMD Radeon R7 260X lub lepsza (opcjonalnie)
- **Miejsce na dysku:** 1 GB wolnego miejsca

#### macOS
- **System operacyjny:** macOS 12 (Monterey) lub nowszy
- **Procesor:** Apple M1/M2 lub Intel Core i5 lub lepszy
- **Pamięć RAM:** 8 GB
- **Karta graficzna:** Apple GPU / Intel Iris Graphics lub lepsza
- **Miejsce na dysku:** 1 GB wolnego miejsca

### Dodatkowe informacje:
- Gra **nie wymaga dedykowanej karty graficznej** - wystarczy zintegrowana karta graficzna
- Zalecane jest uruchomienie gry przy **60 FPS** dla płynnego doświadczenia
- Gra działa w **trybie offline** - nie wymaga połączenia z internetem

---

## Instalacja i uruchomienie

### Windows 10/11

#### Sposób 1: Uruchomienie z pliku wykonywalnego (.exe)

1. **Pobierz grę**
   - Pobierz folder z grą zawierający plik `DRUG.exe`
   - Rozpakuj wszystkie pliki do wybranego folderu na dysku

2. **Uprawnienia**
   - Jeśli Windows Defender lub inny antywirus wyświetli ostrzeżenie, wybierz "Więcej informacji" → "Uruchom mimo to"
   - Gra nie zawiera złośliwego oprogramowania - to standardowe ostrzeżenie dla nieweryfikowanych aplikacji

3. **Uruchomienie**
   - Kliknij dwukrotnie na plik `DRUG.exe`
   - Gra uruchomi się w oknie lub pełnym ekranie (w zależności od ustawień)

4. **Ustawienia ekranu**
   - Aby przełączyć się między trybem okienkowym a pełnoekranowym, użyj kombinacji klawiszy `Alt + Enter`

#### Sposób 2: Uruchomienie z Unity (dla wersji deweloperskiej)

1. Otwórz **Unity Hub**
2. Kliknij "Add" i wskaż folder projektu DRUG
3. Upewnij się, że używasz **Unity 2022.3 LTS** lub nowszego
4. Otwórz projekt
5. W Unity, przejdź do **File → Build Settings**
6. Wybierz **Windows** jako platformę
7. Kliknij **Build And Run**

### macOS (High Sierra lub nowszy)

#### Sposób 1: Uruchomienie z aplikacji (.app)

1. **Pobierz grę**
   - Pobierz folder z grą zawierający plik `DRUG.app`
   - Przenieś folder do wybranej lokalizacji (np. Applications)

2. **Uprawnienia (ważne!)**
   - Przy pierwszym uruchomieniu macOS może zablokować aplikację
   - Przejdź do **Preferencje Systemowe → Bezpieczeństwo i prywatność**
   - Kliknij **"Otwórz mimo to"** obok informacji o zablokowanej aplikacji
   - Alternatywnie: kliknij prawym przyciskiem na `DRUG.app` → **Otwórz** → **Otwórz**

3. **Uruchomienie**
   - Kliknij dwukrotnie na plik `DRUG.app`
   - Gra uruchomi się w oknie lub pełnym ekranie

4. **Ustawienia ekranu**
   - Aby przełączyć się między trybem okienkowym a pełnoekranowym, użyj kombinacji klawiszy `Cmd + F`

#### Sposób 2: Uruchomienie z Unity (dla wersji deweloperskiej)

1. Otwórz **Unity Hub**
2. Kliknij "Add" i wskaż folder projektu DRUG
3. Upewnij się, że używasz **Unity 2022.3 LTS** lub nowszego
4. Otwórz projekt
5. W Unity, przejdź do **File → Build Settings**
6. Wybierz **macOS** jako platformę
7. Kliknij **Build And Run**

### Pierwsze uruchomienie

Po pierwszym uruchomieniu gry:
1. Gra automatycznie utworzy plik zapisu danych
2. Otrzymasz **5000 monet** startowych
3. Twoja postać będzie miała domyślny wygląd
4. Możesz od razu rozpocząć rozgrywkę lub odwiedzić sklep

---

## Sterowanie

Gra wykorzystuje proste i intuicyjne sterowanie klawiaturą. Wszystkie akcje wykonujesz za pomocą zaledwie 5 klawiszy!

### 🎮 Kompletna lista klawiszy sterowania

| Klawisz | Akcja | Szczegóły |
|---------|-------|-----------|
| **W** lub **Spacja** | Skok / Podwójny skok | Główny klawisz do skakania. Naciśnij raz aby skoczyć, przytrzymaj aby naładować wyższy skok |
| **A** | Ruch w lewo | Przesuwa postać w lewo, aby uniknąć przeszkód |
| **D** | Ruch w prawo | Przesuwa postać w prawo, aby uniknąć przeszkód |
| **S** | Szybkie opadanie | Przyspieszenie opadania podczas skoku - użyj, gdy chcesz szybciej wrócić na ziemię |
| **ESC** | Pauza / Menu | Zatrzymuje grę i wyświetla menu pauzy |
| **Enter** | Potwierdzenie | Używany w menu do potwierdzania wyborów |

---

### 🏃 Jak poruszać się postacią

#### Automatyczny ruch do przodu
- **Twoja postać automatycznie biega w prawo** - nie musisz nic robić, aby się poruszała
- **Gra stopniowo przyspiesza** - postać będzie biec coraz szybciej
- **Twoje zadanie:** używaj klawiszy A i D, aby unikać przeszkód poruszając się w lewo i prawo

#### Ruch boczny (lewo/prawo)

**Klawisz A - Ruch w lewo:**
```
Użyj gdy:
✓ Przeszkoda pojawia się po prawej stronie
✓ Przeciwnik atakuje z prawej
✓ Musisz zebrać monetę po lewej stronie
```

**Klawisz D - Ruch w prawo:**
```
Użyj gdy:
✓ Przeszkoda pojawia się po lewej stronie
✓ Przeciwnik atakuje z lewej
✓ Musisz zebrać power-up po prawej stronie
```

**⚠️ UWAGA:** Podczas aktywnego debuffu "Odwrócone sterowanie":
- Klawisz **A** będzie poruszał postać **w prawo**
- Klawisz **D** będzie poruszał postać **w lewo**

---

### 🦘 System skoków - Szczegółowa instrukcja

Gra posiada zaawansowany system **ładowanego skoku**. Wysokość skoku zależy od czasu trzymania klawisza!

#### Podstawowy skok

**1. Mały skok (krótkie naciśnięcie):**
```
Naciśnij: Spacja (szybko puść)
Wysokość: Niska (~6 jednostek)
Użycie: Małe przeszkody, szybkie lądowanie
```

**2. Średni skok (przytrzymanie ~0.5 sekundy):**
```
Naciśnij: Spacja (przytrzymaj 0.5s)
Wysokość: Średnia (~9 jednostek)
Użycie: Normalne przeszkody, standardowe platformy
```

**3. Maksymalny skok (przytrzymanie 1.2+ sekundy):**
```
Naciśnij: Spacja (przytrzymaj 1.2s+)
Wysokość: Maksymalna (~12 jednostek)
Użycie: Wysokie przeszkody, długie przeloty
```

#### Ładowanie skoku - Jak to działa:

1. **Naciśnij i PRZYTRZYMAJ** klawisz **Spacja** lub **W**
2. **Postać zacznie ładować skok** (możesz to zauważyć po animacji)
3. **Im dłużej trzymasz**, tym wyższy będzie skok
4. **Maksymalny czas ładowania:** 1.2 sekundy
5. **Puść klawisz** aby wykonać skok

```
Schemat ładowania:
[Naciśnij Spację] → [Trzymaj 0-1.2s] → [Puść] → [Skok!]
     ↓                    ↓                ↓         ↓
  Start            Ładowanie         Uwolnienie   Wyskok
```

#### Podwójny skok (Power-up)

Gdy zbierzesz power-up **"Podwójny skok"** (🦘):

**Jak używać:**
1. Wykonaj pierwszy skok (normalnie, Spacja)
2. **Gdy jesteś w powietrzu**, naciśnij Spację **ponownie**
3. Postać wykona drugi skok w powietrzu!

**Cechy podwójnego skoku:**
- ✅ Ładuje się **2.5x szybciej** niż normalny skok
- ✅ Możesz kontrolować kierunek (A/D) podczas drugiego skoku
- ✅ Idealny do unikania niespodziewanych przeszkód
- ⏱️ Dostępny przez **5 sekund** po zebraniu power-upu

**Przykład użycia:**
```
Sytuacja: Przeskoczyłeś pierwszą przeszkodę, ale w powietrzu 
          zauważyłeś drugą przeszkodę przed sobą
          
Rozwiązanie: 
1. Jesteś w powietrzu po pierwszym skoku
2. Naciśnij Spację ponownie (drugi skok)
3. Przeskocz drugą przeszkodę!
```

---

### ⬇️ Szybkie opadanie (Klawisz S)

**Funkcja:** Przyspiesza opadanie podczas skoku

**Kiedy używać:**
- ✅ Zrobiłeś zbyt wysoki skok i musisz szybko wylądować
- ✅ Przeszkoda jest nisko, a Ty jesteś wysoko w powietrzu
- ✅ Chcesz precyzyjnie wylądować na platformie
- ✅ Musisz szybko powrócić na ziemię, aby ponownie skoczyć

**Jak używać:**
```
1. Wykonaj skok (Spacja)
2. Gdy jesteś w powietrzu, naciśnij i przytrzymaj S
3. Postać szybciej opadnie na ziemię
```

**⚠️ UWAGA:** Nie możesz używać S na ziemi - działa tylko w powietrzu!

---

### 🎯 Zaawansowane techniki sterowania

#### Technika 1: Skok z ruchem bocznym
```
Cel: Przeskoczyć przeszkodę i jednocześnie przesunąć się w bok

Wykonanie:
1. Naciśnij Spacja (skok)
2. W powietrzu przytrzymaj A lub D (ruch w bok)
3. Lądowanie z boku przeszkody
```

#### Technika 2: Precyzyjne lądowanie
```
Cel: Wylądować dokładnie w wybranym miejscu

Wykonanie:
1. Załaduj skok (przytrzymaj Spację)
2. Puść w odpowiednim momencie
3. W powietrzu użyj S (szybkie opadanie)
4. Skoryguj pozycję za pomocą A/D
```

#### Technika 3: Szybkie omijanie
```
Cel: Szybko ominąć przeszkodę bez skoku

Wykonanie:
1. Zaobserwuj przeszkodę z wyprzedzeniem
2. Naciśnij A lub D w odpowiednim momencie
3. Przesuń postać w bok przeszkody
4. Wróć na środek drogi
```

#### Technika 4: Podwójny skok ratunkowy
```
Cel: Uratować się przed niespodziewaną przeszkodą

Wymaga: Power-up "Podwójny skok"

Wykonanie:
1. Pierwszy skok (Spacja)
2. Zauważyłeś przeszkodę w powietrzu
3. Szybko naciśnij Spację ponownie (drugi skok)
4. Użyj A/D aby skorygować kierunek
```

---

### ⏸️ Menu i pauza

#### Klawisz ESC

**W czasie gry:**
- Naciśnij **ESC** aby zatrzymać grę (pauza)
- Wyświetli się menu z opcjami:
  - **Wznów grę** - kontynuuj rozgrywkę
  - **Ustawienia** - zmień ustawienia (jeśli dostępne)
  - **Menu główne** - wróć do menu głównego
  - **Wyjście** - zamknij grę

**W menu:**
- Naciśnij **ESC** aby cofnąć się do poprzedniego ekranu

#### Klawisz Enter

- Używany w menu do **potwierdzania wyborów**
- Można także klikać myszką na przyciski

---

### 💡 Wskazówki dotyczące sterowania

#### Dla początkujących:

1. **Nie panikuj** 🧘
   - Postać porusza się automatycznie
   - Skup się na obserwacji przeszkód
   - Reaguj spokojnie i z wyprzedzeniem

2. **Trenuj timing skoków** ⏱️
   - Zacznij od małych skoków (krótkie naciśnięcia)
   - Stopniowo ucz się ładować wyższe skoki
   - Obserwuj, jak długo musisz trzymać klawisz

3. **Używaj ruchu bocznego** ↔️
   - Nie wszystko wymaga skoku
   - Czasami lepiej ominąć przeszkodę z boku
   - Nie trzymaj A lub D cały czas - koryguj pozycję w razie potrzeby

4. **Oszczędzaj kontrolę** 🎮
   - Nie skacz bez potrzeby
   - Zbyt częste skakanie = mniejsza kontrola
   - Lepiej raz dobrze niż dwa razy źle

#### Dla zaawansowanych:

1. **Przewiduj przeszkody** 🔮
   - Patrz na ekran z wyprzedzeniem
   - Planuj ruchy 2-3 sekundy przed przeszkodą
   - Przygotuj się na kombinacje przeszkód

2. **Maksymalizuj prędkość** ⚡
   - Używaj minimalnych ruchów bocznych
   - Precyzyjne, krótkie skoki są szybsze
   - Nie trać czasu na zbędne manewry

3. **Kombinuj techniki** 🎯
   - Skok + ruch boczny + szybkie opadanie (S)
   - Podwójny skok + korekta kierunku
   - Szybkie reakcje na debuffy

4. **Adaptuj się do prędkości** 🏃💨
   - Gra przyspiesza - dostosuj timing
   - Przy większej prędkości potrzebne są szybsze reakcje
   - Używaj więcej precyzyjnych, małych skoków

---

### 🎯 Przykładowe sytuacje i rozwiązania

#### Sytuacja 1: Pojedyncza niska przeszkoda
```
Co widzisz: [Postać] -----> [Mała przeszkoda]
Rozwiązanie: Krótkie naciśnięcie Spacji (mały skok)
```

#### Sytuacja 2: Wysoka przeszkoda
```
Co widzisz: [Postać] -----> [Wysoka ściana]
Rozwiązanie: Przytrzymaj Spację 1+ sekundę (wysoki skok)
```

#### Sytuacja 3: Przeszkoda z boku
```
Co widzisz: [Postać] -----> [Przeszkoda po prawej]
Rozwiązanie: Naciśnij A (ruch w lewo) - omiń bez skoku
```

#### Sytuacja 4: Dwie przeszkody jedna za drugą
```
Co widzisz: [Postać] ---> [Przeszkoda 1] [Przeszkoda 2]
Rozwiązanie: 
  - Opcja A: Wysoki, długi skok (przytrzymaj Spację 1.2s)
  - Opcja B: Podwójny skok (jeśli masz power-up)
```

#### Sytuacja 5: Ściana Cię dogania
```
Co widzisz: [Ściana] [Postać] ----> [Przeszkoda]
Rozwiązanie: 
  - Nie używaj A (nie cofaj się)!
  - Szybki skok przez przeszkodę
  - Lub ruch w prawo (D) jeśli możliwe
```

#### Sytuacja 6: Odwrócone sterowanie (debuff)
```
Co widzisz: Czerwona ramka, przeszkoda po prawej
Rozwiązanie: 
  - PAMIĘTAJ: A i D są zamienione!
  - Naciśnij D (zamiast A) aby iść w lewo
  - Skup się i myśl odwrotnie
```

---

### 🎮 Schemat wszystkich klawiszy (Podsumowanie)

```
          [W / Spacja]
          Skok / Podwójny skok
                 ↑
                 |
    [A] ← [Twoja Postać] → [D]
    Lewo    (auto-ruch)    Prawo
                 |
                 ↓
               [S]
        Szybkie opadanie

Specjalne:
- [ESC] - Pauza
- [Enter] - Potwierdzenie w menu
```

---

### ❗ Najczęstsze błędy początkujących

1. ❌ **Zbyt częste skakanie**
   - ✅ Rozwiązanie: Skacz tylko gdy to konieczne

2. ❌ **Trzymanie A lub D cały czas**
   - ✅ Rozwiązanie: Używaj krótkich naciśnięć do korekty pozycji

3. ❌ **Panika przy debuffach**
   - ✅ Rozwiązanie: Zachowaj spokój, adaptuj się do efektu

4. ❌ **Zbyt późna reakcja**
   - ✅ Rozwiązanie: Patrz z wyprzedzeniem 2-3 sekundy

5. ❌ **Nieużywanie klawisza S**
   - ✅ Rozwiązanie: Użyj S gdy musisz szybko wylądować

6. ❌ **Ignorowanie power-upów**
   - ✅ Rozwiązanie: Zbieraj wszystkie niebieskie power-upy!

---

Teraz znasz wszystkie aspekty sterowania! Czas przejść do praktyki – **rozpocznij grę i trenuj!** 🎮🚀

---

## Rozgrywka

### Cel gry

Twoim głównym celem jest **przetrwać jak najdłużej** i zdobyć jak najwięcej punktów. Gra kończy się, gdy zderzy się z przeciwnikiem lub przeszkodą, albo gdy ściana dopadnie Twoją postać.

### Mechanika gry

1. **Automatyczne poruszanie się**
   - Postać automatycznie porusza się w prawo
   - Gra stopniowo przyspiesza wraz z czasem (od 1x do 2,5x prędkości)
   - Musisz unikać przeciwników i przeszkód

2. **Zbieranie punktów**
   - Punkty zdobywasz automatycznie za przetrwanie
   - Zbieraj monety podczas gry
   - Power-up "Podwójne punkty" zwiększa liczbę zdobywanych punktów

3. **Przeszkody i przeciwnicy**
   - Na Twojej drodze pojawią się różne przeszkody
   - Musisz omijać przeciwników poruszających się po ekranie
   - Kolizja z przeszkodą oznacza koniec gry

4. **Ściana**
   - Za Twoją postacią porusza się ściana
   - Jeśli będziesz zbyt wolny, ściana Cię dopadnie
   - Nie możesz cofać się do tyłu

5. **Progresja trudności**
   - Gra stopniowo przyspiesza (do 2,5x prędkości startowej)
   - Przeciwnicy pojawiają się częściej
   - Wymagana jest większa precyzja

### System walut

- **Monety** - zbieraj je podczas rozgrywki
- Monety można wydać w sklepie na nowe skórki
- Po śmierci zebrane monety są zapisywane na Twoim koncie
- Startowa ilość monet: **5000**

### Wskazówki dla graczy:

✅ **Obserwuj ekran** - patrz na nadchodzące przeszkody z wyprzedzeniem

✅ **Używaj skoków mądrze** - nie skacz bez potrzeby, oszczędzaj kontrolę

✅ **Zbieraj power-upy** - mogą znacząco ułatwić rozgrywkę

✅ **Unikaj debuffów** - czerwone power-upy utrudnią Ci grę

✅ **Trenuj timing** - ładowany skok wymaga praktyki

---

## Menu główne

Po uruchomieniu gry zobaczysz menu główne z następującymi opcjami:

### 1. **START GRY**
- Rozpoczyna nową rozgrywkę
- Przenosi Cię do ekranu gry
- Resetuje poprzednie wyniki z rundą

### 2. **SKLEP**
- Otwiera sklep ze skórkami
- Możesz kupić nowe wyglądy dla swojej postaci
- Zobacz sekcję [Sklep](#sklep) poniżej

### 3. **TUTORIAL** (jeśli dostępny)
- Wyświetla instrukcje sterowania
- Pokazuje podstawy rozgrywki
- Polecany dla nowych graczy

### 4. **USTAWIENIA** (jeśli dostępne)
- Pozwala dostosować głośność muzyki i efektów
- Zmiana jakości grafiki
- Inne preferencje

### 5. **WYJŚCIE**
- Zamyka grę
- Automatycznie zapisuje Twoje postępy

---

## Sklep

W sklepie możesz kupić nowe skórki dla swojej postaci za zebrane monety.

### Jak korzystać ze sklepu:

1. **Wejście do sklepu**
   - Wybierz opcję "SKLEP" w menu głównym
   - Zobaczysz listę dostępnych skórek

2. **Przeglądanie skórek**
   - Przewijaj listę dostępnych skórek
   - Każda skórka ma swoją cenę w monetach
   - Skórki, które już posiadasz, są oznaczone

3. **Kupowanie skórek**
   - Kliknij na wybraną skórkę
   - Jeśli masz wystarczająco monet, kliknij "KUP"
   - Monety zostaną automatycznie odjęte z Twojego konta

4. **Wybieranie skórki**
   - Kliknij na posiadaną skórkę
   - Kliknij "WYBIERZ" lub "ZAŁÓŻ"
   - Skórka będzie aktywna w następnej grze

5. **Powrót do menu**
   - Kliknij "POWRÓT" aby wrócić do menu głównego
   - Twoje zakupy są automatycznie zapisywane

### Ważne informacje o sklepie:

- ✅ Skórki kupujesz **raz na zawsze**
- ✅ Nie możesz kupić tej samej skórki dwa razy
- ✅ Zakupy są zapisywane automatycznie
- ✅ Możesz zmieniać skórki w dowolnym momencie
- ❌ Nie można zwrócić kupionych skórek

---

## Power-upy i efekty

Podczas gry będziesz spotykać różne power-upy. Niektóre pomogą Ci w rozgrywce (buffy), inne utrudnią ją (debuffy).

### ✅ Buffy (Pozytywne efekty)

#### 🚀 Przyspieszenie
- **Czas trwania:** 5 sekund
- **Efekt:** Zwiększa prędkość ruchu gracza o 50%
- **Ikona:** Niebieska ramka z ikoną przyspieszenia
- **Wskazówka:** Świetny do szybkiego pokonywania odcinków

#### ⭐ Podwójne punkty
- **Czas trwania:** 5 sekund
- **Efekt:** Wszystkie zdobyte punkty są podwojone
- **Ikona:** Niebieska ramka z gwiazdką
- **Wskazówka:** Zbieraj jak najwięcej punktów podczas aktywnego efektu

#### 🦘 Podwójny skok
- **Czas trwania:** 5 sekund
- **Efekt:** Możliwość wykonania drugiego skoku w powietrzu
- **Ikona:** Niebieska ramka z ikoną skoku
- **Wskazówka:** Naciśnij Spację ponownie w powietrzu, aby wykonać drugi skok
- **Uwaga:** Drugi skok ładuje się szybciej niż normalny (2,5x szybciej)

### ❌ Debuffy (Negatywne efekty)

#### 🔄 Odwrócone sterowanie
- **Czas trwania:** 5 sekund
- **Efekt:** Klawisze A i D są zamienione miejscami
- **Ikona:** Czerwona ramka z ikoną zamiany
- **Wskazówka:** Skup się i dostosuj swoje ruchy

#### ⚡ Losowe impulsy
- **Czas trwania:** 5 sekund
- **Efekt:** Postać co chwilę jest odpychana w losowym kierunku
- **Ikona:** Czerwona ramka z błyskawicą
- **Wskazówka:** Trudno kontrolować postać - uważaj na przeszkody

#### 🐌 Spowolnienie
- **Czas trwania:** 5 sekund
- **Efekt:** Prędkość ruchu gracza zmniejszona do 60%
- **Ikona:** Czerwona ramka z ikoną ślimaka
- **Wskazówka:** Nie cofaj się za bardzo, uważaj na ścianę!

#### 🌫️ Ograniczona widoczność
- **Czas trwania:** 5 sekund
- **Efekt:** Ekran jest częściowo zasłonięty, trudniej zobaczyć przeszkody
- **Ikona:** Czerwona ramka z ikoną mgły
- **Wskazówka:** Najbardziej wymagający debuff - zachowaj spokój

### Jak rozpoznać efekty:

- **Niebieska ramka** wokół ekranu = aktywny buff (pozytywny efekt)
- **Czerwona ramka** wokół ekranu = aktywny debuff (negatywny efekt)
- **Ikony w górnym rogu** pokazują aktywne efekty i ich pozostały czas
- **Możesz mieć wiele efektów jednocześnie**

---

## Zakończenie gry

### Ekran Game Over

Kiedy przegrasz, zobaczysz ekran z Twoimi wynikami:

1. **Wynik (Score)**
   - Liczba punktów zdobytych w tej rundzie
   - Im dłużej przetrwałeś, tym więcej punktów

2. **Zebrane monety**
   - Liczba monet zebranych w tej rundzie
   - Monety są automatycznie dodawane do Twojego konta

3. **Czas przetrwania**
   - Ile czasu udało Ci się przetrwać
   - Mierzony w sekundach lub minutach

### Opcje po zakończeniu gry:

#### 🔄 ZAGRAJ PONOWNIE
- Rozpoczyna nową rozgrywkę
- Zachowuje zebrane monety
- Resetuje wynik do zera

#### 🏠 MENU GŁÓWNE
- Wraca do menu głównego
- Możesz odwiedzić sklep lub zakończyć grę
- Postęp jest zapisany

#### 🚪 WYJŚCIE
- Zamyka grę
- Automatycznie zapisuje wszystkie dane

### Zapisywanie postępów

- ✅ Gra **automatycznie zapisuje** Twoje postępy
- ✅ Zebrane monety są zapisywane po każdej rundzie
- ✅ Kupione skórki są zapisywane natychmiast
- ✅ Nie musisz ręcznie zapisywać gry

---

## Rozwiązywanie problemów

### Gra nie uruchamia się

**Windows:**
- Upewnij się, że masz zainstalowany **DirectX 11** lub nowszy
- Sprawdź, czy antywirus nie blokuje gry
- Uruchom grę jako administrator (prawy klik → "Uruchom jako administrator")
- Sprawdź, czy masz wystarczająco miejsca na dysku (minimum 500 MB)

**macOS:**
- Sprawdź uprawnienia aplikacji w **Preferencje Systemowe → Bezpieczeństwo**
- Upewnij się, że system wspiera **Metal API**
- Sprawdź, czy Twój system to minimum macOS 10.13

### Gra działa wolno / laguje

- Zamknij inne aplikacje działające w tle
- Sprawdź, czy Twój komputer spełnia minimalne wymagania
- Zmniejsz jakość grafiki w ustawieniach (jeśli dostępne)
- Upewnij się, że gra działa na 60 FPS (opcja w ustawieniach)
- Zaktualizuj sterowniki karty graficznej

### Problemy z dźwiękiem

- Sprawdź, czy dźwięk nie jest wyciszony w systemie
- Sprawdź ustawienia głośności w grze
- Upewnij się, że urządzenia audio są poprawnie podłączone
- Uruchom grę ponownie

### Postęp nie zapisuje się

- Upewnij się, że gra ma uprawnienia do zapisu plików
- **Windows:** Sprawdź folder `%AppData%\..\LocalLow\[Nazwa studia]\DRUG`
- **macOS:** Sprawdź folder `~/Library/Application Support/[Nazwa studia]/DRUG`
- Nie zamykaj gry podczas zapisywania (po zakończeniu rundy)

### Gra się zacina / zawiesza

- Sprawdź, czy Twój komputer nie przegrzewa się
- Zamknij niepotrzebne programy w tle
- Uruchom grę ponownie
- Sprawdź, czy masz najnowsze aktualizacje systemu

### Nie mogę kupić skórki w sklepie

- Upewnij się, że masz wystarczająco monet
- Sprawdź, czy nie posiadasz już tej skórki
- Spróbuj wrócić do menu głównego i wejść do sklepu ponownie
- Zrestartuj grę, jeśli problem się utrzymuje

### Sterowanie nie działa poprawnie

- Sprawdź, czy żaden inny program nie przechwytuje klawiszy
- Upewnij się, że używasz prawidłowych klawiszy (W, A, S, D, Spacja)
- Podczas aktywnego debuffu "Odwrócone sterowanie" - klawisze A i D są zamienione
- Uruchom grę ponownie

### Kontakt w sprawie problemów

Jeśli żaden z powyższych sposobów nie pomógł:

1. Sprawdź logi gry:
   - **Windows:** `%AppData%\..\LocalLow\[Nazwa studia]\DRUG\output_log.txt`
   - **macOS:** `~/Library/Logs/[Nazwa studia]/DRUG/Player.log`

2. Opisz problem jak najdokładniej
3. Dołącz informacje o swoim systemie (OS, RAM, procesor)
4. Jeśli to możliwe, załącz zrzut ekranu z błędem

---

## Podziękowania

Dziękujemy za zagranie w **DRUG**! Życzymy miłej rozrywki i powodzenia w ustanawianiu nowych rekordów!

---

**Wersja dokumentacji:** 1.0  
**Data ostatniej aktualizacji:** Styczeń 2026  
**Platforma:** Windows 10/11, macOS 10.13+  
**Silnik gry:** Unity 2D  
**Tryb gry:** Offline, Single-player
