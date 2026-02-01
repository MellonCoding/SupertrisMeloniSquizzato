\# Super Tris - Gioco del Tris Avanzato



\## Descrizione del Progetto



Super Tris è una versione avanzata del classico gioco del tris. Invece di una singola griglia 3x3, il gioco si svolge su 9 griglie 3x3 disposte a formare una griglia più grande 3x3.



\### Come si Gioca



1\. Il gioco si svolge su 9 mini-tris disposti in una griglia 3x3

2\. Ogni mossa che fai determina in quale mini-tris deve giocare il tuo avversario

3\. Se giochi nella cella in alto a sinistra del tuo mini-tris, l'avversario deve giocare nel mini-tris in alto a sinistra

4\. Se un mini-tris è già stato vinto o è pieno, l'avversario può giocare ovunque

5\. Vince chi completa 3 mini-tris in fila (orizzontale, verticale o diagonale)



\## Modalità di Gioco



\### 1. Player vs Player (PVP)

Due giocatori umani si sfidano sullo stesso computer. I giocatori si alternano facendo mosse.



\### 2. Player vs Bot (PVE)

Un giocatore umano (X) affronta un bot intelligente (O). Il bot usa algoritmi avanzati per scegliere le mosse migliori.



\### 3. Bot vs Bot Distribuito (EvE)

Due istanze separate del gioco comunicano tramite un file condiviso. Ogni istanza controlla un bot che gioca contro l'altro bot.



\#### Come Usare la Modalità EvE:



1\. Apri la prima istanza del gioco

2\. Seleziona "Bot vs Bot"

3\. Scegli "Giocatore 1" (farà la prima mossa come X)

4\. Seleziona il file di comunicazione quando richiesto (crea un nuovo file .txt vuoto)

5\. Apri la seconda istanza del gioco

6\. Seleziona "Bot vs Bot"

7\. Scegli "Giocatore 2" (aspetterà la prima mossa come O)

8\. Seleziona lo stesso file usato dalla prima istanza

9\. I bot inizieranno a giocare automaticamente



Importante: Il file di comunicazione viene svuotato all'inizio di ogni partita.



\## Tipi di Bot Disponibili



\### 1. Albero Pesato (Reinforcement Learning)

Questo bot impara giocando partite. Ogni volta che vince, aumenta il "peso" delle mosse che ha fatto. Quando perde, diminuisce questi pesi. Con il tempo, impara quali mosse portano alla vittoria.



Caratteristiche:

\- Migliora giocando molte partite

\- Salva le conoscenze su file

\- Può essere allenato tramite la modalità Training



\### 2. Bot Algoritmico (Minimax)

Questo bot usa l'algoritmo Minimax per valutare le mosse. Calcola le conseguenze di ogni mossa possibile guardando alcuni turni nel futuro.



Caratteristiche:

\- Non impara, usa sempre la stessa strategia

\- Molto forte fin dall'inizio

\- Più lento dell'Albero Pesato



\## Sistema di Allenamento



La modalità Allenamento permette di far giocare il bot Albero Pesato contro il bot Algoritmico per migliorare le sue prestazioni.



\### Come Usare l'Allenamento:



1\. Apri la schermata di selezione

2\. Clicca su "Allenamento"

3\. Imposta il numero di partite da giocare (consigliato: 1000-5000)

4\. Clicca "Avvia Allenamento"

5\. Aspetta che l'allenamento finisca

6\. Clicca "Salva Pesi" per salvare i miglioramenti



\### Statistiche Disponibili:



\- Partite giocate

\- Vittorie, Sconfitte, Pareggi

\- Percentuale di vittorie

\- Stati appresi (quante situazioni di gioco diverse ha visto)

\- Log delle operazioni con timestamp



\## Struttura del Progetto



\### Classi Principali



\#### Tris

Gestisce un singolo mini-tris 3x3. Controlla le mosse valide, verifica le vittorie e tiene traccia del vincitore.



\#### Supertris

Gestisce la griglia completa di 9 mini-tris. Coordina le vittorie dei singoli tris e determina il vincitore finale.



\#### GameManager

Coordina il flusso del gioco. Gestisce i turni, valida le mosse e determina quale mini-tris è obbligatorio.



\### Bot e Intelligenza Artificiale



\#### IBot (Interfaccia)

Definisce i metodi che ogni bot deve implementare:

