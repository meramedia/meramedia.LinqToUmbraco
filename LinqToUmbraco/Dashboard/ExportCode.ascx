<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ExportCode.ascx.cs" Inherits="meramedia.Linq.Core.Dashboard.ExportCode" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

    <style type="text/css">
        .label
        {
            width: 150px;
            float: left;
            display: block; 
        } 
    </style>

    <p id="pageName" style="text-align: center;">Export to .NET</p>

    <cc1:Pane ID="pane_contextName" runat="server">
        <div>
            <em class="label">DataContext Name:</em>
            <asp:TextBox ID="txtDataContextName" runat="server" Style="width: 180px;" Text="MyUmbraco" />
        </div>
        <div>
            <em class="label">Namespace:</em>
            <asp:TextBox ID="txtNamespace" runat="server" Style="width: 180px;" Text="MyUmbraco" />
        </div>
    </cc1:Pane>
    <asp:Panel ID="pnlButtons" runat="server" Style="margin-top: 10px;">
        <asp:Button ID="btnGenerate" runat="server" Text="Submit" OnClick="btnGenerate_Click"
            Style="margin-top: 14px" />
        <em>or </em><a href="#" style="color: Blue; margin-left: 6px;" onclick="UmbClientMgr.closeModalWindow()">
            <%=umbraco.ui.Text("cancel")%></a>
    </asp:Panel>
    <cc1:Pane ID="pane_files" runat="server" Visible="false">
        <p>
            <strong>Don't forget to change the extensions to .cs!</strong>
        </p>
        <asp:HyperLink ID="lnkPoco" runat="server" Text="POCO" Target="_blank" />
        <br />
        <asp:HyperLink ID="lnkAbstractions" runat="server" Text="Abstractions" Target="_blank" Enabled="false" />
    </cc1:Pane>
