using System;
using System.Globalization;
using Codice.Utils;
using UnityEngine;

namespace Metalitix.Scripts.Editor.Tools.DataInterfaces
{
    public class PageQuery
    {
        public int? page { get; private set; }
        public int? limit { get; private set; }
        public string orderBy { get; private set; }
        public int? filterSetID { get; private set; }
        public DateTime startDate { get; private set; }
        public DateTime endDate { get; private set; }

        private const int MinLimit = 1;
        private const int MaxLimit = 500;
        
        public PageQuery(int page, int limit, OrderBy orderBy, DatePeriod datePeriod = DatePeriod.None)
        {
            this.page = page;
            this.orderBy = orderBy.GetOrder();
            CalculateLimit(limit);

            if (datePeriod == DatePeriod.None)
            {
                startDate = default;
                endDate = default;
            }
            else
            {
                CalculateDate(datePeriod);
            }
        }
        
        public PageQuery(int page, int limit, OrderBy orderBy, int filterSetID, DatePeriod datePeriod = DatePeriod.None)
        {
            this.page = page;
            this.orderBy = orderBy.GetOrder();
            this.filterSetID = filterSetID;
            CalculateLimit(limit);

            if (datePeriod == DatePeriod.None)
            {
                startDate = default;
                endDate = default;
            }
            else
            {
                CalculateDate(datePeriod);
            }
        }

        public PageQuery IncreasedPageRepeat()
        {
            page++;
            return this;
        }

        public string GetQuery(UriBuilder uriBuilder)
        {
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            if(page != null)
                query["page"] = page.ToString();

            if (limit != null)
                query["limit"] = limit.ToString();
            
            if(orderBy != null)
                query["orderBy"] = orderBy;
            
            if (startDate != default)
            {
                query["filter.startDate"] = endDate.ToString("o", CultureInfo.GetCultureInfo("en-GB"));
                query["filter.endDate"] = startDate.ToString("o", CultureInfo.GetCultureInfo("en-GB"));
            }

            if (filterSetID != null)
            {
                query["advancedFilterSet[]"] = filterSetID.ToString();
            }
            
            return query.ToString();
        }
        
        private void CalculateLimit(int limitValue)
        {
            var interpolatedValue = Mathf.InverseLerp(MinLimit, MaxLimit, limitValue);
            var resultLimit = Mathf.RoundToInt(Mathf.Lerp(MinLimit, MaxLimit, interpolatedValue));

            limit = resultLimit;
        }

        private void CalculateDate(DatePeriod datePeriod)
        {
            startDate = DateTime.Now;

            switch (datePeriod)
            {
                case DatePeriod.Past24Hours:
                    endDate = startDate.AddDays(-1);
                    break;
                case DatePeriod.Past7Days:
                    endDate = startDate.AddDays(-7);
                    break;
                case DatePeriod.Past30Days:
                    endDate = startDate.AddDays(-30);
                    break;
                case DatePeriod.All:
                    endDate = startDate.AddYears(-2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(datePeriod), datePeriod, null);
            }
        }
    }
}