\- CalcolaMossa: decide quale mossa fare

\- NotificaRisultatoPartita: riceve il risultato per imparare

\- ResetPartita: pulisce lo stato per una nuova partita



\#### AlberoPesato

Bot che impara tramite reinforcement learning. Mantiene una tabella di stati e pesi che vengono aggiornati in base ai risultati.



\#### HeuristicBot

Bot che usa regole strategiche predefinite. Valuta ogni mossa con un punteggio basato su:

\- Posizione (centro è meglio degli angoli, angoli meglio dei lati)

\- Tattica (vince o blocca vittorie?)

\- Strategia (dove manda l'avversario?)



\#### MinimaxBot

Bot che usa l'algoritmo Minimax con potatura alpha-beta. Esplora l'albero delle mosse possibili fino a una certa profondità.



\#### Trainer

Sistema che fa giocare due bot l'uno contro l'altro per allenare il bot che impara.



\### Sistema di Comunicazione (Modalità EvE)



\#### FileManager

Gestisce la scrittura delle mosse su file. Svuota il file all'inizio e appende ogni mossa.



\#### FileWatcher

Monitora il file delle mosse e notifica quando l'avversario ha giocato. Usa FileSystemWatcher per rilevare modifiche.



\### Interfacce Utente



\#### SelectionForm

Menu principale dove si sceglie la modalità di gioco e il tipo di bot.



\#### GameForm

Schermata di gioco principale con la griglia 3x3 di mini-tris. Gestisce i click, aggiorna la visualizzazione e coordina i turni.



\#### TrainingForm

Interfaccia per allenare il bot Albero Pesato. Mostra statistiche in tempo reale e permette di salvare/caricare i pesi.



\#### EvESetupDialog

Dialog per scegliere se essere Giocatore 1 o 2 nella modalità Bot vs Bot distribuito.



\### Helper



\#### ColorManager

Gestisce i colori del tema scuro usato nell'interfaccia.



\## Formato File di Comunicazione



Il file usato per la modalità EvE contiene una mossa per riga nel formato:



```

TURNO NUMERO\_TRIS POSIZIONE

```



Esempio:

```

X 4 5

O 5 3

X 3 7

```



Dove:

\- TURNO: 'X' o 'O'

\- NUMERO\_TRIS: da 0 a 8 (0=alto-sinistra, 8=basso-destra)

\- POSIZIONE: da 0 a 8 (0=alto-sinistra, 8=basso-destra all'interno del mini-tris)



\## Ottimizzazioni Implementate



\### Mosse Libere

Quando il giocatore può scegliere liberamente dove giocare, invece di valutare tutte le 81 caselle, i bot:

1\. Trovano la mossa migliore in ciascuno dei 9 mini-tris

2\. Confrontano le 9 mosse migliori

3\. Scelgono la migliore in assoluto



Questo riduce le valutazioni da circa 81 a circa 45, raddoppiando la velocità.



\## Requisiti Tecnici



\- .NET 9.0 Windows Forms

\- Sistema operativo Windows

\- Visual Studio 2022 o superiore (per compilare)



\## Come Compilare



1\. Apri la solution SupertrisMeloniSquizzato.sln in Visual Studio

2\. Seleziona Build > Build Solution

3\. L'eseguibile sarà in bin/Debug/net9.0-windows/



\## Come Eseguire



1\. Compila il progetto o usa l'eseguibile precompilato

2\. Avvia SupertrisMeloniSquizzato.exe

3\. Scegli la modalità di gioco desiderata

4\. Segui le istruzioni a schermo



\## Risoluzione Problemi



\### Il bot non fa mosse quando può giocare liberamente

Verifica che il file HeuristicBot.cs sia aggiornato con il fix per il conflitto di variabili nel loop.



\### Errore in modalità EvE: "Mossa invalida dell'avversario"

Assicurati che entrambe le istanze stiano usando lo stesso file e che il file non venga modificato manualmente.



\### Il bot Albero Pesato perde sempre

Il bot deve essere allenato prima di giocare bene. Usa la modalità Allenamento per farlo migliorare.



\### Prestazioni lente

Riduci la profondità di ricerca del MinimaxBot o usa il bot Albero Pesato che è più veloce.



\## Autori



Progetto sviluppato come parte di un corso di programmazione orientata agli oggetti.



\## Licenza



Progetto educativo open source.

