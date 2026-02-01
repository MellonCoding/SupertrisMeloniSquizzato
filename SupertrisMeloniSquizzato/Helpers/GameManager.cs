using SupertrisMeloniSquizzato.Classes;

namespace SupertrisMeloniSquizzato.Helpers
{
    internal class GameManager
    {
        private Supertris board;
        private int ProssimoTrisObbligatorio;  
        private char turnoCorrente;
        private bool mossaValida;

        public GameManager()
        {
            board = new Supertris();
            turnoCorrente = 'X';
            ProssimoTrisObbligatorio = -1;
            mossaValida = false;
        }

        // -------------------------------- HELPERS -------------------------------- //

        public char GetTurno() => turnoCorrente;

        public void CambiaTurno() { turnoCorrente = turnoCorrente == 'X' ? 'O' : 'X'; }

        public int GetProssimaTrisObbligatoria() => ProssimoTrisObbligatorio;

        // -------------------------------- END HELPERS ---------------------------- // 

        public bool MakeMove(int numTris, int row, int col)
        {
            // Calcolo le coordinate del mini-tris nella griglia 3x3
            int trisRow = numTris / 3;
            int trisCol = numTris % 3;

            // Calcolo il prossimo tris dove dovrà giocare l'avversario
            int prossimoNumTris = row * 3 + col;

            mossaValida = false;

            // Controllo se la mossa è valida in base alla tris obbligatoria
            if (ProssimoTrisObbligatorio == -1)
            {
                // Mossa libera - può giocare ovunque
                mossaValida = board.MakeMove(turnoCorrente, trisRow, trisCol, row, col);
            }
            else
            {
                // Deve giocare nella tris obbligatoria
                if (ProssimoTrisObbligatorio == numTris)
                {
                    mossaValida = board.MakeMove(turnoCorrente, trisRow, trisCol, row, col);
                }
                else
                {
                    // Mossa non valida - tris sbagliata
                    return false;
                }
            }

            if (!mossaValida) return false;

            // Calcolo coordinate del prossimo mini-tris
            int prossimoTrisRow = prossimoNumTris / 3;
            int prossimoTrisCol = prossimoNumTris % 3;

            // FIX PRINCIPALE: Se il prossimo tris è già completato, la mossa diventa libera
            if (board.IsTrisCompleted(prossimoTrisRow, prossimoTrisCol))
            {
                ProssimoTrisObbligatorio = -1;  // Mossa libera!
            }
            else
            {
                ProssimoTrisObbligatorio = prossimoNumTris;
            }

            return true;
        }

        public char CheckWin()
        {
            return board.CheckWin();
        }

        internal string GetBoardState()
        {
            return board.GetBoardState();
        }
    }
}