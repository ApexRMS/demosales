// SyncroSim Modeling Framework
// Copyright © 2007-2019 Apex Resource Management Solutions Ltd. (ApexRMS). All rights reserved.
// The TERMS OF USE and END USER LICENSE AGREEMENT for this software can be found in the LICENSE file.

using System;
using System.Data;
using SyncroSim.Core;
using SyncroSim.StochasticTime;

namespace SampleSales
{
    class Runtime : StochasticTimeTransformer
    {
        private DataTable m_Regions;
        private DataTable m_Units;
        private DataTable m_Input;
        private DataTable m_Output;
        private RandomGenerator m_RG = new RandomGenerator();

        public override void Initialize()
        {
            base.Initialize();

            this.m_Regions = this.Project.GetDataSheet("Sales_Region").GetData();
            this.m_Units = this.Project.GetDataSheet("Sales_Unit").GetData();
            this.m_Input = this.ResultScenario.GetDataSheet("Sales_InputSales").GetData();
            this.m_Output = this.ResultScenario.GetDataSheet("Sales_OutputSales").GetData();

            this.m_Input.PrimaryKey = new DataColumn[] {
                this.m_Input.Columns["RegionID"],
                this.m_Input.Columns["UnitID"]
            };
        }

        protected override void OnIteration(int iteration)
        {
            base.OnIteration(iteration);

            foreach (DataRow RegionRow in this.m_Regions.Rows)
            {
                foreach (DataRow UnitRow in this.m_Units.Rows)
                {
                    this.CreateSalesForecast(
                        Convert.ToInt32(RegionRow["RegionID"]),
                        Convert.ToInt32(UnitRow["UnitID"]),
                        iteration);
                }
            }
        }

        private void CreateSalesForecast(int regionId, int unitId, int iteration)
        {
            DataRow InputRow = this.m_Input.Rows.Find(new object[] { regionId, unitId });

            if (InputRow != null)
            {
                int MinUnitsSold = Convert.ToInt32(InputRow["MinUnitsSold"]);
                int MaxUnitsSold = Convert.ToInt32(InputRow["MaxUnitsSold"]);
                double UnitPrice = Convert.ToDouble(InputRow["UnitPrice"]);
                double PercentIncrease = 0.0;

                if (!InputRow.IsNull("PercentIncrease"))
                {
                    PercentIncrease = Convert.ToDouble(InputRow["PercentIncrease"]);
                }

                for (int Timestep = this.MinimumTimestep; Timestep <= this.MaximumTimestep; Timestep++)
                {
                    int UnitsSold = Convert.ToInt32(this.m_RG.GetUniformDouble(MinUnitsSold, MaxUnitsSold));

                    this.m_Output.Rows.Add(new object[] {
                        iteration,
                        Timestep, 
                        regionId, 
                        unitId,
                        UnitsSold,
                        UnitsSold * UnitPrice
                    });

                    MinUnitsSold = Convert.ToInt32(MinUnitsSold * (1 + (PercentIncrease / 100)));
                    MaxUnitsSold = Convert.ToInt32(MaxUnitsSold * (1 + (PercentIncrease / 100)));
                }                
            }
        }
    }
}
