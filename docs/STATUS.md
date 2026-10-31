# STATUS — branche feat/finish-chat (PR #1)

## Fait
1. Login : pseudo unique (insensible à la casse, 1–20 caractères, sans espace ni '/'), redemande si pris ; messages `[HH:mm] pseudo : texte` horodatés côté serveur ; annonces d'arrivée et de départ ; `try/finally` dans `ClientHandler.Run()` (désabonnement, retrait du Dictionary, fermeture).
2. Commandes côté serveur : `/msg`, `/users`, `/name`, `/quit`, `/help`.
3. `Logger` abonné à 4 événements du `Server` (`ClientConnected`, `ClientDisconnected`, `MessageReceived`, `ErrorOccurred`), console + `server.log` sous lock.
4. Start/stop : thread Accept dédié, `stop` dans la console serveur (copie sous lock, fermeture hors lock, attente de la fin des handlers).
5. Client robuste : serveur absent, coupure détectée par le thread de lecture (`Environment.Exit`), `/quit` propre.
6. `/joke` (HttpClient statique, appel synchrone, System.Text.Json).
- Tests manuels scriptés (sockets) : login/doublon, broadcast, /msg (+ inconnu), /users, /name (+ doublon), /help, /quit, reset brutal, stop, client sans serveur, /joke.
- docs/report.html, make_zip.ps1.

## Pas fait
- Captures d'écran (docs/screenshots/) et docs/report.pdf : à faire par toi.
- Nom du binôme dans le zip (`NOMBINOME`) et dans le rapport : à renseigner.
- Pas de projet de tests ni de CI (volontaire).

## Risques ouverts
- `/joke` : l'API renvoie des blagues non filtrées (parfois vulgaires/politiques). Vérifier avant capture et démo.
- `/joke` dépend du réseau ; hors ligne → « Service de blagues indisponible » (géré).
- Diagrammes Mermaid chargés par CDN : il faut Internet à l'ouverture de report.html, et attendre le rendu avant Ctrl+P.
- Un client connecté mais pas encore loggé n'est pas dans le Dictionary : à l'arrêt il n'est pas prévenu, sa socket est fermée à la sortie du processus.
- Pendant le `finally`, le pseudo reste réservé jusqu'au `Unregister` (quelques ms) : un relogin immédiat avec le même pseudo peut être refusé une fois.
- `server.log` est créé dans le dossier courant au lancement (lancer depuis la racine ChatApp).
- Affichage des accents : dépend de la page de code de la console Windows (OK en Windows Terminal / VS Code).
