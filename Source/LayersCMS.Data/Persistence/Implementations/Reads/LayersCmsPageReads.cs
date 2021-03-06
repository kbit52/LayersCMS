﻿using System;
using System.Data.SqlClient;
using System.Linq;
using LayersCMS.Data.Domain.Core.Pages;
using LayersCMS.Data.Persistence.Implementations.Reads.Base;
using LayersCMS.Data.Persistence.Interfaces.Reads;
using LayersCMS.Data.Persistence.Models.Pages;
using System.Collections.Generic;
using System.Data;
using ServiceStack.OrmLite;

namespace LayersCMS.Data.Persistence.Implementations.Reads
{
    public class LayersCmsPageReads : LayersCmsReads<LayersCmsPage>,  ILayersCmsPageReads
    {
        #region Implementation of ILayersCmsPageReads

        public LayersCmsPage GetByUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("url");

            // Only if the url isn't for the home page, format the url
            if (url != "/")
            {
                // If the url doesn't start with a forward slash, prepend one to the url
                if (!url.StartsWith("/"))
                    url = String.Format("/{0}", url);

                // If the url ends with a forward slash, remove it
                url = url.TrimEnd('/');   
            }

            // Query the database
            using (IDbConnection conn = GetDbConnection())
            {
                try
                {
                    return conn.QuerySingle<LayersCmsPage>(new { Url = url });
                }
                catch (SqlException e)
                {
                    throw new Exception("If the LayersCmsPage table doesn't exist, the initial configuration hasn't been completed. Go to the CmsConfig action.", e);
                }
            }
        }

        public IEnumerable<LayersCmsPage> GetCollectionForParent(int? parentId)
        {
            using (IDbConnection conn = GetDbConnection())
            {
                return parentId.HasValue 
                        ? conn.Select<LayersCmsPage>(p => parentId.Value == p.ParentId)
                        : conn.Select<LayersCmsPage>(p => parentId == null);
            }
        }

        public IEnumerable<NavigationPageDetails> GetForNavigation(int? parentId)
        {
            using (IDbConnection conn = GetDbConnection())
            {
                List<NavigationPageDetails> output = conn.Select<NavigationPageDetails>(typeof (LayersCmsPage),
                                                                "Active = {0} AND ShowInNavigation = {1} AND ISNULL(ParentId, 0) = {2} AND PublishStart <= {3} AND (PublishEnd IS NULL OR PublishEnd >= {3})",
                                                                true, true, parentId.GetValueOrDefault(0), DateTime.Now);
                return output.OrderBy(p => p.SortOrder).ToList();
            }
        }

        public int GetMaxSortOrderForParent(int? parentId)
        {
            using (IDbConnection conn = GetDbConnection())
            {
                return conn.Scalar<int>("SELECT MAX(SortOrder) + 1 FROM LayersCmsPage WHERE ISNULL(ParentId, 0) = {0}", parentId.GetValueOrDefault(0));
            }
        }

        #endregion
    }
}
