# RomTranslator

**RomTranslator** est un logiciel Windows portable dédié à la traduction de ROMs de jeux vidéo dans la langue de votre choix. L'application est organisée en onglets, chaque onglet correspondant à une console prise en charge : extraction du texte du jeu, édition de la traduction, puis réinjection dans une image de ROM prête à l'emploi.

Le premier module disponible cible la **Sega Saturn**. D'autres consoles seront ajoutées progressivement, sur la base du même socle applicatif.

---

## Fonctionnalités principales

- Interface en français et en anglais
- Thème clair, interface WPF moderne
- Gestion de projets de traduction (création, sauvegarde, export/import)
- Éditeur de table de caractères (correspondance octet ↔ caractère, y compris séquences DTE/MTE)
- Extraction automatique et manuelle de blocs de texte
- Éditeur de traductions avec suivi de statut, glossaire et contrôle de longueur
- Réinjection du texte traduit avec relocalisation des pointeurs
- Génération d'une image de ROM traduite ou d'un patch IPS

## Statut du projet

RomTranslator est publié et fonctionnel, avec le module de traduction Sega Saturn complet. Consultez les [notes de version](CHANGELOG.md) pour le détail de chaque version.

## Installation

RomTranslator est **100 % portable** : aucun installeur n'est nécessaire.

1. Téléchargez la dernière archive de release depuis la page [Releases](https://github.com/patrickjaillet/RomTranslator/releases)
2. Décompressez l'archive dans le dossier de votre choix
3. Lancez `RomTranslator.exe`

Aucune donnée n'est écrite en dehors du dossier de l'application (pas de registre Windows, pas d'`%APPDATA%`).

## Configuration requise

- Windows 10 ou Windows 11 (64 bits)
- Aucun prérequis supplémentaire : la distribution portable inclut le runtime .NET nécessaire (self-contained)

## Utilisation

1. Lancez l'application et sélectionnez l'onglet correspondant à la console de votre jeu
2. Créez un nouveau projet et chargez votre image de ROM
3. Configurez ou sélectionnez une table de caractères adaptée au jeu
4. Extrayez le texte, traduisez les entrées dans l'éditeur intégré
5. Générez la ROM traduite ou le patch correspondant

## Licence

RomTranslator est distribué sous licence **GNU General Public License v3.0 (GPLv3)**. Voir le fichier [LICENSE](LICENSE) pour le texte complet.

## Avertissement légal

RomTranslator est un outil de traduction. Il ne fournit, n'héberge et ne distribue aucune ROM de jeu commercial. L'utilisateur est seul responsable du respect des droits d'auteur applicables aux fichiers qu'il traite avec ce logiciel.

## Contact

- Site officiel : https://patrickjaillet.github.io/RomTranslator
- E-mail : sandefjord.development@proton.me

---

© 2026 Patrick JAILLET
