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
    loadRateLimitTypes();
    loadApplications();
    loadServices();

    $('#ddlApplications').change(loadApplicationDetails);
    $('#ddlApplicationServices').change(loadServiceMethods);
    $('#btnGenerateApplicationKey').click(generateApplicationKey);
    $('#btnRevokeApplicationKey').click(revokeApplicationKey);
    $('#btnApplicationUpdate').click(updateApplication);
    $('#btnApplicationDelete').click(deleteApplication);

    $('#rateDetails').hide();
    $('#ddlApplicationRateLimitType').change(function () {
        if ($('#ddlApplicationRateLimitType').val().length > 0)
            $('#rateDetails').fadeIn('fast');
        else
            $('#rateDetails').fadeOut('fast');
    });
});

function loadRateLimitTypes() {
    $('#ddlApplicationRateLimitType').empty();
    $('#ddlApplicationRateLimitType').append($('<option/>').html('loading...'));

    $.ajax({
        url: 'ajax.aspx',
        data: ({
            'handler': 'passthru',
            '__passthrumethod': 'getRateLimitTypes',
            'nocache': (new Date()).getTime()
        }),
        type: 'POST',
        dataType: 'xml',
        success: function (xml) {
            $('#ddlApplicationRateLimitType').empty();

            $('#ddlApplicationRateLimitType').append($('<option/>').attr('value', '').html("Default"));
            $(xml).find('ratelimittype').each(function () {
                $('#ddlApplicationRateLimitType').append($('<option/>').attr('value', $(this).attr('id')).html($(this).text()));
            });
        }
    });
}

function loadApplications() {
    $('#ddlApplications').empty();
    $('#ddlApplications').append($('<option/>').html('loading...'));

    $.ajax({
        url: 'ajax.aspx',
        data: ({
            'handler': 'passthru',
            '__passthrumethod': 'getApplicationDetailList',
            'nocache': (new Date()).getTime()
        }),
        type: 'POST',
        dataType: 'xml',
        success: function (xml) {
            $('#ddlApplications').empty();

            $(xml).find('application').each(function () {
                var option = $('<option/>')
                                .html($(this).find('name').text())
                                .attr('value', $(this).find('id').text())
                                .data('id', $(this).find('id').text())
                                .data('key', $(this).find('key').text())
                                .data('name', $(this).find('name').text())
                                .data('consumeriprange', $(this).find('consumeriprange').text())
                                .data('public', $(this).find('public').text())
                                .data('logged', $(this).find('logged').text())
                                .data('description', $(this).find('description').text())
                                .data('ratelimittypeid', $(this).find('ratelimittypeid').text())
                                .data('ratelimittype', $(this).find('ratelimittype').text())
                                .data('ratelimit', $(this).find('ratelimit').text())
                                .data('ratelimitinterval', $(this).find('ratelimitinterval').text());
                
                $('#ddlApplications').append(option);
            });

            $('#ddlApplications').append($('<option/>').html('-- new --').attr('value', '-1').data('id', '-1'));
        }
    });
}

function loadApplicationDetails() {
    var application = $('#ddlApplications>option:selected');
    if ($(application).attr('value')[0] != '-') {
        $('#txtApplicationName').val($(application).data('name'));
        $('#txtApplicationKey').val($(application).data('key'));
        $('#txtApplicationIPRange').val($(application).data('consumeriprange'));
        $('#txtApplicationDescription').val($(application).data('description'));
        $('#btnApplicationUpdate').attr('value', 'Update');
        $('#btnApplicationDelete').fadeIn();
        
        if ($(application).data('logged') == 'true')
            $('#chkApplicationLogged').attr('checked', 'checked');
        else
            $('#chkApplicationLogged').removeAttr('checked');
        
        if ($(application).data('public') == 'true')
            $('#chkApplicationPublic').attr('checked', 'checked');
        else
            $('#chkApplicationPublic').removeAttr('checked');

        $('#ddlApplicationRateLimitType').val($(application).data('ratelimittypeid'));
        if ($(application).data('ratelimittypeid').length > 0) {
            $('#txtApplicationRateLimit').val($(application).data('ratelimit'));
            $('#ddlApplicationRateInterval').val($(application).data('ratelimitinterval'));
            $('#rateDetails').fadeIn('fast');
        }
        else
            $('#rateDetails').fadeOut('fast');

        loadServices();
    }
    else {
        $('#txtApplicationName').val('');
        $('#txtApplicationKey').val('');
        $('#txtApplicationIPRange').val('');
        $('#txtApplicationDescription').val('');
        $('#ddlApplicationRateLimitType').val('');
        $('#ddlApplicationRateLimit').val('');
        $('#ddlApplicationRateInterval').val('');
        $('#chkApplicationLogged').removeAttr('checked');
        $('#chkApplicationPublic').removeAttr('checked');

        loadServices();

        if ($(application).attr('value') == '-1') {
            generateApplicationKey();
            //new
            $('#btnApplicationUpdate').attr('value', 'Create');
            $('#btnApplicationDelete').fadeOut();
        }
    }
}

