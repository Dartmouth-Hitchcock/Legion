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
    highlightActiveNavLink();
    $('button').live('click', preventDefault);
});

function highlightActiveNavLink() {
    var $currentPage = $('#nav a').filter(function () {
        var regex = /^\/?(.*\/)?([\d\w_-]+\.[\d\w_-]{2,4})(\?.*)?$/i;
        var path = (document.location.pathname == '/' ? 'default.aspx' : document.location.pathname);

        var current = regex.exec(path);
        var proposed = regex.exec($(this).attr('href'));

        if (current != null && current.length > 0 && proposed != null && proposed.length > 0)
            return (current[2].toLowerCase() == proposed[2].toLowerCase());
        else
            return false;
    });

    if ($currentPage.length == 1) {
        $currentPage.parent('li').addClass('active');
        $currentPage.parents('li.dropdown').addClass('active');
    }
}

function preventDefault(e) {
    if (e != undefined && e != null) {
        if (e.preventDefault)
            e.preventDefault();
        else
            e.returnValue = false;
    }
}

function _GET(name) {
    name = name.replace(/[\[]/, '\\\[').replace(/[\]]/, '\\\]');
    var regex = new RegExp('[\\?&]' + name + '=([^&#]*)');
    var results = regex.exec(window.location.href);
    if (results == null)
        return '';
    else
        return decodeURIComponent(results[1].replace(/\+/g, ' '));
}