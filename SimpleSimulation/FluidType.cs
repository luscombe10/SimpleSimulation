using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSimulation
{
    class FluidType
    {
        protected float viscosity;
        protected float compressibility;
        protected float surfaceVolumeRatio;
        protected float density; // used to determine fluid potential / pressure in wells?
        public FluidType(float flowFactor, float compressibility, float surfaceVolumeRatio, float density)
        {
            this.viscosity = flowFactor;
            this.compressibility = compressibility;
            this.surfaceVolumeRatio = surfaceVolumeRatio;
            this.density = density;
        }

        public float Density
        {
            get => density;
        }
        public float Viscosity
        {
            get => viscosity;
        }

        /// <summary>
        /// Rough approximation of Compressibility decreasing with falling pressure
        /// </summary>
        /// <param name="pressure"></param>
        /// <returns></returns>
        public float Compressibility(float pressure)
        {

            return compressibility;
        }
    }
}
