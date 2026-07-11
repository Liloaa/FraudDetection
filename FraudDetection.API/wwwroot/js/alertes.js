// Connexion au hub SignalR + gestion des mises à jour en direct
// de la page Alertes (nouvelle alerte, changement de statut).
document.addEventListener("DOMContentLoaded", function () {
    const table = document.getElementById("alertes-tbody");
    const liveDot = document.getElementById("live-dot");
    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    if (!table || typeof signalR === "undefined") return;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/alertes")
        .withAutomaticReconnect()
        .build();

    connection.onreconnecting(() => liveDot?.classList.remove("on"));
    connection.onreconnected(() => liveDot?.classList.add("on"));

    connection.start()
        .then(() => liveDot?.classList.add("on"))
        .catch(err => console.error("Connexion SignalR échouée :", err));

    connection.on("NouvelleAlerte", function (alerte) {
        const row = document.createElement("tr");
        row.id = `alerte-${alerte.id}`;
        row.innerHTML = `
            <td class="mono">${new Date(alerte.detecteLe).toLocaleString("fr-FR")}</td>
            <td>#${alerte.transactionId}</td>
            <td class="mono">${(alerte.scoreRisque * 100).toFixed(0)}%</td>
            <td>${alerte.raison ?? ""}</td>
            <td><span class="badge ${alerte.statut}">${alerte.statut}</span></td>
            <td>${statutActionsHtml(alerte.id)}</td>
        `;
        table.prepend(row);
        attachHandlers(row);
    });

    connection.on("AlerteMiseAJour", function (data) {
        const row = document.getElementById(`alerte-${data.id}`);
        if (!row) return;
        const badge = row.querySelector(".badge");
        if (badge) {
            badge.className = `badge ${data.statut}`;
            badge.textContent = data.statut;
        }
    });

    function statutActionsHtml(id) {
        return `
            <button class="btn small normal" data-action="Confirme" data-id="${id}">Confirmer</button>
            <button class="btn small ghost" data-action="Rejete" data-id="${id}">Rejeter</button>
        `;
    }

    function attachHandlers(scope) {
        scope.querySelectorAll("button[data-action]").forEach(btn => {
            btn.addEventListener("click", async () => {
                const id = btn.dataset.id;
                const statut = btn.dataset.action;
                await fetch(`/Alertes?handler=ChangerStatut`, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded",
                        "RequestVerificationToken": antiForgeryToken || ""
                    },
                    body: `id=${id}&statut=${statut}`
                });
            });
        });
    }

    attachHandlers(table);
});