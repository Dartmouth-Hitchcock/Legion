/**
 *	Copyright 2016 Dartmouth-Hitchcock
 *	
 *	Licensed under the Apache License, Version 2.0 (the "License");
 *	you may not use this file except in compliance with the License.
 *	You may obtain a copy of the License at
 *	
 *	    http://www.apache.org/licenses/LICENSE-2.0
 *	
 *	Unless required by applicable law or agreed to in writing, software
 *	distributed under the License is distributed on an "AS IS" BASIS,
 *	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *	See the License for the specific language governing permissions and
 *	limitations under the License.
 */

$(document).ready(function () {
    loadServices();
    loadAssemblies();

    $('#serviceMethods input, #serviceMethods select, #serviceMethods button, #serviceConnectionStrings input, #serviceConnectionStrings button, #serviceConnectionStrings textarea, #serviceConnectionStrings select').attr('disabled', 'disabled');

    $('#ddlServices').change(loadService);
    $('#ddlMethods').change(loadMethodDetails);
    $('#ddlServiceAssembly').change(loadClasses);
    $('#ddlServiceClasses').change(loadMethods);
    $('#ddlServiceSettings').change(loadServiceSetting);
    $('#btnServiceUpdate').click(updateService);
    $('#btnServiceDelete').click(deleteService);
    $('#btnMethodUpdate').click(updateMethod);
    $('#btnServiceSettingUpdate').click(updateServiceSetting);
    $('#btnServiceSettingDelete').click(deleteServiceSetting);
    $('#chkIncludeInLegion').change(includeMethod);
    $('input[type=checkbox][id^=chkImport]').change(importCheck);
    $('input[type=checkbox][id^=chkExport]').change(exportCheck);

    $('#chkServiceRestricted').change(function () {
        if ($('#chkServiceRestricted').attr('checked') == 'checked') {
            $('#chkMethodRestricted').attr('disabled', 'disabled');
            $('#chkMethodRestricted').removeAttr('checked');
        }
        else
            $('#chkMethodRestricted').removeAttr('disabled');
    });

    $('#chkServicePublic').change(function () {
        if ($('#chkServicePublic').attr('checked') == 'checked') {
            $('#chkMethodPublic').attr('disabled', 'disabled');
            $('#chkMethodPublic').removeAttr('checked');
        }
        else
            $('#chkMethodPublic').removeAttr('disabled');
    });

    $('#chkServiceLogged').change(function () {
        if ($('#chkServiceLogged').attr('checked') == 'checked') {
            $('#chkMethodLogged').attr('disabled', 'disabled');
            $('#chkMethodLogged').removeAttr('checked');
        }
        else
            $('#chkMethodLogged').removeAttr('disabled');
    });

    $('#filImport').change(loadImportFile);
    $('#btnShowImportModal').click(function () {
        $('#importSelectFile,#filImport').show();
        $('#importSelectElements').hide();
        $('#barImport').parent().hide();
        $('#btnImport').attr('disabled', 'disabled');
    });

    $('#btnImport').click(importService);
    $('#btnExport').click(exportService);

    var serviceid = _GET('serviceid');
    if (serviceid.length > 0) {
        $.blockUI();
        setTimeout("$('#ddlServices').val(" + serviceid + "); loadService();", 1500);
    }
});

function loadServices() {
    $('#ddlServices').empty().append($('<option/>').html('loading...'));

    $.ajax({
        url: 'ajax.aspx',
        data: ({
            'handler': 'passthru',
            '__passthrumethod': 'getServiceList',
            'nocache': (new Date()).getTime()
        }),
        type: 'POST',
        dataType: 'xml',
        success: function (xml) {
            $('#ddlServices').empty().append($('<option/>').html('&nbsp;').attr('value', '--').data('id', '--'));

            $(xml).find('service').each(function () {
                var option = $('<option/>')
                                .html($(this).find('key').text())
                                .attr('value', $(this).find('id').text())
                                .data('id', $(this).find('id').text())
                                .data('key', $(this).find('key').text())
                                .data('assembly', $(this).find('assembly').text())
                                .data('class', $(this).find('class').text())
                                .data('restricted', $(this).find('restricted').text())
                                .data('public', $(this).find('public').text())
                                .data('logged', $(this).find('logged').text())
                                .data('consumeriprange', $(this).find('consumeriprange').text())
                                .data('description', $(this).find('description').text());

                $('#ddlServices').append(option);
            });

            $('#ddlServices').append($('<option/>').html('-- new --').attr('value', '-1').data('id', '-1'));
        }
    });
}

