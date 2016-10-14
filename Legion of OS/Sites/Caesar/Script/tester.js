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
    loadApplications();
    loadServices();
    addParameterRow();

    $('#ddlServices').change(function () {
        loadMethods();
        $('#parameters tr').remove();
        addParameterRow();
    });

    $('#ddlMethods').change(function () {
        $('#parameters tr').remove();
        addParameterRow();
    });

    $('#btnAddParameter').click(addParameterRow);
    $('#btnCallMethod').click(callMethod);

    $('#parameters input.val').live('keydown', function (event) {
        switch (event.keyCode) {
            case 9: //tab
                if (!event.shiftKey && $(this).parents('tr').next().length == 0) {
                    preventDefault(event);
                    addParameterRow();
                }
                break;
            case 13: //enter
                preventDefault(event);
                callMethod();
                break;
        }
    });

    $('#parameters input.val, #parameters input.var').live('blur', function (event) {
        var tr = $(this).parents('tr');
        if ($(tr).siblings().length > 0) {
            if ($(tr).find("input:text[value!='']").length == 0) {
                var next = $(tr).next();
                if ($(next).length != 0) {
                    $(tr).remove();
                    $(next).find('input:first').focus();
                }
            }
        }
    });
});

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
                                .attr('value', $(this).find('key').text());

                $('#ddlApplications').append(option);
            });

            $("#ddlApplications>option").filter(function (i) { return $(this).text() === "Caesar"; }).attr('selected', 'selected');
        }
    });
}

function loadServices() {
    $.blockUI();

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
                                .data('key', $(this).find('key').text());
                
                $('#ddlServices').append(option);
            });

            $.unblockUI();
        }
    });
}

function loadMethods() {
    var service = $('#ddlServices>option:selected');

    $.blockUI();

    $('#ddlMethods').empty();

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
                var option = $('<option/>')
                        .html($(this).find('key').text())
                        .attr('value', $(this).find('key').text())
                        .data('key', $(this).find('key').text());

                $('#ddlMethods').append(option);                        
            });

            $.unblockUI();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            alert(errorThrown);
            $.unblockUI();
        }
    });
}

function addParameterRow() {
    preventDefault(event);

    var rowVar = $('<input/>').attr('type', 'text').addClass('var input-medium');
    var rowVal = $('<input/>').attr('type', 'text').addClass('val input-xxlarge');

    $('#parameters').append(
        $('<tr/>')
            .append($('<td/>').append(rowVar))
            .append($('<td/>').append(rowVal))
    );

    $(rowVar).focus();
}

function callMethod(event) {
    preventDefault(event);
        
    $.blockUI();

    $('#fault,#error,#exception').slideUp();
    $('#txtFault').empty();
    $('#txtException').empty();
    $('#txtError').empty();
    $('#txtResponse').empty();
    $('#txtResult').empty();

    var params = ''; var key, val;
    $('#parameters tr').each(function () {
        vvar = $(this).find('.var').val();
        val = $(this).find('.val').val();

        if(vvar.length > 0)
            params += ';' + encodeURIComponent(vvar) + '=' + encodeURIComponent(val);
    });

    params = (params.length == 0 ? '' : params.substring(1));

    $.ajax({
        url: 'ajax.aspx',
        data: ({
            'handler': 'callMethod',
            'apikey': $('#ddlApplications>option:selected').val(),
            'service': $('#ddlServices>option:selected').data('key'),
            'method': $('#ddlMethods>option:selected').data('key'),
            'params': params,
            'nocache': (new Date()).getTime()
        }),
        type: 'POST',
        dataType: 'xml',
        success: function (xml) {
            var reply = $(xml).find('reply');

            if ($(reply).find('fault').length > 0) {
                $('#txtFault').html($(reply).find('fault').text())
                $('#fault').slideDown();
            }
            
            if ($(reply).children('error').length > 0) {
                $('#txtError').html($(reply).children('error').text())
                $('#error').slideDown();
            }

            if ($(reply).find('exception').length > 0) {
                $('#txtException').val($(reply).find('exception').text().replace(/\n/g, '<br/>').replace(/ /g, '&nbsp;'))
                $('#exception').slideDown();
            }
            
            var xsl = getFile('xsl/prettyxml.xslt');
            $('#txtResponse').val(xmlToString(transformXML(stringToXml('<response>' + $(reply).find('response').text() + '</response>'), xsl)));
            $('#txtResult').val(xmlToString(transformXML(stringToXml('<result>' + $(reply).find('result').text() + '</result>'), xsl)));

            $.unblockUI();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            alert(errorThrown);
            $.unblockUI();
        }
    });
}

function transformXML(xml, xsl) {
    xml = stringToXml(xml);
    xsl = stringToXml(xsl);

    if (window.ActiveXObject)
        return xml.transformNode(xsl);
    else {
        var processor = new XSLTProcessor();
        processor.importStylesheet(xsl);
        return processor.transformToDocument(xml);
    }
}

function getFile(file) {
    var contents = null;

    $.ajax({
        url: file,
        type: 'GET',
        dataType: 'text',
        async: false,
        success: function (c) {
            contents = c;
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            alert(errorThrown);
        }
    });

    return contents;
}

function stringToXml(s) {
    var xml = s;
    if (typeof s == 'string') {
        if (window.DOMParser)
            xml = (new DOMParser().parseFromString(s, "text/xml"));
        else {
            xml = new ActiveXObject("Microsoft.XMLDOM");
            xml.async = "false";
            xml.loadXML(s);
        }
    }
    return xml;
}

function xmlToString(xmlData) {
    if (typeof xmlData == 'string')
        return xmlData;
    else
        return (window.ActiveXObject ? xmlData.xml : (new XMLSerializer()).serializeToString(xmlData));
}