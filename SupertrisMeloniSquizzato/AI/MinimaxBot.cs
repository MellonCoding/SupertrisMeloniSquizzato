namespace SupertrisMeloniSquizzato.AI
{
    internal class MinimaxBot : IBot
    {
        // -------- CONFIGURAZIONE --------
        private int maxDepth = 5;  // Profondità massima di esplorazione minimax
        private Random random;
        private int mosseGiocate;
        private const int MOSSE_RANDOM = 2;  // Prime 2 mosse random

        // -------- PESI EURISTICI --------
        private const int PesoTrisVinto = 1000;
        private const int PesoLinea = 5;
        private const int PesoCentro = 5;
        private const int PesoAngolo = 2;
        private const int PesoCentroGlobale = 50;
        private const int PesoAngoloGlobale = 20;

        // -------- STRUTTURE DATI --------
        private struct Mossa
        {
            public int coordinataX;
            public int coordinataY;
        }

        private int[,] gameState;  // Stato del gioco 9x9: 0=vuoto, 1=bot, -1=player

        // -------- COSTRUTTORE --------
        public MinimaxBot()
        {
            gameState = new int[9, 9];
            random = new Random();
            mosseGiocate = 0;
        }

        // -------- IMPLEMENTAZIONE INTERFACCIA IBot --------

        public (int numTris, int row, int col)? CalcolaMossa(string boardState, int trisObbligatoria, char turno)
        {
            ParseBoardState(boardState, turno);

            // Prime MOSSE_RANDOM mosse: gioca casualmente per variabilità
            if (mosseGiocate < MOSSE_RANDOM)
            {
                mosseGiocate++;
                return CalcolaMossaRandom(boardState, trisObbligatoria);
            }

            mosseGiocate++;

            Mossa miglioreMossa = new Mossa();
            Mossa mossaIniziale = new Mossa();
            int trisRow, trisCol, numTris, row, col;

            if (trisObbligatoria != -1)
            {
                // Mossa obbligatoria in un tris specifico
                trisRow = trisObbligatoria / 3;
                trisCol = trisObbligatoria % 3;
                mossaIniziale.coordinataX = trisCol * 3;
                mossaIniziale.coordinataY = trisRow * 3;
            }
            else
            {
                // Mossa libera - usa (0,0) come placeholder
                // Il minimax esplorerà tutte le opzioni valide
                mossaIniziale.coordinataX = 0;
                mossaIniziale.coordinataY = 0;
            }

            Minimax(maxDepth, gameState, true, mossaIniziale, int.MinValue, int.MaxValue, ref miglioreMossa, true, trisObbligatoria);

            // Converti coordinate globali in coordinate locali
            numTris = (miglioreMossa.coordinataY / 3) * 3 + (miglioreMossa.coordinataX / 3);
            row = miglioreMossa.coordinataY % 3;
            col = miglioreMossa.coordinataX % 3;

            return (numTris, row, col);
        }

        private (int numTris, int row, int col)? CalcolaMossaRandom(string boardState, int trisObbligatoria)
        {
            List<(int numTris, int row, int col)> mosseValide = new List<(int numTris, int row, int col)>();

            if (trisObbligatoria == -1)
            {
                // Mossa libera - trova tutte le mosse valide in tutti i tris
                for (int numTris = 0; numTris < 9; numTris++)
                {
                    AggiungiMosseValide(mosseValide, boardState, numTris);
                }
            }
            else
            {
                // Mossa obbligatoria - trova mosse nel tris specificato
                AggiungiMosseValide(mosseValide, boardState, trisObbligatoria);
            }

            if (mosseValide.Count == 0)
                return null;

            // Scegli una mossa a caso
            int indice = random.Next(mosseValide.Count);
            return mosseValide[indice];
        }

        private void AggiungiMosseValide(List<(int numTris, int row, int col)> mosse, string boardState, int numTris)
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

        public void NotificaRisultatoPartita(bool? haVinto)
        {
            // Reset del contatore mosse per la prossima partita
            mosseGiocate = 0;
        }

        public void ResetPartita()
        {
            Array.Clear(gameState, 0, gameState.Length);
            mosseGiocate = 0;
        }

        // -------- PARSING BOARD STATE --------

        private void ParseBoardState(string boardState, char turno)
        {
            char botChar = turno;
            char playerChar = (turno == 'X') ? 'O' : 'X';
            int index = 0;
            int trisRow, trisCol, offsetY, offsetX, row, col, globalY, globalX;
            char cell;

            for (int numTris = 0; numTris < 9; numTris++)
            {
                trisRow = numTris / 3;
                trisCol = numTris % 3;
                offsetY = trisRow * 3;
                offsetX = trisCol * 3;

                for (int cellIndex = 0; cellIndex < 9; cellIndex++)
                {
                    row = cellIndex / 3;
                    col = cellIndex % 3;
                    globalY = offsetY + row;
                    globalX = offsetX + col;

                    cell = boardState[index];

                    if (cell == '-') gameState[globalY, globalX] = 0;
                    else if (cell == botChar) gameState[globalY, globalX] = 1;
                    else if (cell == playerChar) gameState[globalY, globalX] = -1;

                    index++;
                }
            }
        }

        // -------- ALGORITMO MINIMAX --------

        private int Minimax(int depth, int[,] matrix, bool isBot, Mossa ultimaMossa, int alpha, int beta, ref Mossa miglioreMossa, bool isPrimaChiamata, int trisObbligatoria = -1)
        {
            int valVittoria = CheckVittoriaGlobale(matrix);
            int xInizio, xFine, yInizio, yFine, numTrisObbligatorio;
            int bestValue, value;
            Mossa tempMossa = new Mossa();
            Mossa dummy = new Mossa();

            if (valVittoria == 1) return int.MaxValue - (maxDepth - depth);
            else if (valVittoria == -1) return int.MinValue + (maxDepth - depth);

            if (depth == 0) return FunzioneValutativa(matrix);

            // Gestione della mossa libera o obbligatoria
            if (isPrimaChiamata && trisObbligatoria == -1)
            {
                // MOSSA LIBERA - esplora tutti i tris disponibili
                xInizio = 0;
                xFine = 9;
                yInizio = 0;
                yFine = 9;
            }
            else if (isPrimaChiamata && trisObbligatoria != -1)
            {
                // MOSSA OBBLIGATORIA - esplora solo il tris specificato
                int trisRow = trisObbligatoria / 3;
                int trisCol = trisObbligatoria % 3;
                xInizio = trisCol * 3;
                xFine = xInizio + 3;
                yInizio = trisRow * 3;
                yFine = yInizio + 3;
            }
            else
            {
                // Mosse successive - usa la logica normale basata sull'ultima mossa
                xInizio = ultimaMossa.coordinataX - (ultimaMossa.coordinataX % 3);
                xFine = xInizio + 3;
                yInizio = ultimaMossa.coordinataY - (ultimaMossa.coordinataY % 3);
                yFine = yInizio + 3;

                numTrisObbligatorio = (yInizio / 3) * 3 + (xInizio / 3);
                if (IsTrisCompleted(matrix, numTrisObbligatorio))
                {
                    xInizio = 0;
                    xFine = 9;
                    yInizio = 0;
                    yFine = 9;
                }
            }

            if (isBot)
            {
                bestValue = int.MinValue;

                for (int i = yInizio; i < yFine; i++)
                {
                    for (int j = xInizio; j < xFine; j++)
                    {
                        if (matrix[i, j] == 0)
                        {
                            tempMossa.coordinataX = j;
                            tempMossa.coordinataY = i;

                            matrix[i, j] = 1;
                            value = Minimax(depth - 1, matrix, false, tempMossa, alpha, beta, ref dummy, false);
                            matrix[i, j] = 0;

                            if (isPrimaChiamata && value > bestValue)
                            {
                                miglioreMossa = tempMossa;
                            }

                            bestValue = Math.Max(bestValue, value);
                            alpha = Math.Max(alpha, value);

                            if (beta <= alpha) break;
                        }
                    }
                    if (beta <= alpha) break;
                }
            }
            else
            {
                bestValue = int.MaxValue;

                for (int i = yInizio; i < yFine; i++)
                {
                    for (int j = xInizio; j < xFine; j++)
                    {
                        if (matrix[i, j] == 0)
                        {
                            tempMossa.coordinataX = j;
                            tempMossa.coordinataY = i;

                            matrix[i, j] = -1;
                            value = Minimax(depth - 1, matrix, true, tempMossa, alpha, beta, ref dummy, false);
                            matrix[i, j] = 0;

                            bestValue = Math.Min(bestValue, value);
                            beta = Math.Min(beta, value);

                            if (beta <= alpha) break;
                        }
                    }
                    if (beta <= alpha) break;
                }
            }

            return bestValue;
        }

        // -------- FUNZIONI DI CONTROLLO VITTORIA --------

        private int CheckVittoriaTris(int[,] matrix, int numTris)
        {
            int trisRow = numTris / 3;
            int trisCol = numTris % 3;
            int offsetY = trisRow * 3;
            int offsetX = trisCol * 3;

            int y, x, first, center;

            for (int row = 0; row < 3; row++)
            {
                y = offsetY + row;
                first = matrix[y, offsetX];
                if (first != 0 && first == matrix[y, offsetX + 1] && first == matrix[y, offsetX + 2]) return first;
            }

            for (int col = 0; col < 3; col++)
            {
                x = offsetX + col;
                first = matrix[offsetY, x];
                if (first != 0 && first == matrix[offsetY + 1, x] && first == matrix[offsetY + 2, x]) return first;
            }

            center = matrix[offsetY + 1, offsetX + 1];
            if (center != 0 && center == matrix[offsetY, offsetX] && center == matrix[offsetY + 2, offsetX + 2]) return center;
            if (center != 0 && center == matrix[offsetY, offsetX + 2] && center == matrix[offsetY + 2, offsetX]) return center;

            return 0;
        }

        private int CheckVittoriaGlobale(int[,] matrix)
        {
            int[] trisWinners = new int[9];
            int first, center;

            for (int i = 0; i < 9; i++) trisWinners[i] = CheckVittoriaTris(matrix, i);

            for (int row = 0; row < 3; row++)
            {
                first = trisWinners[row * 3];
                if (first != 0 && first == trisWinners[row * 3 + 1] && first == trisWinners[row * 3 + 2]) return first;
            }

            for (int col = 0; col < 3; col++)
            {
                first = trisWinners[col];
                if (first != 0 && first == trisWinners[col + 3] && first == trisWinners[col + 6]) return first;
            }

            center = trisWinners[4];
            if (center != 0 && center == trisWinners[0] && center == trisWinners[8]) return center;
            if (center != 0 && center == trisWinners[2] && center == trisWinners[6]) return center;

            return 0;
        }

        private bool IsTrisCompleted(int[,] matrix, int numTris)
        {
            int trisRow = numTris / 3;
            int trisCol = numTris % 3;
            int winner, y, x;

            winner = CheckVittoriaTris(matrix, numTris);
            if (winner != 0) return true;

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    y = trisRow * 3 + row;
                    x = trisCol * 3 + col;
                    if (matrix[y, x] == 0) return false;
                }
            }

            return true;
        }

        // -------- FUNZIONE VALUTATIVA --------

        private int FunzioneValutativa(int[,] matrix)
        {
            int[] valoriTris = new int[9];

            for (int i = 0; i < 9; i++) valoriTris[i] = ValutaSingoloTris(matrix, i);

            return ValutaTrisGlobale(valoriTris);
        }

        private int ValutaSingoloTris(int[,] matrix, int numTris)
        {
            int trisRow = numTris / 3;
            int trisCol = numTris % 3;
            int offsetY = trisRow * 3;
            int offsetX = trisCol * 3;
            int punteggio = 0;
            int winner, y, x;

            winner = CheckVittoriaTris(matrix, numTris);
            if (winner != 0) return winner * PesoTrisVinto;

            for (int row = 0; row < 3; row++)
            {
                y = offsetY + row;
                punteggio += ValutaLinea(matrix[y, offsetX], matrix[y, offsetX + 1], matrix[y, offsetX + 2]);
            }

            for (int col = 0; col < 3; col++)
            {
                x = offsetX + col;
                punteggio += ValutaLinea(matrix[offsetY, x], matrix[offsetY + 1, x], matrix[offsetY + 2, x]);
            }

            punteggio += ValutaLinea(matrix[offsetY, offsetX], matrix[offsetY + 1, offsetX + 1], matrix[offsetY + 2, offsetX + 2]);
            punteggio += ValutaLinea(matrix[offsetY, offsetX + 2], matrix[offsetY + 1, offsetX + 1], matrix[offsetY + 2, offsetX]);

            punteggio += matrix[offsetY + 1, offsetX + 1] * PesoCentro;
            punteggio += matrix[offsetY, offsetX] * PesoAngolo;
            punteggio += matrix[offsetY, offsetX + 2] * PesoAngolo;
            punteggio += matrix[offsetY + 2, offsetX] * PesoAngolo;
            punteggio += matrix[offsetY + 2, offsetX + 2] * PesoAngolo;

            return punteggio;
        }

        private int ValutaTrisGlobale(int[] valoriTris)
        {
            int punteggio = 0;
            int[] angoli = { 0, 2, 6, 8 };

            for (int row = 0; row < 3; row++)
                punteggio += ValutaLineaGlobale(valoriTris[row * 3], valoriTris[row * 3 + 1], valoriTris[row * 3 + 2]);

            for (int col = 0; col < 3; col++)
                punteggio += ValutaLineaGlobale(valoriTris[col], valoriTris[col + 3], valoriTris[col + 6]);

            punteggio += ValutaLineaGlobale(valoriTris[0], valoriTris[4], valoriTris[8]);
            punteggio += ValutaLineaGlobale(valoriTris[2], valoriTris[4], valoriTris[6]);

            if (valoriTris[4] == PesoTrisVinto) punteggio += PesoCentroGlobale;
            else if (valoriTris[4] == -PesoTrisVinto) punteggio -= PesoCentroGlobale;

            foreach (int idx in angoli)
            {
                if (valoriTris[idx] == PesoTrisVinto) punteggio += PesoAngoloGlobale;
                else if (valoriTris[idx] == -PesoTrisVinto) punteggio -= PesoAngoloGlobale;
            }

            return punteggio;
        }

        private int ValutaLinea(int a, int b, int c)
        {
            int somma = a + b + c;
            bool haVuoto = (a == 0 || b == 0 || c == 0);

            if (!haVuoto || somma == 0) return 0;

            return somma * PesoLinea;
        }

        private int ValutaLineaGlobale(int a, int b, int c)
        {
            int somma = a + b + c;
            int punteggio = 0;
            int trisVintiBot = 0;
            int trisVintiPlayer = 0;

            if (a == PesoTrisVinto) trisVintiBot++;
            else if (a == -PesoTrisVinto) trisVintiPlayer++;

            if (b == PesoTrisVinto) trisVintiBot++;
            else if (b == -PesoTrisVinto) trisVintiPlayer++;

            if (c == PesoTrisVinto) trisVintiBot++;
            else if (c == -PesoTrisVinto) trisVintiPlayer++;

            if (trisVintiBot > 0 && trisVintiPlayer > 0) return 0;

            if (trisVintiBot == 2) punteggio += 2000;
            else if (trisVintiPlayer == 2) punteggio -= 2000;

            punteggio += somma;

            return punteggio;
        }
    }
}
