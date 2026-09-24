<!--
  SPDX-License-Identifier: GPL-3.0-only
  © 2026 Patrick JAILLET — RomTranslator
-->
# Spécifications techniques — Module Sega Saturn

> Document de recherche préalable à l'implémentation du module Sega Saturn (Phase 5). Rédigé à partir de
> recherches sur le web (documentation SEGA officielle référencée mais non directement exploitable — PDF
> scanné —, sites de reverse engineering, communautés romhacking) et de connaissances générales sur
> l'architecture Saturn. **Niveau de confiance indiqué pour chaque section** : les affirmations non
> vérifiées auprès d'une source primaire exploitable doivent être confirmées avant d'écrire du code qui en
> dépend de façon critique (notamment les offsets exacts de l'IP.BIN), par exemple par extraction manuelle
> d'un IP.BIN réel avec un éditeur hexadécimal et comparaison avec ce document.

---

## 1. Formats d'image disque pris en charge

**Confiance : élevée** (recoupé par plusieurs sources communautaires indépendantes).

La Sega Saturn utilise des disques CD-ROM (simple ou double couche selon les jeux, un ou plusieurs disques). Les formats d'image courants rencontrés dans la communauté de préservation/traduction :

| Format | Composition | Statut retenu pour la v1 |
|---|---|---|
| **BIN/CUE** | `.bin` (données brutes secteur par secteur, pistes audio et données incluses) + `.cue` (texte décrivant le découpage en pistes/index) | **Retenu**, format prioritaire |
| **CCD/IMG/SUB** (CloneCD) | `.ccd` (description) + `.img` (données) + `.sub` (canaux subcode) | Envisagé en Phase 5.2+, non prioritaire v1 |
| **ISO** simple | Un seul flux de données, sans piste audio ni subcode | Non retenu seul (perd les informations de piste ; beaucoup de jeux Saturn ont des pistes audio CD-DA) |
| **MDF/MDS** (Alcohol 120%) | `.mdf` (données, équivalent du `.bin`) + `.mds` (description, équivalent du `.cue`) | Non retenu v1 (outillage en déclin, convertible vers BIN/CUE) |

**Décision de portée v1** : `IRomLoader`/`IRomValidator` du module Saturn prendront en charge **BIN/CUE** uniquement en premier lieu, ce qui couvre la quasi-totalité des images en circulation dans la communauté de préservation (format recommandé par des projets comme Redump). CCD et MDF/MDS pourront être ajoutés plus tard sans changement d'architecture (nouvelle implémentation d'`IRomLoader`), tant que la conversion préalable vers BIN/CUE reste possible avec des outils tiers largement disponibles.

Le fichier `.cue` référence un ou plusieurs fichiers `.bin`, avec des lignes `TRACK` (numéro et type : `MODE1/2352`, `AUDIO`, etc.) et des marqueurs `INDEX`. Le mode `MODE1/2352` (2 352 octets par secteur brut, dont 2 048 octets de données utiles) est le mode le plus courant pour la piste de données Saturn.

---

## 2. Structure du système de fichiers Saturn

