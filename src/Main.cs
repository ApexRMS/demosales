// SyncroSim Modeling Framework
// Copyright © 2007-2021 Apex Resource Management Solutions Ltd. (ApexRMS). All rights reserved.
// The TERMS OF USE and END USER LICENSE AGREEMENT for this software can be found in the LICENSE file.

using System;
using System.Data;
using SyncroSim.Core;

namespace DemoSales
{
    class Main : StochasticTimeTransformer
    {
        private DataTable m_RegionTable;
        private DataTable m_ItemTable;
        private DataTable m_AnnualSalesTable;
        private DataTable m_OutputSalesTable;
        private RandomGenerator m_RG = new RandomGenerator();

        public override void Initialize()
        {
            base.Initialize();

            this.m_RegionTable = this.Project.GetDataSheet("demosales_Region").GetData();
            this.m_ItemTable = this.Project.GetDataSheet("demosales_Item").GetData();
            this.m_AnnualSalesTable = this.ResultScenario.GetDataSheet("demosales_AnnualSales").GetData();
            this.m_OutputSalesTable = this.ResultScenario.GetDataSheet("demosales_OutputSales").GetData();

            this.m_AnnualSalesTable.PrimaryKey = new DataColumn[] {
                this.m_AnnualSalesTable.Columns["RegionId"],
                this.m_AnnualSalesTable.Columns["ItemId"]
            };
        }

        protected override void OnIteration(int iteration)
        {
            base.OnIteration(iteration);

            foreach (DataRow RegionRow in this.m_RegionTable.Rows)
            {
                foreach (DataRow ItemRow in this.m_ItemTable.Rows)
                {
                    this.CreateSalesForecast(
                        Convert.ToInt32(RegionRow["RegionId"]),
                        Convert.ToInt32(ItemRow["ItemId"]),
                        iteration);
                }
            }
        }

        private void CreateSalesForecast(int regionId, int itemId, int iteration)
        {
            DataRow InputRow = this.m_AnnualSalesTable.Rows.Find(new object[] { regionId, itemId });

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
                    DataRow dr = this.m_OutputSalesTable.NewRow();
                    int ItemsSold = Convert.ToInt32(this.m_RG.GetUniformDouble(MinItemsSold, MaxItemsSold));

                    dr["Iteration"] = iteration;
                    dr["Timestep"] = Timestep; 
                    dr["RegionId"] = regionId; 
                    dr["ItemId"] = itemId;
                    dr["UnitsSold"] = ItemsSold;
                    dr["GrossRevenue"] = ItemsSold * ItemPrice;

                    this.m_OutputSalesTable.Rows.Add(dr);

                    MinItemsSold = Convert.ToInt32(MinItemsSold * (1 + (PercentIncrease / 100)));
                    MaxItemsSold = Convert.ToInt32(MaxItemsSold * (1 + (PercentIncrease / 100)));
                }                
            }
        }
    }
}
