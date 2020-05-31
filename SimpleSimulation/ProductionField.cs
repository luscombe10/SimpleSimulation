using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks.Dataflow;
using MathNet.Numerics.LinearAlgebra;

namespace SimpleSimulation
{

    class ProductionField
    {
        
        public readonly SimulationCell[][] simulationCells;
        private readonly int xDims;
        private readonly int yDims;
        private SimulationCell[,] implicitCellMatrix;
        private bool[,] implicitCalculationMask;
        private float[,] implicitCalculationMatrix;
        private float cellSize;
        public ProductionField(int x, int y, float initialPressure, float permeability, float averagePorosity, FluidType fluid, float cellSize)
        {
            xDims = x;
            yDims = y;
            simulationCells = new SimulationCell[x][];
            this.cellSize = cellSize;
            // populate cartesian simulation cell array
            for (int i = 0; i < x; i++)
            {
                simulationCells[i] = new SimulationCell[y];
                for (int j = 0; j < y; j++)
                {
                    simulationCells[i][j] = new SimulationCell(initialPressure, permeability, averagePorosity, fluid, cellSize);
                }
            }
            // Once all cells placed, link all cells to siblings
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    foreach (var cell in GetCellNeighbours(i, j))
                    {
                        simulationCells[i][j].AddAdjacentCell(cell);
                    }
                }
            }
            // initialise simulation datastructures
            implicitCalculationMask = GenerateImplicitMatrixMask();
            implicitCellMatrix = buildCellMatrix();
            implicitCalculationMatrix = new float[xDims * yDims, xDims * yDims];
        }

        /// <summary>
        /// Get an array of all simulation cells
        /// </summary>
        public SimulationCell[][] SimulationCells
        {
            get => simulationCells;
        }

        /// <summary>
        /// Get all neighbouring cells for the selected simulation cell
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public List<SimulationCell> GetCellNeighbours(int x, int y)
        {
            var neighbourCells = new List<SimulationCell>();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i + j == 0)
                    {
                        continue;
                    }
                    else if ((x + i >= 0 && x + i < xDims) && j == 0)
                    {
                        neighbourCells.Add(simulationCells[x + i][y]);
                    }
                    else if ((y + j >= 0 && y + j < yDims) && i == 0)
                    {
                        neighbourCells.Add(simulationCells[x][y + j]);
                    }
                }
            }
            return neighbourCells;
        }

        /// <summary>
        /// Represents the simulation cells in matrix form for implicit pressure calculations
        /// </summary>
        /// <returns></returns>
        private SimulationCell[,] buildCellMatrix()
        {
            SimulationCell[,] fieldMatrix = new SimulationCell[xDims * yDims, xDims * yDims];
            for (int row = 0; row < (xDims * yDims); row++)
            {
                int column = 0;
                for (int i = 0; i < xDims; i++)
                {
                    for (int j = 0; j < yDims; j++)
                    {
                        fieldMatrix[row, column] = simulationCells[i][j];
                        column++;
                    }
                }
            }
            return fieldMatrix;
        }

        public void ProgressTime()
        {
            for (int x = 0; x < xDims; x++)
            {
                for (int y = 0; y < yDims; y++)
                {
                    simulationCells[x][y].ExplicitUpdatePressure();
                }
            }
        }

        /// <summary>
        /// Generates boolean mask required to do implicit calculations between cells
        /// </summary>
        /// <returns></returns>
        public bool[,] GenerateImplicitMatrixMask()
        {
            int matrixDims = xDims * yDims;
            bool[,] calculationMask = new bool[matrixDims, matrixDims];
            int row = 0;
            for (int i = 0; i < calculationMask.Length; i++)
            {
                int column = i % (matrixDims);
                if (i > 0 & column == 0)
                {
                    row += 1;
                }
                // x axis adjoining cells
                if (
                        // first cell of new x-axis row can't adjoin to previous matrix cell
                        (column == row - 1 && ((column + 1) % xDims != 0 || column == 0)) ||
                        // last cell of x-axis row can't adjoin to next matrix cell
                        (column == row + 1 && column % xDims != 0)
                    )
                {
                    calculationMask[row, column] = true;
                }

                // mark diagonal of matrix
                if (row - 1 >= 0 && column == i)
                {
                    calculationMask[row - 1, column] = true;
                }
                // y axis adjoining cells
                if (row == column)
                {
                    calculationMask[row, column] = true;
                    if (column + xDims < matrixDims)
                    {
                        calculationMask[row, column + xDims] = true;
                    }
                    if (column - xDims >= 0)
                    {
                        calculationMask[row, column - xDims] = true;
                    }
                }
            }
            return calculationMask;
        }

        private float calculateAlpha(SimulationCell cell1, SimulationCell cell2, int timeStep)
        {
            float avgPerm = (cell1.Permeability + cell2.Permeability) / 2;
            float avgPor = (cell1.Porosity + cell2.Porosity) / 2;
            float avgComp = (cell1.Compressibility + cell2.Compressibility) / 2;
            float avgVisc = (cell1.Viscosity + cell2.Viscosity) / 2;
            float alpha = avgPerm / (avgPor * avgVisc * avgComp);
            return alpha * (float)timeStep / (cellSize * cellSize);
        }

        public void ImplicitTimeStep(int timeStep)
        {

            int matrixDims = xDims * yDims;
            for (int i = 0; i < matrixDims; i++)
            {
                var selectedCell = implicitCellMatrix[i, i];
                for (int j = 0; j < matrixDims; j++)
                {
                    if (implicitCalculationMask[i, j])
                    {
                        if (i == j)
                        {
                            // calculate the pressure value in the matrix row
                            float pressure = selectedCell.Pressure;
                            foreach (var adjacentCell in selectedCell.AdjacentCells)
                            {
                                float alpha = calculateAlpha(selectedCell, adjacentCell, timeStep);

                                pressure += selectedCell.Pressure * alpha;
                            }
                            implicitCalculationMatrix[i, j] = pressure;
                        }
                        else
                        {
                            // calculate the adjacent cells in the row
                            float pressure = implicitCellMatrix[i, j].Pressure;
                            float alpha = calculateAlpha(selectedCell, implicitCellMatrix[i, j], timeStep);
                            pressure *= alpha;
                            implicitCalculationMatrix[i, j] = -pressure;
                        }
                    }
                    else
                    {
                        implicitCalculationMatrix[i, j] = 0;
                    }
                }
                // reverse engineer to use matrix all the way through
                
            }
            Matrix<float> calculationMatrix = Matrix<float>.Build.DenseOfArray(implicitCalculationMatrix);
            var pressures = new float[matrixDims];
            for (int i = 0; i < matrixDims; i++)
            {
                pressures[i] = implicitCellMatrix[0, i].Pressure;
            }
            Vector<float> p1Pressure = Vector<float>.Build.DenseOfArray(pressures);
            var p2Pressure = calculationMatrix.Solve(p1Pressure).ToArray();
            for(int i=0; i < matrixDims; i++)
            {
                p2Pressure[i] *= p1Pressure[i];
            }
            Console.WriteLine("test");
                      
        }
    }
}
                    
