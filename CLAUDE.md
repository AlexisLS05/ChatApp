# Méthode (ChatApp)
1. Lire le repo + docs/Project.pdf (sujet) avant toute modif.
2. Un seul plan global (≤ 15 lignes) validé par l'utilisateur, puis exécution sans autre validation.
3. Branche feat/finish-chat, un commit par fonctionnalité, `dotnet build` vert avant chaque commit.
4. Push + PR via gh (sinon push seul). Ne jamais merger.
5. Fin : docs/STATUS.md (fait / pas fait / risques). Pas de projet de tests ni de CI.
Contraintes : threads (pas async), broadcast par delegate/event, protocole texte 1 ligne = 1 message,
parsing des commandes côté serveur, Dictionary<string, ClientHandler> sous lock privé,
pas de Thread.Abort ni BinaryFormatter, commentaires courts en français.
Coupe : 00h30 → commit de ce qui compile ; /joke seulement si 1–5 commités avant 00h15.
