
//appId: '192308807773103',//eventzone.azure
window.fbAsyncInit = function () {
    FB.init({
        appId: '1722319744663376',
        xfbml: true,
        version: 'v2.5'
    });
};

(function(d, s, id){
    var js, fjs = d.getElementsByTagName(s)[0];
    if (d.getElementById(id)) {return;}
    js = d.createElement(s); js.id = id;
    js.src = "//connect.facebook.net/en_US/sdk.js";
    fjs.parentNode.insertBefore(js, fjs);
}(document, 'script', 'facebook-jssdk'));

function share(link) {
    FB.ui({
        method: 'share',
        href: 'http://eventzone.azurewebsites.net/Event/Details/'+link,
    }, function (response) {
    });
}