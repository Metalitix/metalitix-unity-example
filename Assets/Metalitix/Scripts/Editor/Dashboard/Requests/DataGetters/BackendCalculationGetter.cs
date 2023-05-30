using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Metalitix.Scripts.Editor.Tools.DataInterfaces;
using Metalitix.Scripts.Editor.Tools.MetalitixTools;
using Metalitix.Scripts.Runtime.Logger.Extensions;
using UnityEngine;

namespace Metalitix.Scripts.Editor.Dashboard.Requests.DataGetters
{
    public class BackendCalculationGetter
    {
        private readonly int? _filterID;
        private readonly string _periodKey;
        private readonly DataRequest _request;
        private readonly MetalitixBridge _metalitixBridge;

        public BackendCalculationGetter(int? filterID, string periodKey, DataRequest dataRequest, MetalitixBridge metalitixBridge)
        {
            _filterID = filterID;
            _periodKey = periodKey;
            _request = dataRequest;
            _metalitixBridge = metalitixBridge;
        }

        public async Task<(List<Vector4> gazeHeatMap, List<Vector4> positionHeatMap)> GetData()
        {
            HeatMapProcessingResponse[] responses;

            var isFilter = _filterID != null;
            
            if (isFilter)
            {
                responses = await _metalitixBridge.GetFilteredBackendHeatMapProcessing(_filterID.Value, _request.MetalitixProjectData.id,
                    _request.Token, _request.Source.Token);
            }
            else
            {
                responses = await _metalitixBridge.GetBackendHeatMapProcessing(_request.MetalitixProjectData.id,
                    _request.Token, _request.Source.Token);
            }
            
            var gaze = GetTargetData(responses, HeatMapType.heatmap, isFilter);
            var position = GetTargetData(responses, HeatMapType.position, isFilter);

            if (gaze == null || position == null) return (new List<Vector4>(), new List<Vector4>());
            
            var gazeHeatMap = await GetDataFromResponse<List<Vector4>>(gaze);
            var positionHeatMap = await GetDataFromResponse<PositionHeatMap>(position);
            
            return (gazeHeatMap, ParsePositionHeatMap(positionHeatMap.indicators));
        }

        private List<Vector4> ParsePositionHeatMap(Dictionary<string, int> value)
        {
            var data = new List<Vector4>();

            foreach (var keyPair in value)
            {
                string[] vectorValues = keyPair.Key.Split(',');

                var x = float.Parse(vectorValues[0], CultureInfo.InvariantCulture);
                var y = float.Parse(vectorValues[1], CultureInfo.InvariantCulture);
                var z = float.Parse(vectorValues[2], CultureInfo.InvariantCulture);
                var w = keyPair.Value;
                var vector = new Vector4(x, y, z, w);
                data.Add(vector);
            }

            return data;
        }

        private HeatMapProcessingResponse GetTargetData(HeatMapProcessingResponse[] responses, HeatMapType type, bool isFilter)
        {
            var stringType = type.ToString();
            HeatMapProcessingResponse response;

            try
            {
                if (isFilter)
                {
                    response = responses
                        .Where(r => r.heatmapType.Equals(stringType))
                        .Select(r => r)
                        .First();
                }
                else
                {
                    if (String.IsNullOrEmpty(_periodKey)) throw new Exception("Period is empty");
                    
                    response = responses
                        .Where(r => r.timespan.Equals(_periodKey) && r.heatmapType.Equals(stringType))
                        .Select(r => r)
                        .First();
                }
            }
            catch (Exception e)
            {
                MetalitixDebug.Log(this, e.Message);
                return null;
            }

            return response;
        }

        private async Task<T> GetDataFromResponse<T>(HeatMapProcessingResponse response)
        {
            T data = default;

            if (response != null)
            {
                // var gazeStatus = Enum.Parse<HeatMapStatus>(response.status);

                data = await _metalitixBridge.GetFromBucket<T>(response.uploadUrl, _request.Source.Token);
            }

            return data;
        }
    }
}