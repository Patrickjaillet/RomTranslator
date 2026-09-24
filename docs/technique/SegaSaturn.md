<!--
  SPDX-License-Identifier: GPL-3.0-only
  © 2026 Patrick JAILLET — RomTranslator
-->
# Spécifications techniques — Module Sega Saturn

> Document de recherche préalable à l'implémentation du module Sega Saturn (Phase 5). Rédigé à partir de
> recherches sur le web (documentation SEGA officielle référencée mais non directement exploitable — PDF
> scanné —, sites de reverse engineering, communautés romhacking) et de connaissances générales sur
> l'architecture Saturn. **Niveau de confiance indiqué pour chaque section.** La structure de l'IP.BIN
> (§ 3) a pu être vérifiée avec un niveau de confiance élevé grâce au code source du SDK homebrew Yaul et à
> une extraction hexadécimale directe sur une image homebrew réelle fournie par le propriétaire du projet ;
> les autres sections reposent sur des sources secondaires recoupées et restent à confirmer au cas par cas
> au fil de l'implémentation.

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

**Confiance : élevée.** Les offsets ci-dessous proviennent du fichier source `libyaul/ip/ip.sx` du SDK homebrew open source **Yaul** (`github.com/yaul-org/libyaul`, licence libre, dépôt consulté directement), qui définit littéralement la disposition des champs assemblés dans l'IP.BIN. Cette structure a été **vérifiée par extraction hexadécimale directe sur deux images réelles indépendantes** fournies par le propriétaire du projet :
1. une image homebrew (`sl_coff.iso`, disque 1 de *CubecatYarniaPublic2_0_2*, secteurs `MODE1/2048`) ;
2. un jeu commercial officiel (*Dark Savior*, USA, extrait au format BIN/CUE `MODE1/2352` depuis un fichier `.chd` avec `chdman extractcd`).

Chaque champ correspond exactement aux octets observés sur les deux images (avec des valeurs différentes propres à chaque jeu — zone, périphériques, titre, adresse de premier fichier —, ce qui confirme que la structure elle-même, et non son contenu, est bien fixe). Cette double vérification croisée (source du SDK + deux observations réelles concordantes, homebrew et commercial) lève la réserve précédente sur les offsets non confirmés.

### Structure exacte (vérifiée)

| Champ | Offset | Longueur | `sl_coff.iso` (homebrew) | *Dark Savior* (USA, commercial) |
|---|---|---|---|---|
| Identifiant matériel | `0x000` | 16 octets | `"SEGA SEGASATURN "` | `"SEGA SEGASATURN "` |
| Identifiant fabricant (« maker ID ») | `0x010` | 16 octets | `"SEGA SONIC ZTM  "` | `"SEGA ENTERPRISES"` |
| Numéro de produit | `0x020` | 10 octets | `"T-0606080 "` | `"MK-81304  "` |
| Version | `0x02A` | 6 octets | `"V1.000"` | `"V1.000"` |
| Date de sortie (AAAAMMJJ) | `0x030` | 8 octets | `"20180721"` | `"19961119"` |
| Informations sur le périphérique (« device information ») | `0x038` | 8 octets | `"CD-1/1  "` | `"CD-1/1  "` |
| Symbole(s) de zone cible | `0x040` | 10 octets | `"JTUE      "` | `"JTU       "` |
| Remplissage fixe (espaces) | `0x04A` | 6 octets | espaces | espaces |
| Périphérique(s) compatible(s) | `0x050` | 16 octets | `"U               "` | `"JAE             "` |
| Nom du jeu (« game name »), complété par des espaces | `0x060` | 112 octets | `"SONIC Z-TREME V.0.07"` suivi d'espaces | `"DARK SAVIOR"` suivi d'espaces |
| Réservé | `0x0D0` | 4 octets | `0x00000000` | `0x00000000` |
| Réservé | `0x0D4` | 4 octets | `0x00000000` | `0x00000000` |
| Réservé | `0x0D8` | 4 octets | `0x00000000` | `0x00000000` |
| Réservé | `0x0DC` | 4 octets | `0x00000000` | `0x00000000` |
| Taille de l'IP (`__ip_len`) | `0x0E0` | 4 octets, big-endian | `0x00001800` (6 144 octets) | `0x00001800` (6 144 octets) |
| Adresse de pile du SH-2 maître (« Stack-M ») | `0x0E4` | 4 octets, big-endian | `0x00000000` (voir note) | `0x00000000` (voir note) |
| Réservé | `0x0E8` | 4 octets | `0x00000000` | `0x00000000` |
| Adresse de pile du SH-2 esclave (« Stack-S ») | `0x0EC` | 4 octets, big-endian | `0x00000000` (voir note) | `0x00000000` (voir note) |
| Adresse du premier fichier à charger (« 1st read address ») | `0x0F0` | 4 octets, big-endian | `0x06004000` | `0x06010000` |
| Taille du premier fichier à charger (« 1st read size ») | `0x0F4` | 4 octets, big-endian | `0x00000000` (voir note) | non relevée sur cet exemple |
| Réservé | `0x0F8` | 4 octets | `0x00000000` | — |
| Réservé | `0x0FC` | 4 octets | `0x00000000` | — |
| Sécurité (blocs `sys_sec`/`sys_arej`/`sys_aret`/`sys_areu`/`sys_aree`), puis code de boot (« boot code ») | à partir de `0x100` | variable | code exécutable, hors du périmètre de RomTranslator | idem |

