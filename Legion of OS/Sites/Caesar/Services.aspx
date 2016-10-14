<%@ Page Title="Services" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Services.aspx.cs" Inherits="Caesar.Services" ClientIDMode = "Static" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript" src="script/services.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<asp:Panel ID = "pnlMain" runat = "server">
    <div class = "row">
        <div class = "offset1 span11">
            <div class = "row" id = "serviceDetails">
                <div class = "span4 form-horizontal">
                    <div class="control-group">
                        <label class="control-label" for="ddlServices">Service</label>
                        <div class="controls">
                            <select class = "input-large" id = "ddlServices"></select>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="ddlServiceAssembly">Assembly</label>
                        <div class="controls">
                            <select class = "input-large" id = "ddlServiceAssembly"></select>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="ddlServiceClasses">Interface Class</label>
                        <div class="controls">
                            <select class = "input-large" id = "ddlServiceClasses"></select>
                        </div>
                    </div>
                </div>
                <div class = "span5 form-horizontal">
                    <div class="control-group">
                        <label class="control-label" for="txtServiceKey">Service Key</label>
                        <div class="controls">
                            <input type = "text" class = "input-large" id = "txtServiceKey"/>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="txtServiceDescription">Description</label>
                        <div class="controls">
                            <input type = "text" class = "input-large" id = "txtServiceDescription"/>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="txtServiceIPRange">IP Range</label>
                        <div class="controls">
                            <input type = "text" class = "input-large" id = "txtServiceIPRange"/>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="chkServiceRestricted">&nbsp;</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkServiceRestricted"/> Restricted
                            </label>
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkServicePublic"/> Public
                            </label>
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkServiceLogged"/> Logged
                            </label>
                        </div>
                    </div>
                </div>
                <div class = "span2">
                    <div class="control-group">
                        <button class = "btn btn-small btn-primary" id = "btnServiceUpdate"><i class = "icon-white icon-play"></i> Update</button>
                        <button class = "btn btn-small btn-danger" id = "btnServiceDelete"><i class = "icon-white icon-trash"></i> Delete</button>
                    </div>
                    <div class="control-group">&nbsp;</div>
                    <div class="control-group">&nbsp;</div>
                    <div class="control-group">
                        <a href="#modalImport" id = "btnShowImportModal" role="button" class="btn btn-small" data-toggle="modal"><i class = "icon-arrow-up"></i> Import</a>
                        <a href="#modalExport" role="button" class="btn btn-small" data-toggle="modal"><i class = "icon-arrow-down"></i> Export</a>
                    </div>
                </div>
            </div>
            <div class = "row">
                <div class = "span11">
                    <h3>Settings</h3>
                </div>
            </div>
            <div class = "row" id = "serviceSettings">
                <div class = "span3">
                    <select class = "span3" id = "ddlServiceSettings" size = "10" disabled = "disabled"></select>
                </div>
                <div class = "span8" id = "serviceSettingDetails">
                    <div class = "row form-horizontal">
                        <div class = "span6">
                            <div class="control-group">
                                <label class="control-label" for="txtServiceSettingName">Setting Name</label>
                                <div class="controls">
                                    <input type = "text" class = "input-xlarge" id = "txtServiceSettingName" disabled = "disabled" />
                                </div>
                            </div>
                            <div class="control-group">
                                <label class="control-label" for="txtServiceSetting">Setting Value</label>
                                <div class="controls">
                                    <textarea class = "input-xlarge" id = "txtServiceSetting" rows = "4" cols = "50" disabled = "disabled"></textarea>
                                </div>
                            </div>
                            <div class="control-group">
                                <label class="control-label" for="chkEncryptServiceSetting">&nbsp;</label>
                                <div class="controls">
                                    <label class="checkbox inline">
                                        <input type = "checkbox" id = "chkEncryptServiceSetting" disabled = "disabled" /> Encrypt
                                    </label>
                                </div>
                            </div>
                        </div>
                        <div class = "span2">
                            <button class = "btn btn-small btn-primary" id = "btnServiceSettingUpdate" disabled = "disabled"><i class = "icon-white icon-play"></i> Update</button>
                            <button class = "btn btn-small btn-danger" id = "btnServiceSettingDelete" disabled = "disabled"><i class = "icon-white icon-trash"></i> Delete</button>
                        </div>
                    </div>
                </div>
            </div>
            <div class = "row">
                <div class = "span11">
                    <h3>Methods</h3>
                </div>
            </div>
            <div class = "row"  id = "serviceMethods">
                <div class = "span3">
                    <select class = "span3" id = "ddlMethods" size = "20" disabled = "disabled"></select>
                </div>
                <div class = "span8" id = "methodDetails">
                    <div class = "row form-horizontal">
                        <div class = "span6">
                            <div class="control-group">
                                <label class="control-label" for="txtMethodName">Method Name</label>
                                <div class="controls">
                                    <input type = "text" class = "input-xlarge" id = "txtMethodName" disabled = "disabled" />
                                </div>
                            </div>
                            <div class="control-group">
                                <label class="control-label" for="txtMethodKey">Method Key</label>
                                <div class="controls">
                                    <input type = "text" class = "input-xlarge" id = "txtMethodKey" disabled = "disabled" />
                                </div>
                            </div>
                            <div class="control-group">
                                <label class="control-label">&nbsp;</label>
                                <div class="controls">
                                    <label class="checkbox inline">
                                        <input type = "checkbox" id = "chkIncludeInLegion" disabled = "disabled" /> Available
                                    </label>
                                    <label class="checkbox inline">
                                        <input type = "checkbox" id = "chkMethodRestricted" disabled = "disabled" /> Restricted
                                    </label>
                                    <label class="checkbox inline">
                                        <input type = "checkbox" id = "chkMethodPublic" disabled = "disabled" /> Public
                                    </label>
                                    <label class="checkbox inline">
                                        <input type = "checkbox" id = "chkMethodLogged" disabled = "disabled" /> Logged
                                    </label>
                                </div>
                            </div>
                            <hr />
                            
                            <div class="control-group">
                                <label class="control-label" for="chkCacheResult">Cache Result</label>
                                <div class="controls">
                                    <label class="checkbox inline">
                                        <input type = "checkbox" id = "chkCacheResult" disabled = "disabled" />
                                    </label>
                                </div>
                            </div>
                            <div class="control-group">
                                <label class="control-label" for="txtMethodKey">Cached Result Lifetime</label>
                                <div class="controls">
                                    <input type = "text" class = "input-xlarge" id = "txtCachedResultLifetime" disabled = "disabled" />
                                </div>
                            </div>
                        </div>
                        <div class = "span2">
                            <button class = "btn btn-small btn-primary" id = "btnMethodUpdate" disabled = "disabled"><i class = "icon-white icon-play"></i> Update</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div id = "modalImport" class="modal hide fade" tabindex="-1">
        <div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
            <h3>Import Service</h3>
        </div>
        <div class="modal-body">
            <div class = "row" id = "importSelectFile">
                <div class = "span4">
                    <h5>Select the sevice file to import:</h5>
                    <input type = "file" id = "filImport" />
                    <div class="progress progress-striped"  style = "display: none;">
                        <div id = "barImport" class="bar" style="width: 0%;"></div>
                    </div>
                </div>
            </div>
            <div class = "row" id = "importSelectElements" style = "display: none;">
                <div class = "span2 form-horizontal">
                    <div class="control-group">
                        <label class="control-label" for="chkImportServiceKey">Service Key</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkImportServiceKey" value = "servicekey"/>
                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="chkImportAssembly">Assembly</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkImportAssembly" value = "assembly"/>
                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="chkImportInterfaceClass">Interface Class</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkImportInterfaceClass" value = "interfaceclass"/>
                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="chkImportDescription">Description</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkImportDescription" value = "description"/>
                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="chkImportIPRange">IP Range</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkImportIPRange" value = "consumeriprange"/>
                            </label>
                        </div>
                    </div>
                </div>
                <div class = "span2 form-horizontal">
                    <div class="control-group">
                        <label class="control-label" for="chkImportFlags">Flags</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkImportFlags" value = "flags"/>
                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="chkImportSettings">Settings</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkImportSettings" value = "settings"/>
                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="chkImportMethods">Methods</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkImportMethods" value = "methods"/>
                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="">&nbsp;</label>
                        <div class="controls">
                            <label class="checkbox inline">

                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="chkImportAll">All</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkImportAll" value = "all"/>
                            </label>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="modal-footer">
            <button class="btn" data-dismiss="modal" aria-hidden="true"><i class = "icon-remove"></i> Cancel</button>
            <button class="btn btn-success" id = "btnImport"><i class = "icon-arrow-up"></i> Import</button>
        </div>
    </div>

    <div id = "modalExport" class="modal hide fade" tabindex="-1">
        <div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
            <h3>Export Service</h3>
        </div>
        <div class="modal-body">
            <div class = "row">
                <div class = "span4">
                    <h5>Select the sevice elements you would like to export:</h5>
                </div>
            </div>
            <div class = "row">
                <div class = "span2 form-horizontal">
                    <div class="control-group">
                        <label class="control-label" for="chkExportServiceKey">Service Key</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkExportServiceKey" value = "servicekey"/>
                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="chkExportAssembly">Assembly</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkExportAssembly" value = "assembly"/>
                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="chkExportInterfaceClass">Interface Class</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkExportInterfaceClass" value = "interfaceclass"/>
                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="chkExportDescription">Description</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkExportDescription" value = "description"/>
                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="chkExportIPRange">IP Range</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkExportIPRange" value = "consumeriprange"/>
                            </label>
                        </div>
                    </div>
                </div>
                <div class = "span2 form-horizontal">
                    <div class="control-group">
                        <label class="control-label" for="chkImportFlage">Flags</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkExportFlags" value = "flags"/>
                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="chkExportSettings">Settings</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkExportSettings" value = "settings"/>
                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="chkExportMethods">Methods</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkExportMethods" value = "methods"/>
                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="">&nbsp;</label>
                        <div class="controls">
                            <label class="checkbox inline">

                            </label>
                        </div>
                    </div>
                    <div class="control-group">
                        <label class="control-label" for="chkExportAll">All</label>
                        <div class="controls">
                            <label class="checkbox inline">
                                <input type = "checkbox" id = "chkExportAll" value = "all"/>
                            </label>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="modal-footer">
            <button class="btn" data-dismiss="modal" aria-hidden="true"><i class = "icon-remove"></i> Cancel</button>
            <button class="btn btn-success" id = "btnExport"><i class = "icon-arrow-down"></i> Export</button>
        </div>
    </div>
</asp:Panel>
</asp:Content>