function loadAssemblies() {
    $.ajax({
        url: 'ajax.aspx',
        data: ({
            'handler': 'getAssemblies',
            'nocache': (new Date()).getTime()
        }),
        type: 'POST',
        dataType: 'xml',
        success: function (xml) {
            $('#ddlServiceAssembly').empty().append($('<option/>').html('').attr('value', ''));

            $(xml).find('assembly').each(function () {
                var option = $('<option/>')
                        .html($(this).text())
                        .attr('value', $(this).text());

                $('#ddlServiceAssembly').append(option);
            });
        }
    });
}

function loadClasses() {
    var assembly = $('#ddlServiceAssembly>option:selected');
    
    $.blockUI();

    $.ajax({
        url: 'ajax.aspx',
        data: ({
            'handler': 'getAssemblyManifest',
            'assembly': $(assembly).val(),
            'nocache': (new Date()).getTime()
        }),
        type: 'POST',
        dataType: 'xml',
        success: function (xml) {
            $('#ddlServiceClasses').empty().append($('<option/>').html('').attr('value', ''));

            $(xml).find('class').each(function () {
                var option = $('<option/>')
                        .html($(this).text())
                        .attr('value', $(this).text());

                $('#ddlServiceClasses').append(option);
            });

            var c = $('#ddlServices>option:selected').data('class');
            if (c != undefined && c.length > 0)
                $('#ddlServiceClasses').val(c);
            else
                $('#ddlServiceClasses').val('Methods');

            loadMethods();

            $.unblockUI();
        }
    });
}

function loadService() {
    var service = $('#ddlServices>option:selected');

    if ($(service).attr('value')[0] != '-') {
        $('#txtServiceKey').val($(service).data('key'));
        $('#txtServiceDescription').val($(service).data('description'));
        $('#txtServiceIPRange').val($(service).data('consumeriprange'));
        $('#ddlServiceAssembly').val($(service).data('assembly'));
        $('#ddlServiceClasses').val($(service).data('class'));

        if ($(service).data('restricted') == 'true')
            $('#chkServiceRestricted').attr('checked', 'checked');
        else
            $('#chkServiceRestricted').removeAttr('checked');

        if ($(service).data('public') == 'true')
            $('#chkServicePublic').attr('checked', 'checked');
        else
            $('#chkServicePublic').removeAttr('checked');

        if ($(service).data('logged') == 'true')
            $('#chkServiceLogged').attr('checked', 'checked');
        else
            $('#chkServiceLogged').removeAttr('checked');

        $('#btnServiceUpdate').attr('value', 'Update');
        $('#btnServiceDelete').fadeIn();

        $('#ddlMethods').empty().append($('<option/>').html('loading...'));
        $('#ddlServiceSettings').empty().append($('<option/>').html('loading...'));

        $('#txtServiceSettingName, #txtServiceSetting, #txtMethodKey, #txtMethodName').val('');
        $('#chkEncryptServiceSetting, #chkMethodRestricted, #chkMethodPublic, #chkMethodLogged, #chkIncludeInLegion, #chkCacheResult').removeAttr('checked');

        $.blockUI();

        loadClasses();
        loadServiceSettings();

        $('#serviceMethods input, #serviceMethods select, #serviceMethods button, #serviceSettings input, #serviceSettings button, #serviceSettings textarea, #serviceSettings select').not('#txtMethodName').removeAttr('disabled');
    }
    else {
        $('#ddlMethods').empty();
        $('#txtServiceKey, #txtServiceDescription, #txtServiceIPRange, #ddlServiceAssembly, #ddlServiceClasses, #txtServiceSettingName, #txtServiceSetting, #txtMethodKey, #txtMethodName').val('');
        $('#chkServiceRestricted, #chkServicePublic, #chkServiceLogged, #chkEncryptServiceSetting, #chkMethodRestricted, #chkMethodPublic, #chkMethodLogged, #chkIncludeInLegion, #chkCacheResult').removeAttr('checked');

        if ($(service).attr('value') == '-1') {
            $('#serviceMethods input, #serviceMethods select, #serviceMethods button, #serviceSettings input, #serviceSettings textarea, #serviceSettings select, #serviceSettings button').attr('disabled', 'disabled');
            $('#btnServiceSettingUpdate').attr('value', 'Create');
            $('#btnServiceSettingDelete').fadeOut();
        }
    }

    if($('#chkServiceRestricted').attr('checked') == 'checked')
        $('#chkMethodRestricted').attr('disabled', 'disabled');

    if ($('#chkServicePublic').attr('checked') == 'checked')
        $('#chkMethodPublic').attr('disabled', 'disabled');

    if ($('#chkServiceLogged').attr('checked') == 'checked')
        $('#chkMethodLogged').attr('disabled', 'disabled');
}

