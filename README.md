meramedia.LinqToUmbraco
=======================

LinqToUmbraco was removed from Umbraco core, this is a fork of the old code base with improvements so that we can keep using it until something better is in core.

This is a fork to add external (see: preview) XML support to LinqToUmbraco. Make sure to add a partial class with the same name as the generated data context from LinqToUmbraco.

Usage:


        /// <summary>
        /// Instance property which creates a new instance of the UmbracoNodesDataContext class
        /// if the instance has already been created within the current HttpContext then it returns the cached version.
        /// However if the site is in preview mode then it returns the current preview content (this is not cached in the HttpContext)
        /// </summary>
        public static DataContext Instance
        {
            get
            {
                var context = HttpContext.Current.Items["UmbracoDataContext"] as DataContext;

                if (UmbracoContext.Current.InPreviewMode)
                {
                    var umbracoUser = UmbracoContext.Current.UmbracoUser;
                    var previewFileName = umbraco.BusinessLogic.StateHelper.Cookies.Preview.GetValue();
                    var previewPath = String.Format("/App_Data/preview/{0}_{1}.config", umbracoUser.Id, previewFileName);
                    var nodeDataProvider = new NodeDataProvider(previewPath);
                    context = new DataContext { DataProvider = nodeDataProvider };
                }
                else
                {
                    if (context == null)
                    {
                        context = new DataContext();
                        HttpContext.Current.Items["UmbracoDataContext"] = context;
                    }
                }

                return context;
            }
        }

Finally, ensure that the constructor is not protected.
