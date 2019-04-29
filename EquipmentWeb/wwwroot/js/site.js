var app = angular.module('EquipmentApp', ['ui.bootstrap']);
app.run(function () { });

function getUrlParameter(name) {
    name = name.replace(/[\[]/, '\\[').replace(/[\]]/, '\\]');
    var regex = new RegExp('[\\?&]' + name + '=([^&#]*)');
    var results = regex.exec(location.search);
    return results === null ? '' : decodeURIComponent(results[1].replace(/\+/g, ' '));
};

app.controller('EquipmentAppController', ['$rootScope', '$scope', '$http', '$timeout', function ($rootScope, $scope, $http, $timeout) {

    $scope.refresh = function () {
        $http.get('api/Equipments?c=' + new Date().getTime())
            .then(function (data, status) {
                $scope.equipments = data;
            }, function (data, status) {
                    $scope.equipments = undefined;
            });
        $http.get('api/Basket?c=' + new Date().getTime())
            .then(function (data, status) {
                $scope.basket = data;
            }, function (data, status) {
                $scope.basket = undefined;
            });
    };

    $scope.remove = function (item) {
        $http.delete('api/Basket/' + item)
            .then(function (data, status) {
                $scope.refresh();
            })
    };

    $scope.add = function (item, days) {
        var fd = new FormData(); 
        fd.append('item', item);
        fd.append('days', days);

        $http.put('api/Basket/' + item + '/' + days, fd, {
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined }
        })
            .then(function (data, status) {
                $scope.refresh();
                $scope.item = undefined;
            })
    };

    $scope.close = function () {
        $http.post('api/Basket/close')
            .then(function (data, status) {
                $scope.refresh();
            })
    };

    $scope.invoice = function () {
        var culture = getUrlParameter('culture'); 
        window.location.href = 'api/Invoices/Last?c=' + new Date().getTime() + (culture.length > 0 ? '&culture=' + culture : '');

    };

    $scope.invoices = function () {
        var culture = getUrlParameter('culture'); 
        window.location.href = 'api/Invoices?c=' + new Date().getTime() + (culture.length > 0 ? '&culture=' + culture : '');

    };
}]);