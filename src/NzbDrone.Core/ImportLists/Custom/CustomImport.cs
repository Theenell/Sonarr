using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists.Custom
{
    public class CustomImport : ImportListBase<CustomSettings>
    {
        private readonly ICustomImportProxy _customProxy;
        public override string Name => _localizationService.GetLocalizedString("ImportListsCustomListSettingsName");

        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(6);

        public override ImportListType ListType => ImportListType.Advanced;

        public CustomImport(ICustomImportProxy customProxy,
                            IImportListStatusService importListStatusService,
                            IConfigService configService,
                            IParsingService parsingService,
                            ILocalizationService localizationService,
                            Logger logger)
            : base(importListStatusService, configService, parsingService, localizationService, logger)
        {
            _customProxy = customProxy;
        }

        public override IList<ImportListItemInfo> Fetch()
        {
            var series = new List<ImportListItemInfo>();

            try
            {
                var remoteSeries = _customProxy.GetSeries(Settings);

                foreach (var item in remoteSeries)
                {
                    series.Add(new ImportListItemInfo
                    {
                        TvdbId = item.TvdbId
                    });
                }

                _importListStatusService.RecordSuccess(Definition.Id);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to fetch data for list {0} ({1})", Definition.Name, Name);

                _importListStatusService.RecordFailure(Definition.Id);
            }

            return CleanupListItems(series);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            return new { };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(_customProxy.Test(Settings));
        }
    }
}