function generateApplicationKey(event) {
    preventDefault(event);

    $('#txtApplicationKey').val('generating...');

    $.ajax({
        url: 'ajax.aspx',
        data: ({
            'handler': 'passthru',
            '__passthrumethod': 'getNewApplicationKey',
            'nocache': (new Date()).getTime()
        }),
        type: 'POST',
        dataType: 'xml',
        success: function (xml) {
            if ($(xml).find('applicationkey').length == 1)
                $('#txtApplicationKey').val($(xml).find('applicationkey').text());
        }
    });
}

function revokeApplicationKey(event) {
    preventDefault(event);

    if (confirm('Revoking an application key is non-reversible and will take effect immediately. Are you sure you wish to continue?')) {
        $.blockUI();

        $.ajax({
            url: 'ajax.aspx',
            data: ({
                'handler': 'passthru',
                '__passthrumethod': 'revokeApplicationKey',
                'id': $('#ddlApplications>option:selected').data('id'),
                'nocache': (new Date()).getTime()
            }),
            type: 'POST',
            dataType: 'xml',
            success: function (xml) {
                var name = $('#ddlApplications>option:selected').data('name');
                if (name.substring('-1') == 's')
                    name = name + '\'';
                else
                    name = name + '\'s';

                alert(name + ' application key has been revoked.');

                $('#txtApplicationKey').val('');
                $('#ddlApplications>option:selected').data('key', '')
                $.unblockUI();
            }
        });
    }
}

function updateApplication(event) {
    preventDefault(event);

    var application = $('#ddlApplications>option:selected');

    if ($(application).val().length > 0) {
        $.blockUI();

        $.ajax({
            url: 'ajax.aspx',
            data: ({
                'handler': 'passthru',
                '__passthrumethod': 'updateApplication',
                'id': $(application).data('id'),
                'name': $('#txtApplicationName').val(),
                'appkey': $('#txtApplicationKey').val(),
                'range': $('#txtApplicationIPRange').val(),
                'description': $('#txtApplicationDescription').val(),
                'public': ($('#chkApplicationPublic').is(':checked') ? 'true' : 'false'),
                'logged': ($('#chkApplicationLogged').is(':checked') ? 'true' : 'false'),
                'ratelimittypeid': $('#ddlApplicationRateLimitType>option:selected').val(),
                'ratelimit': $('#txtApplicationRateLimit').val(),
                'ratelimitinterval': $('#ddlApplicationRateInterval>option:selected').val(),
                'nocache': (new Date()).getTime()
            }),
            type: 'POST',
            dataType: 'xml',
            success: function (xml) {
                if ($(xml).find('resultcode').length > 0) {
                    if ($(xml).find('resultcode').text() == 'SUCCESS') {
                        if ($(application).data('id') == '-1') {
                            var option = $('<option/>')
                                .html($('#txtApplicationName').val())
                                .attr('value', $(xml).find('id').text())
                                .data('id', $(xml).find('id').text())
                                .data('key', $('#txtApplicationKey').val())
                                .data('name', $('#txtApplicationName').val())
                                .data('consumeriprange', $('#txtApplicationIPRange').val())
                                .data('description', $('#txtApplicationDescription').val())
                                .data('ratelimittypeid', $('#ddlApplicationRateLimitType>option:selected').val())
                                .data('ratelimittype', $('##ddlApplicationRateLimitType>option:selected').html())
                                .data('ratelimit', $('#txtApplicationRateLimit').val())
                                .data('ratelimitinterval', $('#ddlApplicationRateInterval>option:selected').val())
                                .data('public', ($('#chkApplicationPublic').is(':checked') ? 'true' : 'false'))
                                .data('logged', ($('#chkApplicationLogged').is(':checked') ? 'true' : 'false'));

                            $('#ddlApplications').append(option);
                            $('#ddlApplications').val($(xml).find('id').text()).change();
                            updatePermissions(option);
                        }
                        else {
                            $(application)
                                .html($('#txtApplicationName').val())
                                .data('key', $('#txtApplicationKey').val())
                                .data('name', $('#txtApplicationName').val())
                                .data('consumeriprange', $('#txtApplicationIPRange').val())
                                .data('description', $('#txtApplicationDescription').val())
                                .data('ratelimittypeid', $('#ddlApplicationRateLimitType>option:selected').val())
                                .data('ratelimittype', $('#ddlApplicationRateLimitType>option:selected').html())
                                .data('ratelimit', $('#txtApplicationRateLimit').val())
                                .data('ratelimitinterval', $('#ddlApplicationRateInterval>option:selected').val())
                                .data('logged', ($('#chkApplicationPublic').is(':checked') ? 'true' : 'false'))
                                .data('logged', ($('#chkApplicationLogged').is(':checked') ? 'true' : 'false'));

                            updatePermissions($('#ddlApplications>option:selected'));
                        }
                    }
                    else {
                        alert($(xml).find('resultcode').text());
                        $.unblockUI();
                    }
                }
                else {
                    alert('unexpected result');
                    $.unblockUI();
                }
            }
        });
    }
}

