using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyBlog.Models
{
    public class GoogleChartModel
    {
        public class MarketSales
        {
            public string Market { get; set; }
            public int Year { get; set; }
            public decimal TotalSales { get; set; }
        }
    }

    public class SalesChartModel
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public GoogleVisualizationDataTable DataTable { get; set; }
    }
}