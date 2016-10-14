<%@ Page Title="Farm Management" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Servers.aspx.cs" Inherits="Caesar.Servers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript" src="script/servers.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class = "row">
        <div class = "offset1 span4">
            <table class = "table table-striped table-bordered" id = "servers">
                <tr>
                    <th>Server</th>
                    <th>Cache</th>
                    <th>Assemblies</th>
                </tr>
            </table>
        </div>
    </div>
    <div class = "row">
        <div class = "offset1 span10" id = "details"></div>
    </div>
</asp:Content>
