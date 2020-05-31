using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

// start off by assuming 1 / 1 unit cells
namespace SimpleSimulation
{
    class SimulationCell
    {
        protected float permeability;
        protected float porosity;
        protected FluidType fluid;
        protected float pressure;
        protected List<SimulationCell> adjacentCells;
        protected float lastObservedPressure;
        protected float height;

        public SimulationCell(float initialPressure, float permeability, float porosity, FluidType initialfluid, float height)
        {
            pressure = initialPressure;
            this.permeability = permeability;
            this.porosity = porosity;
            fluid = initialfluid;
            lastObservedPressure = initialPressure;
            adjacentCells = new List<SimulationCell>();
            this.height = height; // factor this into pressure calculations?
        }

        public void AddAdjacentCell(SimulationCell otherCell)
        {
            if (adjacentCells.Count >= 6)
            {
                return;
            }
            else
            {
                adjacentCells.Add(otherCell);
            }
        }

        public List<SimulationCell> AdjacentCells
        {
            get => adjacentCells;
        }

        public float Permeability
        {
            get => permeability;
        }

        public float Porosity
        {
            get => porosity;
        }
        public float Pressure
        {
            get => pressure;
            set => pressure = value;
        }

        public float Compressibility
        {
            get => fluid.Compressibility(pressure);
        }

        public float Viscosity
        {
            get => fluid.Viscosity;
        }

        public void UpdateLastObservedPressure()
        {
            lastObservedPressure = pressure;
        }

        public float LastObservedPressure
        {
            get => lastObservedPressure;
        }

        public void ExplicitUpdatePressure()
        {
            float pressureChange = 0;
            foreach(var otherCell in adjacentCells)
            {
                float deltaPressure = pressure - otherCell.lastObservedPressure;
                pressureChange +=  (deltaPressure * ((permeability + otherCell.permeability) / 2) * fluid.Viscosity);
             }
            pressure -= pressureChange;
        }

        public float WellFlow(Well connectedWell)
        {
            float wellPressure = connectedWell.BottomHolePressure;

            float productionArea = 2 * (float)Math.PI * connectedWell.Radius * (height/ 2) + 2; 

            float flow = -(permeability * productionArea * fluid.Viscosity) * 
                (connectedWell.BottomHolePressure - pressure);

            pressure -= (flow / (height * height * height)) / fluid.Compressibility(pressure); // improve this e.g 
            return flow;
        }

    }
}
