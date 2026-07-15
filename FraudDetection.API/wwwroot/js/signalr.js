const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/alertes")
    .withAutomaticReconnect()
    .build();

connection.on("NouvelleAlerte", function (alerte) {
    const tbody = document.getElementById("corpsTableAlertes");
    if (!tbody) return;

    const ligne = document.createElement("tr");
    ligne.innerHTML = `
        <td>${alerte.statut}</td>
        <td>#${alerte.transactionId}</td>
        <td>${(alerte.scoreRisque * 100).toFixed(0)}%</td>
        <td>${alerte.raison ?? ""}</td>
        <td>${new Date(alerte.detecteLe).toLocaleString()}</td>
    `;

    // La nouvelle alerte apparaît en haut du tableau
    tbody.prepend(ligne);
});

connection.start()
    .then(() => console.log("✅ Connecté au hub des alertes"))
    .catch(err => console.error("❌ Erreur de connexion SignalR :", err));