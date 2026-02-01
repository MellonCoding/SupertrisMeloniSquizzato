using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupertrisMeloniSquizzato.Helpers
{
    internal class ColorManager
    {
        // Colori del tema
        public readonly Color coloreSfondo = Color.FromArgb(30, 30, 30);
        public readonly Color coloreTrisNormale = Color.FromArgb(45, 45, 48);
        public readonly Color coloreTrisAttivo = Color.FromArgb(60, 100, 60);
        public readonly Color coloreTrisCompletato = Color.FromArgb(50, 50, 50);
        public readonly Color coloreHover = Color.FromArgb(70, 70, 73);
        public readonly Color coloreX = Color.FromArgb(220, 80, 80);      // Rosso
        public readonly Color coloreO = Color.FromArgb(80, 150, 220);     // Blu
        public readonly Color coloreTesto = Color.White;
        public readonly Color coloreBordo = Color.FromArgb(80, 80, 80);
    }
}
