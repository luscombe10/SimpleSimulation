using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSimulation
{
    abstract class Well
    {
        protected float averageRadius;
        protected FluidType containingFluid;
        protected float bottomHoleDepth;
        protected SimulationCell targetCell;

        public float Area
        {
            get => (float)Math.PI * (averageRadius * averageRadius) ;
        }

        public float BottomHolePressure
        {
            // convert to bar with 10197f constant
            get => ((float)Area * bottomHoleDepth * containingFluid.Density * 9.807f) / 10197f ;
        }

        public float Radius
        {
            get => averageRadius;
        }

        public SimulationCell TargetCell
        {
            get => targetCell;
            set => targetCell = value;
        }
    }
}