function loadMethods() {
    var service = $('#ddlServices>option:selected');

    $.blockUI();

    $.ajax({
        url: 'ajax.aspx',
        data: ({
            'handler': 'getClassManifest',
            'assembly': $('#ddlServiceAssembly').val(),
            'class': $('#ddlServiceClasses').val(),
            'nocache': (new Date()).getTime()
        }),
        type: 'POST',
        dataType: 'xml',
        success: function (xml) {
            $('#ddlMethods').empty();

            $(xml).find('method').each(function () {
                var option = $('<option/>')
                                .html($(this).text())
                                .attr('value', $(this).text())
                                .addClass('notincluded');

                $('#ddlMethods').append(option);
            });

            $.ajax({
                url: 'ajax.aspx',
                data: ({
                    'handler': 'passthru',
                    '__passthrumethod': 'getServiceMethods',
                    'serviceId': $(service).attr('value'),
                    'nocache': (new Date()).getTime()
                }),
                type: 'POST',
                dataType: 'xml',
                success: function (xml) {
                    $(xml).find('method').each(function () {
                        var option = $('#ddlMethods>option[value=' + $(this).find('name').text() + ']');
                        $(option)
                                .removeClass('notincluded')
                                .data('id', $(this).find('id').text())
                                .data('key', $(this).find('key').text())
                                .data('name', $(this).find('name').text())
                                .data('restricted', $(this).find('restricted').text())
                                .data('public', $(this).find('public').text())
                                .data('logged', $(this).find('logged').text())
                                .data('cacheresult', $(this).find('cacheresult').text())
                                .data('cachedresultlifetime', $(this).find('cachedresultlifetime').text());
                    });

                    $.unblockUI();
                }
            });
        }
    });
}

