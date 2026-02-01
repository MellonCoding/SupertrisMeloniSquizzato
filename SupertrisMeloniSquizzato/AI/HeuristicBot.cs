namespace SupertrisMeloniSquizzato.AI
{
    /// <summary>
    /// Bot che usa euristica (regole strategiche) per scegliere le mosse migliori
    /// Non usa alberi di ricerca, ma valuta ogni mossa con un punteggio
    /// </summary>
    internal class HeuristicBot : IBot
    {
        private Random random;

        // Pesi per la valutazione delle mosse
        private const int PESO_VINCITA_GIOCO = 10000;           // Vittoria immediata del Super Tris
        private const int PESO_BLOCCO_VITTORIA_GIOCO = 9000;    // Blocca vittoria avversario nel Super Tris
        private const int PESO_VINCITA_TRIS = 500;              // Vince un mini-tris
        private const int PESO_BLOCCO_VITTORIA_TRIS = 400;      // Blocca vittoria avversario in mini-tris
        private const int PESO_DOPPIA_MINACCIA_TRIS = 300;      // Crea due possibili vittorie in un mini-tris
        private const int PESO_CENTRO_CENTRALE = 50;            // Centro del tris centrale (4,4)
        private const int PESO_CENTRO_TRIS = 25;                // Centro di qualsiasi tris
        private const int PESO_ANGOLO = 15;                     // Angolo di un tris
        private const int PESO_LATO = 10;                       // Lato di un tris
        private const int PESO_MANDA_TRIS_VINTO = -100;          // Manda avversario in tris già vinto
        private const int PESO_MANDA_TRIS_PIENO = -80;           // Manda avversario in tris pieno
        private const int PESO_CONTROLLO_CENTRO = 40;           // Mantiene controllo del centro
        private const int PESO_MANDA_CENTRO = -60;              // Manda avversario al centro (negativo!)

        public HeuristicBot()
        {
            random = new Random();
        }

        public (int numTris, int row, int col)? CalcolaMossa(string boardState, int trisObbligatoria, char turno)
        {
            char avversario = turno == 'X' ? 'O' : 'X';

            if (trisObbligatoria == -1)
            {
                // MOSSA LIBERA - Ottimizzazione:
                // Invece di valutare tutte le ~81 mosse, valutiamo la migliore di ogni tris (9 mosse)
                (int numTris, int row, int col) mossaMiglioreGlobale = (-1, -1, -1);
                int punteggioMiglioreGlobale = int.MinValue;

                // FIX: Usato 'i' invece di 'numTris' per evitare shadowing
                for (int i = 0; i < 9; i++)
                {
                    // Trova la migliore mossa in questo tris
                    var mossaMiglioreTris = TrovaMiglioreMossaInTris(boardState, i, turno, avversario);

                    if (mossaMiglioreTris.HasValue)
                    {
                        int punteggio = mossaMiglioreTris.Value.punteggio;

                        if (punteggio > punteggioMiglioreGlobale ||
                            (punteggio == punteggioMiglioreGlobale && random.Next(2) == 0))
                        {
                            punteggioMiglioreGlobale = punteggio;
                            // FIX: Ora usa 'i' correttamente
                            mossaMiglioreGlobale = (i, mossaMiglioreTris.Value.row, mossaMiglioreTris.Value.col);
                        }
                    }
                }

                return mossaMiglioreGlobale.numTris == -1 ? null : mossaMiglioreGlobale;
            }
            else
            {
                // MOSSA OBBLIGATORIA - Valuta solo il tris specificato
                var mossaMigliore = TrovaMiglioreMossaInTris(boardState, trisObbligatoria, turno, avversario);

                if (mossaMigliore.HasValue)
                    return (trisObbligatoria, mossaMigliore.Value.row, mossaMigliore.Value.col);

                return null;
            }
        }

        /// <summary>
        /// Trova la migliore mossa in un singolo tris
        /// </summary>
        private (int row, int col, int punteggio)? TrovaMiglioreMossaInTris(
            string boardState, int numTris, char turno, char avversario)
        {
            List<(int numTris, int row, int col)> mosse = new List<(int numTris, int row, int col)>();
            AggiungiMosseTris(mosse, boardState, numTris);

            if (mosse.Count == 0)
                return null;

            (int row, int col) mossaMigliore = (mosse[0].row, mosse[0].col);
            int punteggioMigliore = int.MinValue;

            foreach (var mossa in mosse)
            {
                int punteggio = ValutaMossa(boardState, mossa, turno, avversario);

                if (punteggio > punteggioMigliore ||
                    (punteggio == punteggioMigliore && random.Next(2) == 0))
                {
                    punteggioMigliore = punteggio;
                    mossaMigliore = (mossa.row, mossa.col);
                }
            }

            return (mossaMigliore.row, mossaMigliore.col, punteggioMigliore);
        }

        public void NotificaRisultatoPartita(bool? haVinto)
        {
            // Il bot euristico non impara, ignora il risultato
        }

        public void ResetPartita()
        {
            // Nessuno stato da resettare
        }

        /// <summary>
        /// Valuta una singola mossa e restituisce un punteggio
        /// Punteggio alto = mossa migliore
        /// </summary>
        private int ValutaMossa(string boardState, (int numTris, int row, int col) mossa, char turno, char avversario)
        {
            int punteggio = 0;

            // 1. VALUTAZIONE POSIZIONALE - Quanto è buona la posizione?
            punteggio += ValutaPosizione(mossa.numTris, mossa.row, mossa.col);

            // 2. VALUTAZIONE TATTICA - Vince o blocca vittorie?
            punteggio += ValutaTattica(boardState, mossa, turno, avversario);

            // 3. VALUTAZIONE STRATEGICA - Dove manda l'avversario?
            punteggio += ValutaStrategia(boardState, mossa, turno);

            return punteggio;
        }

        /// <summary>
        /// Valuta la posizione della mossa (centro, angolo, lato)
        /// </summary>
        private int ValutaPosizione(int numTris, int row, int col)
        {
            int punteggio = 0;

            // Centro del tris centrale (posizione 4,4 nel Super Tris)
            if (numTris == 4 && row == 1 && col == 1)
            {
                punteggio += PESO_CENTRO_CENTRALE;
            }
            // Centro di qualsiasi tris
            else if (row == 1 && col == 1)
            {
                punteggio += PESO_CENTRO_TRIS;
            }
            // Angoli (posizioni strategiche)
            else if ((row == 0 || row == 2) && (col == 0 || col == 2))
            {
                punteggio += PESO_ANGOLO;
            }
            // Lati
            else
            {
                punteggio += PESO_LATO;
            }

            // Bonus se il tris stesso è in posizione centrale o angolare
            if (numTris == 4) // Tris centrale
            {
                punteggio += PESO_CONTROLLO_CENTRO / 2;
            }
            else if (numTris == 0 || numTris == 2 || numTris == 6 || numTris == 8) // Tris angolari
            {
                punteggio += PESO_ANGOLO / 2;
            }

            return punteggio;
        }

        /// <summary>
        /// Valuta aspetti tattici: vittorie e blocchi
        /// </summary>
        private int ValutaTattica(string boardState, (int numTris, int row, int col) mossa, char turno, char avversario)
        {
            int punteggio = 0;

            // Simula la mossa
            string nuovoBoardState = SimulaMossa(boardState, mossa, turno);

            // Controlla se questa mossa vince il gioco
            if (ControllaVittoriaGioco(nuovoBoardState, turno))
            {
                return PESO_VINCITA_GIOCO; // Vittoria immediata - massima priorità!
            }

            // Controlla se blocca la vittoria dell'avversario nel gioco
            if (MossaBloccoVittoriaGioco(boardState, mossa, avversario))
            {
                punteggio += PESO_BLOCCO_VITTORIA_GIOCO;
            }

            // Controlla se vince un mini-tris
            if (ControllaVittoriaTris(nuovoBoardState, mossa.numTris, turno))
            {
                punteggio += PESO_VINCITA_TRIS;
            }

            // Controlla se blocca vittoria avversario in mini-tris
            if (MossaBloccoVittoriaTris(boardState, mossa, avversario))
            {
                punteggio += PESO_BLOCCO_VITTORIA_TRIS;
            }

            // Controlla se crea doppia minaccia in un mini-tris
            if (CreaDoppiaMinaccia(nuovoBoardState, mossa.numTris, turno))
            {
                punteggio += PESO_DOPPIA_MINACCIA_TRIS;
            }

            return punteggio;
        }

        /// <summary>
        /// Valuta la strategia: dove manda l'avversario?
        /// </summary>
        private int ValutaStrategia(string boardState, (int numTris, int row, int col) mossa, char turno)
        {
            int punteggio = 0;

            // Calcola dove andrà l'avversario dopo questa mossa
            int prossimoTris = mossa.row * 3 + mossa.col;

            // Controlla se il prossimo tris è già vinto
            char vincitoreTris = GetVincitoreTris(boardState, prossimoTris);
            if (vincitoreTris != '-')
            {
                // Manda l'avversario in un tris già vinto = ottimo!
                punteggio += PESO_MANDA_TRIS_VINTO;
            }
            // Controlla se il prossimo tris è pieno
            else if (TrisPieno(boardState, prossimoTris))
            {
                // Manda l'avversario in un tris pieno = molto buono!
                punteggio += PESO_MANDA_TRIS_PIENO;
            }
            // Controlla se manda l'avversario al centro
            else if (prossimoTris == 4)
            {
                // Mandare al centro è pericoloso (negativo!)
                punteggio += PESO_MANDA_CENTRO;
            }

            return punteggio;
        }

        // ==================== METODI DI SUPPORTO ==================== //

        private string SimulaMossa(string boardState, (int numTris, int row, int col) mossa, char simbolo)
        {
            char[] stato = boardState.ToCharArray();
            int offset = mossa.numTris * 9;
            int posizione = mossa.row * 3 + mossa.col;
            stato[offset + posizione] = simbolo;
            return new string(stato);
        }

        private bool ControllaVittoriaGioco(string boardState, char turno)
        {
            // Controlla se il giocatore ha vinto 3 mini-tris in fila
            char[] vincitoriTris = new char[9];
            for (int i = 0; i < 9; i++)
            {
                vincitoriTris[i] = GetVincitoreTris(boardState, i);
            }

            // Controlla righe
            for (int row = 0; row < 3; row++)
            {
                if (vincitoriTris[row * 3] == turno &&
                    vincitoriTris[row * 3 + 1] == turno &&
                    vincitoriTris[row * 3 + 2] == turno)
                    return true;
            }

            // Controlla colonne
            for (int col = 0; col < 3; col++)
            {
                if (vincitoriTris[col] == turno &&
                    vincitoriTris[col + 3] == turno &&
                    vincitoriTris[col + 6] == turno)
                    return true;
            }

            // Controlla diagonali
            if (vincitoriTris[0] == turno && vincitoriTris[4] == turno && vincitoriTris[8] == turno)
                return true;
            if (vincitoriTris[2] == turno && vincitoriTris[4] == turno && vincitoriTris[6] == turno)
                return true;

            return false;
        }

        private bool MossaBloccoVittoriaGioco(string boardState, (int numTris, int row, int col) mossa, char avversario)
        {
            // Simula la mossa dell'avversario
            string statoConMossaAvversario = SimulaMossa(boardState, mossa, avversario);
            return ControllaVittoriaGioco(statoConMossaAvversario, avversario);
        }

        private bool ControllaVittoriaTris(string boardState, int numTris, char turno)
        {
            int offset = numTris * 9;
            char[] tris = boardState.Substring(offset, 9).ToCharArray();

            // Controlla righe
            for (int row = 0; row < 3; row++)
            {
                if (tris[row * 3] == turno && tris[row * 3 + 1] == turno && tris[row * 3 + 2] == turno)
                    return true;
            }

            // Controlla colonne
            for (int col = 0; col < 3; col++)
            {
                if (tris[col] == turno && tris[col + 3] == turno && tris[col + 6] == turno)
                    return true;
            }

            // Controlla diagonali
            if (tris[0] == turno && tris[4] == turno && tris[8] == turno)
                return true;
            if (tris[2] == turno && tris[4] == turno && tris[6] == turno)
                return true;

            return false;
        }

        private bool MossaBloccoVittoriaTris(string boardState, (int numTris, int row, int col) mossa, char avversario)
        {
            string statoConMossaAvversario = SimulaMossa(boardState, mossa, avversario);
            return ControllaVittoriaTris(statoConMossaAvversario, mossa.numTris, avversario);
        }

        private bool CreaDoppiaMinaccia(string boardState, int numTris, char turno)
        {
            // Conta quante righe/colonne/diagonali hanno 2 del nostro simbolo e 1 vuoto
            int offset = numTris * 9;
            char[] tris = boardState.Substring(offset, 9).ToCharArray();
            int minacce = 0;

            // Controlla righe
            for (int row = 0; row < 3; row++)
            {
                int count = 0;
                int vuoti = 0;
                for (int col = 0; col < 3; col++)
                {
                    if (tris[row * 3 + col] == turno) count++;
                    if (tris[row * 3 + col] == '-') vuoti++;
                }
                if (count == 2 && vuoti == 1) minacce++;
            }

            // Controlla colonne
            for (int col = 0; col < 3; col++)
            {
                int count = 0;
                int vuoti = 0;
                for (int row = 0; row < 3; row++)
                {
                    if (tris[row * 3 + col] == turno) count++;
                    if (tris[row * 3 + col] == '-') vuoti++;
                }
                if (count == 2 && vuoti == 1) minacce++;
            }

            // Controlla diagonali
            if ((tris[0] == turno ? 1 : 0) + (tris[4] == turno ? 1 : 0) + (tris[8] == turno ? 1 : 0) == 2 &&
                (tris[0] == '-' ? 1 : 0) + (tris[4] == '-' ? 1 : 0) + (tris[8] == '-' ? 1 : 0) == 1)
                minacce++;

            if ((tris[2] == turno ? 1 : 0) + (tris[4] == turno ? 1 : 0) + (tris[6] == turno ? 1 : 0) == 2 &&
                (tris[2] == '-' ? 1 : 0) + (tris[4] == '-' ? 1 : 0) + (tris[6] == '-' ? 1 : 0) == 1)
                minacce++;

            return minacce >= 2; // Doppia minaccia se ha 2+ modi per vincere
        }

        private char GetVincitoreTris(string boardState, int numTris)
        {
            if (ControllaVittoriaTris(boardState, numTris, 'X')) return 'X';
            if (ControllaVittoriaTris(boardState, numTris, 'O')) return 'O';
            return '-';
        }

        private bool TrisPieno(string boardState, int numTris)
        {
            int offset = numTris * 9;
            for (int i = 0; i < 9; i++)
            {
                if (boardState[offset + i] == '-')
                    return false;
            }
            return true;
        }

        private void AggiungiMosseTris(List<(int numTris, int row, int col)> mosse, string boardState, int numTris)
        {
            int offset = numTris * 9;

            for (int i = 0; i < 9; i++)
            {
                if (boardState[offset + i] == '-')
                {
                    int row = i / 3;
                    int col = i % 3;
                    mosse.Add((numTris, row, col));
                }
            }
        }
    }
}
