using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using umbraco.IO;
using umbraco.Linq.Core.Dashboard;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.web;
using umbraco.cms.helpers;

namespace meramedia.Linq.Core.Dashboard
{
    public partial class ExportCode : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            btnGenerate.Text = umbraco.ui.Text("create");
        }


        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            var codeGen = new CodeGenerator();

            var GeneratedClasses = string.Format(TemplateConstants.POCO_TEMPLATE,
                txtNamespace.Text,
                txtDataContextName.Text,
                codeGen.GenerateDataContextCollections(),
                codeGen.GenerateClasses()
            );

            // As we save in a new folder under Media, we need to ensure it exists
            EnsureExportFolder();
            string pocoFile = Path.Combine(SystemDirectories.Media + TemplateConstants.EXPORT_FOLDER, txtDataContextName.Text + ".txt");

            using (var writer = new StreamWriter(IOHelper.MapPath(pocoFile)))
            {
                writer.Write(GeneratedClasses);
            }

            lnkPoco.NavigateUrl = pocoFile;

            pnlButtons.Visible = false;
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