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
        private DataTable m_Items;
        private DataTable m_Input;
        private DataTable m_Output;
        private RandomGenerator m_RG = new RandomGenerator();

        public override void Initialize()
        {
            base.Initialize();

            this.m_Regions = this.Project.GetDataSheet("sample_sales__Region").GetData();
            this.m_Items = this.Project.GetDataSheet("sample_sales__Item").GetData();
            this.m_Input = this.ResultScenario.GetDataSheet("sample_sales__InputSales").GetData();
            this.m_Output = this.ResultScenario.GetDataSheet("sample_sales__OutputSales").GetData();

            this.m_Input.PrimaryKey = new DataColumn[] {
                this.m_Input.Columns["RegionID"],
                this.m_Input.Columns["ItemID"]
            };
        }

        protected override void OnIteration(int iteration)
        {
            base.OnIteration(iteration);

            foreach (DataRow RegionRow in this.m_Regions.Rows)
            {
                foreach (DataRow ItemRow in this.m_Items.Rows)
                {
                    this.CreateSalesForecast(
                        Convert.ToInt32(RegionRow["RegionID"]),
                        Convert.ToInt32(ItemRow["ItemID"]),
                        iteration);
                }
            }
        }

        private void CreateSalesForecast(int regionId, int itemId, int iteration)
        {
            DataRow InputRow = this.m_Input.Rows.Find(new object[] { regionId, itemId });

            if (InputRow != null)
            {
                int MinItemsSold = Convert.ToInt32(InputRow["CurMinUnitsSold"]);
                int MaxItemsSold = Convert.ToInt32(InputRow["CurMaxUnitsSold"]);
                double ItemPrice = Convert.ToDouble(InputRow["ItemPrice"]);
                double PercentIncrease = 0.0;

                if (!InputRow.IsNull("PercentIncrease"))
                {
                    PercentIncrease = Convert.ToDouble(InputRow["PercentIncrease"]);
                }

                for (int Timestep = this.MinimumTimestep; Timestep <= this.MaximumTimestep; Timestep++)
                {
                    int ItemsSold = Convert.ToInt32(this.m_RG.GetUniformDouble(MinItemsSold, MaxItemsSold));

                    this.m_Output.Rows.Add(new object[] {
                        iteration,
                        Timestep, 
                        regionId, 
                        itemId,
                        ItemsSold,
                        ItemsSold * ItemPrice
                    });

                    MinItemsSold = Convert.ToInt32(MinItemsSold * (1 + (PercentIncrease / 100)));
                    MaxItemsSold = Convert.ToInt32(MaxItemsSold * (1 + (PercentIncrease / 100)));
                }                
            }
        }
    }
}
