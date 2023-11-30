using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Metalitix.Core.Data.Runtime;
using Metalitix.Editor.Data;
using Metalitix.Editor.Web;
using UnityEditor;

namespace Metalitix.Scripts.Dashboard.Editor.Requests.DataGetters
{
#if UNITY_EDITOR
    public class SessionListGetter : DataGetter<SessionsData, SessionData>
    {
        private readonly Dictionary<int, Dictionary<int, SessionData>> _sessionDictionary;
        private readonly Dictionary<int, string[]> _pageHeaders;

        private Dictionary<int, SessionData> _sessionDictionaryInPage;

        public Dictionary<int, string[]> PageHeaders => _pageHeaders;
        public Dictionary<int, Dictionary<int, SessionData>> SessionDictionary => _sessionDictionary;

        public SessionListGetter(DataRequest request, MetalitixBridge metalitixBridge) : base(request, metalitixBridge)
        {
            _sessionDictionary = new Dictionary<int, Dictionary<int, SessionData>>();
            _pageHeaders = new Dictionary<int, string[]>();
        }

        public override async Task<List<SessionData>> GetData()
        {
            do
            {
                _sessionDictionaryInPage = new Dictionary<int, SessionData>();
                data = await MetalitixBridge.GetSessions(ProjectData.id, AuthToken, PageQuery, Source.Token);

                var sessionHeaders = new string[data.items.Length];

                for (var index = 0; index < data.items.Length; index++)
                {
                    var item = data.items[index];

                    var utcTime = item.startDate.ToLocalTime();
                    var startDate = utcTime.ToString("dd MMM yyyy - HH:mm", CultureInfo.GetCultureInfo("en-GB"));
                    var header = $"{startDate}          {item.duration} s";
                    sessionHeaders[index] = header;

                    _sessionDictionaryInPage.Add(index, item);
                    LoadedData.Add(item);
                }

                _pageHeaders.Add(PageQuery.page.Value - 1, sessionHeaders);
                _sessionDictionary.Add(PageQuery.page.Value - 1, _sessionDictionaryInPage);

                await HandleLoading($"Sessions loading for project {ProjectData.title}",
                    data.items.Length, data.pagination.totalItemsCount);
            }
            while (LoadedCount < data.pagination.totalItemsCount && !Source.IsCancellationRequested);

            await Task.Delay(Delay);
            EditorUtility.ClearProgressBar();

            return LoadedData;
        }
    }
#endif
}