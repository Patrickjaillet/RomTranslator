# Icônes de RomTranslator

Toutes les icônes de l'interface sont des graphismes **vectoriels SVG**. Aucune image matricielle (PNG, JPEG, ICO...)
n'est utilisée pour un bouton, un menu ou un onglet.

## Source et licence

Les icônes sont des créations originales du projet, dessinées pour RomTranslator. Elles sont distribuées avec le
projet sous la même licence que le reste du code (GNU GPL v3, voir le fichier `LICENSE` à la racine). Aucune icône
tierce n'est incluse.

## Conventions de dessin

- grille de 24 × 24 unités (`viewBox="0 0 24 24"`) ;
- tracé de 2 unités, sans remplissage, extrémités et jonctions arrondies ;
- uniquement des éléments `<path>` (pas de `circle`, `rect`, `line`...), pour que la conversion en `Geometry` WPF
  se réduise à recopier l'attribut `d` ;
- nom de fichier en minuscules, mots séparés par des tirets : `file-plus.svg`.

## Utilisation dans l'application

Chaque fichier `nom.svg` correspond à :

- une ressource `Geometry` nommée `Icon.nom` dans `RomTranslator.App/Resources/Icons.xaml` ;
- une constante dans `RomTranslator.App/ViewModels/IconKeys.cs`.

Les tests automatiques vérifient que ces trois représentations restent identiques.
