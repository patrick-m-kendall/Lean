﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Algorithm.Framework;
using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Algorithm.Framework.Selection;
using QuantConnect.Data.Fundamental;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Orders;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// This example algorithm defines its own custom coarse/fine fundamental selection model
    /// combined with the MACD alpha model.
    /// </summary>
    public class CustomFrameworkModelsAlgorithm : QCAlgorithmFramework
    {
        public override void Initialize()
        {
            // Set requested data resolution
            UniverseSettings.Resolution = Resolution.Minute;

            SetStartDate(2013, 10, 07);  //Set Start Date
            SetEndDate(2013, 10, 11);    //Set End Date
            SetCash(100000);             //Set Strategy Cash

            PortfolioSelection = new CustomFundamentalPortfolioSelectionModel();
            Alpha = new MacdAlphaModel(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(30), 0.01m);
            PortfolioConstruction = new SimplePortfolioConstructionModel();
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            if (orderEvent.Status.IsFill())
            {
                Debug($"Purchased Stock: {orderEvent.Symbol}");
            }
        }

        /// <summary>
        /// Defines a custom <see cref="FundamentalPortfolioSelectionModel"/> that takes the top 100 by
        /// dollar volume and then the top 20 by earnings yield
        /// </summary>
        public class CustomFundamentalPortfolioSelectionModel : FundamentalPortfolioSelectionModel
        {
            public CustomFundamentalPortfolioSelectionModel()
                : base(filterFineData: true)
            {
            }

            /// <summary>
            /// Defines the coarse fundamental selection function.
            /// </summary>
            /// <param name="algorithm">The algorithm instance</param>
            /// <param name="coarse">The coarse fundamental data used to perform filtering</param>
            /// <returns>An enumerable of symbols passing the filter</returns>
            public override IEnumerable<Symbol> SelectCoarse(QCAlgorithmFramework algorithm, IEnumerable<CoarseFundamental> coarse)
            {
                return coarse
                    .OrderByDescending(c => c.DollarVolume)
                    .Select(c => c.Symbol)
                    .Take(100);
            }

            /// <summary>
            /// Defines the fine fundamental selection function.
            /// </summary>
            /// <param name="algorithm">The algorithm instance</param>
            /// <param name="fine">The fine fundamental data used to perform filtering</param>
            /// <returns>An enumerable of symbols passing the filter</returns>
            public override IEnumerable<Symbol> SelectFine(QCAlgorithmFramework algorithm, IEnumerable<FineFundamental> fine)
            {
                return fine
                    .OrderByDescending(f => f.ValuationRatios.EarningYield)
                    .Select(f => f.Symbol)
                    .Take(20);
            }
        }
    }
}
