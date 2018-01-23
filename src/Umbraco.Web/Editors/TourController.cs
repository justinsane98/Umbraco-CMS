﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class TourController : UmbracoAuthorizedJsonController
    {
        public IEnumerable<BackOfficeTourFile> GetTours()
        {
            var result = new List<BackOfficeTourFile>();

            if (UmbracoConfig.For.UmbracoSettings().BackOffice.Tours.EnableTours == false)
                return result;

            var filters = TourFilterResolver.Current.Filters.ToList();
            
            //get all filters that will be applied to all tour aliases
            var aliasOnlyFilters = filters.Where(x => x.PluginName == null && x.TourFileName == null).ToList();

            //don't pass in any filters for core tours that have a plugin name assigned
            var nonPluginFilters = filters.Where(x => x.PluginName == null).ToList();

            //add core tour files
            var coreToursPath = Path.Combine(IOHelper.MapPath(SystemDirectories.Config), "BackOfficeTours");
            if (Directory.Exists(coreToursPath))
            {
                foreach (var tourFile in Directory.EnumerateFiles(coreToursPath, "*.json"))
                {
                    TryParseTourFile(tourFile, result, nonPluginFilters, aliasOnlyFilters);
                }
            }

            //collect all tour files in packages
            foreach (var plugin in Directory.EnumerateDirectories(IOHelper.MapPath(SystemDirectories.AppPlugins)))
            {
                var pluginName = Path.GetFileName(plugin.TrimEnd('\\'));
                var pluginFilters = filters.Where(x => x.PluginName != null && x.PluginName.IsMatch(pluginName)).ToList();

                //If there is any filter applied to match the plugin only (no file or tour alias) then ignore the plugin entirely
                var isPluginFiltered = pluginFilters.Any(x => x.TourFileName == null && x.TourAlias == null);
                if (isPluginFiltered) continue;

                //combine matched package filters with filters not specific to a package
                var combinedFilters = nonPluginFilters.Concat(pluginFilters).ToList();

                foreach (var backofficeDir in Directory.EnumerateDirectories(plugin, "backoffice"))
                {
                    foreach (var tourDir in Directory.EnumerateDirectories(backofficeDir, "tours"))
                    {
                        foreach (var tourFile in Directory.EnumerateFiles(tourDir, "*.json"))
                        {
                            TryParseTourFile(tourFile, result, combinedFilters, aliasOnlyFilters, pluginName);
                        }
                    }
                }
            }

            return result.OrderBy(x => x.FileName, StringComparer.InvariantCultureIgnoreCase);
        }

        private void TryParseTourFile(string tourFile,
            ICollection<BackOfficeTourFile> result,
            List<BackOfficeTourFilter> filters,
            List<BackOfficeTourFilter> aliasOnlyFilters,
            string pluginName = null)
        {
            var fileName = Path.GetFileNameWithoutExtension(tourFile);
            if (fileName == null) return;

            //get the filters specific to this file
            var fileFilters = filters.Where(x => x.TourFileName != null && x.TourFileName.IsMatch(fileName)).ToList();
            
            //If there is any filter applied to match the file only (no tour alias) then ignore the file entirely
            var isFileFiltered = fileFilters.Any(x => x.TourAlias == null);
            if (isFileFiltered) return;

            //now combine all aliases to filter below
            var aliasFilters = aliasOnlyFilters.Concat(filters.Where(x => x.TourAlias != null))
                .Select(x => x.TourAlias)
                .ToList();

            try
            {
                var contents = File.ReadAllText(tourFile);
                var tours = JsonConvert.DeserializeObject<BackOfficeTour[]>(contents);

                var tour = new BackOfficeTourFile
                {
                    FileName = Path.GetFileNameWithoutExtension(tourFile),
                    PluginName = pluginName,
                    Tours = tours
                        .Where(x => aliasFilters.Count == 0 || aliasFilters.All(filter => filter.IsMatch(x.Alias)) == false)
                        .ToArray()
                };

                //don't add if all of the tours are filtered
                if (tour.Tours.Any())
                    result.Add(tour);
            }
            catch (IOException e)
            {
                throw new IOException("Error while trying to read file: " + tourFile, e);
            }
            catch (JsonReaderException e)
            {
                throw new JsonReaderException("Error while trying to parse content as tour data: " + tourFile, e);
            }
        }
    }
}