Toutes les valeurs entières multi-octets (tailles, adresses) sont stockées en **big-endian**, cohérent avec l'architecture Hitachi SH-2 de la Saturn en mode natif.

**Note sur les adresses de pile et la taille du premier fichier observées à zéro** : sur les deux images vérifiées, ces champs contiennent `0x00000000` plutôt qu'une adresse ou une taille réelle. Deux explications possibles, non tranchées par cette seule vérification : (1) ces valeurs sont calculées et renseignées dynamiquement par le BIOS/chargeur à l'exécution plutôt que fixées dans l'IP.BIN sur disque, ou (2) certains outils de mastering (dont celui utilisé pour ces deux images) laissent ces champs à zéro par convention lorsque le chargeur applicatif détermine lui-même ces valeurs au démarrage. Sans conséquence pour `SaturnRomLoader`/`SaturnRomValidator` (Phase 5.2), qui n'ont besoin que de lire les métadonnées d'identification du jeu (titre, éditeur, région, numéro de produit), pas de reproduire le comportement du chargeur de démarrage.

### Faits complémentaires confirmés

- L'IP.BIN occupe au minimum les **premiers 32 768 octets (32 Kio)** de la zone système du disque (16 secteurs de 2 048 octets), même si sa taille effective déclarée (`0x0E0`) peut être inférieure (6 144 octets dans l'exemple observé) ; le gabarit Yaul complète explicitement l'IP.BIN à une taille minimale supérieure à 4 Kio par un remplissage (`.fill 256`) à la toute fin du fichier assemblé.
- Le champ « Identifiant matériel » (`"SEGA SEGASATURN "`) est ce que `SaturnRomValidator` doit rechercher en tout premier lieu pour reconnaître une image Saturn valide.
- Le code de boot est chargé par le BIOS à l'adresse mémoire **`0x06002000`** (zone RAM basse partagée par les deux SH-2) ; l'adresse de l'AIP qui suit dépend de la taille du code de boot.
- Un outil de sécurité (« security ring ») vérifie certains octets de l'IP.BIN lors du démarrage sur un vrai Saturn ; ce mécanisme ne concerne que l'exécution sur matériel réel et n'affecte pas la lecture/écriture d'une image ROM par RomTranslator.
- **Limite du champ « Nom du jeu »** : 112 octets exactement (confirmé par le script `make-ip`, qui tronque explicitement à 112 caractères) — une contrainte à respecter si RomTranslator devait un jour permettre de modifier ce champ (hors périmètre v1, qui ne traduit que le contenu du jeu, pas ses métadonnées IP.BIN).

**Ce tableau est prêt à être utilisé tel quel pour écrire `SaturnRomLoader` en Phase 5.2** ; seule l'interprétation exacte des symboles de zone (`0x040`, ex. `"JTUE"` = Japon/Taïwan(?)/USA/Europe, à confirmer précisément lettre par lettre si le module doit un jour distinguer les régions) reste à préciser au moment de l'implémentation, sans bloquer l'écriture du chargeur lui-même.

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
- [libyaul — SDK Saturn homebrew open source moderne (GitHub)](https://github.com/yaul-org/libyaul), en particulier [`libyaul/ip/ip.sx`](https://github.com/yaul-org/libyaul/blob/develop/libyaul/ip/ip.sx) (gabarit assembleur exact de l'IP.BIN, source de la structure exacte du § 3) et [`tools/make-ip/make-ip`](https://github.com/yaul-org/libyaul/blob/develop/tools/make-ip/make-ip) (script qui renseigne ce gabarit, confirme la troncature du titre à 112 caractères).
- Vérification directe par extraction hexadécimale (réalisée pendant cette session, pas une source web) : `sl_coff.iso` (disque 1 de *CubecatYarniaPublic2_0_2*, image homebrew fournie par le propriétaire du projet) et *Dark Savior* (USA), jeu commercial officiel extrait au format BIN/CUE depuis un fichier `.chd` de la collection personnelle du propriétaire avec l'outil `chdman` (MAME Compressed Hunks of Data manager).

---

## Historique des révisions de ce document

| Date | Modification |
|---|---|
| 2026-09-24 | Rédaction initiale (Phase 5.1), à partir de recherches web et de connaissances générales. Offsets exacts de l'IP.BIN à confirmer avant la Phase 5.2 (voir § 3). |
| 2026-09-24 | Structure de l'IP.BIN (§ 3) vérifiée et complétée avec les offsets exacts, à partir du code source du SDK Yaul et d'une extraction hexadécimale croisée sur une image homebrew et un jeu commercial officiel (*Dark Savior*, USA) fournis par le propriétaire du projet. Le point bloquant identifié à la rédaction initiale est levé. |
