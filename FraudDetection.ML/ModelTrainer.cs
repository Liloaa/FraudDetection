using Microsoft.ML;
using Microsoft.ML.Trainers.FastTree;

namespace FraudDetection.ML
{
    internal class ModelTrainer
    {
        // Cette méthode va tout faire : charger, entraîner, évaluer, sauvegarder.
        // On la marque "static" pour pouvoir l'appeler facilement depuis Program.cs
        // sans avoir à créer un objet ModelTrainer avant.
        public static void TrainAndSaveModel()
        {
            // MLContext est le point d'entrée OBLIGATOIRE de toute application ML.NET.
            // C'est un peu comme le "DbContext" que vous utilisez côté backend avec
            // Entity Framework : un objet central qui donne accès à tous les outils.
            // Le "seed: 0" fixe le générateur aléatoire interne, pour que l'entraînement
            // donne TOUJOURS le même résultat à chaque exécution (utile pour tester
            // et comparer, sinon les résultats varieraient légèrement à chaque fois).
            var mlContext = new MLContext(seed: 0);

            // On charge le fichier CSV et on le transforme en "IDataView" : c'est le
            // format interne que ML.NET utilise pour manipuler des données, un peu
            // comme un DataTable ou une liste, mais optimisé pour le Machine Learning.
            // <TransactionData> dit à ML.NET : "utilise le moule qu'on a créé avant
            // pour comprendre les colonnes de ce fichier".
            IDataView dataView = mlContext.Data.LoadFromTextFile<TransactionData>(
                path: "Data/transactions_dataset.csv",
                hasHeader: true,      // la 1ère ligne est un en-tête, pas une donnée
                separatorChar: ','    // le fichier est séparé par des virgules
            );

            // ON NE DONNE PAS TOUTES LES DONNÉES POUR L'ENTRAÎNEMENT.
            // On en garde une partie de côté (ici 20%) pour TESTER le modèle après,
            // sur des exemples qu'il n'a JAMAIS vus pendant l'apprentissage.
            // Pourquoi c'est indispensable : sinon on ne pourrait pas savoir si le
            // modèle a vraiment "compris" les motifs, ou s'il a juste "mémorisé"
            // par cœur les 200 exemples sans savoir généraliser à de nouveaux cas.
            // C'est exactement comme réviser un examen avec des annales (train),
            // puis passer un VRAI examen avec des questions différentes (test).
            var split = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            // ----- CONSTRUCTION DU PIPELINE (la chaîne dont on a parlé) -----

            var pipeline = mlContext.Transforms
                // Étape 1 : on rassemble nos 4 colonnes dans une seule colonne
                // technique appelée "Features" - c'est une exigence de ML.NET,
                // tous les algorithmes attendent une colonne "Features" unique.
                .Concatenate("Features",
                    nameof(TransactionData.Montant),
                    nameof(TransactionData.HeureTransaction),
                    nameof(TransactionData.FrequenceTransactions24h),
                    nameof(TransactionData.EcartMontantMoyen))
                // Étape 2 : on choisit l'algorithme FastTree, qu'on "ajoute" à la
                // suite de l'étape 1. FastTree est un algorithme de type "arbre de
                // décision" : il apprend une série de questions/réponses successives
                // (ex: "montant > 500000 ? -> heure < 5h ? -> fréquence > 8 ? -> fraude")
                // C'est fiable, rapide, et ça marche bien même avec peu de données,
                // contrairement à d'autres algorithmes qui ont besoin de millions
                // d'exemples pour être efficaces.
                .Append(mlContext.BinaryClassification.Trainers.FastTree(
                    new Microsoft.ML.Trainers.FastTree.FastTreeBinaryTrainer.Options
                    {
                        LabelColumnName = "Label",
                        FeatureColumnName = "Features",

                        // On limite le nombre d'arbres construits (par défaut c'est 100).
                        // Moins d'arbres = le modèle apprend les tendances GENERALES,
                        // pas les détails/exceptions de chaque ligne individuelle.
                        NumberOfTrees = 25,

                        // On limite la complexité de chaque arbre (moins de "branches").
                        // Un arbre trop détaillé peut créer une règle ultra-spécifique
                        // pour un seul cas, ce qui cause la sur-confiance qu'on a vue.
                        NumberOfLeaves = 8,

                        // On exige qu'au moins 10 exemples suivent la même branche avant
                        // de la considérer valide. Ça évite de créer des règles basées
                        // sur 1 ou 2 cas isolés (bruit) plutôt que sur une vraie tendance.
                        MinimumExampleCountPerLeaf = 10
                    }
                ));

            // ----- ENTRAÎNEMENT -----

            // "Fit" veut dire "ajuste-toi à ces données" = c'est LE moment où le
            // modèle apprend réellement. On lui donne uniquement le TrainSet
            // (80% des données), jamais le TestSet à cette étape.
            var model = pipeline.Fit(split.TrainSet);

            // ----- ÉVALUATION -----

            // On demande au modèle fraîchement entraîné de prédire sur le TestSet
            // (les 20% qu'il n'a jamais vus), pour voir s'il se débrouille bien.
            var predictions = model.Transform(split.TestSet);

            // ML.NET calcule automatiquement plusieurs métriques de qualité.
            var metrics = mlContext.BinaryClassification.Evaluate(predictions);

            Console.WriteLine("=== Résultats de l'évaluation ===");
            Console.WriteLine($"Précision (Accuracy)   : {metrics.Accuracy:P2}");
            Console.WriteLine($"Rappel (Recall)         : {metrics.PositiveRecall:P2}");
            Console.WriteLine($"F1 Score                : {metrics.F1Score:P2}");

            // ----- SAUVEGARDE DU MODÈLE -----

            // On sauvegarde le modèle entraîné dans un fichier .zip, pour pouvoir
            // le réutiliser plus tard SANS avoir à ré-entraîner à chaque fois
            // (l'entraînement peut prendre du temps, on ne veut le faire qu'une fois).
            // "dataView.Schema" dit au fichier .zip "voici la structure des données
            // que ce modèle attend en entrée" - nécessaire pour le recharger plus tard.
            mlContext.Model.Save(model, dataView.Schema, "fraud_model.zip");

            Console.WriteLine("\nModèle sauvegardé avec succès sous fraud_model.zip");
        }
    }
}