function loadMethodDetails() {
    var method = $('#ddlMethods>option:selected');
    if ($(method).attr('value')[0] != '-') {
        $('#txtMethodName').val($(method).val());

        if (!$(method).hasClass('notincluded')) {
            $('#txtMethodKey').val($(method).data('key'));

            if ($('#chkServiceRestricted').attr('checked') == 'checked') {
                $('#chkMethodRestricted').attr('disabled', 'disabled');
                $('#chkMethodRestricted').removeAttr('checked');                
            }
            else if ($(method).data('restricted') == 'true')
                $('#chkMethodRestricted').attr('checked', 'checked');
            else
                $('#chkMethodRestricted').removeAttr('checked');

            if ($('#chkServicePublic').attr('checked') == 'checked') {
                $('#chkMethodPublic').attr('disabled', 'disabled');
                $('#chkMethodPublic').removeAttr('checked');
            }
            else if ($(method).data('public') == 'true')
                $('#chkMethodPublic').attr('checked', 'checked');
            else
                $('#chkMethodPublic').removeAttr('checked');

            if ($('#chkServiceLogged').attr('checked') == 'checked') {
                $('#chkMethodLogged').attr('disabled', 'disabled');
                $('#chkMethodLogged').removeAttr('checked');
            }
            else if ($(method).data('logged') == 'true')
                $('#chkMethodLogged').attr('checked', 'checked');
            else
                $('#chkMethodLogged').removeAttr('checked');

            if ($(method).data('cacheresult') == 'true') {
                $('#chkCacheResult').attr('checked', 'checked');
                $('#txtCachedResultLifetime').val($(method).data('cachedresultlifetime'));
            }
            else {
                $('#chkCacheResult').removeAttr('checked');
                $('#txtCachedResultLifetime').val('');
            }

            $('#chkIncludeInLegion').attr('checked', 'checked');
            $('#btnMethodUpdate').attr('value', 'Update');
        }
        else {
            $('#txtMethodKey').val($(method).val().toLowerCase());
            $('#txtCachedResultLifetime').val('');
            $('#chkMethodRestricted, #chkMethodPublic, #chkMethodLogged, #chkIncludeInLegion, #chkCacheResult').removeAttr('checked');
            $('#btnMethodUpdate').attr('value', 'Update');
        }
    }
    else {
        $('#txtMethodKey, #txtMethodName, #txtCachedResultLifetime').val('');
        $('#chkMethodRestricted, #chkMethodPublic, #chkMethodLogged, #chkIncludeInLegion, #chkCacheResult').removeAttr('checked');

        if ($(method).attr('value') == '-1') //new
            $('#btnMethodUpdate').attr('value', 'Create');
    }
}

function updateService(event) {
    preventDefault(event);

    var service = $('#ddlServices>option:selected');

    $.blockUI();

    $.ajax({
        url: 'ajax.aspx',
        data: ({
            'handler': 'passthru',
            '__passthrumethod': 'updateService',
            'id': $(service).data('id'),
            'servicekey': $('#txtServiceKey').val(),
            'description': $('#txtServiceDescription').val(),
            'consumeriprange': $('#txtServiceIPRange').val(),
            'assembly': $('#ddlServiceAssembly').val(),
            'class': $('#ddlServiceClasses').val(),
            'restricted': ($('#chkServiceRestricted').is(':checked') ? 'true' : 'false'),
            'public': ($('#chkServicePublic').is(':checked') ? 'true' : 'false'),
            'logged': ($('#chkServiceLogged').is(':checked') ? 'true' : 'false'),
            'nocache': (new Date()).getTime()
        }),
        type: 'POST',
        dataType: 'xml',
        success: function (xml) {
            if ($(xml).find('resultcode').length == 1) {
                if ($(xml).find('resultcode').text() == 'SUCCESS') {
                    if ($(service).data('id') == '-1') {
                        var option = $('<option/>')
                                    .html($('#txtServiceKey').val())
                                    .attr('value', $(xml).find('id').text())
                                    .data('id', $(xml).find('id').text())
                                    .data('key', $('#txtServiceKey').val())
                                    .data('description', $('#txtServiceDescription').val())
                                    .data('consumeriprange', $('#txtServiceIPRange').val())
                                    .data('assembly', $('#ddlServiceAssembly').val())
                                    .data('class', $('#ddlServiceClasses').val())
                                    .data('restricted', ($('#chkServiceRestricted').is(':checked') ? 'true' : 'false'))
                                    .data('public', ($('#chkServicePublic').is(':checked') ? 'true' : 'false'))
                                    .data('logged', ($('#chkServiceLogged').is(':checked') ? 'true' : 'false'));

                        $('#ddlServices').append(option);
                        $('#ddlServices').val($(xml).find('id').text()).change();
                    }
                    else {
                        $(service)
                            .html($('#txtServiceKey').val())
                            .data('key', $('#txtServiceKey').val())
                            .data('description', $('#txtServiceDescription').val())
                            .data('consumeriprange', $('#txtServiceIPRange').val())
                            .data('assembly', $('#ddlServiceAssembly').val())
                            .data('class', $('#ddlServiceClasses').val())
                            .data('restricted', ($('#chkServiceRestricted').is(':checked') ? 'true' : 'false'))
                            .data('public', ($('#chkServicePublic').is(':checked') ? 'true' : 'false'))
                            .data('logged', ($('#chkServiceLogged').is(':checked') ? 'true' : 'false'));
                    }
                }
                else
                    alert($(xml).find('resultcode').text());
            }
            else
                alert('unexpected result');

            $.unblockUI();
        }
    });
}

