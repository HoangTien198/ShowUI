using System.Collections.Generic;

namespace ShowUI.Common
{
    public static class ConvertSamplingThroughtput
    {
        public static List<WipPCModel> ConvertSamplingThroughtputFunc(this List<ThroughtputService.WipPCModel> lstService, List<WipPCModel> lstModel)
        {
            foreach (var item in lstService)
            {
                lstModel.Add(new WipPCModel() { STATION_NAME = item.STATION_NAME, WIPTOTAL = item.WIPTOTAL });
            }
            return lstModel;
        }
    }
}