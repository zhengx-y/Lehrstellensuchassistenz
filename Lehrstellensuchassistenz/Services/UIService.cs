using System;

namespace Lehrstellensuchassistenz.Services
{
    public class UIService
    {
        public int ZoomLevel { get; private set; } = 0;
        private const int ZoomMin = 0;
        private const int ZoomMax = 4;
        private const double ZoomStep = 1.1;

        /// <summary>
        /// Berechnet den Skalierungsfaktor basierend auf dem Mausrad-Delta.
        /// Gibt null zurück, wenn das Zoom-Limit erreicht ist.
        /// </summary>
        public double? GetZoomFactor(int delta)
        {
            if (delta > 0 && ZoomLevel < ZoomMax)
            {
                ZoomLevel++;
                return ZoomStep;
            }

            if (delta < 0 && ZoomLevel > ZoomMin)
            {
                ZoomLevel--;
                return 1 / ZoomStep;
            }

            return null; // Limit erreicht
        }

        /// <summary>
        /// Setzt das Zoom-Level auf den Standardwert zurück.
        /// </summary>
        public void ResetZoom()
        {
            ZoomLevel = 0;
        }
    }
}