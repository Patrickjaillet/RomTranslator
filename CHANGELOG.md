# Notes de version

Toutes les modifications notables de RomTranslator sont documentées dans ce fichier.

Le format s'inspire de [Keep a Changelog](https://keepachangelog.com/fr/1.1.0/), et le projet suit le [versionnage sémantique](https://semver.org/lang/fr/) (SemVer).

## [1.0.2] - 2026-09-24

Première version publique. Socle applicatif et module de traduction Sega Saturn complets.

### Ajouté

- Socle applicatif WPF portable (Windows 10/11, .NET 10), thème clair, interface en français et en anglais.
- Gestion de projets de traduction : création, ouverture, enregistrement, export/import, sauvegarde incrémentale automatique.
- Éditeur de table de caractères : correspondance octet ↔ caractère, y compris séquences compressées (DTE/MTE).
- Extraction automatique (balayage heuristique) ou manuelle (plage d'adresses) du texte d'une ROM.
- Éditeur de traduction : suivi de statut par entrée, glossaire avec détection d'incohérences, recherche/remplacement globale, aperçu par table de caractères, contrôle de longueur en direct, annulation/rétablissement.
- Réinjection du texte traduit avec relocalisation automatique des pointeurs lorsqu'une traduction dépasse la longueur du texte source.
- Génération d'une image ROM traduite complète ou d'un patch IPS.
- Outils transverses : visualiseur hexadécimal, comparaison binaire entre deux images, journal d'activité, écran de paramètres (langue, dossiers portables).
- **Module Sega Saturn** (premier module console) : chargement et validation d'image disque (BIN/CUE, lecture de l'en-tête IP.BIN), détection du jeu, tables de caractères prédéfinies (ASCII, Shift-JIS hiragana/katakana), extraction, édition et réinjection complètes intégrées dans un onglet dédié, assistant de création de projet en trois étapes.
- Intégration continue (GitHub Actions, build et tests sur Windows).

### Sécurité et confidentialité

- 100 % portable : aucun installeur, aucune écriture en dehors du dossier de l'application (ni registre, ni `%APPDATA%`), vérifié par des tests automatiques.
- Aucune ROM, y compris homebrew, n'est fournie, hébergée ou distribuée avec le logiciel.

### Limitations connues

- Le balayage automatique du texte ne couvre que les tables de caractères fournies (ASCII, Shift-JIS hiragana/katakana) ou une table `.tbl` externe créée manuellement ; les jeux utilisant des kanji ou un encodage propriétaire non couvert nécessitent une table dédiée.
- Le relogement des traductions trop longues pour tenir à la place du texte source dépend d'une zone de réserve et d'une table de pointeurs désignées manuellement (aucune détection automatique).
- Formats disque pris en charge : BIN/CUE, secteurs Mode 1 (cuits ou bruts) uniquement ; le Mode 2 Form 2 n'est pas géré.
- Seul le format de patch IPS est proposé (pas de xdelta).
