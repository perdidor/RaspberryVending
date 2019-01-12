<%@ Page Title="Адрес устройства № " Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="ChangeDeviceAddress.aspx.cs" Inherits="User_ChangeDeviceAddress" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div id="templatemo_content_wrapper">
        <div id="templatemo_content">
            <!-- end of main column -->
            <div class="section_w960">
                <asp:Label ID="Label1" runat="server" Text="Адрес: " Font-Bold="True" Font-Names="Arial" Font-Size="12pt"></asp:Label><asp:TextBox runat="server" ID="addressbox" Font-Bold="True" Font-Names="Arial" Font-Size="12pt" Width="580px"></asp:TextBox >
                <input runat="server" id="latitude" type="hidden"/>
                <input runat="server" id="longtitude" type="hidden"/>
                <input runat="server" id="wvdidtext" type="hidden"/>
                &nbsp
                <asp:Button ID="devaddressavebutton" runat="server" Text="Сохранить" Enabled="False" EnableViewState="False" OnClick="devaddressavebutton_Click" />
                &nbsp
                <asp:Button ID="addresscancelbutton" runat="server" Text="Отмена" Width="108px" OnClick="addresscancelbutton_Click" />
            </div>
            <div class="cleaner"></div>
            <div id='myMap' style='width: auto; height: 500px; padding: 0 30px 0 0;'></div>
            <script type='text/javascript'>
                function loadMapScenario()
                {
                    var map = new Microsoft.Maps.Map(document.getElementById('myMap'), {
                        supportedMapTypes: [Microsoft.Maps.MapTypeId.road, Microsoft.Maps.MapTypeId.aerial],
                        mapTypeId: Microsoft.Maps.MapTypeId.road,
                        //zoom: 12
                    });
                    var center = map.getCenter();
                    var Events = Microsoft.Maps.Events;
                    var Pushpin = Microsoft.Maps.Pushpin;
                    var Location = Microsoft.Maps.Location;
                    placepin(document.getElementById('<%= latitude.ClientID %>').value, document.getElementById('<%= longtitude.ClientID %>').value);
                    Microsoft.Maps.Events.addHandler(map, 'click', function (e) { handleArgs('mapClick', e); });
                    function handleArgs(id, e) {
                        if (e.targetType == "map") {
                            if (e.isPrimary) {
                                for (var i = map.entities.getLength() - 1; i >= 0; i--) {
                                    var pushpin = map.entities.get(i);
                                    if (pushpin instanceof Microsoft.Maps.Pushpin) {
                                        map.entities.removeAt(i);
                                    }
                                    var point = new Microsoft.Maps.Point(e.getX(), e.getY());
                                    var loc = e.target.tryPixelToLocation(point);
                                    var DevicePin = new Microsoft.Maps.Pushpin(loc, { color: '#f00', text: 'W', title: document.getElementById('<%= wvdidtext.ClientID %>').value/*, subTitle: 'Subtitle'*/ });
                                    map.entities.push(DevicePin);
                                    Microsoft.Maps.loadModule('Microsoft.Maps.Search', function () {
                                        var searchManager = new Microsoft.Maps.Search.SearchManager(map);
                                        var reverseGeocodeRequestOptions = {
                                            location: loc,
                                            callback: function (answer, userData) {
                                            map.setView({ bounds: answer.bestView });
                                            document.getElementById('<%= addressbox.ClientID %>').value = answer.address.formattedAddress;
                                            document.getElementById('<%= latitude.ClientID %>').value = loc.latitude;
                                            document.getElementById('<%= longtitude.ClientID %>').value = loc.longitude;
                                            document.getElementById('<%= devaddressavebutton.ClientID %>').disabled = false;
                                        }
                                    };
                                    searchManager.reverseGeocode(reverseGeocodeRequestOptions);
                                });
                                }
                            }
                        }
                    }
                    function placepin(lat, long) {
                            var pinloc = new Location(lat, long);
                            var DevicePin = new Microsoft.Maps.Pushpin(pinloc, { color: '#f00', text: 'W', title: document.getElementById('<%= wvdidtext.ClientID %>').value/*, subTitle: 'Subtitle'*/});
                            map.entities.push(DevicePin);
                            map.setView({ center: pinloc, zoom: 15 });
                    }
                }
            </script>
            <script type='text/javascript' src='https://www.bing.com/api/maps/mapcontrol?callback=loadMapScenario&key=<%= WWWVars.BingMapsAPIKey %>' async defer></script>
        </div> <!-- end of content -->
    
    	<div class="cleaner"></div>
    </div> <!-- end of templatmeo_content_wrapper -->
</asp:Content>