**Confiance : moyenne à élevée** (confirmé par plusieurs sources, y compris une référence à la spécification officielle SEGA « Saturn File System Library », mais le PDF source n'a pas pu être extrait en texte pour vérification fine).

- Le système de fichiers du disque de données Saturn est conforme à la norme **ISO 9660**, avec des extensions propres à Saturn (« Extensions for ISO9660 Standard », selon la documentation officielle SEGA référencée par plusieurs sources).
- La **zone système** (« system area ») se trouve au tout début du disque et contient les informations utilisées au démarrage de l'application (voir § 3, IP.BIN).
- La Saturn prend en charge une **structure de sous-dossiers** (contrairement à certaines consoles CD plus anciennes limitées à un répertoire plat).
- Les secteurs de données utilisent le **Mode 2** du CD-ROM, mêlant Form 1 et Form 2 ; tous les secteurs sont normalisés à **2 048 octets** de données utiles côté application (indépendamment de la taille brute du secteur physique, 2 352 octets).
- Un même fichier n'est **pas nécessairement stocké de façon contiguë** : les données sont généralement entrelacées (« interleaved »), notamment pour permettre la lecture simultanée de flux vidéo/audio et de données de jeu. **Point d'attention pour l'extraction/réinjection de texte** : un outil qui suppose une disposition strictement séquentielle des fichiers sur le disque peut se tromper ; la lecture doit passer par la table des fichiers ISO9660 (secteur de départ + taille par fichier), jamais par un simple calcul d'offset relatif au fichier précédent.
- Un service d'accès aux fichiers de haut niveau (« Saturn File System Library ») est fourni par le SDK officiel et prend en charge l'accès de niveau ISO9660 standard (« ISO9660 Level File Access »).

**Conséquence pour l'architecture du module** : `SaturnRomLoader` (Phase 5.2) devra implémenter un analyseur ISO9660 minimal (répertoire racine, table des chemins ou parcours récursif des répertoires, résolution nom de fichier → secteur de départ + taille), plutôt que de supposer une structure de fichier plate ou un format propriétaire.

---

## 3. En-tête IP.BIN (Initial Program)

**Confiance : moyenne** pour la structure générale et la présence des champs ; **faible à confirmer** pour les offsets byte-par-byte exacts de chaque champ, faute d'avoir pu extraire le texte de la spécification officielle SEGA (« Disc Format Standards Specification Sheet », document ST-040-R4-051795, disponible en PDF scanné sur antime.kapsi.fi/sega/files/ST-040-R4-051795.pdf, référence citée par de multiples sources communautaires comme document autoritaire). **Action recommandée avant la Phase 5.2** : extraire l'IP.BIN d'une image ROM de test (homebrew libre de droits) avec un éditeur hexadécimal et comparer avec le tableau ci-dessous, en s'appuyant si possible sur les gabarits `IP.BIN` fournis par le SDK homebrew moderne Yaul (`yaul-org/libyaul` sur GitHub) plutôt que sur ce seul document.

### Faits confirmés par plusieurs sources indépendantes

- L'IP.BIN occupe les **premiers 32 768 octets (32 Kio)** du disque, soit les **16 premiers secteurs** de 2 048 octets de la zone système (certaines sources mentionnent 15 secteurs pour la partie strictement « boot » avant l'AIP — à confirmer).
- Le tout premier champ du disque est la chaîne d'identification matérielle **`"SEGA SEGASATURN"`**, vérifiée par le BIOS de démarrage (« boot ROM ») avant tout chargement : c'est ce motif que `SaturnRomValidator` devra rechercher en tout premier lieu pour reconnaître une image Saturn valide.
- L'IP.BIN est composé d'un **code de boot** (« boot code ») suivi du **programme initial de l'application** (AIP, « Application Initial Program »).
- Le code de boot est chargé par le BIOS à l'adresse mémoire **`0x06002000`** (zone RAM basse) ; l'adresse de l'AIP qui suit dépend de la taille du code de boot et du nombre de codes de zone/région présents dans l'en-tête.
- L'en-tête contient au moins les champs suivants (noms et présence confirmés, offsets exacts non vérifiés) : identifiant matériel, identifiant de fabricant (« maker ID »), numéro de produit et version, date de sortie (« release date »), informations sur les périphériques compatibles, symboles de zone/région, nom du jeu (« game title »), adresses de pile pour les deux processeurs SH-2 (maître et esclave), adresse et taille du premier fichier à charger (habituellement `1ST_READ.BIN`).
- Un outil de sécurité (« security ring ») vérifie certains octets de l'IP.BIN long-mot par long-mot lors du démarrage sur un vrai Saturn ; ce mécanisme concerne uniquement l'exécution sur matériel réel et n'affecte pas la lecture/écriture d'une image ROM par un outil de traduction (aucune préoccupation particulière pour `IRomLoader`/`ITextInjector`, sauf si RomTranslator devait un jour valider l'exécutabilité sur matériel réel, hors périmètre v1).

### Tableau de travail (à vérifier avant implémentation, Phase 5.2)

| Champ | Offset approximatif | Longueur approximative | Confiance |
|---|---|---|---|
| Identifiant matériel (`"SEGA SEGASATURN"`) | `0x0000` | 16 octets | Élevée |
| Identifiant fabricant | après l'identifiant matériel | variable | Moyenne (présence confirmée, offset à vérifier) |
| Numéro de produit / version | — | — | Moyenne (présence confirmée, offset à vérifier) |
| Date de sortie | — | — | Moyenne (présence confirmée, offset à vérifier) |
| Symboles de périphériques compatibles | — | — | Moyenne (présence confirmée, offset à vérifier) |
| Symboles de zone/région | — | — | Moyenne (présence confirmée, offset à vérifier) |
| Titre du jeu | — | jusqu'à 112 octets selon les usages courants du SDK (à confirmer) | Faible |
| Adresses de pile (maître/esclave SH-2) | — | — | Faible |
| Adresse/taille du premier fichier à exécuter | — | — | Faible |

Ce tableau sera complété avec les offsets exacts en Phase 5.2, avant l'écriture de `SaturnRomLoader`, une fois une image de test disponible pour vérification directe.

---

## 4. Formats de compression rencontrés

**Confiance : moyenne** (les formats cités sont bien documentés dans la communauté romhacking, mais leur usage est **spécifique à chaque jeu** : aucune compression n'est imposée par le système d'exploitation Saturn lui-même).

- **PRS** : format de compression LZ77 avec émulation RLE et correspondances étendues, développé par SEGA et utilisé dans de nombreux titres (cité pour *NiGHTS into Dreams*), également présent sur Dreamcast. Des outils communautaires existent (« PRS Utils » sur romhacking.net).
- **Formats LZ « maison »** : plusieurs jeux utilisent des variantes propriétaires de LZ (parfois combinées à du Huffman), parfois identifiables par un en-tête de fichier explicite comme `"LZ00"`.
- **Kosinski** : format de compression employé notamment pour des données de police de caractères sur certains titres.
- **LZSS** : cité pour la compression de graphismes (texture/tuiles).

**Conséquence pour l'architecture** : la compression n'étant pas standardisée au niveau système, `ITextExtractor`/`ITextInjector` du module Saturn devront très probablement être **spécifiques à chaque jeu pris en charge** pour la partie décompression/recompression des blocs de données contenant le texte, même si l'infrastructure générique (chargement ISO9660, IP.BIN, table de caractères) reste commune. Ce point est cohérent avec la réalité du romhacking Saturn : un outil générique ne peut pas décompresser un format propriétaire inconnu sans travail de rétro-ingénierie par jeu.

---

## 5. Stockage du texte et encodages

**Confiance : moyenne**, cohérente avec les pratiques générales du romhacking sur consoles de 5ᵉ génération, mais chaque jeu Saturn peut diverger.

- **Encodage** : les jeux japonais utilisent couramment le **Shift-JIS** pour le texte en katakana/hiragana/kanji ; le texte en alphabet latin (jeux occidentaux, ou menus techniques) utilise couramment de l'**ASCII simple**, parfois une table de correspondance propre au jeu qui ne suit ni Shift-JIS ni ASCII standard (surtout pour les caractères accentués en français, absents de ces deux jeux de caractères — **point d'attention direct pour RomTranslator**, dont l'objectif est la traduction en français).
- **Compression de texte DTE/MTE** (Dual/Multiple Tile Encoding, terminologie héritée du romhacking NES/SNES mais le principe s'applique aussi sur Saturn) : certains jeux remplacent des paires ou séquences de caractères fréquentes par un octet unique, pour économiser l'espace. Un outil de traduction générique doit détecter ce mécanisme via la table de caractères (`ICharacterTable`), qui décrit déjà des séquences multi-octets (voir Phase 4.2, `ICharacterTable.Decode`/`Encode`).
- **Pointeurs** : les chaînes de texte sont référencées par des tables de pointeurs, **relatifs ou absolus selon le jeu** (aucune convention universelle sur Saturn). Un pointeur absolu pointe directement vers une adresse mémoire ou un offset fichier ; un pointeur relatif est calculé depuis une base (début de bloc, début de fichier, ou adresse de chargement en mémoire). **Conséquence directe pour `ITextInjector`** : la relocalisation de pointeurs après réinjection (Phase 5.6) devra être paramétrable par jeu (base de calcul, taille du pointeur — 16 ou 32 bits selon les cas), pas câblée en dur.
- Des outils existants dans la communauté (« Foxy Editor ») prennent en charge l'extraction/édition/réinjection avec gestion Shift-JIS et DTE/MTE sur diverses consoles, ce qui confirme que ces mécanismes sont bien les plus courants à anticiper.

---

## 6. Polices de caractères embarquées

**Confiance : faible à moyenne** (peu de documentation générale trouvée ; les détails varient fortement d'un jeu à l'autre).

- Comme la plupart des consoles de cette génération, la Saturn ne dispose pas de rendu vectoriel : les polices de caractères sont des **fontes bitmap**, généralement stockées sous forme de tuiles (tiles) exploitées par le VDP1 (processeur d'affichage sprites/texture) ou le VDP2 (fonds de défilement).
- Certains jeux stockent leur police compressée (exemple cité : compression Kosinski pour une police) dans un fichier dédié du disque, séparé du texte lui-même.
- Le BIOS Saturn embarque sa propre police système, mais les jeux définissent presque toujours leur propre police plutôt que de réutiliser celle du BIOS, ce qui signifie qu'**une traduction en français devra très probablement inclure une police modifiée** pour ajouter les caractères accentués absents de la police d'origine (é, è, à, ç, etc.) — un point de vigilance direct pour la Phase 5.3 (tables de caractères) et au-delà (pas de tâche ROADMAP dédiée à l'édition de police en v1, à signaler comme limitation, voir § 7).

---

## 7. Limitations connues et cas particuliers non couverts en v1

**Confiance : élevée** pour l'existence de ces cas, recoupée par plusieurs sources.

- **Jeux multi-disques** : plusieurs jeux Saturn sont distribués sur 2 disques ou plus (RPG volumineux notamment). La v1 du module Saturn ne prend en charge que les **projets mono-disque** ; un projet multi-disques nécessiterait de traiter chaque disque comme une image distincte avec ses propres fichiers, sans mécanisme de projet unifié pour l'instant — à documenter comme non couvert et à concevoir explicitement si une demande réelle apparaît.
- **Différences régionales** : un même jeu peut différer significativement entre versions japonaise, américaine et européenne (contenu retiré, correctifs, voire code de zone différent dans l'IP.BIN). RomTranslator ne garantit la compatibilité qu'avec **la version testée explicitement** pour un jeu donné ; aucune tentative de gérer automatiquement plusieurs variantes régionales d'un même titre en v1.
- **Protection anti-copie du disque (« security ring »)** : mécanisme matériel de vérification du disque au démarrage sur console réelle, indépendant du contenu applicatif. Il n'affecte pas la capacité de RomTranslator à lire/modifier une image ROM (qui est un fichier, pas un disque physique), mais signifie qu'**une image reconstituée par RomTranslator devra être gravée ou utilisée via un émulateur/flashcart qui gère cette vérification** pour fonctionner sur une vraie console — hors périmètre de RomTranslator, qui ne produit qu'une image ou un patch.
- **Compressions propriétaires non documentées publiquement** : certains jeux utilisent des formats de compression jamais rétro-conçus par la communauté. Pour un jeu donné, si le format de compression des blocs de texte n'est pas identifié, l'extraction/réinjection ne pourra pas fonctionner pour ce jeu tant qu'un travail de rétro-ingénierie spécifique n'aura pas été fait — cette limitation est inhérente à l'approche par jeu (§ 4) et devra être documentée au cas par cas dans `MEMOIRE.md` au fur et à mesure des jeux pris en charge.

---

## Sources consultées

- [SEGA Confidential General Notice — Disc Format Standards Specification Sheet (ST-040-R4-051795.pdf)](https://antime.kapsi.fi/sega/files/ST-040-R4-051795.pdf) — document officiel SEGA référencé comme autoritaire par la communauté ; PDF scanné, non exploitable en extraction de texte automatique lors de cette recherche.
- [Saturn documentation, libs & stuff (antime.kapsi.fi)](https://antime.kapsi.fi/sega/docs.html) — index de la documentation SEGA officielle disponible publiquement.
- [Sega Saturn IP.BIN (Initial Program) — Retro Reversing](https://www.retroreversing.com/sega-saturn-initial-program-ip)
- [Sega Saturn Hardware Architecture — Retro Reversing](https://www.retroreversing.com/saturn-architecture)
- [Sega Saturn File Formats — Retro Reversing](https://www.retroreversing.com/sega-saturn-file-formats)
- [About IP.BIN — SegaXtreme](https://segaxtreme.net/threads/about-ip-bin.6229/)
- [Saturn Boot Process — SegaXtreme](https://segaxtreme.net/threads/saturn-boot-process.13387/)
- [Notes on the Saturn BIOS — SegaXtreme](https://segaxtreme.net/threads/notes-on-the-saturn-bios.16704/)
- [Saturn System Library (External Specifications) — Sega Retro](https://segaretro.org/images/b/ba/External_Specifications_-_Saturn_File_System_Library.pdf) — PDF scanné, non exploitable en extraction de texte automatique lors de cette recherche.
- [Understanding Sega Saturn BIN/CUE Images: A Practical Guide — uglysketches](https://uglysketches.com/archives/31)
- [Sega Saturn conversion file formats — NightCurves.com](https://www.nightcurves.com/casual-gaming/2469/sega-saturn-conversion-file-formats/)
- [Foxy Editor (extraction/édition/réinjection de texte, romhacking.net)](https://www.romhacking.net/utilities/1849/)
- [PRS Utils — romhacking.net](https://www.romhacking.net/utilities/671/)
- [Sega Saturn Castlevania graphic compression — romhacking.net forum](https://www.romhacking.net/forum/index.php?topic=27989.0)
- [Compressed DAT files on many Saturn Games — SegaXtreme](https://segaxtreme.net/threads/compressed-dat-files-on-many-saturn-games.257/)
- [Translation issues: Hacking the Panzer Dragoon Saga font — SegaXtreme](https://segaxtreme.net/threads/translation-issues-hacking-the-panzer-dragoon-saga-font.16280/)
- [Cracking The Sega Saturn After 20 Years — Hackaday](https://hackaday.com/2016/07/11/cracking-the-sega-saturn-after-20-years/)
- [libyaul — SDK Saturn homebrew open source moderne (GitHub)](https://github.com/yaul-org/libyaul) — non consulté en détail lors de cette recherche (accès direct au dépôt indisponible), recommandé comme référence de vérification en Phase 5.2.

---

## Historique des révisions de ce document

| Date | Modification |
|---|---|
| 2026-09-24 | Rédaction initiale (Phase 5.1), à partir de recherches web et de connaissances générales. Offsets exacts de l'IP.BIN à confirmer avant la Phase 5.2 (voir § 3). |