function deleteService(event) {
    preventDefault(event);

    var service = $('#ddlServices>option:selected');

    $.blockUI();

    $.ajax({
        url: 'ajax.aspx',
        data: ({
            'handler': 'passthru',
            '__passthrumethod': 'deleteService',
            'id': $(service).data('id'),
            'nocache': (new Date()).getTime()
        }),
        type: 'POST',
        dataType: 'xml',
        success: function (xml) {
            if ($(xml).find('resultcode').length == 1) {
                if ($(xml).find('resultcode').text() == 'SUCCESS') {
                    $('#txtServiceKey').val('');
                    $('#txtServiceDescription').val('');
                    $('#ddlServiceAssembly').val('');
                    $('#ddlServiceClasses').val('');
                    $('#chkServiceRestricted').removeAttr('checked');
                    $('#chkServiceLogged').removeAttr('checked');
                    $('#ddlMethods').empty();
                    $(service).remove();
                }
                else
                    alert($(xml).find('resultcode').text());
            }
            else
                alert('unexpected result');

            $.unblockUI();
        }
    });
}

function updateMethod(event) {
    preventDefault(event);

    var method = $('#ddlMethods>option:selected');
    $.blockUI();

    if ($('#chkIncludeInLegion').is(':checked')) {
        $.ajax({
            url: 'ajax.aspx',
            data: ({
                'handler': 'passthru',
                '__passthrumethod': 'updateServiceMethod',
                'id': ($(method).data('id') == undefined ? '-1' : $(method).data('id')),
                'serviceid': $('#ddlServices').val(),
                'methodkey': $('#txtMethodKey').val(),
                'methodname': $('#txtMethodName').val(),
                'cachedresultlifetime': $('#txtCachedResultLifetime').val(),
                'cacheresult': ($('#chkCacheResult').is(':checked') ? 'true' : 'false'),
                'restricted': ($('#chkMethodRestricted').is(':checked') ? 'true' : 'false'),
                'public': ($('#chkMethodPublic').is(':checked') ? 'true' : 'false'),
                'logged': ($('#chkMethodLogged').is(':checked') ? 'true' : 'false'),
                'nocache': (new Date()).getTime()
            }),
            type: 'POST',
            dataType: 'xml',
            success: function (xml) {
                if ($(xml).find('resultcode').length == 1) {
                    if ($(xml).find('resultcode').text() == 'SUCCESS') {
                        if ($(method).data('id') == undefined) {
                            $(method)
                                .removeClass('notincluded')
                                .data('id', $(xml).find('id').text())
                                .data('key', $('#txtMethodKey').val())
                                .data('name', $('#txtMethodName').val())
                                .data('cachedresultlifetime', $('#txtCachedResultLifetime').val())
                                .data('cacheresult', ($('#chkCacheResult').is(':checked') ? 'true' : 'false'))
                                .data('restricted', ($('#chkMethodRestricted').is(':checked') ? 'true' : 'false'))
                                .data('public', ($('#chkMethodPublic').is(':checked') ? 'true' : 'false'))
                                .data('logged', ($('#chkMethodLogged').is(':checked') ? 'true' : 'false'));
                        }
                        else {
                            $(method)
                                .data('key', $('#txtMethodKey').val())
                                .data('name', $('#txtMethodName').val())
                                .data('cachedresultlifetime', $('#txtCachedResultLifetime').val())
                                .data('cacheresult', ($('#chkCacheResult').is(':checked') ? 'true' : 'false'))
                                .data('restricted', ($('#chkMethodRestricted').is(':checked') ? 'true' : 'false'))
                                .data('public', ($('#chkMethodPublic').is(':checked') ? 'true' : 'false'))
                                .data('logged', ($('#chkMethodLogged').is(':checked') ? 'true' : 'false'));
                        }
                    }
                    else
                        alert($(xml).find('resultcode').text());
                }
                else
                    alert('unexpected result');

                $.unblockUI();
            }
        });
    }
    else {
        $.ajax({
            url: 'ajax.aspx',
            data: ({
                'handler': 'passthru',
                '__passthrumethod': 'deleteServiceMethod',
                'id': $(method).data('id'),
                'nocache': (new Date()).getTime()
            }),
            type: 'POST',
            dataType: 'xml',
            success: function (xml) {
                if ($(xml).find('resultcode').length == 1) {
                    if ($(xml).find('resultcode').text() == 'SUCCESS') {
                        $('#txtMethodName, #txtCachedResultLifetime').val('');
                        $('#chkMethodRestricted, #chkMethodPublic, #chkMethodLogged, #chkCacheResult').removeAttr('checked');
                        $(method).removeData().addClass('notincluded');
                    }
                    else
                        alert($(xml).find('resultcode').text());
                }
                else
                    alert('unexpected result');

                $.unblockUI();
            }
        });
    }
}

