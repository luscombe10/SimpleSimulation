using System;
using static System.Console;
using System.Collections.Generic;
namespace SimpleSimulation
{
    class Program
    {
        public static void Main()
        {
            int xDims = 4;
            int yDims = 4;
            List<float> well1Prod = new List<float>();
            List<float> production = new List<float>();
            var field = new ProductionField(xDims, yDims, 200, 0.01f, 0.1f, new FluidType(0.4f, 0.02f, 1.2f, 1000), 500);
            field.SimulationCells[1][1].Pressure = 185.5f;
            /*var well = new ProductionWell(0.25f, new FluidType(0.1f, 0.1f, 1, 400), 1000);
            for (int i = 0; i < 1000; i++)
            {
                float prod = field.SimulationCells[1][1].WellFlow(well);
                well1Prod.Add(prod);
                if (i >0)
                {
                    var well1 = new ProductionWell(0.25f, new FluidType(0.1f, 0.1f, 1, 400), 1000);
                    prod += field.SimulationCells[25][25].WellFlow(well1);
                }
                if (i > 0)
                {
                    var well2 = new ProductionWell(0.25f, new FluidType(0.1f, 0.1f, 1, 400), 1000);
                    prod += field.SimulationCells[48][48].WellFlow(well2);
                }
                if (i > 1000)
                {
                    var well2 = new ProductionWell(0.25f, new FluidType(0.1f, 0.1f, 1, 400), 1000);
                    prod += field.SimulationCells[80][80].WellFlow(well2);
                }
                production.Add(prod);
                *//*field.ImplicitUpdatePressure();*//*
                
                
            }*/
            var mask = field.GenerateImplicitMatrixMask();
            field.ImplicitTimeStep(86400);
        }
    }
}
