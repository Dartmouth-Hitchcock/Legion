<%@ Page Title="Applciations" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Applications.aspx.cs" Inherits="Caesar.Applications" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript" src="script/applications.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<asp:Panel ID = "pnlMain" runat = "server">
    <div class = "row">
        <div class = "offset1 span3">
            <select class = "span3" id = "ddlApplications" size = "30"></select>
        </div>
        <div class = "form-horizontal" id = "applicationDetails">
            <div class = "row">
                <div class = "span8">
                    <div class="control-group">
                        <label class="control-label" for="txtApplicationName">Name</label>
                        <div class="controls">
                            <input type = "text" class = "input-xlarge" id = "txtApplicationName"/>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="txtApplicationKey">API Key</label>
                        <div class="controls">
                            <input type = "text" class = "input-xlarge" id = "txtApplicationKey" disabled = "disabled"/>
                            <button class = "btn" id = "btnGenerateApplicationKey"><i class = "icon-refresh"></i> Generate</button>
                            <button class = "btn" id = "btnRevokeApplicationKey"><i class = "icon-fire"></i> Revoke</button>
                        </div>
                    </div>
                    <div class="control-group" id="rateLimit">
                        <label class="control-label" for="ddlApplicationRateLimitType">Rate Limit</label>
                        <div class="controls">
                            <select class ="input-small" id = "ddlApplicationRateLimitType"></select>
                            <span id ="rateDetails">
                                <input type ="text" class="input-mini" id="txtApplicationRateLimit" /> requests
                                /
                                <select class ="input-small" id = "ddlApplicationRateInterval">
                                    <option value="1">Second</option>
                                    <option value="60">Minute</option>
                                    <option value="3600">Hour</option>
                                    <option value="84600">Day</option>
                                </select>
                                per Legion Host
                            </span>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="txtApplicationIPRange">IP Range</label>
                        <div class="controls">
                            <input type = "text" class = "input-xlarge" id = "txtApplicationIPRange"/>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="">Flags</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkApplicationPublic"/> Public
                            </label>
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkApplicationLogged"/> Logged
                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="txtApplicationDescription">Description</label>
                        <div class="controls">
                            <textarea class = "input-xlarge" id = "txtApplicationDescription" rows = "5" cols="50"></textarea>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="ddlApplicationServices">Permissions</label>
                        <div class="controls">
                            <select id = "ddlApplicationServices" size = "4"></select>
                            <div id = "lstMethods"></div>
                        </div>
                    </div>
                </div>
            </div>
            <div class = "row">
                <div class = "offset8 span3">
                    <button class = "btn btn-primary" id = "btnApplicationUpdate"><i class = "icon-white icon-play"></i> Update</button>
                    <button class = "btn btn-danger" id = "btnApplicationDelete"><i class = "icon-white icon-trash"></i> Delete</button>
                </div>
            </div>
        </div>
    </div>
</asp:Panel>
</asp:Content>