function loadServiceSettings() {
    var service = $('#ddlServices>option:selected');

    $.ajax({
        url: 'ajax.aspx',
        data: ({
            'handler': 'passthru',
            '__passthrumethod': 'getServiceSettings',
            'id': $(service).data('id'),
            'nocache': (new Date()).getTime()
        }),
        type: 'POST',
        dataType: 'xml',
        success: function (xml) {
            if ($(xml).find('resultcode').length == 1) {
                if ($(xml).find('resultcode').text() == 'SUCCESS') {
                    $('#ddlServiceSettings').empty();

                    $(xml).find('setting').each(function () {
                        var option = $('<option/>')
                                        .html($(this).find('name').text())
                                        .attr('value', $(this).find('id').text())
                                        .data('id', $(this).find('id').text())
                                        .data('name', $(this).find('name').text())
                                        .data('setting', $(this).find('value').text())
                                        .data('encrypted', $(this).find('encrypted').text());

                        $('#ddlServiceSettings').append(option);
                    });

                    $('#ddlServiceSettings').append($('<option/>').html('-- new --').attr('value', '-1').data('id', '-1'));
                }
                else
                    alert($(xml).find('resultcode').text());
            }
            else
                alert('unexpected result');

            $.unblockUI();
        }
    });
}

function loadServiceSetting() {
    var cs = $('#ddlServiceSettings>option:selected');

    $('#txtServiceSettingName').val(cs.data('name'));
    $('#txtServiceSetting').val(cs.data('setting'));

    if ($(cs).data('encrypted') == 'true')
        $('#chkEncryptServiceSetting').attr('checked', 'checked');
    else
        $('#chkEncryptServiceSetting').removeAttr('checked');

    if ($(cs).val() == '-1') {
        $('#btnServiceSettingUpdate').attr('value', 'Create');
        $('#btnServiceSettingDelete').fadeOut();
    }
    else{
        $('#btnServiceSettingUpdate').attr('value', 'Update');
        $('#btnServiceSettingDelete').fadeIn();
    }
}

function updateServiceSetting(event) {
    preventDefault(event);

    var cs = $('#ddlServiceSettings>option:selected');
    $.blockUI();

    $.ajax({
        url: 'ajax.aspx',
        data: ({
            'handler': 'passthru',
            '__passthrumethod': 'updateServiceSetting',
            'id': $(cs).data('id'),
            'serviceid': $('#ddlServices').val(),
            'name': $('#txtServiceSettingName').val(),
            'value': $('#txtServiceSetting').val(),
            'encrypted': ($('#chkEncryptServiceSetting').is(':checked') ? 'true' : 'false'),
            'nocache': (new Date()).getTime()
        }),
        type: 'POST',
        dataType: 'xml',
        success: function (xml) {
            if ($(xml).find('resultcode').length == 1) {
                if ($(xml).find('resultcode').text() == 'SUCCESS') {
                    if ($(cs).data('id') == '-1') {
                        var option = $('<option/>')
                                        .html($('#txtServiceSettingName').val())
                                        .attr('value', $(xml).find('id').text())
                                        .data('id', $(xml).find('id').text())
                                        .data('name', $('#txtServiceSettingName').val())
                                        .data('setting', $('#txtServiceSetting').val())
                                        .data('encrypted', ($('#chkEncryptServiceSetting').is(':checked') ? 'true' : 'false'));

                        $('#ddlServiceSettings').append(option);
                        $('#ddlServiceSettings').val($(xml).find('id').text());
                        $('#btnServiceSettingUpdate').attr('value', 'Update');
                        $('#btnServiceSettingDelete').fadeIn();
                    }
                    else {
                        $(cs)
                            .html($('#txtServiceSettingName').val())
                            .data('name', $('#txtServiceSettingName').val())
                            .data('setting', $('#txtServiceSetting').val())
                            .data('encrypted', ($('#chkEncryptServiceSetting').is(':checked') ? 'true' : 'false'));
                    }
                }
                else
                    alert($(xml).find('resultcode').text());
            }
            else
                alert('unexpected result');

            $.unblockUI();
        }
    });
}

