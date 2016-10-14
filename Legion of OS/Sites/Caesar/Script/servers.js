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
    loadHosts();
});

function loadHosts() {
    $.ajax({
        url: 'ajax.aspx',
        data: ({
            'handler': 'passthru',
            '__passthrumethod': 'getServerStatus',
            'nocache': (new Date()).getTime()
        }),
        type: 'POST',
        dataType: 'xml',
        success: function (xml) {
            $('#servers').find('td').parent().remove();

            $(xml).find('server').each(function () {
                var cache = ($(this).find('cacherefreshrequired').text() == 'true' ? false : true);
                var assembly = ($(this).find('assemblyrefreshrequired').text() == 'true' ? false : true);

                var server = $('<tr/>');
                $(server).append($('<td/>').html(
                    $(this).find('hostname').text()
                ).addClass('link').click(dumpCache));

                $(server).append($('<td/>').html(
                    (cache ? 'Current' : 'Stale')).css('color', (cache ? '#00F' : '#F00')).addClass('link').click(refreshCache)
                );

                $(server).append($('<td/>').html(
                    (assembly ? 'Current' : 'Stale')).css('color', (assembly ? '#00F' : '#F00')
                ));

                $('#servers').append(server);
            });
        }
    });
}

function dumpCache() {
    var host = this;

    $('#details').slideUp(function () {
        $(this).html(
            $('<img/>').attr({
                'src': 'images/loader.gif',
                'alt': 'loading...'
            }).css('margin', 'auto')
        );

        $('#details').slideDown(function () {
            setTimeout(function () {
                $.ajax({
                    url: 'ajax.aspx',
                    data: ({
                        'handler': 'getServerCache',
                        'server': $(host).html(),
                        'nocache': (new Date()).getTime()
                    }),
                    type: 'POST',
                    dataType: 'xml',
                    success: function (xml) {
                        $('#details').slideUp(function () {
                            $('#details').html($(xml).find('status').text());
                            $('#details').slideDown();
                        });
                    }
                });
            }, 1000);
        });
    });
}

function refreshCache() {
    var host = $(this).parent().find('td').first();

    $('#details').slideUp(function () {
        $(this).html(
            $('<img/>').attr({
                'src': 'images/loader.gif',
                'alt': 'loading...'
            }).css('margin', 'auto')
        );

        $('#details').slideDown(function () {
            setTimeout(function () {
                $.ajax({
                    url: 'ajax.aspx',
                    data: ({
                        'handler': 'refreshServerCache',
                        'server': $(host).html(),
                        'nocache': (new Date()).getTime()
                    }),
                    type: 'POST',
                    dataType: 'xml',
                    success: function (xml) {
                        $('#details').slideUp(function () {
                            $('#details').html($(xml).find('status').text());
                            $('#details').slideDown();
                        });
                    }
                });
            }, 1000);
        });
    });
}