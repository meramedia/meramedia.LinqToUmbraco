using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using meramedia.Linq.Core.Node;
using meramedia.Linq.Core.Dashboard;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.web;
using umbraco.cms.helpers;
using umbraco.IO;

namespace meramedia.Linq.Core.Dashboard
{
    public partial class ExportCode : UserControl
    {
        private static string _namespace;

        protected void Page_Load(object sender, EventArgs e)
        {            
            btnGenerate.Text = umbraco.ui.Text("create");
            btnFlushCache.Click += btnFlushCache_Click;

            lblNumItemsMediaCache.Text = NodeCacheStatistics.Instance.NumItemsInMediaCache().ToString();
            lblNumItemsNodeCache.Text = NodeCacheStatistics.Instance.NumNodesInNodeCache().ToString();
            lblNumTreesNodeCache.Text = NodeCacheStatistics.Instance.NumTreesInNodeCache().ToString();

            if (!IsPostBack)
            {
                if (!String.IsNullOrEmpty(_namespace))
                    txtNamespace.Text = _namespace;

            }

        }

        void btnFlushCache_Click(object sender, EventArgs e)
        {
            var provider = new NodeDataProvider();
            provider.Flush();

            lblNumItemsMediaCache.Text = NodeCacheStatistics.Instance.NumItemsInMediaCache().ToString();
            lblNumItemsNodeCache.Text = NodeCacheStatistics.Instance.NumNodesInNodeCache().ToString();
            lblNumTreesNodeCache.Text = NodeCacheStatistics.Instance.NumTreesInNodeCache().ToString();
        }


        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            _namespace = txtNamespace.Text;


            var codeGen = new CodeGenerator();            

            var generatedClasses = string.Format(TemplateConstants.POCO_TEMPLATE,
                txtNamespace.Text,
                codeGen.GenerateDataContextCollections(),
                codeGen.GenerateClasses()
            );

            // As we save in a new folder under Media, we need to ensure it exists
            EnsureExportFolder();
            string pocoFile = Path.Combine(SystemDirectories.Media + TemplateConstants.EXPORT_FOLDER, "DataContext.txt");

            using (var writer = new StreamWriter(IOHelper.MapPath(pocoFile)))
            {
                writer.Write(generatedClasses);
            }

            lnkPoco.NavigateUrl = pocoFile;

            //pnlButtons.Visible = false;
            pane_files.Visible = true;
        }



        private static void EnsureExportFolder()
        {
            string packagesDirectory = SystemDirectories.Media + TemplateConstants.EXPORT_FOLDER;
            if (!Directory.Exists(IOHelper.MapPath(packagesDirectory)))
                Directory.CreateDirectory(IOHelper.MapPath(packagesDirectory));
        }
    }
}