function deleteServiceSetting(event) {
    preventDefault(event);

    var cs = $('#ddlServiceSettings>option:selected');
    $.blockUI();

    $.ajax({
        url: 'ajax.aspx',
        data: ({
            'handler': 'passthru',
            '__passthrumethod': 'deleteServiceSetting',
            'id': $(cs).data('id'),
            'serviceid': $('#ddlServices').val(),
            'nocache': (new Date()).getTime()
        }),
        type: 'POST',
        dataType: 'xml',
        success: function (xml) {
            if ($(xml).find('resultcode').length == 1) {
                if ($(xml).find('resultcode').text() == 'SUCCESS') {
                    $(cs).remove();
                    $('#txtServiceSettingName').val('');
                    $('#txtServiceSetting').val('');
                    $('#chkEncryptServiceSetting').removeAttr('checked');
                }
                else
                    alert($(xml).find('resultcode').text());
            }
            else
                alert('unexpected result');

            $.unblockUI();
        }
    });
}

function includeMethod() {
    if ($('#chkIncludeInLegion').is(':checked')) {
        //alert('i');
    }
}

function loadImportFile() {
    var files = document.getElementById('filImport').files;
    if (files.length > 0) {
        $('#filImport').slideUp('slow');
        $('#barImport').parent().slideDown('slow');

        var reader = new FileReader();
        var contents = reader.readAsText(files[0]);

        reader.onprogress = function (evt) {
            if (evt.lengthComputable) {
                var loaded = (evt.loaded / evt.total);
                $('#barImport').css('width', (loaded * 100) + "%");

                if (loaded == 1) {
                    $('#importSelectFile').slideUp('slow');
                    $('#importSelectElements').slideDown('slow');
                }
            }
        };

        reader.onload = function (evt) {
            $('#btnImport').data('dom', evt.target.result);

            var dom;
            if (window.DOMParser) {
                parser = new DOMParser();
                dom = parser.parseFromString(evt.target.result, "text/xml");
            }
            else {
                dom = new ActiveXObject("Microsoft.XMLDOM");
                dom.async = false;
                dom.loadXML(evt.target.result);
            }

            verifyExistence(dom, 'servicekey', '#chkImportServiceKey');
            verifyExistence(dom, 'assembly', '#chkImportAssembly');
            verifyExistence(dom, 'interfaceclass', '#chkImportInterfaceClass');
            verifyExistence(dom, 'description', '#chkImportDescription');
            verifyExistence(dom, 'consumeriprange', '#chkImportIPRange');
            verifyExistence(dom, ['logged', 'restricted', 'public'], '#chkImportFlags');
            verifyExistence(dom, 'settings', '#chkImportSettings');
            verifyExistence(dom, 'methods', '#chkImportMethods');

            $('#btnImport').removeAttr('disabled');
        };
    }
    else
        alert('no file selected');
}

function verifyExistence(dom, nodeName, element) {
    var found = false;

    if(typeof(nodeName) == 'string'){
        if ($(dom).find(nodeName).length == 1){
            $(element).attr('checked', 'checked').removeAttr('disabled');
            found = true;
        }
    }
    else {
        $(nodeName).each(function (i, val) {
            if ($(dom).find(val).length > 1) {
                $(element).attr('checked', 'checked').removeAttr('disabled');
                found = true;
                return false;
            }
        });
    }

    if(!found)
        $(element).removeAttr('checked').attr('disabled', 'disabled');

    importCheck();
}

