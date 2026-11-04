<h1 align="center">🚗 Where Is My Car?</h1>

<p align="center">
  <img alt="Unity" src="https://img.shields.io/badge/Unity-2D-black?logo=unity">
  <img alt="C#" src="https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white">
  <img alt="Android" src="https://img.shields.io/badge/Android-3DDC84?logo=android&logoColor=white">
  <a href="https://github.com/gshantop/Unity_WhereIsMyCar/releases/tag/1.0"><img alt="Release" src="https://img.shields.io/badge/release-1.0-blue"></a>
</p>

2D spēle, kas izstrādāta ar **Unity**. Spēlētājs ar peli velk objektus (mašīnas) spēles laukumā, vienlaikus izvairoties no lidojošiem šķēršļiem un bumbām. Spēle ir pielāgota **Android** ierīcēm, un tajā ir arī otra spēle – **Hanojas tornis**.


---

## 📖 Satura rādītājs

- [Par projektu](#-par-projektu)
- [Ekrānšāviņi](#-ekrānšāviņi)
- [Spēles apraksts un vadība](#-spēles-apraksts-un-vadība)
- [Hanojas tornis](#-hanojas-tornis)
- [Reklāmas (Unity Ads)](#-reklāmas-unity-ads)
- [Izmantotās tehnoloģijas](#-izmantotās-tehnoloģijas)
- [Uzstādīšana un palaišana](#-uzstādīšana-un-palaišana)
- [Projekta struktūra](#-projekta-struktūra)
- [Darāmo darbu saraksts](#-darāmo-darbu-saraksts)

---

## 🎮 Par projektu

**Where Is My Car?** ir 2D Unity spēle, kurā spēlētājs ar peli velk mašīnas pa spēles ainu. Spēles laukumā parādās lidojoši objekti un bumbas, tāpēc jāpārvietojas uzmanīgi, lai tos nesaskartu un neuzspridzinātu bumbu. Projekts veidots kā mācību darbs, lai apgūtu Unity pamatus: ainu veidošanu, objektu fiziku, animācijas, skaņu un lietotāja saskarni.

Projekts papildināts ar pielāgošanu **Android** platformai, reklāmām ar **Unity Ads SDK** un trešo ainu – loģikas spēli **Hanojas tornis**.

## 📸 Ekrānšāviņi

### Sākuma aina

<img width="1011" height="467" alt="image" src="https://github.com/user-attachments/assets/336a1aaf-e22f-4167-ad56-c1ed39a4313a" />

### Pilsētas aina

<img width="1000" height="467" alt="image" src="https://github.com/user-attachments/assets/2e6109fa-d34b-46b3-aa58-370b56ab3cfd" />

### Hanojas tornis

<img width="1000" height="462" alt="image" src="https://github.com/user-attachments/assets/17315a26-6c4a-4192-a89c-4194dcc27f90" />

## 🕹️ Spēles apraksts un vadība

### Spēles ainas

| Aina | Apraksts |
| --- | --- |
| **Sākuma aina** | Spēles galvenā izvēlne ar fona attēlu, animācijām un skaņas efektiem; izvēle starp pilsētas spēli un Hanojas torni |
| **Spēles aina** | Galvenais spēles lauks ar mašīnām, lidojošiem objektiem, bumbām, mākoņiem un animētiem objektiem |
| **Hanojas tornis** | Loģikas spēle ar 6 blokiem un 3 stieņiem |
| **Beigu stāvoklis** | Uzvara vai zaudējums, atkarībā no spēles gaitas |

### Spēles elementi

- **Mašīnas** — objekti, ko spēlētājs velk ar peli vai pirkstu. Katrai mašīnai var mainīt pagrieziena virzienu (spoguļattēlu), un tās sākuma virziens tiek izvēlēts nejauši.
- **Lidojošie objekti** — pārvietojas pa ainu **virs mašīnām**; saskare ar tiem var izraisīt zaudējumu.
- **Bumbas** — var uzsprāgt, un tas ietekmē spēles gaitu.
- **Mākoņi** — lido pāri ainai virs mašīnām.
- **Animēti objekti** — papildina spēles ainu un padara to dzīvāku.
- **Platuma / šauruma pogas** — maina mašīnas izmēru.
- **Sirsniņas** — 3 dzīvības; kad tās beidzas, spēle ir zaudēta.
- **Taimeris un zvaigznes** — jo ātrāk visas mašīnas novietotas, jo vairāk zvaigžņu.

### Vadība

| Darbība | Dators | Telefons |
| --- | --- | --- |
| Vilkt objektu | Peles kreisā poga (turēt un vilkt) | Pirksts uz mašīnas |
| Mainīt objekta pagrieziena virzienu (spoguļattēls) | `SPACE` vai poga | Poga |
| Mainīt objekta platumu / šaurumu | Spēles saskarnes pogas | Spēles saskarnes pogas |
| Pagriezt objektu | Pogas ⟲ / ⟳ | Pogas ⟲ / ⟳ |
| Tuvināt / attālināt karti | Ritenītis vai `+` / `-` | Divi pirksti |
| Bīdīt karti | Labā poga vai kreisā pa tukšu vietu | Viens pirksts pa tukšu vietu |
| Atpakaļ uz pilnu skatu | Dubultklikšķis | Dubultskāriens |

Ja karte ir tuvināta un mašīnu velk pie ekrāna malas, karte pati bīdās līdzi.

## 🗼 Hanojas tornis

Trešā spēles aina, izstrādāta atsevišķā zarā **`hanojas-tornis`**.

### Noteikumi

- Sākumā visi **6 bloki** atrodas uz kreisā stieņa – lielākais apakšā.
- Visu torni jāpārvieto uz **citu stieni** (vidējo vai labējo).
- Vienlaikus var pārvietot tikai **vienu – augšējo** bloku.
- **Lielāku bloku nedrīkst likt uz mazāka** – nepareizs gājiens tiek atcelts, un bloks iekrāsojas sarkans.
- Mazākais iespējamais gājienu skaits 6 blokiem ir **63**.

### Vadība

| Darbība | Vadība |
| --- | --- |
| Pārvietot bloku | Pirkstu vai peli vilkt augšējo bloku uz citu stieni un atlaist |
| Sākt no jauna | Poga ↻ |
| Atgriezties izvēlnē | Poga **Menu** |

### Rezultāts

Uzvaras logā tiek parādīts spēles laiks un zvaigznes (tāpat kā pilsētas ainā):

| Laiks | Zvaigznes |
| --- | --- |
| līdz 1:30 | ⭐⭐⭐ |
| līdz 3:00 | ⭐⭐ |
| ilgāk | ⭐ |

Ainā ir fona mūzika un skaņas efekti, paņemot un noliekot bloku, kā arī uzvarot.

### Skripti

| Skripts | Uzdevums |
| --- | --- |
| `HanoiGameManager.cs` | Spēles loģika: stieņu vietas, gājienu pārbaude, taimeris, uzvara, skaņas |
| `HanoiBlock.cs` | Viens bloks: izmērs, kustības animācija, kļūdas efekts |
| `PanelOnTop.cs` | Uzvaras/zaudējuma logs vienmēr virs citiem objektiem |

## 📺 Reklāmas (Unity Ads)

| Veids | Kad tiek rādīta |
| --- | --- |
| **Pilnekrāna (Interstitial)** | Ainu pārslēgšanas brīdī (Play, Restart, Menu) |
| **Apbalvojošā (Rewarded)** | Pēc spēlētāja izvēles; pēc noskatīšanās pilsētas ainā tiek iznīcināti visi lidojošie objekti |
| **Baneris (Banner)** | Nepārtraukti ekrāna augšā centrā, netraucējot spēlei |

Reklāmas darbojas testa režīmā (`Test Mode`).

## 🛠️ Izmantotās tehnoloģijas

- **Spēles dzinējs:** Unity
- **Programmēšanas valoda:** C#
- **Ievade:** Unity Input System (skārienekrāns un pele)
- **Reklāmas:** Unity Ads SDK
- **Platforma:** Android
- **Izstrādes vide:** Visual Studio
- **Versiju kontrole:** Git / GitHub

## ⚙️ Uzstādīšana un palaišana

### Prasības

- [Unity Hub](https://unity.com/download)
- Unity redaktora versija, kas norādīta failā `ProjectSettings/ProjectVersion.txt`
- Unity **Android Build Support** modulis (APK būvēšanai)
- Git

### Soļi

1. Klonē repozitoriju:
```bash
   git clone https://github.com/gshantop/Unity_WhereIsMyCar.git
```
2. Hanojas torņa versijai pārslēdzies uz zaru:
```bash
   git checkout hanojas-tornis
```
3. Atver **Unity Hub** → **Add** → izvēlies klonēto mapi.
4. Atver projektu ar atbilstošo Unity versiju (Unity Hub piedāvās to instalēt, ja tās nav).
5. Atver sākuma ainu no mapes `Assets/Scenes` un nospied **Play** ▶️.


## ✅ Darāmo darbu saraksts

- [x] Objektu vilkšana ar peli
- [x] Nejauša objektu izvietošana
- [x] Spēles aina
- [ ] Sākuma ainas animācijas
- [x] Papildu audio efekti sākuma ekrānā
- [x] Fona attēla izstiepšana pa visu kameras skatu
- [x] Atgriešanās uz sākuma ainu no spēles
- [x] Spēles ainas papildināšana ar animētiem objektiem
- [x] Pēc zaudējuma bloķēt objektu vilkšanu un bumbu spridzināšanu
- [x] Nejaušs objektu pagrieziena virziens (flip)
- [x] Labot platuma / šauruma pogu darbību pēc pagrieziena virziena maiņas
- [x] Mākoņiem jālido pāri mašīnām, nevis aiz tām
- [x] Pielāgošana Android platformai (skārienekrāns)
- [x] Kartes tuvināšana un bīdīšana
- [x] Pilnekrāna reklāma ainu pārslēgšanas brīdī
- [x] Apbalvojošā reklāma ar bonusu
- [x] Banera reklāma
- [x] Hanojas torņa aina atsevišķā zarā
- [x] APK fails GitHub Releases sadaļā
