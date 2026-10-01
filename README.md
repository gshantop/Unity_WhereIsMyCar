<h1 align="center">🚗 Where Is My Car?</h1>

<p align="center">
  <img alt="Unity" src="https://img.shields.io/badge/Unity-2D-black?logo=unity">
  <img alt="C#" src="https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white">
  <a href="https://github.com/gshantop/Unity_WhereIsMyCar/releases/tag/1.0"><img alt="Release" src="https://img.shields.io/badge/release-1.0-blue"></a>
</p>

2D spēle, kas izstrādāta ar **Unity**. Spēlētājs ar peli velk objektus (mašīnas) spēles laukumā, vienlaikus izvairoties no lidojošiem šķēršļiem un bumbām.


---

## 📖 Satura rādītājs

- [Par projektu](#-par-projektu)
- [Spēles apraksts un vadība](#-spēles-apraksts-un-vadība)
- [Izmantotās tehnoloģijas](#-izmantotās-tehnoloģijas)
- [Uzstādīšana un palaišana](#-uzstādīšana-un-palaišana)
- [Projekta struktūra](#-projekta-struktūra)
- [Darāmo darbu saraksts](#-darāmo-darbu-saraksts)

---

## 🎮 Par projektu

**Where Is My Car?** ir 2D Unity spēle, kurā spēlētājs ar peli velk mašīnas pa spēles ainu. Spēles laukumā parādās lidojoši objekti un bumbas, tāpēc jāpārvietojas uzmanīgi, lai tos nesaskartu un neuzspridzinātu bumbu. Projekts veidots kā mācību darbs, lai apgūtu Unity pamatus: ainu veidošanu, objektu fiziku, animācijas, skaņu un lietotāja saskarni.

## 🕹️ Spēles apraksts un vadība

### Spēles ainas

| Aina | Apraksts |
| --- | --- |
| **Sākuma aina** | Spēles galvenā izvēlne ar fona attēlu, animācijām un skaņas efektiem |
| **Spēles aina** | Galvenais spēles lauks ar mašīnām, lidojošiem objektiem, bumbām, mākoņiem un animētiem objektiem |
| **Beigu stāvoklis** | Uzvara vai zaudējums, atkarībā no spēles gaitas |

### Spēles elementi

- **Mašīnas** — objekti, ko spēlētājs velk ar peli. Katrai mašīnai var mainīt pagrieziena virzienu (spoguļattēlu), un tās sākuma virziens tiek izvēlēts nejauši.
- **Lidojošie objekti** — pārvietojas pa ainu; saskare ar tiem var izraisīt zaudējumu.
- **Bumbas** — var uzsprāgt, un tas ietekmē spēles gaitu.
- **Mākoņi** — fona elements, kam jāatrodas aiz mašīnām.
- **Animēti objekti** — papildina spēles ainu un padara to dzīvāku.
- **Platuma / šauruma pogas** — maina mašīnas izmēru.

### Vadība

| Darbība | Vadība |
| --- | --- |
| Vilkt objektu | Peles kreisā poga (turēt un vilkt) |
| Mainīt objekta pagrieziena virzienu (spoguļattēls) | `SPACE` |
| Mainīt objekta platumu / šaurumu | Spēles saskarnes pogas |

## 🛠️ Izmantotās tehnoloģijas

- **Spēles dzinējs:** Unity
- **Programmēšanas valoda:** C#
- **Izstrādes vide:** Visual Studio
- **Versiju kontrole:** Git / GitHub

## ⚙️ Uzstādīšana un palaišana

### Prasības

- [Unity Hub](https://unity.com/download)
- Unity redaktora versija, kas norādīta failā `ProjectSettings/ProjectVersion.txt`
- Git

### Soļi

1. Klonē repozitoriju:
```bash
   git clone https://github.com/gshantop/Unity_WhereIsMyCar.git
```
2. Atver **Unity Hub** → **Add** → izvēlies klonēto mapi.
3. Atver projektu ar atbilstošo Unity versiju (Unity Hub piedāvās to instalēt, ja tās nav).
4. Atver sākuma ainu no mapes `Assets/Scenes` un nospied **Play** ▶️.

### Gatavā versija

Gatavo spēles būvējumu var lejupielādēt sadaļā [Releases](https://github.com/gshantop/Unity_WhereIsMyCar/releases/tag/1.0).

## 📁 Projekta struktūra

```
Unity_WhereIsMyCar/
├── Assets/            # Spēles resursi: ainas, skripti, attēli, skaņas, animācijas
├── Packages/          # Unity pakotņu atkarības
├── ProjectSettings/   # Unity projekta iestatījumi
└── README.md
```

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


