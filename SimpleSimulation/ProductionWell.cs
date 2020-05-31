using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleSimulation
{
    class ProductionWell: Well
    {
        public ProductionWell(float radius, FluidType initialFluid, float depth)
        {
            averageRadius = radius;
            containingFluid = initialFluid;
            bottomHoleDepth = depth;
        }

        
    }
}
