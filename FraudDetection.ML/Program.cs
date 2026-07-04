using FraudDetection.ML;

// C'est ici que tout démarre. Un programme console C# exécute le code
// de ce fichier du haut vers le bas, dès qu'on le lance.

Console.WriteLine("=== Démarrage de l'entraînement du modèle de détection de fraude ===\n");

// On appelle simplement la méthode qu'on a écrite dans ModelTrainer.cs.
// C'est tout le travail (charger, entraîner, évaluer, sauvegarder) qui
// se déclenche à partir de cette seule ligne.
ModelTrainer.TrainAndSaveModel();

Console.WriteLine("\n=== Terminé ===");