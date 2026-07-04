# FraudDetection.ML — Détection de fraude bancaire par Machine Learning

## Objectif

Ce module entraîne un modèle de Machine Learning capable de prédire si une transaction
bancaire est frauduleuse ou non, à partir de 4 caractéristiques (features) simples.

## Technologie utilisée

- **Langage** : C# (.NET 8)
- **Bibliothèque ML** : ML.NET (Microsoft.ML 5.0.0)
- **Algorithme** : FastTree (arbre de décision boosté), pour une classification binaire
- **Package additionnel** : Microsoft.ML.FastTree

## Structure des fichiers

| Fichier | Rôle |
|---|---|
| `TransactionData.cs` | Décrit une transaction en entrée (les features + le label à prédire) |
| `FraudPrediction.cs` | Décrit la réponse du modèle (prédiction, probabilité, score) |
| `Data/transactions_dataset.csv` | Jeu de données d'entraînement (320 transactions simulées) |
| `ModelTrainer.cs` | Pipeline complet : chargement, split train/test, entraînement, évaluation, sauvegarde |
| `Program.cs` | Point d'entrée : lance l'entraînement puis teste le modèle sur des cas manuels |
| `fraud_model.zip` | Modèle entraîné, prêt à être chargé par l'API (généré automatiquement) |

## Features utilisées (colonnes du dataset)

1. **Montant** — montant de la transaction
2. **HeureTransaction** — heure de la journée (0-23.9), les fraudes sont souvent nocturnes
3. **FrequenceTransactions24h** — nombre de transactions du client dans les 24h précédentes
4. **EcartMontantMoyen** — écart entre cette transaction et la moyenne habituelle du client
5. **EstFraude** (Label) — la réponse à prédire (0 = normale, 1 = fraude)

*Non inclus par manque de temps : type de transaction (dépôt/retrait/virement/paiement),
localisation géographique. Pistes d'amélioration futures.*

## Génération du jeu de données

Les données sont **simulées** (pas de vraies transactions bancaires disponibles), avec :
- ~170-200 transactions normales (montants variés, heures diverses, faible fréquence)
- ~95-120 transactions frauduleuses (montants plus élevés, fréquence élevée, gros écarts)
- Une zone de chevauchement volontaire entre les deux catégories, pour éviter un problème
  trop simple à résoudre (un premier essai avec des catégories trop séparées donnait
  100% d'Accuracy/Recall, ce qui n'est pas réaliste ni crédible)

## Résultats obtenus

| Métrique | Valeur | Interprétation |
|---|---|---|
| Accuracy | 93.51% | Taux de bonnes réponses global |
| Recall (rappel) | 93.10% | % des vraies fraudes correctement détectées — métrique la plus importante pour une banque, car rater une fraude coûte plus cher qu'une fausse alerte |
| F1 Score | 91.53% | Moyenne équilibrée entre précision et rappel |

## Réglages du modèle (régularisation)

Un premier essai avec FastTree en configuration par défaut (100 arbres) donnait des
probabilités extrêmes (0% ou 100% systématiquement), signe de sur-apprentissage
(overfitting) sur un petit jeu de données de 320 lignes. On a réduit la complexité :

```csharp
NumberOfTrees = 25,              // au lieu de 100 par défaut
NumberOfLeaves = 8,
MinimumExampleCountPerLeaf = 10
```

Résultat : de meilleures métriques globales ET des probabilités plus nuancées
(ex: 0.3% à 99.5% au lieu de 0%/100% strict).

## Limites connues (à mentionner dans le rapport)

1. **Calibration encore perfectible** : même après régularisation, le modèle reste assez
   confiant sur la plupart des cas testés. Une piste d'amélioration future serait de
   tester d'autres algorithmes (régression logistique, LightGbm) ou d'augmenter le
   volume de données pour une calibration plus fine des probabilités.
2. **Dataset simulé** : les données sont générées artificiellement, pas issues de vraies
   transactions. Un vrai déploiement nécessiterait des données réelles anonymisées.
3. **Features limitées** : le type de transaction et la localisation ne sont pas pris
   en compte, par manque de temps (délai d'une semaine pour le projet).
4. **Pas de gestion du déséquilibre extrême** : dans la vraie vie, la fraude représente
   souvent <1% des transactions. Notre dataset a un ratio volontairement plus équilibré
   (~35% de fraude) pour faciliter l'entraînement avec peu de données.

## Comment utiliser le modèle dans l'API

Voir `FraudDetection.API/Services/FraudDetectionService.cs` : charge `fraud_model.zip`
et expose une méthode `AnalyserTransaction(montant, heure, frequence, ecart)` qui
retourne un objet `FraudPrediction` (EstFraudePredite, Probability, Score).

**Important** : `fraud_model.zip` doit être copié dans le dossier de l'API avec la
propriété "Copy to Output Directory" activée, sinon l'API ne le trouvera pas au démarrage.
