<%@ Page Title="Tester" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Tester.aspx.cs" Inherits="Caesar.Tester" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript" src="script/tester.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class = "row" id = "tester">
        <div class = "span12">
            <div class = "row">
                <div class = "offset1 span7">
                    <div class = "row form-horizontal">
                        <div class="control-group">
                            <label class="control-label" for="ddlApplications">Application</label>
                            <div class="controls">
                                <select id = "ddlApplications"></select>
                            </div>
                        </div>
                        <div class="control-group">
                            <label class="control-label" for="ddlServices">Service</label>
                            <div class="controls">
                                <select id = "ddlServices"></select>
                            </div>
                        </div>
                        <div class="control-group">
                            <label class="control-label" for="ddlMethods">Method</label>
                            <div class="controls">
                                <select id = "ddlMethods"></select>
                            </div>
                        </div>
                        <div class="control-group">
                            <label class="control-label" for="ddlMethods">Parameters</label>
                            <div class="controls">
                                <table class = "table" id = "parameters"></table>
                            </div>
                        </div>
                    </div>
                    <div id = "row">
                        <div class = "offset5 span4">
                            <button class = "btn" id = "btnAddParameter"><i class = "icon-plus"></i> Add Parameter</button>
                            <button class = "btn btn-success" id = "btnCallMethod"><i class = "icon-white icon-phone"></i> Call Method</button>
                        </div>
                    </div>
                </div>
            </div>
            <hr />
            <div class = "row" id = "exception" style = "display: none;">
                <div class = "offset1 span11">
                    <div class="alert alert-error">
                        <h4>Local Exception</h4>
                        <textarea class = "span11" id = "txtException"></textarea>
                    </div>
                </div>
            </div>
            <div class = "row" id = "fault" style = "display: none;">
                <div class = "offset1 span11">
                    <div class="alert alert-error">
                        <strong>Fault:</strong>
                        <span id = "txtFault"></span>
                    </div>
                </div>
            </div>
            <div class = "row" id = "error" style = "display: none;">
                <div class = "offset1 span11">
                    <div class="alert">
                        <strong>Error:</strong>
                        <span id = "txtError"></span>
                    </div>
                </div>
            </div>
            <div class = "row">
                <div class = "offset1 span4">
                    <h4>Response</h4>
                    <textarea class = "span4" id = "txtResponse" rows = "10" wrap = "off"></textarea>
                </div>
                <div class = "span7">
                    <h4>Result</h4>
                    <textarea class = "span7" id = "txtResult" rows = "10" wrap = "off"></textarea><br />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