function importService() {
    $.blockUI({
        message: '<h2>Importing...</h2><div style = "margin: auto;width: 90%;"><div class="progress progress-striped active"><div class="bar" style="width: 100%;"></div></div></div>'
    });

    if ($('#ddlServices').val() == '--')
        $('#ddlServices').val('-1');

    if ($('#ddlServices').val() == -1) {
        if (!$('#chkImportServiceKey').is(':checked')) {
            alert('You may not create new service with an blank Service Key.\nCreate the service and then try to import the file again.');
            $.unblockUI();
            return;
        }

        if (!$('#chkImportAssembly').is(':checked')) {
            alert('You cannot import a new service with an blank Assembly.\nCreate the service and then try to import the file again.');
            $.unblockUI();
            return;
        }

        if (!$('#chkImportInterfaceClass').is(':checked')) {
            alert('You cannot import a new service with an blank Interface.\nCreate the service and then try to import the file again.');
            $.unblockUI();
            return;
        }
    }

    var elements = '';
    $('input[type=checkbox][id^=chkImport]:checked').each(function () {
        if ($(this).val() != 'all')
            elements += ';' + $(this).val();
    });

    $('#modalImport').modal('hide');

    $.ajax({
        url: 'ajax.aspx',
        data: ({
            'handler': 'importService',
            'id': $('#ddlServices').val(),
            'elements': elements,
            'dom': $('#btnImport').data('dom'),
            'nocache': (new Date()).getTime()
        }),
        type: 'POST',
        dataType: 'xml',
        success: function (xml) {
            if ($(xml).find('error').length > 0) {
                alert($(xml).find('error').text());
                $.unblockUI();
            }
            else if ($(xml).find('resultcode').length > 0) {
                if ($(xml).find('resultcode').text() == 'SUCCESS')
                    window.location = window.location.pathname + '?serviceid=' + $(xml).find('serviceid').text();
            }
        }
    });
}

function exportService() {
    if ($('#ddlServices').val() != '--' && $('#ddlServices').val() != '-1') {
        var elements = '';
        $('input[type=checkbox][id^=chkExport]:checked').each(function () {
            if($(this).val() != 'all')
                elements += ';' + $(this).val();
        });

        var downloader = $('<iframe/>').attr({
            src: 'Services.aspx?exportservice=true&serviceid=' + $('#ddlServices').val() + '&elements=' + elements,
            height: 1,
            width: 1,
            frameborder: 0
        });

        $('body').append(downloader);
    }
    else
        alert('You must select a service to export.');
}

function importCheck() {
    if ($(this).attr('id') == 'chkImportAll') {
        if ($('#chkImportAll').is(':checked'))
            $('input[type=checkbox][id^=chkImport]').not(':disabled').attr('checked', 'checked');
        else
            $('input[type=checkbox][id^=chkImport]').removeAttr('checked');
    }
    else {
        var allChecked = true;
        $('input[type=checkbox][id^=chkImport]').not('#chkImportAll').each(function () {
            if (!$(this).is(':checked')) {
                allChecked = false;
                return false;
            }
        });

        if (allChecked)
            $('#chkImportAll').attr('checked', 'checked');
        else
            $('#chkImportAll').removeAttr('checked');
    }
}

function exportCheck() {
    if ($(this).attr('id') == 'chkExportAll') {
        if ($('#chkExportAll').is(':checked'))
            $('input[type=checkbox][id^=chkExport]').attr('checked', 'checked');
        else
            $('input[type=checkbox][id^=chkExport]').removeAttr('checked');
    }
    else {
        var allChecked = true;
        $('input[type=checkbox][id^=chkExport]').not('#chkExportAll').each(function () {
            if (!$(this).is(':checked')) {
                allChecked = false;
                return false;
            }
        });

        if(allChecked)
            $('#chkExportAll').attr('checked', 'checked');
        else
            $('#chkExportAll').removeAttr('checked');
    }
}