function deleteApplication(event) {
    preventDefault(event);

    if (confirm('Are you sure?')) {
        var application = $('#ddlApplications>option:selected');

        $.blockUI();

        $.ajax({
            url: 'ajax.aspx',
            data: ({
                'handler': 'passthru',
                '__passthrumethod': 'deleteApplication',
                'id': $(application).data('id'),
                'nocache': (new Date()).getTime()
            }),
            type: 'POST',
            dataType: 'xml',
            success: function (xml) {
                if ($(xml).find('resultcode').length == 1) {
                    if ($(xml).find('resultcode').text() == 'SUCCESS') {
                        $('#txtApplicationName').val('');
                        $('#txtApplicationKey').val('');
                        $('#txtApplicationIPRange').val('');
                        $('#txtApplicationDescription').val('');
                        $('#chkApplicationLogged').removeAttr('checked');
                        $(application).remove();
                        loadServices();
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

function loadServices() {
    $('#ddlApplicationServices').empty().append($('<option/>').html('loading...'));
    $('#lstMethods').empty().html('loading...');

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
            $('#ddlApplicationServices').empty();

            $(xml).find('service').each(function () {
                var option = $('<option/>')
                                .html($(this).find('key').text())
                                .attr('value', $(this).find('id').text())
                                .data('id', $(this).find('id').text())
                                .data('key', $(this).find('key').text())
                                .data('assembly', $(this).find('assembly').text())
                                .data('class', $(this).find('class').text())
                                .data('restricted', $(this).find('restricted').text())
                                .data('description', $(this).find('description').text());

                $('#ddlApplicationServices').append(option);
            });

            $.ajax({
                url: 'ajax.aspx',
                data: ({
                    'handler': 'passthru',
                    '__passthrumethod': 'getMethodList',
                    'nocache': (new Date()).getTime()
                }),
                type: 'POST',
                dataType: 'xml',
                success: function (xml) {
                    $('#lstMethods').empty();
                    $(xml).find('method').each(function () {
                        var check = $('<input/>')
                                .attr({
                                    'id': 'chkMethod' + $(this).find('id').text(),
                                    'type': 'checkbox'
                                })
                                .data('id', $(this).find('id').text())
                                .data('restricted', $(this).find('restricted').text());

                        if ($(this).find('restricted').text() == 'false')
                            $(check).attr({ 'checked': 'checked', 'disabled': 'disabled' });

                        var item = $('<label/>')
                                    .attr('id', 'method' + $(this).find('id').text())
                                    .addClass('checkbox')
                                    .css('display', 'none')
                                    .append(check)
                                    .append(
                                        $('<span/>')
                                            .attr('for', 'method' + $(this).find('id').text())
                                            .html($(this).find('key').text())
                                    );

                        $('#lstMethods').append(item);
                    });

                    var application = $('#ddlApplications>option:selected');
                    if (application.length == 1) {
                        $.ajax({
                            url: 'ajax.aspx',
                            data: ({
                                'handler': 'passthru',
                                '__passthrumethod': 'getApplicationPermissions',
                                'applicationId': $(application).attr('value'),
                                'nocache': (new Date()).getTime()
                            }),
                            type: 'POST',
                            dataType: 'xml',
                            success: function (xml) {
                                $('#lstMthods input[id!=chkMethod]:not([disabled])').removeAttr('checked');

                                $(xml).find('permission').each(function () {
                                    $('#chkMethod' + $(this).find('id').text()).attr('checked', 'checked');
                                });

                                $.unblockUI();
                            }
                        });
                    }
                    else {
                        $.unblockUI();
                    }
                }
            });
        }
    });
}

function loadServiceMethods() {
    var service = $('#ddlApplicationServices>option:selected');

    $.blockUI();

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
            $('label[id^=method]').hide();
            $(xml).find('method').each(function () {
                $('#method' + $(this).find('id').text()).show()
            });

            $.unblockUI();
        }
    });
}

function updatePermissions(application) {
    var permissions = '';

    if ($(application).attr('value') != '-1') {
        $.blockUI();

        $('input[id^=chkMethod]').each(function () {
            if ($(this).is(':checked') && $(this).data('restricted') == 'true')
                permissions += $(this).data('id') + ';';
        });

        if (permissions.length > 0)
            permissions = permissions.substring(0, permissions.length - 1);

        $.ajax({
            url: 'ajax.aspx',
            data: ({
                'handler': 'passthru',
                '__passthrumethod': 'updateApplicationPermissions',
                'applicationId': $(application).attr('value'),
                'permissions': permissions,
                'nocache': (new Date()).getTime()
            }),
            type: 'POST',
            dataType: 'xml',
            success: function (xml) {
                if ($(xml).find('resultcode').length > 0) {
                    if ($(xml).find('resultcode').text() != 'SUCCESS')
                        alert($(xml).find('resultcode').text());
                }
                else
                    alert('unexpected result');

                $.unblockUI();
            }
        });
    }
}